targetScope = 'local'

@secure()
@description('GitHub personal access token used to authenticate the extension.')
param githubToken string

@description('The personal account that owns the repository.')
param owner string

@description('The name of the repository to create.')
param repoName string

param repoConfig {
  description: string
  topics: string[]
}

@description('GitHub users to grant write access to.')
param collaborators string[]

extension github with {
  token: githubToken
}

var labels = [
  {
    name: 'bug'
    description: 'Something is not working'
    color: 'd73a4a'
  }
  {
    name: 'enhancement'
    description: 'New feature or request'
    color: 'a2eeef'
  }
]

resource repo 'Repository' = {
  owner: owner
  name: repoName
  description: repoConfig.description
  homepage: 'https://${owner}.github.io/${repoName}'
  visibility: 'Public'
  topics: repoConfig.topics
  hasIssues: true
  hasWiki: false
  allowSquashMerge: true
  allowMergeCommit: false
  allowRebaseMerge: false
  deleteBranchOnMerge: true
}

resource readme 'GitHubFile' = {
  owner: repo.owner
  repo: repo.name
  path: 'README.md'
  commitMessage: 'Add README'
  content: $'''
# ${repoName}

Managed with [bicep-ext-github](https://github.com/anthony-c-martin/bicep-ext-github).
'''
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
    branches: [main]
  pull_request:
    branches: [main]
permissions:
  contents: read
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Build
        run: echo "Add your build and test commands here"
'''
  dependsOn: [
    readme
  ]
}

resource resLabels 'Label' = [for label in labels: {
  owner: repo.owner
  repo: repo.name
  name: label.name
  color: label.color
  description: label.description
}]

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
        requireCodeOwnerReview: false
        requireLastPushApproval: false
        requiredApprovingReviewCount: 1
        requiredReviewThreadResolution: true
      }
    }
    {
      type: 'required_status_checks'
      parameters: {
        strictRequiredStatusChecksPolicy: true
        requiredStatusChecks: [
          {
            context: 'build'
          }
        ]
      }
    }
    {
      type: 'deletion'
    }
    {
      type: 'non_fast_forward'
    }
  ]
  dependsOn: [
    readme
    ciWorkflow
  ]
}

// Grant a friend/collaborator write access.
resource resCollaborators 'Collaborator' = [for collaborator in collaborators: {
  owner: repo.owner
  repo: repo.name
  user: collaborator
  permission: 'push'
}]
