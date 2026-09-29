using System.Runtime.CompilerServices;

namespace Continuum.CSharpFunctionalExtensions;

internal static class USValidatorRegistration
{
    // Runs when the assembly loads, which is always before any US validator can produce an error,
    // so the US codes behave like built-in codes for localization.
#pragma warning disable CA2255 // Intentional: library must register its default templates before first use.
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Initialize()
    {
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.PhoneNumber, USValidatorErrorStrings.PhoneNumber);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.StateAbbreviation, USValidatorErrorStrings.StateAbbreviation);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.ZipCode, USValidatorErrorStrings.ZipCode);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.TaxId, USValidatorErrorStrings.TaxId);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.TaxIdFormat, USValidatorErrorStrings.TaxIdFormat);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.EinFormat, USValidatorErrorStrings.EinFormat);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.SsnFormat, USValidatorErrorStrings.SsnFormat);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.ItinFormat, USValidatorErrorStrings.ItinFormat);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.Ssn, USValidatorErrorStrings.Ssn);
        ErrorLocalization.RegisterDefaultTemplate(USValidationErrorCodes.Itin, USValidatorErrorStrings.Itin);
    }
}
