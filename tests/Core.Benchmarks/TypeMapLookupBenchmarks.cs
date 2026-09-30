using System.Collections.Frozen;
using System.Reflection.Emit;

using BenchmarkDotNet.Attributes;

namespace Continuum.Core.Benchmarks;

/// <summary>
/// Compares <see cref="Dictionary{TKey, TValue}"/> and <see cref="FrozenDictionary{TKey, TValue}"/> for the
/// Type-to-name and name-to-Type lookups performed on every event read and write.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class TypeMapLookupBenchmarks
{
    private Dictionary<Type, string> _typeMap = null!;
    private Dictionary<string, Type> _nameMap = null!;
    private FrozenDictionary<Type, string> _frozenTypeMap = null!;
    private FrozenDictionary<string, Type> _frozenNameMap = null!;

    private Type[] _lookupTypes = null!;
    private string[] _lookupNames = null!;

    [Params(10, 100, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var module = AssemblyBuilder.DefineDynamicAssembly(new("TypeMapBench"), AssemblyBuilderAccess.Run).DefineDynamicModule("m");
        var types = Enumerable.Range(0, Count).Select(i => module.DefineType($"Bench.Event{i}").CreateType()).ToArray();

        _typeMap = types.ToDictionary(t => t, t => $"bench.{t.Name.ToLowerInvariant()}-v1");
        _nameMap = _typeMap.ToDictionary(kv => kv.Value, kv => kv.Key);
        _frozenTypeMap = _typeMap.ToFrozenDictionary();
        _frozenNameMap = _nameMap.ToFrozenDictionary();

        var random = new Random(42);
        _lookupTypes = Enumerable.Range(0, 1024).Select(_ => types[random.Next(Count)]).ToArray();
        // Fresh string instances so lookups hash and compare rather than hit reference equality.
        _lookupNames = _lookupTypes.Select(t => new string(_typeMap[t].AsSpan())).ToArray();
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = 1024)]
    [BenchmarkCategory("TypeToName")]
    public int TypeToName_Dictionary()
    {
        var total = 0;
        foreach (var type in _lookupTypes)
        {
            total += _typeMap[type].Length;
        }
        return total;
    }

    [Benchmark(OperationsPerInvoke = 1024)]
    [BenchmarkCategory("TypeToName")]
    public int TypeToName_Frozen()
    {
        var total = 0;
        foreach (var type in _lookupTypes)
        {
            total += _frozenTypeMap[type].Length;
        }
        return total;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = 1024)]
    [BenchmarkCategory("NameToType")]
    public int NameToType_Dictionary()
    {
        var total = 0;
        foreach (var name in _lookupNames)
        {
            total += _nameMap[name].Name.Length;
        }
        return total;
    }

    [Benchmark(OperationsPerInvoke = 1024)]
    [BenchmarkCategory("NameToType")]
    public int NameToType_Frozen()
    {
        var total = 0;
        foreach (var name in _lookupNames)
        {
            total += _frozenNameMap[name].Name.Length;
        }
        return total;
    }
}
