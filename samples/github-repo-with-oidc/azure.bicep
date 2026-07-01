targetScope = 'subscription'

import { AzureOidcConfig, GitHubRepoConfig, OwnerRoleDefinition } from './types.bicep'

extension msGraph
extension az

param gitHubRepo GitHubRepoConfig

param acrResourceGroup {
  name: string
  location: string
}

resource githubApp 'Microsoft.Graph/applications@v1.0' = {
  displayName: gitHubRepo.name
  uniqueName: gitHubRepo.name

  resource childSymbolicname 'federatedIdentityCredentials@v1.0' = {
    name: '${githubApp.uniqueName}/${gitHubRepo.name}'
    audiences: ['api://AzureADTokenExchange']
    issuer: 'https://token.actions.githubusercontent.com'
    subject: 'repo:${gitHubRepo.owner}/${gitHubRepo.name}:ref:refs/heads/main'
    description: 'GitHub OIDC Connection for ${gitHubRepo.owner}/${gitHubRepo.name}'
  }
}

resource githubSp 'Microsoft.Graph/servicePrincipals@v1.0' = {
  appId: githubApp.appId
}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: acrResourceGroup.name
  location: acrResourceGroup.location
}

module ownerRoleAssignment 'rbac.bicep' = {
  scope: resourceGroup
  params: {
    principalId: githubSp.id
    roleDefinition: OwnerRoleDefinition
  }
}

output oidcConfig AzureOidcConfig = {
  tenantId: subscription().tenantId
  subscriptionId: subscription().subscriptionId
  clientId: githubApp.appId
}
