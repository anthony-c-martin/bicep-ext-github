# Collaborator

Represents a collaborator for a GitHub repository.

## Example usage

### Adding a collaborator with read permissions

This example shows how to add a collaborator with read permissions to a repository.

```bicep
resource collaborator 'Collaborator' = {
  owner: 'myusername'
  repo: 'my-repo'
  user: 'collaborator-username'
  permission: 'read'
}
```

### Adding a collaborator with admin permissions

This example shows how to add a collaborator with admin permissions to a repository.

```bicep
resource adminCollaborator 'Collaborator' = {
  owner: 'myorganization'
  repo: 'important-repo'
  user: 'admin-user'
  permission: 'admin'
}
```

## Argument reference

The following arguments are available:

- `owner` - (Required) The owner of the repository
- `repo` - (Required) The repository
- `user` - (Required) The collaborator user handle
- `permission` - (Optional) The collaborator permissions

## Notes

When working with the `Collaborator` resource, ensure you have the extension imported in your Bicep file:

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

Ensure you have appropriate permissions to manage repository collaborators. Available permission levels are:

- `read` - Can read and clone the repository
- `write` - Can read, clone, and push to the repository
- `admin` - Can read, clone, push, and manage repository settings

## Additional reference

For more information, see the following links:

- [GitHub Collaborators API documentation][00]
- [Repository permission levels][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/collaborators/collaborators
[01]: https://docs.github.com/en/organizations/managing-access-to-your-organizations-repositories/repository-permission-levels-for-an-organization

