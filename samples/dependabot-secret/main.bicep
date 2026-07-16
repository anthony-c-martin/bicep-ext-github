targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param secretName string
@secure()
param secretValue string

extension github with {
  token: githubToken
}

resource dependabotSecret 'DependabotSecret' = {
  owner: owner
  repo: repoName
  name: secretName
  value: secretValue
}

output dependabotSecret object = dependabotSecret
