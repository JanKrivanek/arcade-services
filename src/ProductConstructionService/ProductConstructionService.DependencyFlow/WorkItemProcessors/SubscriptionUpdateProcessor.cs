﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ProductConstructionService.DependencyFlow.WorkItems;
using ProductConstructionService.WorkItems;

namespace ProductConstructionService.DependencyFlow.WorkItemProcessors;

public class SubscriptionUpdateProcessor : WorkItemProcessor<SubscriptionUpdateWorkItem>
{
    private readonly IActorFactory _actorFactory;

    public SubscriptionUpdateProcessor(IActorFactory actorFactory)
    {
        _actorFactory = actorFactory;
    }

    public override async Task<bool> ProcessWorkItemAsync(
        SubscriptionUpdateWorkItem workItem,
        CancellationToken cancellationToken)
    {
        var actor = _actorFactory.CreatePullRequestActor(PullRequestActorId.Parse(workItem.ActorId));
        await actor.ProcessPendingUpdatesAsync(workItem);
        return true;
    }
}
