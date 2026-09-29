// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).
//           Switched to BenchmarkSwitcher.

using BenchmarkDotNet.Running;
using Continuum.Benchmarks;

var summary = BenchmarkSwitcher.FromAssembly(typeof(Benchmarker).Assembly).Run(args);
