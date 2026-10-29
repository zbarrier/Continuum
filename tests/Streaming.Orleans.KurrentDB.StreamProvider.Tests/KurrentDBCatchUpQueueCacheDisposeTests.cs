using Continuum.Orleans.Streams;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Storage;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that a disposed queue cache gives its blocks back to the shared pool, so blocks are not stranded when a
///     queue leaves the silo and do not stop the pool from trimming.
/// </summary>
public class KurrentDBCatchUpQueueCacheDisposeTests
{
	private const int BlockSize = 4096;

	private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

	private static readonly StreamName Stream = new("test-1", "test", null, "1");

	private sealed class FakeTypeMapper : TypeMapper;

	private sealed class FakeSerializer : IGrainStorageSerializer
	{
		public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

		public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
	}

	private static KurrentDBCatchUpQueueCache CreateCache(KurrentDBQueueCacheBufferPool pool)
	{
		var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
		{
			ConnectionName = "test",
			ProviderName = "test",
			TypeMapper = new FakeTypeMapper(),
			GrainStorageSerializer = new FakeSerializer(),
		});
		var logger = NullLogger.Instance;
		var cursorTracker = new KurrentDBCursorTracker();
		var timePurge = new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
		var evictionStrategy = new KurrentDBCatchUpEvictionStrategy(logger, timePurge, null, null, adapter, cursorTracker);
		return new KurrentDBCatchUpQueueCache("queue", 100, pool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null);
	}

	private static StreamedEvent<KurrentDBRawEvent> CreateEvent(ulong streamVersion, ulong commitPosition, int dataSize) =>
		new(Guid.NewGuid(), "test-event", Stream, streamVersion, new StreamedEventCursor("test", Stream.Category, Stream.Value, commitPosition, 0),
			DateTime.UtcNow, new KurrentDBRawEvent(new byte[dataSize]));

	[Fact]
	public void Disposing_A_Cache_Returns_Its_Blocks_And_Unregisters_It()
	{
		var pool = new KurrentDBQueueCacheBufferPool(() => new FixedSizeBuffer(BlockSize), new KurrentDBStreamCacheBufferPoolOptions(), TimeProvider.System);
		var cache = CreateCache(pool);
		Assert.Equal(1, pool.ActiveCaches);

		// Each event takes most of a block, so several blocks are held.
		cache.Add([CreateEvent(0, 100, 3000), CreateEvent(1, 200, 3000), CreateEvent(2, 300, 3000)], DateTime.UtcNow);
		Assert.True(pool.InUse >= 3);

		cache.Dispose();
		cache.Dispose();

		Assert.Equal(0, pool.InUse);
		Assert.Equal(pool.Allocated, pool.Available);
		Assert.Equal(0, pool.ActiveCaches);
	}
}
