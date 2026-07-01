using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')
param sampleSecretValue = 'replace-with-secret-value'

param owner = 'anthony-c-martin'
param repoName = 'testing-repo'
param collaboratorName = 'majastrz'
