using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

using ManagedCode.Orleans.Identity.Core.Extensions;

using Microsoft.AspNetCore.Authorization;

using Orleans.Providers;

namespace GetRDone.Hello;

[Authorize]
[LogConsistencyProvider(ProviderName = GetRDoneStorage.EventLogProvider)]
public sealed class HelloGrain(IServiceProvider serviceProvider)
    : EventDrivenGrain<HelloState, HelloEvent>(serviceProvider, GetRDoneStorage.DefaultConnectionName), IHelloGrain
{
    public async Task<Result<SayHelloResponse>> SayHelloAsync(SayHelloCommand cmd)
    {
        var initiatedBy = Guid.Parse(OrleansIdentityContext.RequireAuthenticatedUserId());

        return await Result.Success()
            .BindWithStreamTransaction(this, transaction =>
            {
                transaction.Apply(new HelloSaidEvent(initiatedBy, cmd.Message, DateTimeOffset.UtcNow));
            })
            .Map(_ => new SayHelloResponse($"Hello, {State.LastMessage}!", State.GreetingCount));
    }
}

[GenerateSerializer]
public sealed class HelloState : EventDrivenState<HelloState, HelloEvent>
{
    public HelloState()
    {
        On<HelloSaidEvent>(Handle);
    }

    [Id(0)]
    public int GreetingCount { get; set; }

    [Id(1)]
    public string LastMessage { get; set; } = string.Empty;

    private void Handle(HelloSaidEvent evt)
    {
        GreetingCount++;
        LastMessage = evt.Message;
    }
}
