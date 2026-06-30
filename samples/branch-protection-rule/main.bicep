targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param teamSlug string
param protectedBranch string

extension github with {
  token: githubToken
}

resource branchProtection 'BranchProtectionRule' = {
  owner: owner
  repo: repoName
  branch: protectedBranch
  strictStatusChecks: true
  requiredStatusCheckContexts: [
    'build'
    'lint'
  ]
  enablePullRequestReviews: true
  dismissStaleReviews: true
  requireCodeOwnerReviews: false
  requiredApprovingReviewCount: 1
  requireLastPushApproval: false
  enforceAdmins: true
  requireSignedCommits: false
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

output branchProtectionRule object = branchProtection
