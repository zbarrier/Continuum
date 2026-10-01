using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Continuum.CSharpFunctionalExtensions.SourceGenerators;

/// <summary>
///     Checks the <c>format</c> argument of Continuum.CSharpFunctionalExtensions APIs whose <c>format</c> parameter is marked
///     <c>[StringSyntax(StringSyntaxAttribute.CompositeFormat)]</c>, such as the <c>RequestError</c> constructors, the
///     <c>RequestErrors</c> factories and <c>ValidationErrorEntry</c>.
///     CFE005 reports constant formats that would throw <c>FormatException</c> at runtime; CFE006 warns about formats built per
///     call (interpolation or concatenation), which should pass values such as identifiers as arguments instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorFormatAnalyzer : DiagnosticAnalyzer
{
    private const string Namespace = "Continuum.CSharpFunctionalExtensions";

    internal static readonly DiagnosticDescriptor InvalidFormat = new(
        id: "CFE005",
        title: "Invalid error format string",
        messageFormat: "The format string is invalid: {0}",
        category: "Continuum.CSharpFunctionalExtensions",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The composite format would throw FormatException, either when the error is constructed (RequestError) or when its message is formatted.");

    internal static readonly DiagnosticDescriptor NonConstantFormat = new(
        id: "CFE006",
        title: "Error format string is built per call",
        messageFormat: "Pass values as format arguments (for example \"Order {{0}} was not found.\", orderId) instead of building the format string",
        category: "Continuum.CSharpFunctionalExtensions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Formats built with interpolation or concatenation lose structured arguments, bypass localization and defeat the format validation cache.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(InvalidFormat, NonConstantFormat);

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
        var containingNamespace = type?.ContainingNamespace?.ToDisplayString();
        if (type is null || containingNamespace is null ||
            (containingNamespace != Namespace && !containingNamespace.StartsWith(Namespace + ".", System.StringComparison.Ordinal)))
        {
            return;
        }

        IArgumentOperation? format = null, formatArguments = null;
        foreach (var argument in arguments)
        {
            switch (argument.Parameter?.Name)
            {
                case "format": format = argument; break;
                case "arguments": formatArguments = argument; break;
            }
        }

        if (format is null || format.ArgumentKind == ArgumentKind.DefaultValue || !IsCompositeFormat(format.Parameter))
        {
            return;
        }

        var value = Unwrap(format.Value);
        if (value.ConstantValue is { HasValue: true, Value: string text })
        {
            var argumentCount = GetArgumentCount(formatArguments);
            var error = CompositeFormatChecker.Check(text, argumentCount);
            if (error is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidFormat, format.Syntax.GetLocation(), error));
            }
            return;
        }

        if (value is IInterpolatedStringOperation || (value is IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } &&
            value.Type?.SpecialType == SpecialType.System_String))
        {
            context.ReportDiagnostic(Diagnostic.Create(NonConstantFormat, format.Syntax.GetLocation()));
        }
    }

    private static bool IsCompositeFormat(IParameterSymbol? parameter)
    {
        if (parameter is null)
        {
            return false;
        }

        foreach (var attribute in parameter.GetAttributes())
        {
            if (attribute.AttributeClass is { Name: "StringSyntaxAttribute" } &&
                attribute.AttributeClass.ContainingNamespace?.ToDisplayString() == "System.Diagnostics.CodeAnalysis" &&
                attribute.ConstructorArguments.Length > 0 &&
                attribute.ConstructorArguments[0].Value is "CompositeFormat")
            {
                return true;
            }
        }
        return false;
    }

    private static int? GetArgumentCount(IArgumentOperation? argument)
    {
        if (argument is null || argument.ArgumentKind == ArgumentKind.DefaultValue)
        {
            return argument is null ? null : 0;
        }

        var value = Unwrap(argument.Value);
        if (value.ConstantValue is { HasValue: true, Value: null } || value is IDefaultValueOperation)
        {
            return 0;
        }

        if (value is IArrayCreationOperation creation)
        {
            if (creation.Initializer is { } initializer)
            {
                return initializer.ElementValues.Length;
            }
            return creation.DimensionSizes.Length == 1 && creation.DimensionSizes[0].ConstantValue is { HasValue: true, Value: int size }
                ? size
                : null;
        }

        if (value is ICollectionExpressionOperation collection)
        {
            foreach (var element in collection.Elements)
            {
                if (element is ISpreadOperation)
                {
                    return null;
                }
            }
            return collection.Elements.Length;
        }

        return null;
    }

    private static IOperation Unwrap(IOperation value)
    {
        while (value is IConversionOperation conversion)
        {
            value = conversion.Operand;
        }
        return value;
    }
}

/// <summary>Parses composite format strings using the same rules as <c>string.Format</c>.</summary>
internal static class CompositeFormatChecker
{
    private const int MaxIndex = 1_000_000;

    /// <summary>Returns a description of the problem, or null if the format is valid for <paramref name="argumentCount"/> arguments.</summary>
    /// <param name="format">The composite format string.</param>
    /// <param name="argumentCount">The number of arguments, or null if unknown (only syntax is checked).</param>
    public static string? Check(string format, int? argumentCount)
    {
        var i = 0;
        while (i < format.Length)
        {
            var c = format[i++];
            if (c == '}')
            {
                if (i < format.Length && format[i] == '}')
                {
                    i++;
                    continue;
                }
                return $"unexpected '}}' at position {i - 1}; use '}}}}' for a literal brace";
            }

            if (c != '{')
            {
                continue;
            }

            if (i < format.Length && format[i] == '{')
            {
                i++;
                continue;
            }

            var start = i - 1;
            if (i >= format.Length || !IsDigit(format[i]))
            {
                return $"the placeholder at position {start} must start with an argument index; use '{{{{' for a literal brace";
            }

            var index = 0;
            while (i < format.Length && IsDigit(format[i]))
            {
                index = index * 10 + (format[i++] - '0');
                if (index >= MaxIndex)
                {
                    return $"the argument index at position {start} is too large";
                }
            }

            SkipSpaces(format, ref i);

            if (i < format.Length && format[i] == ',')
            {
                i++;
                SkipSpaces(format, ref i);
                if (i < format.Length && format[i] == '-')
                {
                    i++;
                }
                if (i >= format.Length || !IsDigit(format[i]))
                {
                    return $"the alignment of the placeholder at position {start} must be an integer";
                }
                while (i < format.Length && IsDigit(format[i]))
                {
                    i++;
                }
                SkipSpaces(format, ref i);
            }

            if (i < format.Length && format[i] == ':')
            {
                i++;
                while (i < format.Length && format[i] != '}')
                {
                    if (format[i] == '{')
                    {
                        return $"the format specifier of the placeholder at position {start} cannot contain '{{'";
                    }
                    i++;
                }
            }

            if (i >= format.Length || format[i] != '}')
            {
                return $"the placeholder at position {start} is not closed with '}}'";
            }
            i++;

            if (argumentCount is { } count && index >= count)
            {
                return count == 0
                    ? $"placeholder {{{index}}} has no matching argument (no arguments were passed)"
                    : $"placeholder {{{index}}} has no matching argument (only {count} argument{(count == 1 ? " was" : "s were")} passed)";
            }
        }

        return null;
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static void SkipSpaces(string format, ref int i)
    {
        while (i < format.Length && format[i] == ' ')
        {
            i++;
        }
    }
}
