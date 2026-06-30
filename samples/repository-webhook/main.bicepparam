using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param owner = 'anthony-c-martin'
param repoName = 'testing-repo'

// Use a request inspector or webhook.site endpoint here.
param webhookUrl = 'https://example.com/github-webhook'

// Prefer loading a real secret from an environment variable or secure variable store.
param webhookSecret = 'replace-me-with-a-real-secret'
