using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param owner = 'anthony-c-martin'
param repoName = 'testing-repo'
param branchName = 'release/v1'
param sourceBranch = 'main'
