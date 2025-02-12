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

output repo object = repo
output collaborator object = collaborator
