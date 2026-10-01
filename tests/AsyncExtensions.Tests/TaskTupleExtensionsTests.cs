namespace Continuum.AsyncExtensions.Tests;

public sealed class TaskTupleExtensionsTests
{
    [Fact]
    public async Task Awaiting_two_tasks_returns_both_results()
    {
        var (a, b) = await (Task.FromResult(1), Task.FromResult(2));

        Assert.Equal(1, a);
        Assert.Equal(2, b);
    }

    [Fact]
    public async Task Awaiting_tasks_with_different_result_types_returns_each_result()
    {
        var (number, text, flag) = await (Task.FromResult(42), Task.FromResult("hello"), Task.FromResult(true));

        Assert.Equal(42, number);
        Assert.Equal("hello", text);
        Assert.True(flag);
    }

    [Fact]
    public async Task Awaiting_six_tasks_returns_results_in_order()
    {
        var result = await (Task.FromResult(1), Task.FromResult(2L), Task.FromResult("3"), Task.FromResult(4.0), Task.FromResult('5'), Task.FromResult(6m));

        Assert.Equal((1, 2L, "3", 4.0, '5', 6m), result);
    }

    [Fact]
    public async Task Awaiting_seven_tasks_returns_results_in_order()
    {
        var result = await (Task.FromResult(1), Task.FromResult(2), Task.FromResult(3), Task.FromResult(4), Task.FromResult(5),
            Task.FromResult(6), Task.FromResult(7));

        Assert.Equal((1, 2, 3, 4, 5, 6, 7), result);
    }

    [Fact]
    public async Task Awaiting_eight_tasks_deconstructs_across_the_nested_tuple()
    {
        var (a, b, c, d, e, f, g, h) = await (Task.FromResult(1), Task.FromResult("2"), Task.FromResult(3L), Task.FromResult(4.0),
            Task.FromResult(true), Task.FromResult('6'), Task.FromResult(7m), Task.FromResult("8"));

        Assert.Equal(1, a);
        Assert.Equal("2", b);
        Assert.Equal(3L, c);
        Assert.Equal(4.0, d);
        Assert.True(e);
        Assert.Equal('6', f);
        Assert.Equal(7m, g);
        Assert.Equal("8", h);
    }

    [Fact]
    public async Task Awaiting_twelve_tasks_returns_results_in_order()
    {
        var result = await (Task.FromResult(1), Task.FromResult(2), Task.FromResult(3), Task.FromResult(4), Task.FromResult(5),
            Task.FromResult(6), Task.FromResult(7), Task.FromResult(8), Task.FromResult(9), Task.FromResult(10),
            Task.FromResult(11), Task.FromResult("12"));

        Assert.Equal((1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, "12"), result);
    }

    [Fact]
    public async Task Awaiting_twelve_tasks_throws_when_the_last_task_fails()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await (Task.FromResult(1), Task.FromResult(2),
            Task.FromResult(3), Task.FromResult(4), Task.FromResult(5), Task.FromResult(6), Task.FromResult(7), Task.FromResult(8),
            Task.FromResult(9), Task.FromResult(10), Task.FromResult(11), Task.FromException<int>(new InvalidOperationException("last"))));

        Assert.Equal("last", ex.Message);
    }

    [Fact]
    public async Task Awaiting_resumes_on_the_captured_synchronization_context()
    {
        using var context = new SingleThreadSynchronizationContext();
        var first = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        bool completedBeforeAwait = true;

        var resumed = context.RunAsync(async () =>
        {
            // The context runs one callback at a time, so this cannot run until the await below has suspended.
            context.Post(_ => _ = Task.Run(() =>
            {
                first.SetResult(1);
                second.SetResult("two");
            }), null);

            var awaitable = (first.Task, second.Task);
            completedBeforeAwait = first.Task.IsCompleted || second.Task.IsCompleted;
            var (number, text) = await awaitable;
            return (number, text, Thread.CurrentThread, SynchronizationContext.Current);
        });

        var (number, text, thread, current) = await resumed;
        Assert.False(completedBeforeAwait);
        Assert.Equal(1, number);
        Assert.Equal("two", text);
        Assert.Same(context.Thread, thread);
        Assert.Same(context, current);
    }

    [Fact]
    public async Task Awaiting_resumes_on_the_current_task_scheduler()
    {
        var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
        var first = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        bool completedBeforeAwait = true;

        var resumed = Task.Factory.StartNew(async () =>
        {
            // The exclusive scheduler runs one task at a time, so this cannot run until the await below has suspended.
            _ = Task.Factory.StartNew(() => _ = Task.Run(() =>
            {
                first.SetResult(1);
                second.SetResult("two");
            }), CancellationToken.None, TaskCreationOptions.None, scheduler);

            var awaitable = (first.Task, second.Task);
            completedBeforeAwait = first.Task.IsCompleted || second.Task.IsCompleted;
            var (number, text) = await awaitable;
            return (number, text, TaskScheduler.Current);
        }, CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap();

        var (number, text, current) = await resumed;
        Assert.False(completedBeforeAwait);
        Assert.Equal(1, number);
        Assert.Equal("two", text);
        Assert.Same(scheduler, current);
    }

    [Fact]
    public async Task Awaiting_tasks_runs_them_concurrently()
    {
        var first = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var combined = Task.Run(async () => await (first.Task, second.Task));

        second.SetResult(2);
        Assert.False(combined.IsCompleted);
        first.SetResult(1);

        Assert.Equal((1, 2), await combined);
    }

    [Fact]
    public async Task Awaiting_waits_for_every_task_before_throwing()
    {
        var slow = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var failing = Task.FromException<string>(new InvalidOperationException("boom"));

        var combined = Task.Run(async () => await (slow.Task, failing));

        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.False(combined.IsCompleted);

        slow.SetResult(1);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => combined);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task Awaiting_throws_the_first_exception_when_several_tasks_fail()
    {
        var first = new InvalidOperationException("first");
        var second = new ArgumentException("second");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (Task.FromException<int>(first), Task.FromException<int>(second)));

        Assert.Same(first, ex);
    }

    [Fact]
    public async Task Awaiting_throws_when_a_task_is_canceled()
    {
        var canceled = Task.FromCanceled<int>(new CancellationToken(canceled: true));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await (Task.FromResult(1), canceled));
    }
}

