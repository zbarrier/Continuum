using Orleans.Configuration;
using Orleans.Providers.Streams.Common;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     The pool of fixed size blocks shared by every queue cache of a stream provider on a silo. Unlike the Orleans
///     <see cref="ObjectPool{T}" />, which only grows, it releases free blocks once demand has stayed lower for a while,
///     so memory used during a catch-up is given back after the subscription settles.
/// </summary>
/// <remarks>
///     Pulling agents allocate and free blocks on different threads. Trimming has to read the free list and several
///     related counters together, so every operation runs under one lock. Blocks are only allocated or freed when one
///     fills or is purged, so the lock is rarely taken.
/// </remarks>
internal sealed class KurrentDBQueueCacheBufferPool : IObjectPool<FixedSizeBuffer>
{
    private readonly Lock _lock = new();
    private readonly Stack<FixedSizeBuffer> _free = new();
    private readonly Func<FixedSizeBuffer> _factory;
    private readonly KurrentDBStreamCacheBufferPoolOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly IObjectPoolMonitor? _monitor;
    private readonly TimeSpan? _monitorWriteInterval;

    private long _allocated;
    private long _inUse;
    private long _windowPeak;
    private int _activeCaches;
    private int _windowsBelowTarget;
    private DateTimeOffset _windowEnd;
    private DateTimeOffset _nextReport;

    public KurrentDBQueueCacheBufferPool(Func<FixedSizeBuffer> factory, KurrentDBStreamCacheBufferPoolOptions options, TimeProvider timeProvider,
        IObjectPoolMonitor? monitor = null, TimeSpan? monitorWriteInterval = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _factory = factory;
        _options = options;
        _timeProvider = timeProvider;
        _monitor = monitor;
        _monitorWriteInterval = monitor is not null ? monitorWriteInterval : null;
        if (options.TrimmingEnabled)
        {
            _windowEnd = timeProvider.GetUtcNow() + options.TrimWindow;
        }
    }

    /// <summary>The number of blocks the pool owns, in use or free.</summary>
    public long Allocated
    {
        get { lock (_lock) { return _allocated; } }
    }

    /// <summary>The number of blocks held by queue caches.</summary>
    public long InUse
    {
        get { lock (_lock) { return _inUse; } }
    }

    /// <summary>The number of free blocks held by the pool.</summary>
    public int Available
    {
        get { lock (_lock) { return _free.Count; } }
    }

    /// <summary>The number of queue caches currently using the pool.</summary>
    public int ActiveCaches
    {
        get { lock (_lock) { return _activeCaches; } }
    }

    public FixedSizeBuffer Allocate()
    {
        FixedSizeBuffer block;
        lock (_lock)
        {
            if (!_free.TryPop(out block!))
            {
                block = _factory();
                _allocated++;
            }
            _inUse++;
            if (_inUse > _windowPeak)
            {
                _windowPeak = _inUse;
            }
            TryTrim();
        }
        block.Pool = this;
        _monitor?.TrackObjectAllocated();
        TryReport();
        return block;
    }

    public void Free(FixedSizeBuffer resource)
    {
        lock (_lock)
        {
            _free.Push(resource);
            _inUse--;
            TryTrim();
        }
        _monitor?.TrackObjectReleased();
        TryReport();
    }

    /// <summary>Records that a queue cache has started using the pool.</summary>
    public void RegisterCache()
    {
        lock (_lock)
        {
            _activeCaches++;
        }
    }

    /// <summary>Records that a queue cache has stopped using the pool.</summary>
    public void UnregisterCache()
    {
        lock (_lock)
        {
            _activeCaches--;
        }
    }

    private void TryTrim()
    {
        if (!_options.TrimmingEnabled)
        {
            return;
        }
        var now = _timeProvider.GetUtcNow();
        if (now < _windowEnd)
        {
            return;
        }

        // Each active cache always holds a block it is writing to, so the minimum grows with the caches on this silo.
        var minimum = (long)_activeCaches + _options.ExtraBlocks;
        var target = Math.Max(minimum, _windowPeak + _options.Headroom);
        if (_allocated > target)
        {
            _windowsBelowTarget++;
            if (_windowsBelowTarget >= _options.ConsecutiveWindows)
            {
                // Only free blocks are dropped; they become garbage and are reclaimed by the next gen 2 collection.
                while (_allocated > target && _free.TryPop(out _))
                {
                    _allocated--;
                }
                _windowsBelowTarget = 0;
            }
        }
        else
        {
            _windowsBelowTarget = 0;
        }

        _windowPeak = _inUse;
        _windowEnd = now + _options.TrimWindow;
    }

    private void TryReport()
    {
        if (_monitorWriteInterval is not { } interval)
        {
            return;
        }
        long allocated;
        long free;
        lock (_lock)
        {
            var now = _timeProvider.GetUtcNow();
            if (now < _nextReport)
            {
                return;
            }
            _nextReport = now + interval;
            allocated = _allocated;
            free = _free.Count;
        }
        _monitor!.Report(allocated, free, allocated - free);
    }
}
