using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class BranchProtectionRuleHandler : GithubResourceHandlerBase<BranchProtectionRule, BranchProtectionRuleIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async _ =>
        {
            await Task.CompletedTask;
            return GetResponse(request);
        });

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            BranchProtectionRequiredStatusChecksUpdate? statusChecks = null;
            if (request.Properties.RequiredStatusCheckContexts is { Length: > 0 } contexts)
            {
                statusChecks = new BranchProtectionRequiredStatusChecksUpdate(
                    request.Properties.StrictStatusChecks,
                    contexts);
            }

            BranchProtectionRequiredReviewsUpdate? reviews = null;
            if (request.Properties.EnablePullRequestReviews)
            {
                BranchProtectionRequiredReviewsDismissalRestrictionsUpdate? dismissalRestrictions = null;
                if (request.Properties.DismissalRestrictionTeams is { Length: > 0 } teamSlugs && request.Properties.DismissalRestrictionUsers is { Length: > 0 } users)
                {
                    dismissalRestrictions = new BranchProtectionRequiredReviewsDismissalRestrictionsUpdate(
                        new BranchProtectionTeamCollection([..teamSlugs]),
                        new BranchProtectionUserCollection([..users]));
                }
                else if (request.Properties.DismissalRestrictionTeams is { Length: > 0 } teamOnlySlugs)
                {
                    dismissalRestrictions = new BranchProtectionRequiredReviewsDismissalRestrictionsUpdate(new BranchProtectionTeamCollection([..teamOnlySlugs]));
                }
                else if (request.Properties.DismissalRestrictionUsers is { Length: > 0 } userOnlyLogins)
                {
                    dismissalRestrictions = new BranchProtectionRequiredReviewsDismissalRestrictionsUpdate(new BranchProtectionUserCollection([..userOnlyLogins]));
                }

                var reviewCount = Math.Clamp(request.Properties.RequiredApprovingReviewCount, 0, 6);
                reviews = dismissalRestrictions is null
                    ? new BranchProtectionRequiredReviewsUpdate(
                        request.Properties.DismissStaleReviews,
                        request.Properties.RequireCodeOwnerReviews,
                        reviewCount,
                        request.Properties.RequireLastPushApproval)
                    : new BranchProtectionRequiredReviewsUpdate(
                        dismissalRestrictions,
                        request.Properties.DismissStaleReviews,
                        request.Properties.RequireCodeOwnerReviews,
                        reviewCount,
                        request.Properties.RequireLastPushApproval);
            }

            BranchProtectionPushRestrictionsUpdate? pushRestrictions = null;
            if (request.Properties.PushRestrictionTeams is { Length: > 0 } pushTeams && request.Properties.PushRestrictionUsers is { Length: > 0 } pushUsers)
            {
                pushRestrictions = new BranchProtectionPushRestrictionsUpdate(
                    new BranchProtectionTeamCollection([..pushTeams]),
                    new BranchProtectionUserCollection([..pushUsers]));
            }
            else if (request.Properties.PushRestrictionTeams is { Length: > 0 } pushOnlyTeams)
            {
                pushRestrictions = new BranchProtectionPushRestrictionsUpdate(new BranchProtectionTeamCollection([..pushOnlyTeams]));
            }
            else if (request.Properties.PushRestrictionUsers is { Length: > 0 } pushOnlyUsers)
            {
                pushRestrictions = new BranchProtectionPushRestrictionsUpdate(new BranchProtectionUserCollection([..pushOnlyUsers]));
            }

            var settings = new BranchProtectionSettingsUpdate(
                statusChecks,
                reviews,
                pushRestrictions,
                request.Properties.RequireSignedCommits,
                request.Properties.EnforceAdmins,
                request.Properties.RequiredLinearHistory,
                request.Properties.AllowForcePushes,
                request.Properties.AllowDeletions,
                request.Properties.BlockCreations,
                request.Properties.RequiredConversationResolution,
                request.Properties.LockBranch);

            await client.Repository.Branch.UpdateBranchProtection(
                request.Properties.Owner,
                request.Properties.Repo,
                request.Properties.Branch,
                settings);

            return GetResponse(request);
        });

    protected override BranchProtectionRuleIdentifiers GetIdentifiers(BranchProtectionRule properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Branch = properties.Branch,
        };
}
