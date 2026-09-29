// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).
//           Added conversion benchmarks and SIMD vs scalar jobs.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Continuum.Benchmarks;

[Config(typeof(Config))]
public class Benchmarker
{
	class Config : ManualConfig
	{
		public Config()
		{
			var baseJob = Job.Default.WithRuntime(CoreRuntime.Core10_0).WithGcServer(true).WithGcForce(true);

			AddJob(baseJob.WithId("SIMD"));
			AddJob(baseJob.WithId("Scalar").WithEnvironmentVariable("DOTNET_EnableHWIntrinsic", "0"));
			AddDiagnoser(MemoryDiagnoser.Default);
		}
	}

	readonly NewId _id = NewId.Next();
	readonly Guid _guid = NewId.NextGuid();
	readonly Guid _sequentialGuid = NewId.NextSequentialGuid();

	[Benchmark(Baseline = true, Description = "Next")]
	public NewId GetNext()
	{
		return NewId.Next();
	}

	[Benchmark(Description = "Next(batch)", OperationsPerInvoke = 100)]
	public NewId[] GetNextBatch()
	{
		return NewId.Next(100);
	}

	[Benchmark(Description = "NextGuid")]
	public Guid GetNextGuid()
	{
		return NewId.NextGuid();
	}

	[Benchmark(Description = "NextSequentialGuid")]
	public Guid GetNextSequentialGuid()
	{
		return NewId.NextSequentialGuid();
	}

	[Benchmark(Description = "ToGuid")]
	public Guid ToGuid()
	{
		return _id.ToGuid();
	}

	[Benchmark(Description = "ToSequentialGuid")]
	public Guid ToSequentialGuid()
	{
		return _id.ToSequentialGuid();
	}

	[Benchmark(Description = "FromGuid")]
	public NewId FromGuid()
	{
		return NewId.FromGuid(_guid);
	}

	[Benchmark(Description = "FromSequentialGuid")]
	public NewId FromSequentialGuid()
	{
		return NewId.FromSequentialGuid(_sequentialGuid);
	}

	[Benchmark(Description = "ToByteArray")]
	public byte[] ToByteArray()
	{
		return _id.ToByteArray();
	}
}
