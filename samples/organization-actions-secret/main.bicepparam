using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param org = 'your-org'
param secretName = 'MY_ORG_SECRET'
param secretValue = readEnvironmentVariable('MY_ORG_SECRET')
param visibility = 'all'
