using System.Net;

using BenchmarkDotNet.Attributes;

using GrpcStatusCodeEnum = Grpc.Core.StatusCode;

namespace Continuum.CSharpFunctionalExtensions.Benchmarks;

[MemoryDiagnoser]
public class RequestErrorConstructionBenchmarks
{
    private const int IdCount = 1024;
    private const int UniqueFormatCount = 4096;

    private static readonly ErrorArgument[] NoArgs = [];
    private static readonly ErrorArgument[] OneArg = [ErrorArgument.From("order-42")];
    private static readonly ErrorArgument[] ThreeArgs = [ErrorArgument.From("order-42"), ErrorArgument.From(17L), ErrorArgument.From(3.5d)];

    private ErrorArgument[][] _idArguments = [];
    private string[] _distinctFormats = [];
    private string[] _uniqueFormats = [];
    private int _iteration;

    [GlobalSetup]
    public void Setup()
    {
        _idArguments = new ErrorArgument[IdCount][];
        _distinctFormats = new string[IdCount];
        for (var i = 0; i < IdCount; i++)
        {
            _idArguments[i] = [ErrorArgument.From(Guid.NewGuid())];
            _distinctFormats[i] = "Order {0} failed (variant " + i + ").";
        }
    }

    [IterationSetup(Target = nameof(UniqueFormat_EveryCall))]
    public void UniqueFormatSetup()
    {
        _iteration++;
        _uniqueFormats = new string[UniqueFormatCount];
        for (var i = 0; i < UniqueFormatCount; i++)
        {
            _uniqueFormats[i] = "Order {0} failed (iter " + _iteration + ", n " + i + ").";
        }
    }

    [Benchmark(Baseline = true)]
    public RequestError NullFormat_NoArgs() =>
        new(HttpStatusCode.NotFound, GrpcStatusCodeEnum.NotFound, "not_found");

    [Benchmark]
    public RequestError SameFormat_ZeroPlaceholders() =>
        new(HttpStatusCode.NotFound, GrpcStatusCodeEnum.NotFound, "not_found", "The order was not found.", NoArgs);

    [Benchmark]
    public RequestError SameFormat_OnePlaceholder() =>
        new(HttpStatusCode.NotFound, GrpcStatusCodeEnum.NotFound, "not_found", "Order {0} was not found.", OneArg);

    [Benchmark]
    public RequestError SameFormat_ThreePlaceholders() =>
        new(HttpStatusCode.BadRequest, GrpcStatusCodeEnum.InvalidArgument, "invalid", "Order {0} line {1} has invalid quantity {2:F2}.", ThreeArgs);

    [Benchmark(OperationsPerInvoke = IdCount)]
    public RequestError SameFormat_DistinctIds()
    {
        RequestError last = null!;
        var ids = _idArguments;
        for (var i = 0; i < ids.Length; i++)
        {
            last = new(HttpStatusCode.NotFound, GrpcStatusCodeEnum.NotFound, "not_found", "Order {0} was not found.", ids[i]);
        }
        return last;
    }

    [Benchmark(OperationsPerInvoke = IdCount)]
    public RequestError DistinctFormats_1024_Cycled()
    {
        RequestError last = null!;
        var formats = _distinctFormats;
        for (var i = 0; i < formats.Length; i++)
        {
            last = new(HttpStatusCode.NotFound, GrpcStatusCodeEnum.NotFound, "not_found", formats[i], OneArg);
        }
        return last;
    }

    [Benchmark(OperationsPerInvoke = UniqueFormatCount)]
    public RequestError UniqueFormat_EveryCall()
    {
        RequestError last = null!;
        var formats = _uniqueFormats;
        for (var i = 0; i < formats.Length; i++)
        {
            last = new(HttpStatusCode.NotFound, GrpcStatusCodeEnum.NotFound, "not_found", formats[i], OneArg);
        }
        return last;
    }
}
