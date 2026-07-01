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

    [TypeProperty("Topics for the repository")]
    public string[]? Topics { get; set; }

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

    [TypeProperty("Whether to allow updating pull request branches")]
    public bool AllowUpdateBranch { get; set; }
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

  [TypeProperty("Actors that can bypass the ruleset")]
  public RepositoryRulesetBypassActor[]? BypassActors { get; set; }

  [TypeProperty("Optional JSON array string for bypass actors (deprecated: use bypassActors)")]
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

public class RepositoryRulesetCommitMessageRuleParameters
{
  [JsonPropertyName("operator")]
  [TypeProperty("The operator for the pattern (contains or starts_with)")]
  public string? Operator { get; set; }

  [JsonPropertyName("pattern")]
  [TypeProperty("The pattern for commit messages")]
  public string? Pattern { get; set; }
}

public class RepositoryRulesetCommitAuthorRuleParameters
{
  [JsonPropertyName("operator")]
  [TypeProperty("The operator for the pattern (contains or starts_with)")]
  public string? Operator { get; set; }

  [JsonPropertyName("pattern")]
  [TypeProperty("The pattern for commit author")]
  public string? Pattern { get; set; }
}

public class RepositoryRulesetBranchNamePatternRuleParameters
{
  [JsonPropertyName("operator")]
  [TypeProperty("The operator for the pattern (contains or starts_with)")]
  public string? Operator { get; set; }

  [JsonPropertyName("pattern")]
  [TypeProperty("The pattern for branch names")]
  public string? Pattern { get; set; }
}

public class RepositoryRulesetTagNamePatternRuleParameters
{
  [JsonPropertyName("operator")]
  [TypeProperty("The operator for the pattern (contains or starts_with)")]
  public string? Operator { get; set; }

  [JsonPropertyName("pattern")]
  [TypeProperty("The pattern for tag names")]
  public string? Pattern { get; set; }
}

public class RepositoryRulesetRequiredDeploymentsRuleParameters
{
  [JsonPropertyName("required_deployment_environments")]
  [TypeProperty("List of environments that must be successfully deployed")]
  public string[]? RequiredDeploymentEnvironments { get; set; }
}

public class RepositoryRulesetBypassActor
{
  [JsonPropertyName("actor_id")]
  [TypeProperty("The ID of the actor")]
  public long ActorId { get; set; }

  [JsonPropertyName("actor_type")]
  [TypeProperty("The type of actor (OrganizationAdmin, RepositoryRole, Team, or Integration)")]
  public string? ActorType { get; set; }

  [JsonPropertyName("bypass_mode")]
  [TypeProperty("The bypass mode (always or pull_request)")]
  public string? BypassMode { get; set; }
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

  [TypeProperty("Deployment reviewers for this environment")]
  public EnvironmentReviewer[]? Reviewers { get; set; }

  [TypeProperty("Alternative environment names allowed for deployment")]
  public string[]? DeploymentBranchPolicyEnvironments { get; set; }
}

public class EnvironmentReviewer
{
  [TypeProperty("The ID of the reviewer (user ID or team ID)")]
  public long Id { get; set; }

  [TypeProperty("The type of reviewer (User or Team)")]
  public string Type { get; set; } = "User";
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
