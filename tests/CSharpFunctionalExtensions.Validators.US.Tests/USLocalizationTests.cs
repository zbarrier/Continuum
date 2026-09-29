using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions.Validators.US.Tests;

public class USLocalizationTests
{
    [Fact]
    public void DefaultLocalizer_UsesUSEnglishTemplate()
    {
        var error = (ValidationError)Result.IsUSZipCode("1", "Zip").Error;

        var message = error.Entries[0].GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), new ErrorMessageLocalizer());

        Assert.Equal(string.Format(ExpectedUSValidatorErrorStrings.ZipCode, "Zip"), message);
    }

    [Fact]
    public void CustomTranslation_ForUSCode_IsUsed()
    {
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", ExpectedUSValidationErrorCodes.ZipCode, "'{0}' n'est pas un code postal américain valide.");
        var error = (ValidationError)Result.IsUSZipCode("1", "Zip").Error;

        var message = error.Entries[0].GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("'Zip' n'est pas un code postal américain valide.", message);
    }

    [Fact]
    public void CustomMessage_ForUSCode_IsPreserved()
    {
        var localizer = new ErrorMessageLocalizer();
        localizer.AddTranslation("fr", ExpectedUSValidationErrorCodes.ZipCode, "traduit");
        var error = new ValidationError("Zip", ExpectedUSValidationErrorCodes.ZipCode, "Custom '{0}'.", "Zip");

        var message = error.Entries[0].GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer);

        Assert.Equal("Custom 'Zip'.", message);
    }

    [Fact]
    public void EnsureRegistered_CanBeCalledRepeatedly()
    {
        USValidators.EnsureRegistered();
        USValidators.EnsureRegistered();

        Assert.Throws<ArgumentException>(() => ErrorLocalization.RegisterDefaultTemplate(ExpectedUSValidationErrorCodes.ZipCode, "unused {0}"));
    }

    [Fact]
    public void AllUSCodes_AreRegistered()
    {
        var codes = typeof(USValidationErrorCodes).GetFields().Select(f => (string)f.GetRawConstantValue()!);

        foreach (var code in codes)
        {
            Assert.Throws<ArgumentException>(() => ErrorLocalization.RegisterDefaultTemplate(code, "unused {0}"));
        }
    }
}
