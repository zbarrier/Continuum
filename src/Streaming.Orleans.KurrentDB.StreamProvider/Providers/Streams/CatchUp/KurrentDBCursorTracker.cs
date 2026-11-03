using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

/// <summary>
///     Tracks how far each reading cache cursor has read so the cache never purges, and therefore never checkpoints,
///     past an event that some consumer has not seen yet.
/// </summary>
/// <remarks>
///     <para>
///         The Orleans pooled cache purges purely on age, and the checkpoint is written from the purge callback. Without
///         this watermark a slow consumer would have its unread events aged out and checkpointed away, so a silo restart
///         would skip them. Progress is tracked by <c>$all</c> commit position because that is the only value that orders
///         events across every stream in the cache.
///     </para>
///     <para>
///         Cursors are either reading or caught up. Only reading cursors hold back purging. A caught-up cursor has read
///         every cached event for its stream, so it is indexed by stream and woken when an event for that stream is
///         added, before any purge can run.
///     </para>
///     <para>
///         All members are called from the stream pulling agent's serialized scheduler, the same as the Orleans pooled
///         cache itself, so no locking is needed.
///     </para>
/// </remarks>
public sealed class KurrentDBCursorTracker
{
	private readonly HashSet<KurrentDBCursorEntry> _reading = new(ReferenceEqualityComparer.Instance);
	private readonly Dictionary<StreamId, HashSet<KurrentDBCursorEntry>> _caughtUpByStream = [];
	private ulong? _purgeSnapshot;
	private bool _inPurge;

	/// <summary>
	///     Registers a newly created cursor as reading. Until it reads anything it holds the watermark at zero, which
	///     blocks all purging; that is the safe direction, since a cursor that has read nothing may still need the
	///     oldest event.
	/// </summary>
	public KurrentDBCursorEntry Register(StreamId streamId, object cacheCursor)
	{
		var entry = new KurrentDBCursorEntry(streamId, cacheCursor);
		_reading.Add(entry);
		return entry;
	}

	/// <summary>
	///     Records that a cursor has been handed the event at <paramref name="commitPosition" />.
	/// </summary>
	public void RecordProgress(KurrentDBCursorEntry entry, ulong commitPosition)
	{
		if (!entry.IsReleased)
		{
			entry.CommitPosition = commitPosition;
		}
	}

	/// <summary>
	///     Moves a cursor that has read every cached event for its stream out of the purge watermark.
	/// </summary>
	public void MarkCaughtUp(KurrentDBCursorEntry entry)
	{
		if (entry.IsReleased || entry.IsCaughtUp)
		{
			return;
		}
		_reading.Remove(entry);
		entry.IsCaughtUp = true;
		if (!_caughtUpByStream.TryGetValue(entry.StreamId, out var entries))
		{
			entries = new HashSet<KurrentDBCursorEntry>(ReferenceEqualityComparer.Instance);
			_caughtUpByStream[entry.StreamId] = entries;
		}
		entries.Add(entry);
	}

	/// <summary>
	///     Removes and returns the caught-up cursors for <paramref name="streamId" />, returning them to reading with
	///     the watermark at <paramref name="commitPosition" />.
	/// </summary>
	public IReadOnlyCollection<KurrentDBCursorEntry> Wake(StreamId streamId, ulong commitPosition)
	{
		if (!_caughtUpByStream.Remove(streamId, out var entries))
		{
			return [];
		}
		foreach (var entry in entries)
		{
			entry.IsCaughtUp = false;
			entry.CommitPosition = commitPosition;
			_reading.Add(entry);
		}
		return entries;
	}

	/// <summary>
	///     Removes a disposed cursor so it stops holding the watermark down.
	/// </summary>
	public void Unregister(KurrentDBCursorEntry entry)
	{
		if (entry.IsReleased)
		{
			return;
		}
		entry.IsReleased = true;
		if (!entry.IsCaughtUp)
		{
			_reading.Remove(entry);
			return;
		}
		if (_caughtUpByStream.TryGetValue(entry.StreamId, out var entries) && entries.Remove(entry) && entries.Count == 0)
		{
			_caughtUpByStream.Remove(entry.StreamId);
		}
	}

	/// <summary>
	///     Computes the watermark once for a purge pass, so each purge candidate does not walk every cursor.
	/// </summary>
	public void BeginPurge()
	{
		_purgeSnapshot = ComputeSlowestCommitPosition();
		_inPurge = true;
	}

	/// <summary>
	///     Ends a purge pass started by <see cref="BeginPurge" />.
	/// </summary>
	public void EndPurge()
	{
		_inPurge = false;
		_purgeSnapshot = null;
	}

	/// <summary>
	///     The commit position every reading cursor has read up to, or <see langword="null" /> when no cursor is reading
	///     and purging is unrestricted.
	/// </summary>
	public ulong? GetSlowestCommitPosition() => _inPurge ? _purgeSnapshot : ComputeSlowestCommitPosition();

	private ulong? ComputeSlowestCommitPosition()
	{
		var slowest = default(ulong?);
		foreach (var entry in _reading)
		{
			if (slowest is null || entry.CommitPosition < slowest)
			{
				slowest = entry.CommitPosition;
			}
		}
		return slowest;
	}
}
