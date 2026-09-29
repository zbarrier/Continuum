#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Global localization settings for error messages.
/// </summary>
public static class ErrorLocalization
{
    private static IErrorMessageLocalizer _default = new ErrorMessageLocalizer();

    /// <summary>
    ///     The localizer used when none is passed to <see cref="Error.GetFormattedMessage(System.Globalization.CultureInfo, IErrorMessageLocalizer?)"/>.
    /// </summary>
    public static IErrorMessageLocalizer Default
    {
        get => _default;
        set => _default = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Registers the English default template for an error code defined outside the core library
    ///     (for example, by an add-on validator package). Registered codes behave like built-in codes:
    ///     translations replace the default template, while custom messages are preserved.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     The code is built in, or is already registered with a different template.
    /// </exception>
    public static void RegisterDefaultTemplate(string code, string englishTemplate)
    {
        ArgumentException.ThrowIfNullOrEmpty(code);
        ArgumentException.ThrowIfNullOrEmpty(englishTemplate);

        EnglishLanguage.Register(code, englishTemplate);
    }
}
