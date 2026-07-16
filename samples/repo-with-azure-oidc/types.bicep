@export()
type GitHubRepoConfig = {
  owner: string
  name: string
  collaborators: string[]
}

@export()
type AzureOidcConfig = {
  tenantId: string
  subscriptionId: string
  clientId: string
}

@export()
var OwnerRoleDefinition = '8e3af657-a8ff-443c-a75c-2fe8c4bcb635'
