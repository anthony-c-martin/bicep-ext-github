targetScope = 'local'

import { GitHubRepoConfig } from './types.bicep'

extension az
extension local

param gitHubRepo GitHubRepoConfig

param acrResourceGroup {
  subscriptionId: string
  name: string
  location: string
}

resource getAuthToken 'Command' = {
  command: 'gh auth token'
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
    gitHubToken: trim(getAuthToken.stdOut)
    gitHubRepo: gitHubRepo
    azureOidcConfig: azure.outputs.oidcConfig
  }
}

output repo string = 'https://github.com/${gitHubRepo.owner}/${gitHubRepo.name}'
