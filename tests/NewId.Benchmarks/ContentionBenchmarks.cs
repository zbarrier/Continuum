using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Continuum.Benchmarks;

[Config(typeof(Config))]
public class ContentionBenchmarks
{
    const int IdsPerThread = 10_000;

    class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core10_0).WithGcServer(true));
            AddDiagnoser(MemoryDiagnoser.Default);
            AddDiagnoser(ThreadingDiagnoser.Default);
        }
    }

    [Params(1, 4, 16)]
    public int Threads { get; set; }

    Thread[] _threads = [];
    Barrier _start = null!;
    Barrier _done = null!;
    volatile bool _stop;

    [GlobalSetup]
    public void Setup()
    {
        _stop = false;
        _start = new Barrier(Threads + 1);
        _done = new Barrier(Threads + 1);
        _threads = new Thread[Threads];

        for (var i = 0; i < Threads; i++)
        {
            _threads[i] = new Thread(Worker) { IsBackground = true };
            _threads[i].Start();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _stop = true;
        _start.SignalAndWait();
        foreach (var thread in _threads)
            thread.Join();
    }

    void Worker()
    {
        while (true)
        {
            _start.SignalAndWait();
            if (_stop)
                return;

            for (var i = 0; i < IdsPerThread; i++)
                NewId.Next();

            _done.SignalAndWait();
        }
    }

    [Benchmark(Description = "Next (contended)", OperationsPerInvoke = IdsPerThread)]
    public void NextContended()
    {
        _start.SignalAndWait();
        _done.SignalAndWait();
    }
}
