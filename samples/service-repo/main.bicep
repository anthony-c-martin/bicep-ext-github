targetScope = 'local'

// End-to-end bootstrap of a brand-new service repository.
//
// This sample provisions a repository and then layers on the surrounding
// configuration a platform team would typically set up on day one:
//   * seed files (README, CODEOWNERS, CI workflow) that create the initial commit
//   * a protected default branch + a long-lived integration branch
//   * triage labels
//   * hardened GitHub Actions permissions
//   * CI/CD configuration (Actions + Dependabot secrets and variables)
//   * deployment environments with approvals and per-environment config
//   * access control (an owning team, a human maintainer, a deploy key)
//   * an outbound webhook for ChatOps/CI notifications
//
// Deploy with: bicep local-deploy ./samples/service-repo/main.bicepparam

@secure()
param githubToken string

@description('The user or organization that will own the repository.')
param owner string

@description('The name of the repository to create.')
param repoName string

@description('The organization used for team management (usually the same as owner for org repos).')
param org string

@description('Slug-friendly display name for the owning team.')
param teamName string

@description('An additional human collaborator to grant write access.')
param maintainer string

@description('Numeric GitHub user or team id allowed to approve production deployments.')
param productionReviewerId int

@description('Public SSH key used as a read-only deploy key.')
param deployKey string

@description('HTTPS endpoint that should receive repository webhook deliveries.')
param webhookUrl string

@secure()
@description('Token published to Actions for pushing packages.')
param nugetApiKey string

@secure()
@description('Token published to Dependabot for restoring from a private feed.')
param privateFeedToken string

@secure()
@description('Connection string exposed only to the production environment.')
param productionConnectionString string

@secure()
@description('Shared secret used to sign webhook deliveries.')
param webhookSecret string

extension github with {
  token: githubToken
}

// ---------------------------------------------------------------------------
// 1. The repository itself.
// ---------------------------------------------------------------------------
resource repo 'Repository' = {
  owner: owner
  name: repoName
  description: 'Service repository bootstrapped with bicep-ext-github'
  visibility: 'Private'
  hasIssues: true
  hasWiki: false
  allowSquashMerge: true
  allowMergeCommit: false
  allowRebaseMerge: false
  deleteBranchOnMerge: true
  allowAutoMerge: true
}

// ---------------------------------------------------------------------------
// 2. Seed files. The first file creates the initial commit (and the default
//    branch), so the files are chained to guarantee ordering.
// ---------------------------------------------------------------------------
resource readme 'GitHubFile' = {
  owner: repo.owner
  repo: repo.name
  path: 'README.md'
  commitMessage: 'Add README'
  content: '''
# ${repoName}

Bootstrapped automatically with [bicep-ext-github](https://github.com/anthony-c-martin/bicep-ext-github).
'''
}

resource codeowners 'GitHubFile' = {
  owner: repo.owner
  repo: repo.name
  path: '.github/CODEOWNERS'
  commitMessage: 'Add CODEOWNERS'
  content: '* @${org}/${teamName}\n'
  dependsOn: [
    readme
  ]
}

resource ciWorkflow 'GitHubFile' = {
  owner: repo.owner
  repo: repo.name
  path: '.github/workflows/ci.yml'
  commitMessage: 'Add CI workflow'
  content: '''
name: ci
on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
permissions:
  contents: read
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: echo "build $BUILD_CONFIGURATION"
'''
  dependsOn: [
    codeowners
  ]
}

// ---------------------------------------------------------------------------
// 3. Branching: a long-lived integration branch off the default branch.
// ---------------------------------------------------------------------------
resource develop 'Branch' = {
  owner: repo.owner
  repo: repo.name
  name: 'develop'
  sourceBranch: 'main'
  dependsOn: [
    ciWorkflow
  ]
}

// ---------------------------------------------------------------------------
// 4. Protect the default branch with a ruleset (require reviewed PRs).
// ---------------------------------------------------------------------------
resource mainProtection 'RepositoryRuleset' = {
  owner: repo.owner
  repo: repo.name
  name: 'protect-main'
  target: 'branch'
  enforcement: 'active'
  conditions: {
    refName: {
      include: [
        '~DEFAULT_BRANCH'
      ]
      exclude: []
    }
  }
  rules: [
    {
      type: 'pull_request'
      parameters: {
        dismissStaleReviewsOnPush: true
        requireCodeOwnerReview: true
        requireLastPushApproval: true
        requiredApprovingReviewCount: 1
        requiredReviewThreadResolution: true
      }
    }
    {
      type: 'deletion'
    }
    {
      type: 'non_fast_forward'
    }
  ]
}

// ---------------------------------------------------------------------------
// 5. Triage labels.
// ---------------------------------------------------------------------------
resource bugLabel 'Label' = {
  owner: repo.owner
  repo: repo.name
  name: 'bug'
  color: 'd73a4a'
  description: 'Something is not working'
}

resource needsTriageLabel 'Label' = {
  owner: repo.owner
  repo: repo.name
  name: 'needs-triage'
  color: 'fbca04'
  description: 'Awaiting initial review'
}

// ---------------------------------------------------------------------------
// 6. Harden GitHub Actions: read-only default token, no PR approvals from bots.
// ---------------------------------------------------------------------------
resource actionsPermissions 'RepositoryActionsPermissions' = {
  owner: repo.owner
  repo: repo.name
  enabled: true
  allowedActions: 'all'
  defaultWorkflowPermissions: 'read'
  canApprovePullRequestReviews: false
}

// ---------------------------------------------------------------------------
// 7. CI/CD configuration: repo-level Actions variable + secret, and a
//    Dependabot secret for private package restores.
// ---------------------------------------------------------------------------
resource buildConfig 'ActionsVariable' = {
  owner: repo.owner
  repo: repo.name
  name: 'BUILD_CONFIGURATION'
  value: 'Release'
}

resource nugetSecret 'ActionsSecret' = {
  owner: repo.owner
  repo: repo.name
  name: 'NUGET_API_KEY'
  value: nugetApiKey
}

resource feedToken 'DependabotSecret' = {
  owner: repo.owner
  repo: repo.name
  name: 'PRIVATE_FEED_TOKEN'
  value: privateFeedToken
}

// ---------------------------------------------------------------------------
// 8. Deployment environments. Production requires an approval and a wait timer.
// ---------------------------------------------------------------------------
resource staging 'Environment' = {
  owner: repo.owner
  repo: repo.name
  name: 'staging'
  waitTimer: 0
  preventSelfReview: false
  protectedBranches: false
  customBranchPolicies: true
}

resource production 'Environment' = {
  owner: repo.owner
  repo: repo.name
  name: 'production'
  waitTimer: 10
  preventSelfReview: true
  protectedBranches: true
  customBranchPolicies: false
  reviewers: [
    {
      id: productionReviewerId
      type: 'Team'
    }
  ]
}

resource productionAppEnv 'EnvironmentVariable' = {
  owner: repo.owner
  repo: repo.name
  environment: production.name
  name: 'APP_ENV'
  value: 'production'
}

resource productionConnection 'EnvironmentSecret' = {
  owner: repo.owner
  repo: repo.name
  environment: production.name
  name: 'DATABASE_CONNECTION_STRING'
  value: productionConnectionString
}

// ---------------------------------------------------------------------------
// 9. Access control: an owning team, a human maintainer, and a deploy key.
// ---------------------------------------------------------------------------
resource platformTeam 'Team' = {
  org: org
  name: teamName
  description: 'Owns and operates the ${repoName} service'
  privacy: 'closed'
}

resource teamAccess 'TeamRepositoryPermission' = {
  org: org
  teamSlug: platformTeam.slug!
  owner: repo.owner
  repo: repo.name
  permission: 'maintain'
}

resource humanMaintainer 'Collaborator' = {
  owner: repo.owner
  repo: repo.name
  user: maintainer
  permission: 'push'
}

resource ciDeployKey 'DeployKey' = {
  owner: repo.owner
  repo: repo.name
  title: 'ci-deploy'
  key: deployKey
  readOnly: true
}

// ---------------------------------------------------------------------------
// 10. Outbound webhook for CI/ChatOps notifications.
// ---------------------------------------------------------------------------
resource notifications 'RepositoryWebhook' = {
  owner: repo.owner
  repo: repo.name
  url: webhookUrl
  contentType: 'json'
  active: true
  secret: webhookSecret
  events: [
    'push'
    'pull_request'
    'deployment_status'
  ]
}

output repositoryUrl string = 'https://github.com/${owner}/${repoName}'
output developBranchSha string = develop.sha!
output owningTeamSlug string = platformTeam.slug!
