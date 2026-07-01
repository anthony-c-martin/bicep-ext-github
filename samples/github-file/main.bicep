targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param filePath string = 'docs/hello-from-bicep.txt'

extension github with {
  token: githubToken
}

resource file 'GitHubFile' = {
  owner: owner
  repo: repoName
  path: filePath
  content: 'Hello from Bicep Local Deploy!'
  commitMessage: 'Add file via bicep-ext-github sample'
}

output createdFile object = file
