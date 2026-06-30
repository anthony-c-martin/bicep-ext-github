targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param keyTitle string
param deployKeyPublic string

extension github with {
  token: githubToken
}

resource deployKey 'DeployKey' = {
  owner: owner
  repo: repoName
  title: keyTitle
  key: deployKeyPublic
  readOnly: true
}

output repositoryDeployKey object = deployKey
