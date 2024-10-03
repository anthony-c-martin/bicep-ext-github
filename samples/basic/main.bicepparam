using 'main.bicep'

param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param owner = 'anthony-c-martin'
param repoName = 'bicep-ext-github'
param collaboratorName = 'anthony-c-martin'
