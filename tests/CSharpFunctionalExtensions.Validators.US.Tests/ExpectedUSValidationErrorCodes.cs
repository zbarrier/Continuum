namespace Continuum.CSharpFunctionalExtensions.Validators.US.Tests;

// Intentionally duplicated from USValidationErrorCodes so that any change to a
// library code is caught by the tests instead of silently flowing through.
public static class ExpectedUSValidationErrorCodes
{
    public const string EinFormat = "us_ein_format";
    public const string Itin = "us_itin";
    public const string ItinFormat = "us_itin_format";
    public const string PhoneNumber = "us_phone_number";
    public const string PhoneNumberExtension = "us_phone_number_extension";
    public const string Ssn = "us_ssn";
    public const string SsnFormat = "us_ssn_format";
    public const string StateAbbreviation = "us_state_abbreviation";
    public const string TaxId = "us_tax_id";
    public const string TaxIdFormat = "us_tax_id_format";
    public const string ZipCode = "us_zip_code";
}
