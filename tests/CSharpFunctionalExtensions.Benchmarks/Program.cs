using BenchmarkDotNet.Running;

using Continuum.CSharpFunctionalExtensions.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(RequestErrorConstructionBenchmarks).Assembly).Run(args);
