using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param org = 'your-org'
param variableName = 'MY_ORG_VARIABLE'
param variableValue = 'example-value'
param visibility = 'all'
