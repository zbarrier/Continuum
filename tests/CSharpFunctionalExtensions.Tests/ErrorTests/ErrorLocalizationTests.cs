using System.Globalization;
using System.Net;

namespace Continuum.CSharpFunctionalExtensions.Tests.ErrorTests;

public class ErrorLocalizationTests
{
    const string PropertyName = "Name";

    static ValidationErrorEntry NotEmptyEntry() =>
        ((ValidationError)Result.NotEmpty(string.Empty, PropertyName).Error).Entries[0];

    [Fact]
    public void EntryMessage_French_UsesFrenchTemplate()
    {
        var message = NotEmptyEntry().GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), new ErrorMessageLocalizer());

        Assert.Equal("'Name' ne doit pas être vide.", message);
    }

    [Fact]
    public void EntryMessage_SpecificCulture_FallsBackToParent()
    {
        var message = NotEmptyEntry().GetFormattedMessage(CultureInfo.GetCultureInfo("de-AT"), new ErrorMessageLocalizer());

        Assert.Equal("'Name' darf nicht leer sein.", message);
    }

    [Fact]
    public void EntryMessage_UnknownCulture_FallsBackToEnglish()
    {
        var message = NotEmptyEntry().GetFormattedMessage(CultureInfo.InvariantCulture, new ErrorMessageLocalizer());

        Assert.Equal(string.Format(ExpectedValidatorErrorStrings.NotEmpty, PropertyName), message);
    }

    [Fact]
    public void EntryMessage_MissingTranslationForCode_FallsBackToEnglish()
    {
        var entry = new ValidationError(PropertyName, AddOnCode, AddOnTemplate, PropertyName).Entries[0];

        var message = entry.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), new ErrorMessageLocalizer());

        Assert.Equal(string.Format(AddOnTemplate, PropertyName), message);
    }

    [Fact]
    public void EntryMessage_RegisteredAddOnCode_UsesCustomTranslation()
    {
        ErrorLocalization.RegisterDefaultTemplate(AddOnCode, AddOnTemplate);
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", AddOnCode, "'{0}' n'est pas valide.");
        var entry = new ValidationError(PropertyName, AddOnCode, AddOnTemplate, PropertyName).Entries[0];

        var message = entry.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("'Name' n'est pas valide.", message);
    }

    [Fact]
    public void EntryMessage_RegisteredAddOnCode_CustomMessageIsPreserved()
    {
        ErrorLocalization.RegisterDefaultTemplate(AddOnCode, AddOnTemplate);
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", AddOnCode, "'{0}' n'est pas valide.");
        var entry = new ValidationError(PropertyName, AddOnCode, "Custom '{0}'.", PropertyName).Entries[0];

        var message = entry.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("Custom 'Name'.", message);
    }

    [Fact]
    public void EntryMessage_UnregisteredCode_CustomMessageIsPreserved()
    {
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", "unregistered_code", "traduit");
        var entry = new ValidationError(PropertyName, "unregistered_code", "'{0}' is bad.", PropertyName).Entries[0];

        var message = entry.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("'Name' is bad.", message);
    }

    [Fact]
    public void RegisterDefaultTemplate_SameTemplateTwice_IsIdempotent()
    {
        ErrorLocalization.RegisterDefaultTemplate(AddOnCode, AddOnTemplate);
        ErrorLocalization.RegisterDefaultTemplate(AddOnCode, AddOnTemplate);
    }

    [Fact]
    public void RegisterDefaultTemplate_DifferentTemplate_Throws()
    {
        ErrorLocalization.RegisterDefaultTemplate(AddOnCode, AddOnTemplate);

        Assert.Throws<ArgumentException>(() => ErrorLocalization.RegisterDefaultTemplate(AddOnCode, "other {0}"));
    }

    [Fact]
    public void RegisterDefaultTemplate_BuiltInCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => ErrorLocalization.RegisterDefaultTemplate(ExpectedValidationErrorCodes.NotEmpty, "x {0}"));
    }

    const string AddOnCode = "addon_code";
    const string AddOnTemplate = "'{0}' is not valid.";

    [Fact]
    public void EntryMessage_CustomTranslation_OverridesBuiltIn()
    {
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", ExpectedValidationErrorCodes.NotEmpty, "{0} est requis.");

        var message = NotEmptyEntry().GetFormattedMessage(CultureInfo.GetCultureInfo("fr-CA"), localizer);

        Assert.Equal("Name est requis.", message);
    }

    [Fact]
    public void EntryMessage_ClearTranslations_RestoresBuiltIn()
    {
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", ExpectedValidationErrorCodes.NotEmpty, "{0} est requis.");
        localizer.ClearTranslations();

        var message = NotEmptyEntry().GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("'Name' ne doit pas être vide.", message);
    }

    [Fact]
    public void EntryMessage_Disabled_UsesEnglish()
    {
        var localizer = new ErrorMessageLocalizer { Enabled = false };

        var message = NotEmptyEntry().GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal(string.Format(ExpectedValidatorErrorStrings.NotEmpty, PropertyName), message);
    }

    [Fact]
    public void EntryMessage_LocalizerCulture_OverridesRequestedCulture()
    {
        var localizer = new ErrorMessageLocalizer { Culture = CultureInfo.GetCultureInfo("de") };

        var message = NotEmptyEntry().GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("'Name' darf nicht leer sein.", message);
    }

    [Fact]
    public void EntryMessage_DisplayNameResolver_ReplacesPropertyName()
    {
        var localizer = new ErrorMessageLocalizer
        {
            DisplayNameResolver = (name, culture) => culture.TwoLetterISOLanguageName == "fr" && name == PropertyName ? "Nom" : null,
        };

        var message = NotEmptyEntry().GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("'Nom' ne doit pas être vide.", message);
    }

    [Fact]
    public void EntryMessage_CustomFormat_IsNotReplacedByTranslation()
    {
        var error = new ValidationError(PropertyName, ExpectedValidationErrorCodes.NotEmpty, "{0} is required!", PropertyName);

        var message = error.Entries[0].GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), new ErrorMessageLocalizer());

        Assert.Equal("Name is required!", message);
    }

    [Fact]
    public void EntryMessage_FormatsArgumentsWithRequestedCulture()
    {
        var error = new ValidationError(PropertyName, "custom", "{0}: {1}", PropertyName, 1.5m);

        var message = error.Entries[0].GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), new ErrorMessageLocalizer());

        Assert.Equal("Name: 1,5", message);
    }

    [Fact]
    public void ToString_IsNotLocalized()
    {
        var localizer = new ErrorMessageLocalizer { Culture = CultureInfo.GetCultureInfo("fr") };
        var entry = NotEmptyEntry();

        Assert.Contains(string.Format(ExpectedValidatorErrorStrings.NotEmpty, PropertyName), entry.ToString());
        Assert.DoesNotContain("vide", entry.ToString());
        Assert.NotEqual(entry.ToString(), entry.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer));
    }

    [Fact]
    public void ValidationError_FormattedMessage_LocalizesEachEntry()
    {
        var error = (ValidationError)Result.NotEmpty(string.Empty, PropertyName).Error;

        var message = error.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), new ErrorMessageLocalizer());

        Assert.Contains("'Name' ne doit pas être vide.", message);
    }

    [Fact]
    public void RequestError_DefaultTemplate_IsLocalizedByCode()
    {
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", ErrorCodes.NotFound, "Introuvable.");
        var error = new RequestError(HttpStatusCode.NotFound, Grpc.Core.StatusCode.NotFound, ErrorCodes.NotFound);

        var message = error.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("Introuvable.", message);
    }

    [Fact]
    public void RequestError_CustomFormat_IsNotReplaced()
    {
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", ErrorCodes.NotFound, "Introuvable.");
        var error = new RequestError(HttpStatusCode.NotFound, Grpc.Core.StatusCode.NotFound, ErrorCodes.NotFound, "Order {0} missing.", 42);

        var message = error.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("Order 42 missing.", message);
    }

    [Fact]
    public void AllBuiltInTranslations_AreValidFormatStrings()
    {
        var codes = typeof(ValidationErrorCodes).GetFields()
            .Concat(typeof(ErrorCodes).GetFields())
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();
        var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Where(c => c.Name.Length > 0);
        var localizer = new ErrorMessageLocalizer();
        var args = Enumerable.Range(0, 6).Select(i => (object)i).ToArray();

        foreach (var culture in cultures)
        {
            foreach (var code in codes)
            {
                if (localizer.TryGetTemplate(code, culture, out var template))
                {
                    var ex = Record.Exception(() => string.Format(culture, template, args));
                    Assert.True(ex is null, $"{culture.Name}/{code}: {template}");
                }
            }
        }
    }

    [Fact]
    public void AllCodes_HaveEnglishTemplate()
    {
        var localizer = new ErrorMessageLocalizer();
        var codes = typeof(ValidationErrorCodes).GetFields()
            .Concat(typeof(ErrorCodes).GetFields())
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Where(code => code != ErrorCodes.Ok);

        foreach (var code in codes)
        {
            Assert.True(localizer.TryGetTemplate(code, CultureInfo.GetCultureInfo("en"), out _), code);
        }
    }
}
