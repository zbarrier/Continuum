#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

internal static class LocalizedMessageFormatter
{
    /// <summary>
    ///     Returns true when <paramref name="format"/> is not customized, meaning it is empty or equal
    ///     to the English default for <paramref name="code"/>, so the catalog template may replace it.
    ///     Add-on packages make their codes eligible via <see cref="ErrorLocalization.RegisterDefaultTemplate"/>.
    /// </summary>
    public static bool IsDefaultFormat(string code, string? format)
    {
        if (string.IsNullOrEmpty(format))
        {
            return true;
        }

        return string.Equals(format, EnglishLanguage.GetTranslation(code), StringComparison.Ordinal);
    }

    public static string Format(string code, string? format, ErrorArgument[] arguments, bool firstArgumentIsPropertyName,
        CultureInfo uiCulture, CultureInfo formatCulture, IErrorMessageLocalizer? localizer)
    {
        localizer ??= ErrorLocalization.Default;

        var template = format ?? string.Empty;
        if (IsDefaultFormat(code, format) && localizer.TryGetTemplate(code, uiCulture, out var localized))
        {
            template = localized;
        }

        var values = ErrorArgument.ToObjects(arguments);
        if (firstArgumentIsPropertyName && values.Length > 0 && values[0] is string propertyName)
        {
            var displayName = localizer.GetDisplayName(propertyName, uiCulture);
            if (!string.Equals(displayName, propertyName, StringComparison.Ordinal))
            {
                values[0] = displayName;
            }
        }

        return string.Format(formatCulture, template, values);
    }
}
