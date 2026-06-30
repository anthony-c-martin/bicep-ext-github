targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param teamOrg string
param teamSlug string

@secure()
param azureOidc {
  tenantId: string
  subscriptionId: string
  clientId: string
}

var defaultBranch = 'main'

extension github with {
  token: githubToken
}

resource repo 'Repository' = {
  owner: owner
  name: repoName
  description: 'Secure baseline repository managed by Bicep'
  visibility: 'Private'
  hasIssues: true
  hasProjects: false
  hasWiki: false
  hasDownloads: false
  allowSquashMerge: true
  allowMergeCommit: false
  allowRebaseMerge: false
  deleteBranchOnMerge: true
  allowAutoMerge: true
}

resource teamPermission 'TeamRepositoryPermission' = {
  org: teamOrg
  repo: repo.name
  owner: owner
  teamSlug: teamSlug
  permission: 'maintain'
}

resource branchProtection 'BranchProtectionRule' = {
  owner: owner
  repo: repo.name
  branch: defaultBranch
  strictStatusChecks: true
  requiredStatusCheckContexts: [
    'build'
    'test'
    'security-scan'
  ]
  enablePullRequestReviews: true
  dismissStaleReviews: true
  requireCodeOwnerReviews: true
  requiredApprovingReviewCount: 2
  requireLastPushApproval: true
  enforceAdmins: true
  requireSignedCommits: true
  requiredLinearHistory: true
  allowForcePushes: false
  allowDeletions: false
  requiredConversationResolution: true
  lockBranch: false
  blockCreations: false
  pushRestrictionTeams: [
    teamSlug
  ]
  dismissalRestrictionTeams: [
    teamSlug
  ]
}

resource ruleset 'RepositoryRuleset' = {
  owner: owner
  repo: repo.name
  name: 'default'
  target: 'branch'
  enforcement: 'active'
  conditions: {
    refName: {
      include: [
        'refs/heads/${defaultBranch}'
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
        requiredApprovingReviewCount: 2
        requiredReviewThreadResolution: true
      }
    }
  ]
}

resource azureClientIdSecret 'ActionsSecret' = {
  owner: owner
  repo: repo.name
  name: 'AZURE_CLIENT_ID'
  value: azureOidc.clientId
}

resource azureSubscriptionIdSecret 'ActionsSecret' = {
  owner: owner
  repo: repo.name
  name: 'AZURE_SUBSCRIPTION_ID'
  value: azureOidc.subscriptionId
}

resource azureTenantIdSecret 'ActionsSecret' = {
  owner: owner
  repo: repo.name
  name: 'AZURE_TENANT_ID'
  value: azureOidc.tenantId
}

resource securityLabel 'Label' = {
  owner: owner
  repo: repo.name
  name: 'security'
  color: 'B60205'
  description: 'Security-related issue or task'
}

resource bugLabel 'Label' = {
  owner: owner
  repo: repo.name
  name: 'bug'
  color: 'D73A4A'
  description: 'Bug report'
}
