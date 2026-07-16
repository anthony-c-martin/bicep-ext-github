using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param org = 'your-org'
param teamSlug = 'platform-engineering'
param username = 'anthony-c-martin'
param role = 'maintainer'
