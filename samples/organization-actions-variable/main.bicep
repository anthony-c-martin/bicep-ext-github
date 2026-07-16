targetScope = 'local'

@secure()
param githubToken string

param org string
param variableName string
param variableValue string
param visibility string

extension github with {
  token: githubToken
}

resource orgVariable 'OrganizationActionsVariable' = {
  org: org
  name: variableName
  value: variableValue
  visibility: visibility
}

output organizationActionsVariable object = orgVariable
