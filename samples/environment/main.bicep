targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param environmentName string

extension github with {
  token: githubToken
}

resource environment 'Environment' = {
  owner: owner
  repo: repoName
  name: environmentName
  waitTimer: 0
  preventSelfReview: true
  protectedBranches: true
  customBranchPolicies: false
}

output deploymentEnvironment object = environment
