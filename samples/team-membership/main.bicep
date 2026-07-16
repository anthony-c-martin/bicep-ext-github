targetScope = 'local'

@secure()
param githubToken string

param org string
param teamSlug string
param username string
param role string

extension github with {
  token: githubToken
}

resource membership 'TeamMembership' = {
  org: org
  teamSlug: teamSlug
  username: username
  role: role
}

output teamMembership object = membership
