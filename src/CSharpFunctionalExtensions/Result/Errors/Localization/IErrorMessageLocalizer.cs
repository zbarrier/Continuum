#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Resolves localized message templates for error codes and localized display names for properties.
/// </summary>
public interface IErrorMessageLocalizer
{
    /// <summary>
    ///     Gets the message template for <paramref name="code"/> in <paramref name="culture"/>,
    ///     falling back to parent cultures and then English.
    /// </summary>
    bool TryGetTemplate(string code, CultureInfo culture, out string template);

    /// <summary>
    ///     Gets the display name for <paramref name="propertyName"/> in <paramref name="culture"/>.
    ///     Returns <paramref name="propertyName"/> when no display name is available.
    /// </summary>
    string GetDisplayName(string propertyName, CultureInfo culture);
}
