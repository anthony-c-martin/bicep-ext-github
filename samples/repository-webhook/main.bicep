targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param webhookUrl string

@secure()
param webhookSecret string

extension github with {
  token: githubToken
}

resource webhook 'RepositoryWebhook' = {
  owner: owner
  repo: repoName
  url: webhookUrl
  contentType: 'json'
  secret: webhookSecret
  insecureSsl: false
  active: true
  events: [
    'push'
    'pull_request'
  ]
}

output repositoryWebhook object = webhook
