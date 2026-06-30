using System.Text.Json.Serialization;
using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Bicep.Extension.Github;

public enum Visibility
{
    Public,
    Private,
    Internal
}

public class RepositoryIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}

[BicepDocHeading("Repository", "Represents a GitHub repository.")]
[BicepDocExample(
    "Creating a basic repository",
    "This example shows how to create a basic GitHub repository.",
    @"resource repo 'Repository' = {
  owner: 'myusername'
  name: 'my-new-repo'
  description: 'A sample repository'
  visibility: 'Public'
  hasIssues: true
  hasWiki: true
}
"
)]
[BicepDocExample(
    "Creating a private template repository",
    "This example shows how to create a private template repository with advanced settings.",
    @"resource templateRepo 'Repository' = {
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
"
)]
[BicepDocCustom("Notes", @"When working with the `Repository` resource, ensure you have the extension imported in your Bicep file:

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
```")]
[BicepDocCustom("Additional reference", @"For more information, see the following links:

- [GitHub Repository API documentation][00]
- [GitHub Repository settings][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/repos/repos
[01]: https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features")]
[ResourceType("Repository")]
public class Repository : RepositoryIdentifiers
{
    [TypeProperty("The repository description")]
    public string? Description { get; set; }

    [TypeProperty("The repository homepage url")]
    public string? Homepage { get; set; }

    [TypeProperty("The repository visibility")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Visibility? Visibility { get; set; }

    [TypeProperty("Whether the repository has issues enabled")]
    public bool HasIssues { get; set; }

    [TypeProperty("Whether the repository has projects enabled")]
    public bool HasProjects { get; set; }

    [TypeProperty("Whether the repository has wiki enabled")]
    public bool HasWiki { get; set; }

    [TypeProperty("Whether the repository has downloads enabled")]
    public bool HasDownloads { get; set; }

    [TypeProperty("Whether the repository is a template")]
    public bool IsTemplate { get; set; }

    [TypeProperty("Whether to allow squash merges")]
    public bool AllowSquashMerge { get; set; }

    [TypeProperty("Whether to allow merge commits")]
    public bool AllowMergeCommit { get; set; }

    [TypeProperty("Whether to allow rebase merges")]
    public bool AllowRebaseMerge { get; set; }

    [TypeProperty("Whether to automatically delete head branches")]
    public bool DeleteBranchOnMerge { get; set; }

    [TypeProperty("Whether to allow auto-merge")]
    public bool AllowAutoMerge { get; set; }
}

public class CollaboratorIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Repo { get; set; }

    [TypeProperty("The collaborator user handle", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string User { get; set; }
}

[BicepDocHeading("Collaborator", "Represents a collaborator for a GitHub repository.")]
[BicepDocExample(
    "Adding a collaborator with read permissions",
    "This example shows how to add a collaborator with read permissions to a repository.",
    @"resource collaborator 'Collaborator' = {
  owner: 'myusername'
  repo: 'my-repo'
  user: 'collaborator-username'
  permission: 'read'
}
"
)]
[BicepDocExample(
    "Adding a collaborator with admin permissions",
    "This example shows how to add a collaborator with admin permissions to a repository.",
    @"resource adminCollaborator 'Collaborator' = {
  owner: 'myorganization'
  repo: 'important-repo'
  user: 'admin-user'
  permission: 'admin'
}
"
)]
[BicepDocCustom("Notes", @"When working with the `Collaborator` resource, ensure you have the extension imported in your Bicep file:

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
- `admin` - Can read, clone, push, and manage repository settings")]
[BicepDocCustom("Additional reference", @"For more information, see the following links:

- [GitHub Collaborators API documentation][00]
- [Repository permission levels][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/collaborators/collaborators
[01]: https://docs.github.com/en/organizations/managing-access-to-your-organizations-repositories/repository-permission-levels-for-an-organization")]
[ResourceType("Collaborator")]
public class Collaborator : CollaboratorIdentifiers
{
    [TypeProperty("The collaborator permissions")]
    public string? Permission { get; set; }
}

public class LabelIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Repo { get; set; }

    [TypeProperty("The label name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}

[BicepDocHeading("Label", "Represents a label for a GitHub repository.")]
[BicepDocExample(
    "Creating a bug label",
    "This example shows how to create a bug label with a red color.",
    @"resource bugLabel 'Label' = {
  owner: 'myusername'
  repo: 'my-repo'
  name: 'bug'
  color: 'd73a4a'
  description: 'Something is not working'
}
"
)]
[BicepDocExample(
    "Creating multiple labels",
    "This example shows how to create multiple labels for issue categorization.",
    @"resource enhancementLabel 'Label' = {
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
"
)]
[BicepDocCustom("Notes", @"When working with the `Label` resource, ensure you have the extension imported in your Bicep file:

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
- Default GitHub labels can be modified or replaced")]
[BicepDocCustom("Additional reference", @"For more information, see the following links:

- [GitHub Labels API documentation][00]
- [Managing labels][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/issues/labels
[01]: https://docs.github.com/en/issues/using-labels-and-milestones-to-track-work/managing-labels")]
[ResourceType("Label")]
public class Label : LabelIdentifiers
{
    [TypeProperty("The label color", ObjectTypePropertyFlags.Required)]
    public required string Color { get; set; }

    [TypeProperty("The label description", ObjectTypePropertyFlags.None)]
    public string? Description { get; set; }
}

public class ActionsSecretIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Repo { get; set; }

    [TypeProperty("The secret name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}

[BicepDocHeading("ActionsSecret", "Represents a GitHub Actions secret for a repository.")]
[BicepDocExample(
    "Creating an Actions secret",
    "This example shows how to create a secret for GitHub Actions.",
    @"resource apiSecret 'ActionsSecret' = {
  owner: 'myusername'
  repo: 'my-repo'
  name: 'API_KEY'
  value: 'super-secret-api-key'
}
"
)]
[BicepDocExample(
    "Creating multiple Actions secrets",
    "This example shows how to create multiple secrets for different environments.",
    @"resource dbConnectionSecret 'ActionsSecret' = {
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
"
)]
[BicepDocCustom("Notes", @"When working with the `ActionsSecret` resource, ensure you have the extension imported in your Bicep file:

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
- Secret names cannot start with `GITHUB_` as these are reserved")]
[BicepDocCustom("Additional reference", @"For more information, see the following links:

- [GitHub Actions Secrets API documentation][00]
- [Using secrets in GitHub Actions][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/actions/secrets
[01]: https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions")]
[ResourceType("ActionsSecret")]
public class ActionsSecret : ActionsSecretIdentifiers
{
    [TypeProperty("The secret value", ObjectTypePropertyFlags.Required | ObjectTypePropertyFlags.WriteOnly, isSecure: true)]
    public string? Value { get; set; }
}

public class ActionsVariableIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Repo { get; set; }

    [TypeProperty("The variable name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}

[BicepDocHeading("ActionsVariable", "Represents a GitHub Actions variable for a repository.")]
[BicepDocExample(
    "Creating an Actions variable",
    "This example shows how to create a variable for GitHub Actions.",
    @"resource buildConfig 'ActionsVariable' = {
  owner: 'myusername'
  repo: 'my-repo'
  name: 'BUILD_CONFIGURATION'
  value: 'Release'
}
"
)]
[BicepDocExample(
    "Creating multiple Actions variables",
    "This example shows how to create multiple variables for workflow configuration.",
    @"resource nodeVersion 'ActionsVariable' = {
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
"
)]
[BicepDocCustom("Notes", @"When working with the `ActionsVariable` resource, ensure you have the extension imported in your Bicep file:

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
- Variable names cannot start with `GITHUB_` as these are reserved")]
[BicepDocCustom("Additional reference", @"For more information, see the following links:

- [GitHub Actions Variables API documentation][00]
- [Using variables in GitHub Actions][01]

<!-- Link reference definitions -->
[00]: https://docs.github.com/en/rest/actions/variables
[01]: https://docs.github.com/en/actions/learn-github-actions/variables")]
[ResourceType("ActionsVariable")]
public class ActionsVariable : ActionsVariableIdentifiers
{
    [TypeProperty("The variable value", ObjectTypePropertyFlags.Required, isSecure: false)]
    public required string Value { get; set; }
}

public class BranchProtectionRuleIdentifiers
{
  [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Owner { get; set; }

  [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Repo { get; set; }

  [TypeProperty("The branch to protect", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Branch { get; set; }
}

[ResourceType("BranchProtectionRule")]
public class BranchProtectionRule : BranchProtectionRuleIdentifiers
{
  [TypeProperty("Whether status checks must be strict")]
  public bool StrictStatusChecks { get; set; }

  [TypeProperty("Required status check contexts")]
  public string[]? RequiredStatusCheckContexts { get; set; }

  [TypeProperty("Whether review requirements are enabled")]
  public bool EnablePullRequestReviews { get; set; } = true;

  [TypeProperty("Whether stale reviews are dismissed on new pushes")]
  public bool DismissStaleReviews { get; set; }

  [TypeProperty("Whether code owner reviews are required")]
  public bool RequireCodeOwnerReviews { get; set; }

  [TypeProperty("Required number of approving reviews (0-6)")]
  public int RequiredApprovingReviewCount { get; set; } = 1;

  [TypeProperty("Whether the last push must be approved by another reviewer")]
  public bool RequireLastPushApproval { get; set; }

  [TypeProperty("Whether branch protection is enforced for admins")]
  public bool EnforceAdmins { get; set; }

  [TypeProperty("Whether signed commits are required")]
  public bool RequireSignedCommits { get; set; }

  [TypeProperty("Whether linear history is required")]
  public bool RequiredLinearHistory { get; set; }

  [TypeProperty("Whether force pushes are allowed")]
  public bool? AllowForcePushes { get; set; }

  [TypeProperty("Whether branch deletions are allowed")]
  public bool AllowDeletions { get; set; }

  [TypeProperty("Whether conversations must be resolved before merge")]
  public bool RequiredConversationResolution { get; set; }

  [TypeProperty("Whether the protected branch is read-only")]
  public bool LockBranch { get; set; }

  [TypeProperty("Whether restrictions block branch creation pushes")]
  public bool BlockCreations { get; set; }

  [TypeProperty("Team slugs allowed to push")]
  public string[]? PushRestrictionTeams { get; set; }

  [TypeProperty("User logins allowed to push")]
  public string[]? PushRestrictionUsers { get; set; }

  [TypeProperty("Team slugs that can dismiss reviews")]
  public string[]? DismissalRestrictionTeams { get; set; }

  [TypeProperty("User logins that can dismiss reviews")]
  public string[]? DismissalRestrictionUsers { get; set; }
}

public class RepositoryRulesetIdentifiers
{
  [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Owner { get; set; }

  [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Repo { get; set; }

  [TypeProperty("The ruleset name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Name { get; set; }
}

[ResourceType("RepositoryRuleset")]
public class RepositoryRuleset : RepositoryRulesetIdentifiers
{
  [TypeProperty("The ruleset target. Common value: branch")]
  public string Target { get; set; } = "branch";

  [TypeProperty("The enforcement mode. Common values: active, evaluate")]
  public string Enforcement { get; set; } = "active";

  [TypeProperty("The ruleset conditions", ObjectTypePropertyFlags.Required)]
  public required RepositoryRulesetConditions Conditions { get; set; }

  [TypeProperty("The ruleset rules", ObjectTypePropertyFlags.Required)]
  public required RepositoryRulesetRule[] Rules { get; set; }

  [TypeProperty("Optional JSON array string for bypass actors")]
  public string? BypassActorsJson { get; set; }
}

public class RepositoryRulesetConditions
{
  [JsonPropertyName("ref_name")]
  [TypeProperty("The ref name condition")]
  public RepositoryRulesetRefNameCondition? RefName { get; set; }
}

public class RepositoryRulesetRefNameCondition
{
  [JsonPropertyName("include")]
  [TypeProperty("Included ref name patterns")]
  public string[]? Include { get; set; }

  [JsonPropertyName("exclude")]
  [TypeProperty("Excluded ref name patterns")]
  public string[]? Exclude { get; set; }
}

public class RepositoryRulesetRule
{
  [JsonPropertyName("type")]
  [TypeProperty("The ruleset rule type", ObjectTypePropertyFlags.Required)]
  public required string Type { get; set; }

  [JsonPropertyName("parameters")]
  [TypeProperty("The ruleset rule parameters")]
  public RepositoryRulesetPullRequestRuleParameters? Parameters { get; set; }
}

public class RepositoryRulesetPullRequestRuleParameters
{
  [JsonPropertyName("dismiss_stale_reviews_on_push")]
  [TypeProperty("Whether stale reviews are dismissed on push")]
  public bool? DismissStaleReviewsOnPush { get; set; }

  [JsonPropertyName("require_code_owner_review")]
  [TypeProperty("Whether code owner reviews are required")]
  public bool? RequireCodeOwnerReview { get; set; }

  [JsonPropertyName("require_last_push_approval")]
  [TypeProperty("Whether the last push must be approved")]
  public bool? RequireLastPushApproval { get; set; }

  [JsonPropertyName("required_approving_review_count")]
  [TypeProperty("The required approving review count")]
  public int? RequiredApprovingReviewCount { get; set; }

  [JsonPropertyName("required_review_thread_resolution")]
  [TypeProperty("Whether review threads must be resolved")]
  public bool? RequiredReviewThreadResolution { get; set; }
}

public class TeamRepositoryPermissionIdentifiers
{
  [TypeProperty("The organization that owns the team", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Org { get; set; }

  [TypeProperty("The team slug", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string TeamSlug { get; set; }

  [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Owner { get; set; }

  [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Repo { get; set; }
}

[ResourceType("TeamRepositoryPermission")]
public class TeamRepositoryPermission : TeamRepositoryPermissionIdentifiers
{
  [TypeProperty("The permission to grant (pull, triage, push, maintain, admin)", ObjectTypePropertyFlags.Required)]
  public required string Permission { get; set; }
}

public class EnvironmentIdentifiers
{
  [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Owner { get; set; }

  [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Repo { get; set; }

  [TypeProperty("The environment name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Name { get; set; }
}

[ResourceType("Environment")]
public class Environment : EnvironmentIdentifiers
{
  [TypeProperty("Wait timer in minutes")]
  public int WaitTimer { get; set; }

  [TypeProperty("Whether self-review is prevented")]
  public bool PreventSelfReview { get; set; }

  [TypeProperty("Whether protected branches can deploy")]
  public bool ProtectedBranches { get; set; }

  [TypeProperty("Whether custom branch policies can deploy")]
  public bool CustomBranchPolicies { get; set; }
}

public class RepositoryWebhookIdentifiers
{
  [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Owner { get; set; }

  [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Repo { get; set; }

  [TypeProperty("The webhook delivery URL", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Url { get; set; }
}

[ResourceType("RepositoryWebhook")]
public class RepositoryWebhook : RepositoryWebhookIdentifiers
{
  [TypeProperty("Events this webhook subscribes to")]
  public string[]? Events { get; set; }

  [TypeProperty("Whether the webhook is active")]
  public bool Active { get; set; } = true;

  [TypeProperty("Payload content type (json or form)")]
  public string ContentType { get; set; } = "json";

  [TypeProperty("Shared secret for webhook signature", isSecure: true)]
  public string? Secret { get; set; }

  [TypeProperty("Whether SSL verification is disabled")]
  public bool InsecureSsl { get; set; }
}

public class DeployKeyIdentifiers
{
  [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Owner { get; set; }

  [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Repo { get; set; }

  [TypeProperty("The deploy key title", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
  public required string Title { get; set; }
}

[ResourceType("DeployKey")]
public class DeployKey : DeployKeyIdentifiers
{
  [TypeProperty("The public deploy key", ObjectTypePropertyFlags.Required)]
  public required string Key { get; set; }

  [TypeProperty("Whether the deploy key is read-only")]
  public bool ReadOnly { get; set; } = true;
}

public class Configuration
{
    [TypeProperty("The GitHub personal access token with the required permissions", ObjectTypePropertyFlags.Required, isSecure: true)]
    public required string Token { get; set; }
}