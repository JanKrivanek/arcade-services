// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Maestro.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DependencyUpdater.Tests;

[TestFixture, NonParallelizable]
public class CheckSubscriptionsAsyncTests : DependencyUpdaterTests
{
    [Test]
    [Ignore("Disabling scheduled subscription updates in Maestro (https://github.com/dotnet/arcade-services/issues/3808)")]
    public async Task NeedsUpdateSubscription()
    {
        var channel = new Channel
        {
            Name = "channel",
            Classification = "class"
        };
        var oldBuild = new Build
        {
            AzureDevOpsBranch = "source.branch",
            AzureDevOpsRepository = "source.repo",
            AzureDevOpsBuildNumber = "old.build.number",
            Commit = "oldSha",
            DateProduced = DateTimeOffset.UtcNow.AddDays(-2)
        };
        var location = "https://source.feed/index.json";
        var build = new Build
        {
            AzureDevOpsBranch = "source.branch",
            AzureDevOpsRepository = "source.repo",
            AzureDevOpsBuildNumber = "build.number",
            Commit = "sha",
            DateProduced = DateTimeOffset.UtcNow,
            Assets =
            [
                new Asset
                {
                    Name = "source.asset",
                    Version = "1.0.1",
                    NonShipping = false,
                    Locations =
                    [
                        new AssetLocation
                        {
                            Location = location,
                            Type = LocationType.NugetFeed
                        }
                    ]
                }
            ]
        };
        var buildChannel = new BuildChannel
        {
            Build = build,
            Channel = channel
        };
        var subscription = new Subscription
        {
            Channel = channel,
            SourceRepository = "source.repo",
            TargetRepository = "target.repo",
            TargetBranch = "target.branch",
            Enabled = true,
            PolicyObject = new SubscriptionPolicy
            {
                MergePolicies = null,
                UpdateFrequency = UpdateFrequency.EveryDay
            },
            LastAppliedBuild = oldBuild
        };
        var repoInstallation = new Repository
        {
            RepositoryName = "target.repo",
            InstallationId = 1
        };
        await Context.Repositories.AddAsync(repoInstallation);
        await Context.Subscriptions.AddAsync(subscription);
        await Context.BuildChannels.AddAsync(buildChannel);
        await Context.SaveChangesAsync();

        SubscriptionActor
            .Setup(a => a.UpdateAsync(build.Id))
            .Returns(Task.CompletedTask);

        var updater = ActivatorUtilities.CreateInstance<DependencyUpdater>(Scope.ServiceProvider);
        await updater.CheckDailySubscriptionsAsync(CancellationToken.None);
    }

    [Test]
    public async Task NoUpdateSubscriptionBecauseNotEnabled()
    {
        var channel = new Channel
        {
            Name = "channel",
            Classification = "class"
        };
        var oldBuild = new Build
        {
            AzureDevOpsBranch = "source.branch",
            AzureDevOpsRepository = "source.repo",
            AzureDevOpsBuildNumber = "old.build.number",
            Commit = "oldSha",
            DateProduced = DateTimeOffset.UtcNow.AddDays(-2)
        };
        var location = "https://source.feed/index.json";
        var build = new Build
        {
            AzureDevOpsBranch = "source.branch",
            AzureDevOpsRepository = "source.repo",
            AzureDevOpsBuildNumber = "build.number",
            Commit = "sha",
            DateProduced = DateTimeOffset.UtcNow,
            Assets =
            [
                new Asset
                {
                    Name = "source.asset",
                    Version = "1.0.1",
                    NonShipping = true,
                    Locations =
                    [
                        new AssetLocation
                        {
                            Location = location,
                            Type = LocationType.NugetFeed
                        }
                    ]
                }
            ]
        };
        var buildChannel = new BuildChannel
        {
            Build = build,
            Channel = channel
        };
        var subscription = new Subscription
        {
            Channel = channel,
            SourceRepository = "source.repo",
            TargetRepository = "target.repo",
            TargetBranch = "target.branch",
            Enabled = false,
            PolicyObject = new SubscriptionPolicy
            {
                MergePolicies = null,
                UpdateFrequency = UpdateFrequency.EveryDay
            },
            LastAppliedBuild = oldBuild
        };
        var repoInstallation = new Repository
        {
            RepositoryName = "target.repo",
            InstallationId = 1
        };
        await Context.Repositories.AddAsync(repoInstallation);
        await Context.Subscriptions.AddAsync(subscription);
        await Context.BuildChannels.AddAsync(buildChannel);
        await Context.SaveChangesAsync();

        SubscriptionActor.Verify(a => a.UpdateAsync(build.Id), Times.Never());

        var updater = ActivatorUtilities.CreateInstance<DependencyUpdater>(Scope.ServiceProvider);
        await updater.CheckDailySubscriptionsAsync(CancellationToken.None);
    }

    [Test]
    public async Task OneEveryBuildSubscription()
    {
        var channel = new Channel
        {
            Name = "channel",
            Classification = "class"
        };
        var subscription = new Subscription
        {
            Channel = channel,
            SourceRepository = "source.repo",
            TargetRepository = "target.repo",
            TargetBranch = "target.branch",
            Enabled = true,
            PolicyObject = new SubscriptionPolicy
            {
                MergePolicies = null,
                UpdateFrequency = UpdateFrequency.EveryBuild
            }
        };
        await Context.Subscriptions.AddAsync(subscription);
        await Context.SaveChangesAsync();

        var updater = ActivatorUtilities.CreateInstance<DependencyUpdater>(Scope.ServiceProvider);
        await updater.CheckDailySubscriptionsAsync(CancellationToken.None);
    }

    [Test]
    public async Task UpToDateSubscription()
    {
        var channel = new Channel
        {
            Name = "channel",
            Classification = "class"
        };
        var build = new Build
        {
            AzureDevOpsBranch = "source.branch",
            AzureDevOpsRepository = "source.repo",
            AzureDevOpsBuildNumber = "build.number",
            Commit = "sha",
            DateProduced = DateTimeOffset.UtcNow
        };
        var buildChannel = new BuildChannel
        {
            Build = build,
            Channel = channel
        };
        var subscription = new Subscription
        {
            Channel = channel,
            SourceRepository = "source.repo",
            TargetRepository = "target.repo",
            TargetBranch = "target.branch",
            PolicyObject = new SubscriptionPolicy
            {
                MergePolicies = null,
                UpdateFrequency = UpdateFrequency.EveryDay
            },
            LastAppliedBuild = build
        };
        await Context.Subscriptions.AddAsync(subscription);
        await Context.BuildChannels.AddAsync(buildChannel);
        await Context.SaveChangesAsync();

        var updater = ActivatorUtilities.CreateInstance<DependencyUpdater>(Scope.ServiceProvider);
        await updater.CheckDailySubscriptionsAsync(CancellationToken.None);
    }
}
