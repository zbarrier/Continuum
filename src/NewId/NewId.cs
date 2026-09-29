// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).
//           Replaced x86-only Ssse3 intrinsics with cross-platform Vector128, removed allocations in Guid conversions.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Continuum;

/// <summary>
/// A NewId is a type that fits into the same space as a Guid/Uuid/unique identifier,
/// but is guaranteed to be both unique and ordered, assuming it is generated using
/// a single instance of the generator for each network address used.
/// </summary>
public readonly struct NewId :
    IEquatable<NewId>,
    IComparable<NewId>,
    IComparable,
    IFormattable
{
    /// <summary>An empty <see cref="NewId"/> with all bits set to zero.</summary>
    public static readonly NewId Empty = new NewId(0, 0, 0, 0);

    static readonly INewIdFormatter BraceFormatter = new DashedHexFormatter('{', '}');
    static readonly INewIdFormatter DashedHexFormatter = new DashedHexFormatter();
    static readonly INewIdFormatter HexFormatter = new HexFormatter();
    static readonly INewIdFormatter ParenFormatter = new DashedHexFormatter('(', ')');

    static INewIdGenerator? _generator;
    static ITickProvider? _tickProvider;
    static IWorkerIdProvider? _workerIdProvider;
    static IProcessIdProvider? _processIdProvider;

    readonly int _a;
    readonly int _b;
    readonly int _c;
    readonly int _d;

    /// <summary>
    /// Creates a NewId using the specified byte array.
    /// </summary>
    /// <param name="bytes"></param>
    public NewId(in byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));
        if (bytes.Length != 16)
            throw new ArgumentException("Exactly 16 bytes expected", nameof(bytes));

        FromByteArray(bytes, out _a, out _b, out _c, out _d);
    }

    /// <summary>Creates a <see cref="NewId"/> from a <see cref="Guid"/>-formatted string.</summary>
    /// <param name="value">A string accepted by <see cref="Guid.Guid(string)"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is null or empty.</exception>
    public NewId(in string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("must not be null or empty", nameof(value));

        var guid = new Guid(value);

        var bytes = guid.ToByteArray();

        FromByteArray(bytes, out _a, out _b, out _c, out _d);
    }

    /// <summary>Creates a <see cref="NewId"/> from its four internal 32-bit components.</summary>
    /// <param name="a">The high 32 bits of the timestamp.</param>
    /// <param name="b">The low 32 bits of the timestamp.</param>
    /// <param name="c">The first worker/process component.</param>
    /// <param name="d">The second worker/process and sequence component.</param>
    public NewId(int a, int b, int c, int d)
    {
        _a = a;
        _b = b;
        _c = c;
        _d = d;
    }

    /// <summary>Creates a <see cref="NewId"/> from components laid out like the <see cref="Guid"/> constructor.</summary>
    /// <param name="a">The first 4 bytes.</param>
    /// <param name="b">The next 2 bytes.</param>
    /// <param name="c">The next 2 bytes.</param>
    /// <param name="d">Byte 8.</param>
    /// <param name="e">Byte 9.</param>
    /// <param name="f">Byte 10.</param>
    /// <param name="g">Byte 11.</param>
    /// <param name="h">Byte 12.</param>
    /// <param name="i">Byte 13.</param>
    /// <param name="j">Byte 14.</param>
    /// <param name="k">Byte 15.</param>
    public NewId(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
    {
        _a = (f << 24) | (g << 16) | (h << 8) | i;
        _b = (j << 24) | (k << 16) | (d << 8) | e;
        _c = (c << 16) | (ushort)b;
        _d = (int)(a & 0xFFFF0000) | ((a >> 8) & 0x00FF) | ((a << 8) & 0xFF00);
    }

    static IWorkerIdProvider WorkerIdProvider => _workerIdProvider ??= new BestPossibleWorkerIdProvider();

    static IProcessIdProvider? ProcessIdProvider => _processIdProvider;

    static ITickProvider TickProvider => _tickProvider ??= new DateTimeTickProvider();

    /// <summary>Gets the UTC timestamp at which this identifier was generated.</summary>
    public DateTime Timestamp
    {
        get
        {
            var ticks = (long)(((ulong)_a << 32) | (uint)_b);

            return new DateTime(ticks, DateTimeKind.Utc);
        }
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj == null)
            return 1;
        if (!(obj is NewId))
            throw new ArgumentException("Argument must be a NewId");

        return CompareTo((NewId)obj);
    }

    /// <inheritdoc />
    public int CompareTo(NewId other)
    {
        if (_a != other._a)
            return (uint)_a < (uint)other._a ? -1 : 1;
        if (_b != other._b)
            return (uint)_b < (uint)other._b ? -1 : 1;
        if (_c != other._c)
            return (uint)_c < (uint)other._c ? -1 : 1;
        if (_d != other._d)
            return (uint)_d < (uint)other._d ? -1 : 1;

        return 0;
    }

    /// <inheritdoc />
    public bool Equals(NewId other)
    {
        return other._a == _a && other._b == _b && other._c == _c && other._d == _d;
    }

    /// <summary>Formats this identifier using a <see cref="Guid"/>-style format specifier.</summary>
    /// <param name="format"><c>D</c> (default), <c>N</c>, <c>B</c>, or <c>P</c>; append <c>S</c> (for example <c>DS</c>) for sequential byte order.</param>
    /// <param name="formatProvider">Ignored.</param>
    /// <returns>The formatted identifier.</returns>
    /// <exception cref="FormatException"><paramref name="format"/> is not a supported specifier.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format))
            format = "D";

        var sequential = false;
        if (format.Length == 2 && (format[1] == 'S' || format[1] == 's'))
            sequential = true;
        else if (format.Length != 1)
            throw new FormatException("The format string must be exactly one character or null");

        var formatCh = format[0];
        var bytes = sequential ? GetSequentialFormatterArray() : GetFormatterArray();

        if (formatCh == 'B' || formatCh == 'b')
            return BraceFormatter.Format(bytes);
        if (formatCh == 'P' || formatCh == 'p')
            return ParenFormatter.Format(bytes);
        if (formatCh == 'D' || formatCh == 'd')
            return DashedHexFormatter.Format(bytes);
        if (formatCh == 'N' || formatCh == 'n')
            return HexFormatter.Format(bytes);

        throw new FormatException("The format string was not valid");
    }

    static readonly ThreadLocal<byte[]> _formatterArray = new ThreadLocal<byte[]>(() => new byte[16]);

    /// <summary>Formats this identifier using a custom formatter.</summary>
    /// <param name="formatter">The formatter to use.</param>
    /// <param name="sequential"><see langword="true"/> to pass bytes in sequential order instead of <see cref="Guid"/> order.</param>
    /// <returns>The formatted identifier.</returns>
    public string ToString(INewIdFormatter formatter, bool sequential = false)
    {
        var bytes = sequential ? GetSequentialFormatterArray() : GetFormatterArray();

        return formatter.Format(bytes);
    }

    byte[] GetFormatterArray()
    {
        var bytes = _formatterArray.Value!;

        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            var vector = Unsafe.As<NewId, Vector128<byte>>(ref Unsafe.AsRef(in this));
            var byteArrayShuffle = Vector128.Create((byte)15, 14, 12, 13, 9, 8, 11, 10, 5, 4, 3, 2, 1, 0, 7, 6);
            var result = Vector128.Shuffle(vector, byteArrayShuffle);
            MemoryMarshal.TryWrite(bytes, in result);
            return bytes;
        }

        bytes[15] = (byte)(_b >> 16);
        bytes[14] = (byte)(_b >> 24);
        bytes[13] = (byte)_a;
        bytes[12] = (byte)(_a >> 8);
        bytes[11] = (byte)(_a >> 16);
        bytes[10] = (byte)(_a >> 24);
        bytes[9] = (byte)_b;
        bytes[8] = (byte)(_b >> 8);
        bytes[7] = (byte)(_c >> 16);
        bytes[6] = (byte)(_c >> 24);
        bytes[5] = (byte)_c;
        bytes[4] = (byte)(_c >> 8);
        bytes[3] = (byte)(_d >> 8);
        bytes[2] = (byte)_d;
        bytes[1] = (byte)(_d >> 16);
        bytes[0] = (byte)(_d >> 24);

        return bytes;
    }

    byte[] GetSequentialFormatterArray()
    {
        var bytes = _formatterArray.Value!;

        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            var vector = Unsafe.As<NewId, Vector128<byte>>(ref Unsafe.AsRef(in this));
            var byteArrayShuffle = Vector128.Create((byte)3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12);
            var result = Vector128.Shuffle(vector, byteArrayShuffle);
            MemoryMarshal.TryWrite(bytes, in result);
            return bytes;
        }

        bytes[15] = (byte)_d;
        bytes[14] = (byte)(_d >> 8);
        bytes[13] = (byte)(_d >> 16);
        bytes[12] = (byte)(_d >> 24);
        bytes[11] = (byte)_c;
        bytes[10] = (byte)(_c >> 8);
        bytes[9] = (byte)(_c >> 16);
        bytes[8] = (byte)(_c >> 24);
        bytes[7] = (byte)_b;
        bytes[6] = (byte)(_b >> 8);
        bytes[5] = (byte)(_b >> 16);
        bytes[4] = (byte)(_b >> 24);
        bytes[3] = (byte)_a;
        bytes[2] = (byte)(_a >> 8);
        bytes[1] = (byte)(_a >> 16);
        bytes[0] = (byte)(_a >> 24);

        return bytes;
    }

    /// <summary>
    /// Converts the NewId to a Guid in the MSSQL Server ordered format. 
    /// This is the format that should be used for MSSQL Server for clustered indexes.
    /// </summary>
    /// <returns>MSSQL Server ordered Guid.</returns>
    public Guid ToGuid()
    {
        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            var bytes = Unsafe.As<NewId, Vector128<byte>>(ref Unsafe.AsRef(in this));
            var shuffled = Vector128.Shuffle(bytes, Vector128.Create((byte)13, 12, 14, 15, 8, 9, 10, 11, 5, 4, 3, 2, 1, 0, 7, 6));
            return Unsafe.As<Vector128<byte>, Guid>(ref shuffled);
        }

        var a = (int)(_d & 0xFFFF0000) | ((_d >> 8) & 0x00FF) | ((_d << 8) & 0xFF00);
        var b = (short)_c;
        var c = (short)(_c >> 16);
        var d = (byte)(_b >> 8);
        var e = (byte)_b;
        var f = (byte)(_a >> 24);
        var g = (byte)(_a >> 16);
        var h = (byte)(_a >> 8);
        var i = (byte)_a;
        var j = (byte)(_b >> 24);
        var k = (byte)(_b >> 16);

        return new Guid(a, b, c, d, e, f, g, h, i, j, k);
    }

    /// <summary>
    /// Converts the NewId to a Guid in sequential format sometimes referred to as lexicographical order. 
    /// This is the format that should be used for Postgres and MySql for clustered indexes. This is typically 
    /// the preferred format used in systems and the MSSQL Server format is converted to and from this format 
    /// during load and save operations.
    /// </summary>
    /// <returns>Lexicographical ordered Guid.</returns>
    public Guid ToSequentialGuid()
    {
        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            var bytes = Unsafe.As<NewId, Vector128<byte>>(ref Unsafe.AsRef(in this));
            var shuffled = Vector128.Shuffle(bytes, Vector128.Create((byte)0, 1, 2, 3, 6, 7, 4, 5, 11, 10, 9, 8, 15, 14, 13, 12));
            return Unsafe.As<Vector128<byte>, Guid>(ref shuffled);
        }

        var a = _a;
        var b = (short)(_b >> 16);
        var c = (short)_b;
        var d = (byte)(_c >> 24);
        var e = (byte)(_c >> 16);
        var f = (byte)(_c >> 8);
        var g = (byte)_c;
        var h = (byte)(_d >> 24);
        var i = (byte)(_d >> 16);
        var j = (byte)(_d >> 8);
        var k = (byte)_d;

        return new Guid(a, b, c, d, e, f, g, h, i, j, k);
    }

    /// <summary>Creates a <see cref="NewId"/> from a <see cref="Guid"/> in MSSQL Server ordered format.</summary>
    /// <param name="guid">The <see cref="Guid"/> to convert.</param>
    /// <returns>The equivalent <see cref="NewId"/>.</returns>
    public static NewId FromGuid(in Guid guid)
    {
        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            FromByteVector(Unsafe.As<Guid, Vector128<byte>>(ref Unsafe.AsRef(in guid)), out NewId newId);
            return newId;
        }

        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);
        FromByteArray(bytes, out int a, out int b, out int c, out int d);

        return new NewId(a, b, c, d);
    }

    /// <summary>Creates a <see cref="NewId"/> from a <see cref="Guid"/> in sequential (lexicographical) format.</summary>
    /// <param name="guid">The <see cref="Guid"/> to convert.</param>
    /// <returns>The equivalent <see cref="NewId"/>.</returns>
    public static NewId FromSequentialGuid(in Guid guid)
    {
        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            FromSequentialByteVector(Unsafe.As<Guid, Vector128<byte>>(ref Unsafe.AsRef(in guid)), out NewId newId);
            return newId;
        }

        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);
        FromSequentialByteArray(bytes, out int a, out int b, out int c, out int d);

        return new NewId(a, b, c, d);
    }

    /// <summary>Returns the 16-byte representation of this identifier in <see cref="Guid"/> byte order.</summary>
    /// <returns>A new 16-byte array.</returns>
    public byte[] ToByteArray()
    {
        var bytes = new byte[16];

        if (Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian)
        {
            var vector = Unsafe.As<NewId, Vector128<byte>>(ref Unsafe.AsRef(in this));
            var byteArrayShuffle = Vector128.Create((byte)13, 12, 14, 15, 8, 9, 10, 11, 5, 4, 3, 2, 1, 0, 7, 6);
            var result = Vector128.Shuffle(vector, byteArrayShuffle);
            MemoryMarshal.TryWrite(bytes, in result);
            return bytes;
        }

        bytes[15] = (byte)(_b >> 16);
        bytes[14] = (byte)(_b >> 24);
        bytes[13] = (byte)_a;
        bytes[12] = (byte)(_a >> 8);
        bytes[11] = (byte)(_a >> 16);
        bytes[10] = (byte)(_a >> 24);
        bytes[9] = (byte)_b;
        bytes[8] = (byte)(_b >> 8);
        bytes[7] = (byte)(_c >> 24);
        bytes[6] = (byte)(_c >> 16);
        bytes[5] = (byte)(_c >> 8);
        bytes[4] = (byte)_c;
        bytes[3] = (byte)(_d >> 24);
        bytes[2] = (byte)(_d >> 16);
        bytes[1] = (byte)_d;
        bytes[0] = (byte)(_d >> 8);

        return bytes;
    }

    /// <summary>Formats this identifier using the <c>D</c> format.</summary>
    /// <returns>The formatted identifier.</returns>
    public override string ToString()
    {
        return ToString("D", null);
    }

    /// <summary>Formats this identifier using a <see cref="Guid"/>-style format specifier.</summary>
    /// <param name="format">See <see cref="ToString(string, IFormatProvider)"/>.</param>
    /// <returns>The formatted identifier.</returns>
    public string ToString(string? format)
    {
        return ToString(format, null);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (obj.GetType() != typeof(NewId))
            return false;
        return Equals((NewId)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var result = _a;
            result = (result * 397) ^ _b;
            result = (result * 397) ^ _c;
            result = (result * 397) ^ _d;
            return result;
        }
    }

    /// <summary>Determines whether two identifiers are equal.</summary>
    /// <param name="left">The first identifier.</param>
    /// <param name="right">The second identifier.</param>
    /// <returns><see langword="true"/> if the identifiers are equal.</returns>
    public static bool operator ==(in NewId left, in NewId right)
    {
        return left._a == right._a && left._b == right._b && left._c == right._c && left._d == right._d;
    }

    /// <summary>Determines whether two identifiers are not equal.</summary>
    /// <param name="left">The first identifier.</param>
    /// <param name="right">The second identifier.</param>
    /// <returns><see langword="true"/> if the identifiers differ.</returns>
    public static bool operator !=(in NewId left, in NewId right)
    {
        return !(left == right);
    }

    /// <summary>Determines whether <paramref name="left"/> sorts before <paramref name="right"/>.</summary>
    /// <param name="left">The first identifier.</param>
    /// <param name="right">The second identifier.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>.</returns>
    public static bool operator <(in NewId left, in NewId right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>Determines whether <paramref name="left"/> sorts after <paramref name="right"/>.</summary>
    /// <param name="left">The first identifier.</param>
    /// <param name="right">The second identifier.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>.</returns>
    public static bool operator >(in NewId left, in NewId right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>Replaces the generator used by the static <c>Next</c> methods.</summary>
    /// <param name="generator">The generator to use.</param>
    public static void SetGenerator(INewIdGenerator generator)
    {
        _generator = generator;
    }

    /// <summary>Sets the worker identifier provider used when the default generator is created. Must be called before the first identifier is generated to take effect.</summary>
    /// <param name="provider">The provider to use.</param>
    public static void SetWorkerIdProvider(IWorkerIdProvider provider)
    {
        _workerIdProvider = provider;
    }

    /// <summary>Sets the process identifier provider used when the default generator is created. Must be called before the first identifier is generated to take effect.</summary>
    /// <param name="provider">The provider to use.</param>
    public static void SetProcessIdProvider(IProcessIdProvider provider)
    {
        _processIdProvider = provider;
    }

    /// <summary>Sets the tick provider used when the default generator is created. Must be called before the first identifier is generated to take effect.</summary>
    /// <param name="provider">The provider to use.</param>
    public static void SetTickProvider(ITickProvider provider)
    {
        _tickProvider = provider;
    }

    static SpinLock _spinLock = new SpinLock(false);

    static INewIdGenerator _getGenerator()
    {
        if (_generator != null)
            return _generator;

        var lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);

            _generator ??= new NewIdGenerator(TickProvider, WorkerIdProvider, ProcessIdProvider);
        }
        finally
        {
            if (lockTaken)
                _spinLock.Exit();
        }

        return _generator;
    }

    /// <summary>
    /// Generate a NewId
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NewId Next()
    {
        return _getGenerator().Next();
    }

    /// <summary>
    /// Generate an array of NewIds
    /// </summary>
    /// <param name="count">The number of NewIds to generate</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NewId[] Next(int count)
    {
        var ids = new NewId[count];

        _getGenerator().Next(ids, 0, count);

        return ids;
    }

    /// <summary>
    /// Generate an array of NewIds
    /// </summary>
    /// <param name="index">The starting offset for the newly generated ids</param>
    /// <param name="count">The number of NewIds to generate</param>
    /// <param name="ids">An existing array</param>
    /// <returns></returns>
    public static ArraySegment<NewId> Next(NewId[] ids, int index, int count)
    {
        return _getGenerator().Next(ids, index, count);
    }

    /// <summary>
    /// Converts the NewId to a Guid in the MSSQL Server ordered format. 
    /// This is the format that should be used for MSSQL Server for clustered indexes.
    /// </summary>
    /// <returns>MSSQL Server ordered Guid.</returns>
    public static Guid NextGuid()
    {
        return _getGenerator().NextGuid();
    }

    /// <summary>
    /// Returns a Guid in sequential format sometimes referred to as lexicographical order. 
    /// This is the format that should be used for Postgres and MySql for clustered indexes. 
    /// This is typically the preferred format used in systems and the MSSQL Server format 
    /// is converted to and from this format during load and save operations.
    /// </summary>
    /// <returns>Lexicographical ordered Guid.</returns>
    public static Guid NextSequentialGuid()
    {
        return _getGenerator().NextSequentialGuid();
    }

    static void FromByteArray(ReadOnlySpan<byte> bytes, out Int32 a, out Int32 b, out Int32 c, out Int32 d)
    {
        a = (bytes[10] << 24) | (bytes[11] << 16) | (bytes[12] << 8) | bytes[13];
        b = (bytes[14] << 24) | (bytes[15] << 16) | (bytes[8] << 8) | bytes[9];
        c = (bytes[7] << 24) | (bytes[6] << 16) | (bytes[5] << 8) | bytes[4];
        d = (bytes[3] << 24) | (bytes[2] << 16) | (bytes[0] << 8) | bytes[1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void FromByteVector(Vector128<byte> vector, out NewId newId)
    {
        Debug.Assert(Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian);

        var shuffle = Vector128.Create((byte)13, 12, 11, 10, 9, 8, 15, 14, 4, 5, 6, 7, 1, 0, 2, 3);
        var result = Vector128.Shuffle(vector, shuffle);
        newId = Unsafe.As<Vector128<byte>, NewId>(ref result);
    }

    static void FromSequentialByteArray(ReadOnlySpan<byte> bytes, out Int32 a, out Int32 b, out Int32 c, out Int32 d)
    {
        a = bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0];
        b = bytes[5] << 24 | bytes[4] << 16 | bytes[7] << 8 | bytes[6];
        c = bytes[8] << 24 | bytes[9] << 16 | bytes[10] << 8 | bytes[11];
        d = bytes[12] << 24 | bytes[13] << 16 | bytes[14] << 8 | bytes[15];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void FromSequentialByteVector(Vector128<byte> vector, out NewId newId)
    {
        Debug.Assert(Vector128.IsHardwareAccelerated && BitConverter.IsLittleEndian);

        var shuffle = Vector128.Create((byte)0, 1, 2, 3, 6, 7, 4, 5, 11, 10, 9, 8, 15, 14, 13, 12);
        var result = Vector128.Shuffle(vector, shuffle);
        newId = Unsafe.As<Vector128<byte>, NewId>(ref result);
    }
}
