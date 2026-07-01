using 'main.bicep'

param acrResourceGroup = {
  subscriptionId: 'd08e1a72-8180-4ed3-8125-9dff7376b0bd'
  name: 'bicep-local-deploy-test'
  location: 'East US 2'
}

param gitHubRepo = {
  owner: 'anthony-c-martin'
  name: 'bicep-local-deploy-test'
  collaborators: [
    'majastrz'
    'alex-frankel'
  ]
}
