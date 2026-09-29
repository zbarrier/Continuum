using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Continuum.CSharpFunctionalExtensions.SourceGenerators;

/// <summary>
///     Checks constant HTTP/gRPC status codes passed to <c>RequestError</c> constructors and <c>RequestErrors</c> factories.
///     CFE002 warns about pairs outside the recommended mapping; CFE003 warns when <c>retryAfter</c> is set for a
///     status that is not retryable.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RequestErrorStatusAnalyzer : DiagnosticAnalyzer
{
    private const string Namespace = "Continuum.CSharpFunctionalExtensions";

    internal static readonly DiagnosticDescriptor UnusualStatusPair = new(
        id: "CFE002",
        title: "Unusual HTTP/gRPC status code pair",
        messageFormat: "HTTP {0} is not a recommended pair for gRPC {1}; recommended HTTP codes: {2}",
        category: "Continuum.CSharpFunctionalExtensions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "RequestError HTTP and gRPC status codes should follow the recommended mapping documented on RequestError.");

    internal static readonly DiagnosticDescriptor RetryAfterNotRetryable = new(
        id: "CFE003",
        title: "RetryAfter set for a non-retryable status",
        messageFormat: "RetryAfter is only meaningful for retryable gRPC statuses (Unavailable, ResourceExhausted, Aborted, DeadlineExceeded), not {0}",
        category: "Continuum.CSharpFunctionalExtensions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Callers should not retry non-retryable errors, so a RetryAfter hint is misleading.");

    private static readonly string[] GrpcNames =
    [
        "OK", "Cancelled", "Unknown", "InvalidArgument", "DeadlineExceeded", "NotFound", "AlreadyExists",
        "PermissionDenied", "ResourceExhausted", "FailedPrecondition", "Aborted", "OutOfRange", "Unimplemented",
        "Internal", "Unavailable", "DataLoss", "Unauthenticated",
    ];

    private static readonly Dictionary<int, int[]> RecommendedHttp = new()
    {
        [1] = [499],
        [2] = [500],
        [3] = [400, 413, 414, 415, 422, 431],
        [4] = [408, 504],
        [5] = [404, 410],
        [6] = [409],
        [7] = [403],
        [8] = [429, 507],
        [9] = [400, 412, 428],
        [10] = [409],
        [11] = [400, 416],
        [12] = [405, 501],
        [13] = [500],
        [14] = [502, 503],
        [15] = [500],
        [16] = [401],
    };

    private static readonly HashSet<int> Retryable = [4, 8, 10, 14];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(UnusualStatusPair, RetryAfterNotRetryable);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(
            ctx => Analyze(ctx, ((IObjectCreationOperation)ctx.Operation).Constructor, ((IObjectCreationOperation)ctx.Operation).Arguments),
            OperationKind.ObjectCreation);
        context.RegisterOperationAction(
            ctx => Analyze(ctx, ((IInvocationOperation)ctx.Operation).TargetMethod, ((IInvocationOperation)ctx.Operation).Arguments),
            OperationKind.Invocation);
    }

    private static void Analyze(OperationAnalysisContext context, IMethodSymbol? method, ImmutableArray<IArgumentOperation> arguments)
    {
        var type = method?.ContainingType;
        if (type is null || type.ContainingNamespace?.ToDisplayString() != Namespace ||
            (type.Name != "RequestError" && type.Name != "RequestErrors"))
        {
            return;
        }

        int? http = null, grpc = null;
        IArgumentOperation? retryAfter = null;
        foreach (var argument in arguments)
        {
            switch (argument.Parameter?.Name)
            {
                case "httpStatusCode": http = GetInt(argument.Value); break;
                case "grpcStatusCode": grpc = GetInt(argument.Value); break;
                case "retryAfter": retryAfter = argument; break;
            }
        }

        if (grpc is not { } g || g < 0 || g >= GrpcNames.Length)
        {
            return;
        }

        if (http is { } h && RecommendedHttp.TryGetValue(g, out var allowed) && System.Array.IndexOf(allowed, h) < 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(UnusualStatusPair, context.Operation.Syntax.GetLocation(),
                h, GrpcNames[g], string.Join(", ", allowed)));
        }

        if (retryAfter is not null && retryAfter.ArgumentKind != ArgumentKind.DefaultValue && !IsNull(retryAfter.Value) &&
            !Retryable.Contains(g))
        {
            context.ReportDiagnostic(Diagnostic.Create(RetryAfterNotRetryable, retryAfter.Syntax.GetLocation(), GrpcNames[g]));
        }
    }

    private static int? GetInt(IOperation value)
    {
        var constant = value.ConstantValue;
        if (!constant.HasValue || constant.Value is null)
        {
            return null;
        }
        try
        {
            return System.Convert.ToInt32(constant.Value, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    private static bool IsNull(IOperation value)
    {
        while (value is IConversionOperation conversion)
        {
            value = conversion.Operand;
        }
        return value.ConstantValue is { HasValue: true, Value: null } || value is IDefaultValueOperation;
    }
}
