using BenchmarkDotNet.Running;

using Continuum.Core.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(TypeMapLookupBenchmarks).Assembly).Run(args);
