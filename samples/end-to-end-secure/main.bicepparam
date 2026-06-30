using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param azureOidc = {
  tenantId: readEnvironmentVariable('AZURE_TENANT_ID')
  subscriptionId: readEnvironmentVariable('AZURE_SUBSCRIPTION_ID')
  clientId: readEnvironmentVariable('AZURE_CLIENT_ID')
}

param owner = 'anthony-c-martin'
param repoName = 'secure-e2e-sample'
param teamOrg = 'your-org'
param teamSlug = 'platform-engineering'
