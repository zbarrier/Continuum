#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     English-only localizer without display name resolution. Used for diagnostic output such as ToString().
/// </summary>
internal sealed class InvariantLocalizer : IErrorMessageLocalizer
{
    public static readonly InvariantLocalizer Instance = new();

    private InvariantLocalizer() { }

    public bool TryGetTemplate(string code, CultureInfo culture, out string template)
    {
        var value = EnglishLanguage.GetTranslation(code);
        template = value ?? string.Empty;
        return value is not null;
    }

    public string GetDisplayName(string propertyName, CultureInfo culture) => propertyName;
}
