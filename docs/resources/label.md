# Label

Represents a label for a GitHub repository.

## Example usage

### Creating a bug label

This example shows how to create a bug label with a red color.

```bicep
resource bugLabel 'Label' = {
  owner: 'myusername'
  repo: 'my-repo'
  name: 'bug'
  color: 'd73a4a'
  description: 'Something is not working'
}
```

### Creating multiple labels

This example shows how to create multiple labels for issue categorization.

```bicep
resource enhancementLabel 'Label' = {
  owner: 'myorganization'
  repo: 'project-repo'
  name: 'enhancement'
  color: 'a2eeef'
  description: 'New feature or request'
}

resource documentationLabel 'Label' = {
  owner: 'myorganization'
  repo: 'project-repo'
  name: 'documentation'
  color: '0075ca'
  description: 'Improvements or additions to documentation'
}
```

## Argument reference

The following arguments are available:

- `color` - (Required) The label color
- `name` - (Required) The label name
- `owner` - (Required) The owner of the repository
- `repo` - (Required) The repository
- `description` - (Optional) The label description

## Notes

When working with the `Label` resource, ensure you have the extension imported in your Bicep file:

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

- Colors should be specified as 6-character hex codes without the # prefix
- Label names are case-sensitive
- Default GitHub labels can be modified or replaced

## Additional reference

For more information, see the following links:

- [GitHub Labels API documentation][00]
- [Managing labels][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/issues/labels
[01]: https://docs.github.com/en/issues/using-labels-and-milestones-to-track-work/managing-labels

