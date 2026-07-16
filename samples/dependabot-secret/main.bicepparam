using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param owner = 'anthony-c-martin'
param repoName = 'testing-repo'
param secretName = 'MY_DEPENDABOT_SECRET'
param secretValue = readEnvironmentVariable('MY_DEPENDABOT_SECRET')
