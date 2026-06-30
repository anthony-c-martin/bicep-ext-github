targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string

extension github with {
  token: githubToken
}

resource ruleset 'RepositoryRuleset' = {
  owner: owner
  repo: repoName
  name: 'main-branch-pr-policy'
  target: 'branch'
  enforcement: 'active'
  conditions: {
    refName: {
      include: [
        'refs/heads/main'
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
  ]
}

output repositoryRuleset object = ruleset
