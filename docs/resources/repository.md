# Repository

Represents a GitHub repository.

## Example usage

### Creating a basic repository

This example shows how to create a basic GitHub repository.

```bicep
resource repo 'Repository' = {
  owner: 'myusername'
  name: 'my-new-repo'
  description: 'A sample repository'
  visibility: 'Public'
  hasIssues: true
  hasWiki: true
}
```

### Creating a private template repository

This example shows how to create a private template repository with advanced settings.

```bicep
resource templateRepo 'Repository' = {
  owner: 'myorganization'
  name: 'my-template'
  description: 'A template repository for new projects'
  visibility: 'Private'
  isTemplate: true
  hasIssues: true
  hasProjects: true
  hasWiki: false
  deleteBranchOnMerge: true
  allowAutoMerge: true
}
```

## Argument reference

The following arguments are available:

- `name` - (Required) The repository
- `owner` - (Required) The owner of the repository
- `allowAutoMerge` - (Optional) Whether to allow auto-merge
- `allowMergeCommit` - (Optional) Whether to allow merge commits
- `allowRebaseMerge` - (Optional) Whether to allow rebase merges
- `allowSquashMerge` - (Optional) Whether to allow squash merges
- `deleteBranchOnMerge` - (Optional) Whether to automatically delete head branches
- `description` - (Optional) The repository description
- `hasDownloads` - (Optional) Whether the repository has downloads enabled
- `hasIssues` - (Optional) Whether the repository has issues enabled
- `hasProjects` - (Optional) Whether the repository has projects enabled
- `hasWiki` - (Optional) Whether the repository has wiki enabled
- `homepage` - (Optional) The repository homepage url
- `isTemplate` - (Optional) Whether the repository is a template
- `visibility` - (Optional) The repository visibility (Can be `Public`, `Private`, or `Internal`)

## Notes

When working with the `Repository` resource, ensure you have the extension imported in your Bicep file:

```bicep
// main.bicep
targetScope = 'local'
@secure()
param githubToken string
extension githubExtension with {
  token: githubToken
}

// main.bicepparam
using 'main.bicep'
param githubToken = '<your-github-token>'
```

## Additional reference

For more information, see the following links:

- [GitHub Repository API documentation][00]
- [GitHub Repository settings][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/repos/repos
[01]: https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features

