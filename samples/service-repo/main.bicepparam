using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param owner = 'your-org'
param repoName = 'payments-service'
param org = 'your-org'
param teamName = 'payments'
param maintainer = 'octocat'
param productionReviewerId = 0
param deployKey = 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAExampleKeyReplaceMe ci@example.com'
param webhookUrl = 'https://ci.example.com/github/webhook'

// Secrets are sourced from environment variables so they never live in the file.
param nugetApiKey = readEnvironmentVariable('NUGET_API_KEY')
param privateFeedToken = readEnvironmentVariable('PRIVATE_FEED_TOKEN')
param productionConnectionString = readEnvironmentVariable('PRODUCTION_CONNECTION_STRING')
param webhookSecret = readEnvironmentVariable('WEBHOOK_SECRET')
