targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param environmentName string
param variableName string
param variableValue string

extension github with {
  token: githubToken
}

resource environmentVariable 'EnvironmentVariable' = {
  owner: owner
  repo: repoName
  environment: environmentName
  name: variableName
  value: variableValue
}

output environmentVariable object = environmentVariable
