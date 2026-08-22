targetScope = 'local'

import { GitHubRepoConfig } from './types.bicep'

extension az

@secure()
param githubToken string

param gitHubRepo GitHubRepoConfig

param acrResourceGroup {
  subscriptionId: string
  name: string
  location: string
}

module azure 'azure.bicep' = {
  scope: subscription(acrResourceGroup.subscriptionId)
  params: {
    gitHubRepo: gitHubRepo
    acrResourceGroup: acrResourceGroup
  }
}

module ghSecrets 'github.bicep' = {
  params: {
    gitHubToken: githubToken
    gitHubRepo: gitHubRepo
    azureOidcConfig: azure.outputs.oidcConfig
  }
}

output repo string = 'https://github.com/${gitHubRepo.owner}/${gitHubRepo.name}'
