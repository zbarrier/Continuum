using System.Collections.Concurrent;

namespace Continuum.AsyncExtensions.Tests;

/// <summary>
/// A synchronization context that runs every posted callback on one dedicated thread, like a UI thread.
/// </summary>
internal sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = [];

    public SingleThreadSynchronizationContext()
    {
        Thread = new Thread(RunLoop) { IsBackground = true, Name = "Simulated UI thread" };
        Thread.Start();
    }

    public Thread Thread { get; }

    public override void Post(SendOrPostCallback d, object? state) => _queue.Add((d, state));

    public override void Send(SendOrPostCallback d, object? state) => throw new NotSupportedException();

    public Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        Post(async _ =>
        {
            try
            {
                completion.SetResult(await action());
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        }, null);
        return completion.Task;
    }

    public void Dispose() => _queue.CompleteAdding();

    private void RunLoop()
    {
        SetSynchronizationContext(this);
        foreach (var (callback, state) in _queue.GetConsumingEnumerable())
        {
            callback(state);
        }
    }
}

