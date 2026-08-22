targetScope = 'local'

@secure()
@description('GitHub personal access token used to authenticate the extension.')
param githubToken string

@description('The account or organization that owns the repository.')
param owner string

@description('The repository to configure.')
param repoName string

@description('GitHub users to grant push access to.')
param collaborators string[]

extension github with {
  token: githubToken
}

@description('App id of the GitHub Actions app, used to scope required status checks to checks it reports.')
var githubActionsIntegrationId = 15368

resource repository 'Repository' = {
  owner: owner
  name: repoName
  visibility: 'Public'
  hasIssues: true
  hasProjects: false
  hasWiki: false
  hasDownloads: false
  allowSquashMerge: true
  allowMergeCommit: false
  allowRebaseMerge: false
  allowAutoMerge: true
  allowUpdateBranch: true
  deleteBranchOnMerge: true
}

resource mainProtection 'RepositoryRuleset' = {
  owner: repository.owner
  repo: repository.name
  name: 'protect-main'
  target: 'branch'
  enforcement: 'active'
  bypassActors: [
    {
      actorType: 'RepositoryRole'
      actorId: 5 // repository administrators
      bypassMode: 'always'
    }
  ]
  conditions: {
    refName: {
      include: [
        '~DEFAULT_BRANCH'
      ]
      exclude: []
    }
  }
  rules: [
    {
      type: 'pull_request'
      parameters: {
        dismissStaleReviewsOnPush: true
        requireCodeOwnerReview: false
        requireLastPushApproval: false
        requiredApprovingReviewCount: 1
        requiredReviewThreadResolution: true
      }
    }
    {
      type: 'required_status_checks'
      parameters: {
        strictRequiredStatusChecksPolicy: false
        requiredStatusChecks: [
          {
            context: 'Build Extension'
            integrationId: githubActionsIntegrationId
          }
          {
            context: 'Publish Locally'
            integrationId: githubActionsIntegrationId
          }
        ]
      }
    }
    {
      type: 'deletion'
    }
    {
      type: 'non_fast_forward'
    }
  ]
}

resource repositoryCollaborators 'Collaborator' = [for collaborator in collaborators: {
  owner: repository.owner
  repo: repository.name
  user: collaborator
  permission: 'push'
}]
