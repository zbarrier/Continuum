using Continuum.CSharpFunctionalExtensions;

namespace Continuum.Domain.Tests;

public class ResultSessionScopeExtensionsTests
{
    [Fact]
    public async Task Success_WithAction_AppliesEventsAndSaves()
    {
        var scope = new FakeScope();
        var result = await Result.Success().BindWithSessionScope(scope, s => s.Apply(new CounterIncremented(2)));
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Version);
        Assert.Single(result.Value.Changes);
        Assert.Equal(1, scope.SaveCount);
        Assert.Equal(2, scope.LastSession!.State.Count);
    }

    [Fact]
    public async Task Failure_SkipsSessionAndPropagatesError()
    {
        var scope = new FakeScope();
        var source = Result.Failure(new ValidationError("test", "boom", null));
        var result = await source.BindWithSessionScope(scope, s => s.Apply(new CounterIncremented(1)));
        Assert.True(result.IsFailure);
        Assert.Equal(source.Error, result.Error);
        Assert.Equal(0, scope.SessionsCreated);
        Assert.Equal(0, scope.SaveCount);
    }

    [Fact]
    public async Task Success_WithFailingFunc_ReturnsFailure()
    {
        var scope = new FakeScope();
        var result = await Result.Success().BindWithSessionScope(scope, s => Result.Failure(new ValidationError("test", "nope", null)));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GenericSuccess_PassesValueToFunc()
    {
        var scope = new FakeScope();
        var result = await Result.Success(5).BindWithSessionScope(scope, (s, by) => { s.Apply(new CounterIncremented(by)); return Result.Success(); });
        Assert.True(result.IsSuccess);
        Assert.Equal(5, scope.LastSession!.State.Count);
    }

    [Fact]
    public async Task TaskSuccess_WithAction_Saves()
    {
        var scope = new FakeScope();
        var result = await Task.FromResult(Result.Success()).BindWithSessionScope(scope, s => s.Apply(new CounterIncremented(3)));
        Assert.True(result.IsSuccess);
        Assert.Equal(1, scope.SaveCount);
    }

    [Fact]
    public async Task ValueTaskFailure_SkipsSession()
    {
        var scope = new FakeScope();
        var result = await new ValueTask<Result>(Result.Failure(new ValidationError("test", "x", null))).BindWithSessionScope(scope, s => s.Apply(new CounterIncremented(3)));
        Assert.True(result.IsFailure);
        Assert.Equal(0, scope.SessionsCreated);
    }

    [Fact]
    public async Task ThrowingCallback_LogsExceptionAndReturnsFailure()
    {
        var scope = new FakeScope();
        var boom = new InvalidOperationException("boom");
        var result = await Result.Success().BindWithSessionScope(scope, (Action<IEventDrivenSession<CounterState, CounterEvent>>)(_ => throw boom));
        Assert.True(result.IsFailure);
        Assert.Same(boom, Assert.Single(scope.Logger.Exceptions));
        Assert.Equal(0, scope.SaveCount);
    }
}
