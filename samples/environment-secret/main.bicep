targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param environmentName string
param secretName string
@secure()
param secretValue string

extension github with {
  token: githubToken
}

resource environmentSecret 'EnvironmentSecret' = {
  owner: owner
  repo: repoName
  environment: environmentName
  name: secretName
  value: secretValue
}

output environmentSecret object = environmentSecret
