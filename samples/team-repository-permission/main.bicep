targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param teamOrg string
param teamSlug string
param permission string

extension github with {
  token: githubToken
}

resource teamPermission 'TeamRepositoryPermission' = {
  org: teamOrg
  repo: repoName
  owner: owner
  teamSlug: teamSlug
  permission: permission
}

output teamRepositoryPermission object = teamPermission
