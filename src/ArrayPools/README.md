# Continuum.ArrayPools

Utilities for working with `System.Buffers.ArrayPool<T>`.

## Features

- `ArrayPoolUtilities.CreatePreWarmed` creates a pool with `ArrayPool<T>.Create` and pre-warms it with the same limits,
  so no arrays are allocated beyond what the pool keeps. Prefer this for new pools.
- `ArrayPoolUtilities.InitializeArrayPool` pre-warms the buckets of an existing array pool so the first rentals do not allocate.
- `ArrayPoolUtilities.SelectBucketIndex` and `ArrayPoolUtilities.GetMaxSizeForBucket` map between array lengths and pool buckets.
  Bucket 0 holds arrays of length 16, and each subsequent bucket doubles the length.
- `ArrayPoolUtilities.MaxPooledArrayLength` (2^30) and `ArrayPoolUtilities.MaxBucketIndex` (26) describe the largest
  arrays the built-in pools will store.

## Limits

- `maxRentArrayLength` (`maxArrayLength` for `CreatePreWarmed`) must be between 1 and `MaxPooledArrayLength`.
- `maxArrayInstancesPerBucket` (`maxArraysPerBucket` for `CreatePreWarmed`) must be greater than 0.
- The optional `minRentArrayLength` (default 1) skips pre-warming buckets for smaller arrays; it must be between 1 and
  the maximum length.
- Invalid arguments throw `ArgumentNullException` or `ArgumentOutOfRangeException`.

## Which pools benefit

- Pools created with `ArrayPool<T>.Create(maxArrayLength, maxArraysPerBucket)` benefit fully, as long as
  `maxRentArrayLength` and `maxArrayInstancesPerBucket` do not exceed the pool's own limits.
  Arrays beyond those limits are allocated and then discarded.
- `ArrayPool<T>.Shared` stores arrays per thread and per processor core, and trims them under memory pressure,
  so pre-warming it from a single thread has limited and temporary effect.

## Memory usage

Every bucket is filled with arrays of the bucket's full length, and bucket sizes double, so pre-warming allocates roughly
`2 * maxBucketLength * sizeof(T) * maxArrayInstancesPerBucket` bytes, where `maxBucketLength` is `maxRentArrayLength`
rounded up to the next power of two (minimum 16). This can approach four times `maxRentArrayLength` when it is just above
a power of two. Choose limits that match your real workload.

## Usage

```csharp
using System.Buffers;
using Continuum.ArrayPools;

// Create and pre-warm in one call (recommended).
var pool = ArrayPoolUtilities.CreatePreWarmed<byte>(maxArrayLength: 1024 * 1024, maxArraysPerBucket: 16);

// Skip the small buckets when only large arrays are rented.
var largeOnly = ArrayPoolUtilities.CreatePreWarmed<byte>(maxArrayLength: 1024 * 1024, maxArraysPerBucket: 16, minRentArrayLength: 64 * 1024);

// Or pre-warm an existing pool; keep the values within the pool's own limits.
var existing = ArrayPool<byte>.Create(maxArrayLength: 1024 * 1024, maxArraysPerBucket: 16);
ArrayPoolUtilities.InitializeArrayPool(existing, maxRentArrayLength: 1024 * 1024, maxArrayInstancesPerBucket: 16);
```
