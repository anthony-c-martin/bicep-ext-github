targetScope = 'local'

@secure()
param githubToken string

param org string
param secretName string
@secure()
param secretValue string
param visibility string

extension github with {
  token: githubToken
}

resource orgSecret 'OrganizationActionsSecret' = {
  org: org
  name: secretName
  value: secretValue
  visibility: visibility
}

output organizationActionsSecret object = orgSecret
