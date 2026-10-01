using System.Buffers;

namespace Continuum.ArrayPools.Tests;

/// <summary>
/// Wraps an <see cref="ArrayPool{T}"/> and records every rent and return, optionally failing on a given rent.
/// </summary>
internal sealed class RecordingArrayPool<T>(ArrayPool<T> inner, int? failOnRentNumber = null) : ArrayPool<T>
{
    public List<int> RentedLengths { get; } = [];

    public List<T[]> Rented { get; } = [];

    public List<T[]> Returned { get; } = [];

    public override T[] Rent(int minimumLength)
    {
        if (failOnRentNumber == RentedLengths.Count + 1)
        {
            throw new OutOfMemoryException("Simulated rent failure.");
        }

        RentedLengths.Add(minimumLength);
        var array = inner.Rent(minimumLength);
        Rented.Add(array);
        return array;
    }

    public override void Return(T[] array, bool clearArray = false)
    {
        Returned.Add(array);
        inner.Return(array, clearArray);
    }
}
