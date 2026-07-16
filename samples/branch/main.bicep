targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param branchName string
param sourceBranch string

extension github with {
  token: githubToken
}

resource branch 'Branch' = {
  owner: owner
  repo: repoName
  name: branchName
  sourceBranch: sourceBranch
}

output branchSha string = branch.sha!
