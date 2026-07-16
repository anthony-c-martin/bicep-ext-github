targetScope = 'local'

@secure()
param githubToken string

param owner string
param repoName string
param allowedActions string
param defaultWorkflowPermissions string

extension github with {
  token: githubToken
}

resource actionsPermissions 'RepositoryActionsPermissions' = {
  owner: owner
  repo: repoName
  enabled: true
  allowedActions: allowedActions
  defaultWorkflowPermissions: defaultWorkflowPermissions
  canApprovePullRequestReviews: false
}

output repositoryActionsPermissions object = actionsPermissions
