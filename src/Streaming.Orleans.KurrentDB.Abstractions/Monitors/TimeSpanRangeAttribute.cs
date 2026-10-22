using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Continuum.Streaming.Orleans.KurrentDB.Monitors;

/// <summary>
///     Validates that a <see cref="TimeSpan" /> lies within an inclusive range.
/// </summary>
/// <remarks>
///     <see cref="RangeAttribute" /> with a <see cref="Type" /> operand relies on reflection-based type conversion that
///     is not trim or Native AOT safe, so <see cref="TimeSpan" /> ranges are validated with this attribute instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
internal sealed class TimeSpanRangeAttribute : ValidationAttribute
{
    /// <summary>
    ///     Creates a range from two <see cref="TimeSpan" /> strings in invariant constant ("c") format.
    /// </summary>
    public TimeSpanRangeAttribute(string minimum, string maximum)
        : base("The field {0} must be between {1} and {2}.")
    {
        Minimum = TimeSpan.ParseExact(minimum, "c", CultureInfo.InvariantCulture);
        Maximum = TimeSpan.ParseExact(maximum, "c", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     The inclusive lower bound.
    /// </summary>
    public TimeSpan Minimum { get; }

    /// <summary>
    ///     The inclusive upper bound.
    /// </summary>
    public TimeSpan Maximum { get; }

    /// <inheritdoc />
    public override bool IsValid(object? value) => value is not TimeSpan timeSpan || (timeSpan >= Minimum && timeSpan <= Maximum);

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Minimum, Maximum);
}
