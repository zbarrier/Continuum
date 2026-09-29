#nullable enable

using System.Collections.Concurrent;
using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Default <see cref="IErrorMessageLocalizer"/>. Looks up templates by error code using the
///     requested culture, then its parent cultures, then English. Custom translations added via
///     <see cref="AddTranslation"/> take precedence over built-in translations.
/// </summary>
public class ErrorMessageLocalizer : IErrorMessageLocalizer
{
    private readonly ConcurrentDictionary<(string Culture, string Code), string> _overrides = new();
    private readonly ConcurrentDictionary<(string Culture, string Code), string?> _cache = new();

    /// <summary>
    ///     When false, all lookups resolve to English regardless of the requested culture.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Optional culture that overrides the culture passed to lookups.
    /// </summary>
    public CultureInfo? Culture { get; set; }

    /// <summary>
    ///     Optional hook that translates a property name into a display name for a culture.
    ///     Return null to use the property name unchanged.
    /// </summary>
    public Func<string, CultureInfo, string?>? DisplayNameResolver { get; set; }

    /// <summary>
    ///     Adds or replaces the template for <paramref name="code"/> in <paramref name="culture"/>.
    ///     Templates use positional placeholders; {0} is the property name for validation codes.
    /// </summary>
    public void AddTranslation(string culture, string code, string template)
    {
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(template);

        _overrides[(culture, code)] = template;
        _cache.Clear();
    }

    /// <summary>
    ///     Removes all custom translations added via <see cref="AddTranslation"/>.
    /// </summary>
    public void ClearTranslations()
    {
        _overrides.Clear();
        _cache.Clear();
    }

    /// <inheritdoc/>
    public bool TryGetTemplate(string code, CultureInfo culture, out string template)
    {
        ArgumentNullException.ThrowIfNull(code);

        var effective = !Enabled ? CultureInfo.GetCultureInfo(EnglishLanguage.Culture) : (Culture ?? culture ?? CultureInfo.CurrentUICulture);

        var current = effective;
        while (!Equals(current, CultureInfo.InvariantCulture))
        {
            var value = Lookup(current.Name, code);
            if (value is not null)
            {
                template = value;
                return true;
            }

            current = current.Parent;
        }

        var english = Lookup(EnglishLanguage.Culture, code);
        template = english ?? string.Empty;
        return english is not null;
    }

    /// <inheritdoc/>
    public string GetDisplayName(string propertyName, CultureInfo culture)
    {
        if (DisplayNameResolver is null || propertyName is null)
        {
            return propertyName!;
        }

        return DisplayNameResolver(propertyName, Culture ?? culture ?? CultureInfo.CurrentUICulture) ?? propertyName;
    }

    private string? Lookup(string culture, string code) =>
        _cache.GetOrAdd((culture, code), key =>
            _overrides.TryGetValue(key, out var custom) ? custom : Languages.GetTranslation(key.Culture, key.Code));
}
