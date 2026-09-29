// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).
//           Replaced x86-only Ssse3 intrinsics with cross-platform Vector128, replaced SpinLock with System.Threading.Lock.

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Continuum;

/// <summary>Default thread-safe <see cref="INewIdGenerator"/> combining a timestamp, worker identifier, optional process identifier, and sequence.</summary>
public class NewIdGenerator : INewIdGenerator
{
    readonly int _c;
    readonly int _d;
    readonly short _gb;
    readonly short _gc;
    readonly ITickProvider _tickProvider;
    int _a;
    int _b;
    long _lastTick;
    int _sequence;

    readonly Lock _lock = new();

    /// <summary>Initializes a new instance of the <see cref="NewIdGenerator"/> class.</summary>
    /// <param name="tickProvider">The timestamp source.</param>
    /// <param name="workerIdProvider">The worker identifier source.</param>
    /// <param name="processIdProvider">An optional process identifier source; when <see langword="null"/>, the last two worker identifier bytes are used instead.</param>
    /// <param name="workerIndex">The index passed to <see cref="IWorkerIdProvider.GetWorkerId(int)"/>.</param>
    public NewIdGenerator(ITickProvider tickProvider, IWorkerIdProvider workerIdProvider, IProcessIdProvider? processIdProvider = null, int workerIndex = 0)
    {
        _tickProvider = tickProvider;

        var workerId = workerIdProvider.GetWorkerId(workerIndex);

        _c = workerId[0] << 24 | workerId[1] << 16 | workerId[2] << 8 | workerId[3];

        if (processIdProvider != null)
        {
            var processId = processIdProvider.GetProcessId();
            _d = processId[0] << 24 | processId[1] << 16;
        }
        else
            _d = workerId[4] << 24 | workerId[5] << 16;

        _gb = (short)_c;
        _gc = (short)(_c >> 16);
    }

    /// <inheritdoc />
    public NewId Next()
    {
        var ticks = _tickProvider.Ticks;

        _lock.Enter();

        if (ticks > _lastTick)
            UpdateTimestamp(ticks);
        else if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
            UpdateTimestamp(_lastTick + 1);

        var sequence = _sequence++;

        var a = _a;
        var b = _b;

        _lock.Exit();

        return new NewId(a, b, _c, _d | sequence);
    }

    /// <summary>
    /// Converts the NewId to a Guid in the MSSQL Server ordered format. 
    /// This is the format that should be used for MSSQL Server for clustered indexes.
    /// </summary>
    /// <returns>MSSQL Server ordered Guid.</returns>
    public Guid NextGuid()
    {
        var ticks = _tickProvider.Ticks;

        _lock.Enter();

        if (ticks > _lastTick)
            UpdateTimestamp(ticks);
        else if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
            UpdateTimestamp(_lastTick + 1);

        var sequence = _sequence++;

        var a = _a;
        var b = _b;

        _lock.Exit();

        // swapping high and low byte, because SQL-server is doing the wrong ordering otherwise
        var sequenceSwapped = (sequence << 8 | sequence >> 8 & 0x00FF) & 0xFFFF;

        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            var vec = Vector128.Create(a, b, _c, _d | sequenceSwapped);
            var result = Vector128.Shuffle(vec.AsByte(), Vector128.Create((byte)12, 13, 14, 15, 8, 9, 10, 11, 5, 4, 3, 2, 1, 0, 7, 6));
            return Unsafe.As<Vector128<byte>, Guid>(ref result);
        }

        var d = (byte)(b >> 8);
        var e = (byte)b;
        var f = (byte)(a >> 24);
        var g = (byte)(a >> 16);
        var h = (byte)(a >> 8);
        var i = (byte)a;
        var j = (byte)(b >> 24);
        var k = (byte)(b >> 16);

        return new Guid(_d | sequenceSwapped, _gb, _gc, d, e, f, g, h, i, j, k);
    }

    /// <summary>
    /// Returns a Guid in sequential format sometimes referred to as lexicographical order. 
    /// This is the format that should be used for Postgres and MySql for clustered indexes. 
    /// This is typically the preferred format used in systems and the MSSQL Server format 
    /// is converted to and from this format during load and save operations.
    /// </summary>
    /// <returns>Lexicographical ordered Guid.</returns>
    public Guid NextSequentialGuid()
    {
        var ticks = _tickProvider.Ticks;

        _lock.Enter();

        if (ticks > _lastTick)
            UpdateTimestamp(ticks);
        else if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
            UpdateTimestamp(_lastTick + 1);

        var sequence = _sequence++;

        var a = _a;
        var v = _b;
        var b = (short)(_b >> 16);
        var c = (short)_b;

        _lock.Exit();

        // swapping high and low byte, because SQL-server is doing the wrong ordering otherwise
        var sequenceSwapped = (sequence << 8 | sequence >> 8 & 0x00FF) & 0xFFFF;

        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            var vec = Vector128.Create(a, v, _c, _d | sequenceSwapped);
            var result = Vector128.Shuffle(vec.AsByte(), Vector128.Create((byte)0, 1, 2, 3, 6, 7, 4, 5, 11, 10, 9, 8, 15, 14, 13, 12));
            return Unsafe.As<Vector128<byte>, Guid>(ref result);
        }

        var d = (byte)(_gc >> 8);
        var e = (byte)_gc;
        var f = (byte)(_gb >> 8);
        var g = (byte)_gb;

        var h = (byte)((_d | sequenceSwapped) >> 24);
        var i = (byte)((_d | sequenceSwapped) >> 16);
        var j = (byte)((_d | sequenceSwapped) >> 8);
        var k = (byte)(_d | sequenceSwapped);

        return new Guid(a, b, c, d, e, f, g, h, i, j, k);
    }

    /// <inheritdoc />
    public ArraySegment<NewId> Next(NewId[] ids, int index, int count)
    {
        if (index + count > ids.Length)
            throw new ArgumentOutOfRangeException(nameof(count));

        var ticks = _tickProvider.Ticks;

        _lock.Enter();

        if (ticks > _lastTick)
            UpdateTimestamp(ticks);

        var limit = index + count;
        for (var i = index; i < limit; i++)
        {
            if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
                UpdateTimestamp(_lastTick + 1);

            ids[i] = new NewId(_a, _b, _c, _d | _sequence++);
        }

        _lock.Exit();

        return new ArraySegment<NewId>(ids, index, count);
    }

    void UpdateTimestamp(long tick)
    {
        _b = (int)(tick & 0xFFFFFFFF);
        _a = (int)(tick >> 32);

        _sequence = 0;
        _lastTick = tick;
    }
}
