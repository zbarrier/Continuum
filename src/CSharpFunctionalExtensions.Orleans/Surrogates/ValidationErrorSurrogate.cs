using Orleans;

namespace Continuum.CSharpFunctionalExtensions.Orleans;

/// <summary>
///     Orleans surrogate for <see cref="ValidationError"/>.
/// </summary>
[GenerateSerializer, Immutable, Alias("Continuum.ValidationError")]
public struct ValidationErrorSurrogate
{
    /// <summary>The validation failures.</summary>
    [Id(0)] public List<ValidationErrorEntrySurrogate> Entries;
}

/// <summary>
///     Orleans surrogate for <see cref="ValidationErrorEntry"/>.
/// </summary>
[GenerateSerializer, Immutable, Alias("Continuum.ValidationErrorEntry")]
public struct ValidationErrorEntrySurrogate
{
    /// <summary>The severity of the failure.</summary>
    [Id(0)] public ValidationSeverity Severity;
    /// <summary>The property or field that failed validation.</summary>
    [Id(1)] public string Target;
    /// <summary>The machine-readable failure code.</summary>
    [Id(2)] public string Code;
    /// <summary>The composite format string, or empty to use the localized template.</summary>
    [Id(3)] public string? Format;
    /// <summary>The format arguments.</summary>
    [Id(4)] public ErrorArgument[]? Arguments;
}

/// <summary>
///     Converts between <see cref="ValidationError"/> and <see cref="ValidationErrorSurrogate"/>.
/// </summary>
[RegisterConverter]
public sealed class ValidationErrorSurrogateConverter : IConverter<ValidationError, ValidationErrorSurrogate>
{
    /// <inheritdoc/>
    public ValidationError ConvertFromSurrogate(in ValidationErrorSurrogate surrogate)
    {
        var entries = new List<ValidationErrorEntry>(surrogate.Entries.Count);
        foreach (var item in surrogate.Entries)
        {
            entries.Add(new ValidationErrorEntry(item.Severity, item.Target, item.Code, item.Format, item.Arguments));
        }

        return new ValidationError(entries);
    }

    /// <inheritdoc/>
    public ValidationErrorSurrogate ConvertToSurrogate(in ValidationError value)
    {
        var entries = new List<ValidationErrorEntrySurrogate>(value.Entries.Count);
        foreach (var item in value.Entries)
        {
            entries.Add(new ValidationErrorEntrySurrogate
            {
                Severity = item.Severity,
                Target = item.Target,
                Code = item.Code,
                Format = item.Format,
                Arguments = item.Arguments,
            });
        }

        return new ValidationErrorSurrogate { Entries = entries };
    }
}
