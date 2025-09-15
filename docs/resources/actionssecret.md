# ActionsSecret

Represents a GitHub Actions secret for a repository.

## Example usage

### Creating an Actions secret

This example shows how to create a secret for GitHub Actions.

```bicep
resource apiSecret 'ActionsSecret' = {
  owner: 'myusername'
  repo: 'my-repo'
  name: 'API_KEY'
  value: 'super-secret-api-key'
}
```

### Creating multiple Actions secrets

This example shows how to create multiple secrets for different environments.

```bicep
resource dbConnectionSecret 'ActionsSecret' = {
  owner: 'myorganization'
  repo: 'production-app'
  name: 'DATABASE_URL'
  value: 'postgresql://user:password@host:port/database'
}

resource deployKeySecret 'ActionsSecret' = {
  owner: 'myorganization'
  repo: 'production-app'
  name: 'DEPLOY_KEY'
  value: 'ssh-rsa AAAAB3NzaC1yc2E...'
}
```

## Argument reference

The following arguments are available:

- `name` - (Required) The secret name
- `owner` - (Required) The owner of the repository
- `repo` - (Required) The repository
- `value` - (Required) The secret value

## Notes

When working with the `ActionsSecret` resource, ensure you have the extension imported in your Bicep file:

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

Remember when working with the `ActionsSecret` resource:

- Secret values are encrypted and cannot be retrieved after creation
- Secret names must be unique within the repository
- Secrets are available to all workflows in the repository
- Secret names cannot start with `GITHUB_` as these are reserved

## Additional reference

For more information, see the following links:

- [GitHub Actions Secrets API documentation][00]
- [Using secrets in GitHub Actions][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/actions/secrets
[01]: https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions

