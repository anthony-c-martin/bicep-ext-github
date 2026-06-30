using 'main.bicep'

// Use the following to set the token:
// export GITHUB_TOKEN=$(gh auth token)
param githubToken = readEnvironmentVariable('GITHUB_TOKEN')

param owner = 'anthony-c-martin'
param repoName = 'testing-repo'
param keyTitle = 'ci-deploy-key'

// Paste the contents of a real public key here.
param deployKeyPublic = 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAA... replace-me'
