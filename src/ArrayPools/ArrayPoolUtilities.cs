using System.Buffers;
using System.Numerics;

namespace Continuum.ArrayPools;

/// <summary>
/// Utilities for working with <see cref="ArrayPool{T}"/> instances.
/// </summary>
public static class ArrayPoolUtilities
{
    /// <summary>
    /// The largest array length that the built-in <see cref="ArrayPool{T}"/> implementations will pool (2^30).
    /// </summary>
    public const int MaxPooledArrayLength = 1024 * 1024 * 1024;

    /// <summary>
    /// The index of the bucket that holds arrays of length <see cref="MaxPooledArrayLength"/>.
    /// </summary>
    public const int MaxBucketIndex = 26;

    /// <summary>
    /// Creates an array pool with <see cref="ArrayPool{T}.Create(int, int)"/> and pre-warms every bucket using the
    /// same limits, so no arrays are allocated beyond what the pool will retain.
    /// </summary>
    /// <remarks>
    /// See <see cref="InitializeArrayPool{T}(ArrayPool{T}, int, int, int)"/> for the memory cost of pre-warming.
    /// </remarks>
    /// <typeparam name="T">The element type of the pooled arrays.</typeparam>
    /// <param name="maxArrayLength">The largest array length the pool will store and pre-warm.</param>
    /// <param name="maxArraysPerBucket">The number of arrays the pool will store, and pre-warm, per bucket.</param>
    /// <param name="minRentArrayLength">
    /// The smallest array length expected to be rented; buckets for smaller arrays are not pre-warmed.
    /// </param>
    /// <returns>The pre-warmed array pool.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxArrayLength"/>, <paramref name="maxArraysPerBucket"/>, or <paramref name="minRentArrayLength"/>
    /// is less than or equal to zero, <paramref name="maxArrayLength"/> is greater than <see cref="MaxPooledArrayLength"/>,
    /// or <paramref name="minRentArrayLength"/> is greater than <paramref name="maxArrayLength"/>.
    /// </exception>
    public static ArrayPool<T> CreatePreWarmed<T>(int maxArrayLength, int maxArraysPerBucket, int minRentArrayLength = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxArrayLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxArrayLength, MaxPooledArrayLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxArraysPerBucket);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minRentArrayLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minRentArrayLength, maxArrayLength);

        var pool = ArrayPool<T>.Create(maxArrayLength, maxArraysPerBucket);
        InitializeArrayPool(pool, maxArrayLength, maxArraysPerBucket, minRentArrayLength);
        return pool;
    }

    /// <summary>
    /// Pre-warms an array pool by renting and returning arrays for every bucket from
    /// <paramref name="minRentArrayLength"/> up to <paramref name="maxRentArrayLength"/>, so subsequent rentals do not allocate.
    /// </summary>
    /// <remarks>
    /// Pre-warming is reliable only for pools created with <see cref="ArrayPool{T}.Create(int, int)"/> where
    /// <paramref name="maxRentArrayLength"/> does not exceed the pool's <c>maxArrayLength</c>, and
    /// <paramref name="maxArrayInstancesPerBucket"/> does not exceed its <c>maxArraysPerBucket</c>; arrays beyond
    /// those limits are allocated and then discarded rather than pooled.
    /// For <see cref="ArrayPool{T}.Shared"/>, returned arrays are stored per thread and per processor core, and
    /// may be trimmed under memory pressure, so pre-warming from a single thread has limited and temporary effect.
    /// <para>
    /// Bucket sizes double, so the total memory allocated is roughly
    /// <c>2 * maxRentArrayLength * sizeof(T) * maxArrayInstancesPerBucket</c> bytes. For example, pre-warming a
    /// <see cref="long"/> pool to <see cref="MaxPooledArrayLength"/> with 16 arrays per bucket allocates about 256 GB.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The element type of the pooled arrays.</typeparam>
    /// <param name="arrayPool">The pool to pre-warm.</param>
    /// <param name="maxRentArrayLength">The largest array length expected to be rented from the pool.</param>
    /// <param name="maxArrayInstancesPerBucket">The number of arrays to create for each bucket.</param>
    /// <param name="minRentArrayLength">
    /// The smallest array length expected to be rented; buckets for smaller arrays are not pre-warmed.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="arrayPool"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxRentArrayLength"/>, <paramref name="maxArrayInstancesPerBucket"/>, or <paramref name="minRentArrayLength"/>
    /// is less than or equal to zero, <paramref name="maxRentArrayLength"/> is greater than <see cref="MaxPooledArrayLength"/>,
    /// or <paramref name="minRentArrayLength"/> is greater than <paramref name="maxRentArrayLength"/>.
    /// </exception>
    public static void InitializeArrayPool<T>(ArrayPool<T> arrayPool, int maxRentArrayLength, int maxArrayInstancesPerBucket, int minRentArrayLength = 1)
    {
        ArgumentNullException.ThrowIfNull(arrayPool);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRentArrayLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxRentArrayLength, MaxPooledArrayLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxArrayInstancesPerBucket);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minRentArrayLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minRentArrayLength, maxRentArrayLength);

        int maxBucketIndex = SelectBucketIndex(maxRentArrayLength);
        var rented = new T[maxArrayInstancesPerBucket][];
        for (int bucketIndex = SelectBucketIndex(minRentArrayLength); bucketIndex <= maxBucketIndex; bucketIndex++)
        {
            int bucketSize = GetMaxSizeForBucket(bucketIndex);
            int rentedCount = 0;
            try
            {
                for (; rentedCount < rented.Length; rentedCount++)
                {
                    rented[rentedCount] = arrayPool.Rent(bucketSize);
                }
            }
            finally
            {
                for (int j = 0; j < rentedCount; j++)
                {
                    arrayPool.Return(rented[j]);
                    rented[j] = null!;
                }
            }
        }
    }

    /// <summary>
    /// Gets the index of the pool bucket that serves arrays of the specified size.
    /// Bucket 0 holds arrays of length 16, and each subsequent bucket doubles the length.
    /// </summary>
    /// <param name="bufferSize">The requested array length.</param>
    /// <returns>The zero-based bucket index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bufferSize"/> is less than or equal to zero, or greater than <see cref="MaxPooledArrayLength"/>.
    /// </exception>
    public static int SelectBucketIndex(int bufferSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bufferSize, MaxPooledArrayLength);
        uint bits = ((uint)bufferSize - 1) >> 4;
        return 32 - BitOperations.LeadingZeroCount(bits);
    }

    /// <summary>
    /// Gets the array length held by the specified pool bucket.
    /// </summary>
    /// <param name="bucketIndex">The zero-based bucket index.</param>
    /// <returns>The array length for the bucket (<c>16 &lt;&lt; bucketIndex</c>).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bucketIndex"/> is negative, or greater than <see cref="MaxBucketIndex"/>.
    /// </exception>
    public static int GetMaxSizeForBucket(int bucketIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bucketIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bucketIndex, MaxBucketIndex);
        return 16 << bucketIndex;
    }
}
