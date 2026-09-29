namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Machine-readable codes assigned to <see cref="ValidationErrorEntry.Code"/> by the US validators.
///     These codes are also the keys used to look up localized message templates.
/// </summary>
public static class USValidationErrorCodes
{
    /// <summary>The value is not a correctly formatted EIN.</summary>
    public const string EinFormat = "us_ein_format";
    /// <summary>The value is not a valid ITIN.</summary>
    public const string Itin = "us_itin";
    /// <summary>The value is not a correctly formatted ITIN.</summary>
    public const string ItinFormat = "us_itin_format";
    /// <summary>The value is not a valid US phone number.</summary>
    public const string PhoneNumber = "us_phone_number";
    /// <summary>The value is not a valid US phone number extension.</summary>
    public const string PhoneNumberExtension = "us_phone_number_extension";
    /// <summary>The value is not a valid SSN.</summary>
    public const string Ssn = "us_ssn";
    /// <summary>The value is not a correctly formatted SSN.</summary>
    public const string SsnFormat = "us_ssn_format";
    /// <summary>The value is not a valid US postal state abbreviation.</summary>
    public const string StateAbbreviation = "us_state_abbreviation";
    /// <summary>The value is not a valid US tax ID.</summary>
    public const string TaxId = "us_tax_id";
    /// <summary>The value is not a correctly formatted US tax ID (EIN, SSN, or ITIN).</summary>
    public const string TaxIdFormat = "us_tax_id_format";
    /// <summary>The value is not a valid US zip code.</summary>
    public const string ZipCode = "us_zip_code";
}
