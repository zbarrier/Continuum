using Orleans.Configuration;
using Orleans.Providers.Streams.Common;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that the shared cache block pool releases free blocks once demand has stayed lower for enough windows,
///     and never releases blocks that are in use.
/// </summary>
public class KurrentDBQueueCacheBufferPoolTests
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(20);

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private static (KurrentDBQueueCacheBufferPool Pool, ManualTimeProvider Time) CreatePool(KurrentDBStreamCacheBufferPoolOptions? options = null)
    {
        var time = new ManualTimeProvider();
        var pool = new KurrentDBQueueCacheBufferPool(() => new FixedSizeBuffer(16), options ?? new KurrentDBStreamCacheBufferPoolOptions(), time);
        return (pool, time);
    }

    private static List<FixedSizeBuffer> Spike(KurrentDBQueueCacheBufferPool pool, int count)
    {
        var blocks = Enumerable.Range(0, count).Select(_ => pool.Allocate()).ToList();
        foreach (var block in blocks)
        {
            block.Dispose();
        }
        return blocks;
    }

    // Allocating and freeing one block runs the window check without changing demand.
    private static void Touch(KurrentDBQueueCacheBufferPool pool) => pool.Allocate().Dispose();

    [Fact]
    public void A_Failed_Block_Allocation_Leaves_The_Counts_Unchanged()
    {
        var fail = false;
        var pool = new KurrentDBQueueCacheBufferPool(() => fail ? throw new OutOfMemoryException() : new FixedSizeBuffer(16), new KurrentDBStreamCacheBufferPoolOptions(), new ManualTimeProvider());
        pool.Allocate();

        fail = true;
        Assert.Throws<OutOfMemoryException>(() => pool.Allocate());

        Assert.Equal(1, pool.Allocated);
        Assert.Equal(1, pool.InUse);
        Assert.Equal(0, pool.Available);
    }

    [Fact]
    public void Free_Blocks_Are_Released_After_Two_Windows_Below_The_Target()
    {
        var (pool, time) = CreatePool();
        pool.RegisterCache();
        Spike(pool, 20);
        Assert.Equal(20, pool.Allocated);

        // The first window includes the spike, so its peak keeps every block.
        time.Advance(Window);
        Touch(pool);
        Assert.Equal(20, pool.Allocated);

        // One quiet window is not enough.
        time.Advance(Window);
        Touch(pool);
        Assert.Equal(20, pool.Allocated);

        // The second quiet window trims to max(caches + extra, peak + headroom) = max(3, 1 + 2) = 3.
        time.Advance(Window);
        Touch(pool);
        Assert.Equal(3, pool.Allocated);
    }

    [Fact]
    public void A_Busy_Window_Resets_The_Count()
    {
        var (pool, time) = CreatePool();
        pool.RegisterCache();
        Spike(pool, 20);
        time.Advance(Window);
        Touch(pool);

        time.Advance(Window);
        Touch(pool);
        Spike(pool, 20);

        time.Advance(Window);
        Touch(pool);
        Assert.Equal(20, pool.Allocated);

        time.Advance(Window);
        Touch(pool);
        Assert.Equal(20, pool.Allocated);
    }

    [Fact]
    public void The_Minimum_Grows_With_Active_Caches()
    {
        var (pool, time) = CreatePool();
        for (var i = 0; i < 10; i++)
        {
            pool.RegisterCache();
        }
        Spike(pool, 30);
        for (var i = 0; i < 3; i++)
        {
            time.Advance(Window);
            Touch(pool);
        }

        Assert.Equal(12, pool.Allocated);
    }

    [Fact]
    public void Blocks_In_Use_Are_Never_Released()
    {
        var (pool, time) = CreatePool(new KurrentDBStreamCacheBufferPoolOptions { ExtraBlocks = 0, Headroom = 0, ConsecutiveWindows = 1 });
        var held = Enumerable.Range(0, 5).Select(_ => pool.Allocate()).ToList();
        Spike(pool, 10);

        time.Advance(Window);
        Touch(pool);
        time.Advance(Window);
        Touch(pool);

        Assert.Equal(5, pool.InUse);
        Assert.True(pool.Allocated >= 5);
        foreach (var block in held)
        {
            block.Dispose();
        }
        Assert.Equal(0, pool.InUse);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Trimming_Can_Be_Turned_Off(int windowMilliseconds)
    {
        var (pool, time) = CreatePool(new KurrentDBStreamCacheBufferPoolOptions { TrimWindow = TimeSpan.FromMilliseconds(windowMilliseconds) });
        pool.RegisterCache();
        Spike(pool, 20);
        for (var i = 0; i < 5; i++)
        {
            time.Advance(Window);
            Touch(pool);
        }

        Assert.Equal(20, pool.Allocated);
    }

    [Fact]
    public async Task Concurrent_Allocate_And_Free_Keep_Counts_Consistent()
    {
        var (pool, _) = CreatePool();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < 10_000; i++)
            {
                var block = pool.Allocate();
                block.Dispose();
            }
        })));

        Assert.Equal(0, pool.InUse);
        Assert.Equal(pool.Allocated, pool.Available);
    }

    [Fact]
    public void A_Block_Disposed_Twice_Is_Returned_Once()
    {
        var (pool, _) = CreatePool();
        var block = pool.Allocate();
        block.Dispose();
        block.Dispose();

        Assert.Equal(0, pool.InUse);
        Assert.Equal(1, pool.Available);
    }
}
