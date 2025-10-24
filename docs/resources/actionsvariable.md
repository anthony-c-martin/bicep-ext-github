# ActionsVariable

Represents a GitHub Actions variable for a repository.

## Example usage

### Creating an Actions variable

This example shows how to create a variable for GitHub Actions.

```bicep
resource buildConfig 'ActionsVariable' = {
  owner: 'myusername'
  repo: 'my-repo'
  name: 'BUILD_CONFIGURATION'
  value: 'Release'
}
```

### Creating multiple Actions variables

This example shows how to create multiple variables for workflow configuration.

```bicep
resource nodeVersion 'ActionsVariable' = {
  owner: 'myorganization'
  repo: 'web-app'
  name: 'NODE_VERSION'
  value: '18.x'
}

resource deployEnvironment 'ActionsVariable' = {
  owner: 'myorganization'
  repo: 'web-app'
  name: 'DEPLOY_ENVIRONMENT'
  value: 'production'
}

resource maxRetries 'ActionsVariable' = {
  owner: 'myorganization'
  repo: 'web-app'
  name: 'MAX_RETRIES'
  value: '3'
}
```

## Argument reference

The following arguments are available:

- `name` - (Required) The variable name
- `owner` - (Required) The owner of the repository
- `repo` - (Required) The repository
- `value` - (Required) The variable value

## Notes

When working with the `ActionsVariable` resource, ensure you have the extension imported in your Bicep file:

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

Note that:

- Variables are not encrypted and are visible to anyone with read access to the repository
- Variable names must be unique within the repository
- Variables are available to all workflows in the repository
- Use variables for non-sensitive configuration data
- Variable names cannot start with `GITHUB_` as these are reserved

## Additional reference

For more information, see the following links:

- [GitHub Actions Variables API documentation][00]
- [Using variables in GitHub Actions][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/actions/variables
[01]: https://docs.github.com/en/actions/learn-github-actions/variables

