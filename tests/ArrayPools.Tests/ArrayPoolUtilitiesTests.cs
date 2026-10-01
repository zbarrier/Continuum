using System.Buffers;

namespace Continuum.ArrayPools.Tests;

public sealed class ArrayPoolUtilitiesTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(16, 0)]
    [InlineData(17, 1)]
    [InlineData(32, 1)]
    [InlineData(33, 2)]
    [InlineData(1024, 6)]
    [InlineData(1025, 7)]
    [InlineData(ArrayPoolUtilities.MaxPooledArrayLength - 1, ArrayPoolUtilities.MaxBucketIndex)]
    [InlineData(ArrayPoolUtilities.MaxPooledArrayLength, ArrayPoolUtilities.MaxBucketIndex)]
    public void SelectBucketIndex_returns_the_bucket_for_the_size(int bufferSize, int expected)
    {
        Assert.Equal(expected, ArrayPoolUtilities.SelectBucketIndex(bufferSize));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(ArrayPoolUtilities.MaxPooledArrayLength + 1)]
    [InlineData(int.MaxValue)]
    public void SelectBucketIndex_throws_for_out_of_range_sizes(int bufferSize)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPoolUtilities.SelectBucketIndex(bufferSize));
        Assert.Equal("bufferSize", ex.ParamName);
    }

    [Theory]
    [InlineData(0, 16)]
    [InlineData(1, 32)]
    [InlineData(6, 1024)]
    [InlineData(ArrayPoolUtilities.MaxBucketIndex, ArrayPoolUtilities.MaxPooledArrayLength)]
    public void GetMaxSizeForBucket_returns_the_size_for_the_bucket(int bucketIndex, int expected)
    {
        Assert.Equal(expected, ArrayPoolUtilities.GetMaxSizeForBucket(bucketIndex));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(ArrayPoolUtilities.MaxBucketIndex + 1)]
    [InlineData(int.MaxValue)]
    public void GetMaxSizeForBucket_throws_for_out_of_range_indexes(int bucketIndex)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPoolUtilities.GetMaxSizeForBucket(bucketIndex));
        Assert.Equal("bucketIndex", ex.ParamName);
    }

    [Fact]
    public void GetMaxSizeForBucket_round_trips_with_SelectBucketIndex()
    {
        for (int i = 0; i <= ArrayPoolUtilities.MaxBucketIndex; i++)
        {
            Assert.Equal(i, ArrayPoolUtilities.SelectBucketIndex(ArrayPoolUtilities.GetMaxSizeForBucket(i)));
        }
    }

    [Theory]
    [InlineData(0, 1, "maxArrayLength")]
    [InlineData(-1, 1, "maxArrayLength")]
    [InlineData(ArrayPoolUtilities.MaxPooledArrayLength + 1, 1, "maxArrayLength")]
    [InlineData(16, 0, "maxArraysPerBucket")]
    [InlineData(16, -1, "maxArraysPerBucket")]
    public void CreatePreWarmed_throws_for_out_of_range_arguments(int maxArrayLength, int maxArraysPerBucket, string paramName)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ArrayPoolUtilities.CreatePreWarmed<byte>(maxArrayLength, maxArraysPerBucket));
        Assert.Equal(paramName, ex.ParamName);
    }

    [Fact]
    public void CreatePreWarmed_returns_a_pool_whose_rentals_reuse_pre_warmed_arrays()
    {
        const int instances = 4;
        var pool = ArrayPoolUtilities.CreatePreWarmed<byte>(maxArrayLength: 1024, maxArraysPerBucket: instances);

        foreach (int size in new[] { 16, 100, 1024 })
        {
            var first = new byte[instances][];
            for (int i = 0; i < instances; i++)
            {
                first[i] = pool.Rent(size);
            }

            foreach (var array in first)
            {
                pool.Return(array);
            }

            var second = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (int i = 0; i < instances; i++)
            {
                second.Add(pool.Rent(size));
            }

            Assert.All(first, array => Assert.Contains(array, second));
            foreach (byte[] array in second)
            {
                pool.Return(array);
            }
        }
    }

    [Fact]
    public void CreatePreWarmed_fills_every_bucket_up_to_the_maximum_length()
    {
        const int instances = 2;
        var pool = ArrayPoolUtilities.CreatePreWarmed<byte>(maxArrayLength: 100, maxArraysPerBucket: instances);

        for (int bucketIndex = 0; bucketIndex <= ArrayPoolUtilities.SelectBucketIndex(100); bucketIndex++)
        {
            int size = ArrayPoolUtilities.GetMaxSizeForBucket(bucketIndex);
            var rented = new byte[instances][];
            for (int i = 0; i < instances; i++)
            {
                rented[i] = pool.Rent(size);
                Assert.Equal(size, rented[i].Length);
            }

            foreach (var array in rented)
            {
                pool.Return(array);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void CreatePreWarmed_throws_for_an_out_of_range_minimum_length(int minRentArrayLength)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ArrayPoolUtilities.CreatePreWarmed<byte>(100, 1, minRentArrayLength));
        Assert.Equal("minRentArrayLength", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void InitializeArrayPool_throws_for_an_out_of_range_minimum_length(int minRentArrayLength)
    {
        var pool = new RecordingArrayPool<byte>(ArrayPool<byte>.Create());

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ArrayPoolUtilities.InitializeArrayPool(pool, 100, 1, minRentArrayLength));

        Assert.Equal("minRentArrayLength", ex.ParamName);
        Assert.Empty(pool.Rented);
    }

    [Theory]
    [InlineData(17, new[] { 32, 32, 64, 64, 128, 128 })]
    [InlineData(33, new[] { 64, 64, 128, 128 })]
    [InlineData(64, new[] { 64, 64, 128, 128 })]
    [InlineData(100, new[] { 128, 128 })]
    public void InitializeArrayPool_skips_buckets_below_the_minimum_length(int minRentArrayLength, int[] expected)
    {
        var pool = new RecordingArrayPool<byte>(ArrayPool<byte>.Create());

        ArrayPoolUtilities.InitializeArrayPool(pool, maxRentArrayLength: 100, maxArrayInstancesPerBucket: 2, minRentArrayLength);

        Assert.Equal(expected, pool.RentedLengths);
    }

    [Fact]
    public void InitializeArrayPool_throws_for_a_null_pool()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => ArrayPoolUtilities.InitializeArrayPool<byte>(null!, 16, 1));
        Assert.Equal("arrayPool", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(ArrayPoolUtilities.MaxPooledArrayLength + 1)]
    public void InitializeArrayPool_throws_for_an_out_of_range_length(int maxRentArrayLength)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ArrayPoolUtilities.InitializeArrayPool(ArrayPool<byte>.Create(), maxRentArrayLength, 1));
        Assert.Equal("maxRentArrayLength", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InitializeArrayPool_throws_for_an_out_of_range_instance_count(int maxArrayInstancesPerBucket)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ArrayPoolUtilities.InitializeArrayPool(ArrayPool<byte>.Create(), 16, maxArrayInstancesPerBucket));
        Assert.Equal("maxArrayInstancesPerBucket", ex.ParamName);
    }

    [Fact]
    public void InitializeArrayPool_does_not_touch_the_pool_when_arguments_are_invalid()
    {
        var pool = new RecordingArrayPool<byte>(ArrayPool<byte>.Create());

        Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPoolUtilities.InitializeArrayPool(pool, 0, 1));

        Assert.Empty(pool.Rented);
        Assert.Empty(pool.Returned);
    }

    [Fact]
    public void InitializeArrayPool_rents_each_bucket_size_the_requested_number_of_times()
    {
        var pool = new RecordingArrayPool<byte>(ArrayPool<byte>.Create());

        ArrayPoolUtilities.InitializeArrayPool(pool, maxRentArrayLength: 100, maxArrayInstancesPerBucket: 3);

        int[] expected = [16, 16, 16, 32, 32, 32, 64, 64, 64, 128, 128, 128];
        Assert.Equal(expected, pool.RentedLengths);
    }

    [Fact]
    public void InitializeArrayPool_returns_every_rented_array()
    {
        var pool = new RecordingArrayPool<byte>(ArrayPool<byte>.Create());

        ArrayPoolUtilities.InitializeArrayPool(pool, maxRentArrayLength: 1024, maxArrayInstancesPerBucket: 4);

        Assert.Equal(pool.Rented.Count, pool.Returned.Count);
        Assert.Equal(pool.Rented.ToHashSet(ReferenceEqualityComparer.Instance), pool.Returned.ToHashSet(ReferenceEqualityComparer.Instance));
    }

    [Fact]
    public void InitializeArrayPool_makes_later_rentals_reuse_the_pre_warmed_arrays()
    {
        const int instances = 4;
        var inner = ArrayPool<byte>.Create(maxArrayLength: 1024, maxArraysPerBucket: instances);
        var pool = new RecordingArrayPool<byte>(inner);

        ArrayPoolUtilities.InitializeArrayPool(pool, maxRentArrayLength: 1024, maxArrayInstancesPerBucket: instances);

        var prewarmed = pool.Returned.ToHashSet(ReferenceEqualityComparer.Instance);
        foreach (int size in new[] { 16, 100, 1024 })
        {
            var rentedLater = new byte[instances][];
            for (int i = 0; i < instances; i++)
            {
                rentedLater[i] = inner.Rent(size);
                Assert.Contains(rentedLater[i], prewarmed);
            }

            foreach (var array in rentedLater)
            {
                inner.Return(array);
            }
        }
    }

    [Fact]
    public void InitializeArrayPool_returns_already_rented_arrays_when_a_rent_fails()
    {
        // Third rent fails: two arrays from bucket 0 are outstanding at that point.
        var pool = new RecordingArrayPool<byte>(ArrayPool<byte>.Create(), failOnRentNumber: 3);

        Assert.Throws<OutOfMemoryException>(
            () => ArrayPoolUtilities.InitializeArrayPool(pool, maxRentArrayLength: 64, maxArrayInstancesPerBucket: 4));

        Assert.Equal(2, pool.Rented.Count);
        Assert.Equal(pool.Rented, pool.Returned);
    }

    [Fact]
    public void InitializeArrayPool_returns_arrays_from_earlier_buckets_when_a_later_bucket_fails()
    {
        // Bucket 0 (2 rents) completes, bucket 1 fails on its second rent.
        var pool = new RecordingArrayPool<byte>(ArrayPool<byte>.Create(), failOnRentNumber: 4);

        Assert.Throws<OutOfMemoryException>(
            () => ArrayPoolUtilities.InitializeArrayPool(pool, maxRentArrayLength: 64, maxArrayInstancesPerBucket: 2));

        Assert.Equal(3, pool.Rented.Count);
        Assert.Equal(pool.Rented, pool.Returned);
    }
}
