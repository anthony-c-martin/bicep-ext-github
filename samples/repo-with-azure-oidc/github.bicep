targetScope = 'local'

import { GitHubRepoConfig, AzureOidcConfig } from './types.bicep'

extension github with {
  token: gitHubToken
}

@secure()
param gitHubToken string
param gitHubRepo GitHubRepoConfig
param azureOidcConfig AzureOidcConfig

resource repo 'Repository' = {
  owner: gitHubRepo.owner
  name: gitHubRepo.name
  visibility: 'Public'
  description: 'Demo repo created with https://github.com/anthony-c-martin/bicep-ext-github/samples/github-repo-with-oidc/main.bicepparam'
  allowSquashMerge: true
}

resource tenantId 'ActionsSecret' = {
  owner: repo.owner
  repo: repo.name
  name: 'AZURE_TENANT_ID'
  #disable-next-line use-secure-value-for-secure-inputs
  value: azureOidcConfig.tenantId
}

resource subscriptionId 'ActionsSecret' = {
  owner: repo.owner
  repo: repo.name
  name: 'AZURE_SUBSCRIPTION_ID'
  #disable-next-line use-secure-value-for-secure-inputs
  value: azureOidcConfig.subscriptionId
}

resource clientId 'ActionsSecret' = {
  owner: repo.owner
  repo: repo.name
  name: 'AZURE_CLIENT_ID'
  #disable-next-line use-secure-value-for-secure-inputs
  value: azureOidcConfig.clientId
}

resource collaborators 'Collaborator' = [
  for user in gitHubRepo.collaborators: {
    owner: repo.owner
    repo: repo.name
    user: user
    permission: 'write'
  }
]
