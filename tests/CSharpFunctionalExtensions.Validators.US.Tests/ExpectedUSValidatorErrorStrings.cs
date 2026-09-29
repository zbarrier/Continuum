namespace Continuum.CSharpFunctionalExtensions.Validators.US.Tests;

// Intentionally duplicated from USValidatorErrorStrings so that any change to a
// library template is caught by the tests instead of silently flowing through.
public static class ExpectedUSValidatorErrorStrings
{
    public const string PhoneNumber = "'{0}' is not a valid US phone number.";
    public const string PhoneNumberExtension = "'{0}' is not a valid US phone number extension.";
    public const string StateAbbreviation = "'{0}' is not a valid US postal state abbreviation.";
    public const string ZipCode = "'{0}' is not a valid US zip code.";
    public const string TaxId = "'{0}' is not a valid US Tax ID.";
    public const string TaxIdFormat = "'{0}' is not formatted correctly. An EIN should use the '12-1234567' format and a SSN or ITIN should use the '123-12-1234' format.";
    public const string EinFormat = "'{0}' is not formatted correctly. An EIN should use the '12-1234567' format.";
    public const string SsnFormat = "'{0}' is not formatted correctly. A SSN should use the '123-12-1234' format.";
    public const string ItinFormat = "'{0}' is not formatted correctly. An ITIN should use the '123-12-1234' format.";
    public const string Ssn = "'{0}' is not a valid SSN. An SSN must start with a digit other than '9'. An ITIN must start with '9'.";
    public const string Itin = "'{0}' is not a valid ITIN. An ITIN must start with '9'. An SSN must start with a digit other than '9'.";
}
