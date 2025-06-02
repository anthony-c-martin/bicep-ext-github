targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param collaboratorName string

extension github with {
  token: githubToken
}

resource repo 'Repository' = {
  owner: owner
  name: repoName
  description: 'Test bicep repository'
  visibility: 'Public'
}

resource collaborator 'Collaborator' = {
  owner: owner
  repo: repo.name
  user: collaboratorName
}

resource bugLabel 'Label' = {
  owner: owner
  repo: repo.name
  name: 'bug'
  description: 'Report a bug!'
  color: 'f29513'
}

resource secret 'ActionsSecret' = {
  owner: owner
  repo: repo.name
  name: 'MY_SECRET'
  value: 'super-secret-value'
}

resource variable 'ActionsVariable' = {
  owner: owner
  repo: repoName
  name: 'MY_VARIABLE'
  value: 'just-another-value'
}

output repo object = repo
output collaborator object = collaborator
output variable object = variable
