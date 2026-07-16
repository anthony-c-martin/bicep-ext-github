targetScope = 'local'

@secure()
param githubToken string

param org string
param teamName string
param description string
param privacy string

extension github with {
  token: githubToken
}

resource team 'Team' = {
  org: org
  name: teamName
  description: description
  privacy: privacy
}

output teamSlug string = team.slug!
output teamId int = team.id!
