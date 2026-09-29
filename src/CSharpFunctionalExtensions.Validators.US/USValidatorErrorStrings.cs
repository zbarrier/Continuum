namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Default (English) message templates for the US validators. {0} is always the property name.
/// </summary>
public static class USValidatorErrorStrings
{
    /// <summary>{0} property.</summary>
    public const string PhoneNumber = "'{0}' is not a valid US phone number.";
    /// <summary>{0} property.</summary>
    public const string PhoneNumberExtension = "'{0}' is not a valid US phone number extension.";
    /// <summary>{0} property.</summary>
    public const string StateAbbreviation = "'{0}' is not a valid US postal state abbreviation.";
    /// <summary>{0} property.</summary>
    public const string ZipCode = "'{0}' is not a valid US zip code.";
    /// <summary>{0} property.</summary>
    public const string TaxId = "'{0}' is not a valid US Tax ID.";
    /// <summary>{0} property.</summary>
    public const string TaxIdFormat = "'{0}' is not formatted correctly. An EIN should use the '12-1234567' format and a SSN or ITIN should use the '123-12-1234' format.";
    /// <summary>{0} property.</summary>
    public const string EinFormat = "'{0}' is not formatted correctly. An EIN should use the '12-1234567' format.";
    /// <summary>{0} property.</summary>
    public const string SsnFormat = "'{0}' is not formatted correctly. A SSN should use the '123-12-1234' format.";
    /// <summary>{0} property.</summary>
    public const string ItinFormat = "'{0}' is not formatted correctly. An ITIN should use the '123-12-1234' format.";
    /// <summary>{0} property.</summary>
    public const string Ssn = "'{0}' is not a valid SSN. An SSN must start with a digit other than '9'. An ITIN must start with '9'.";
    /// <summary>{0} property.</summary>
    public const string Itin = "'{0}' is not a valid ITIN. An ITIN must start with '9'. An SSN must start with a digit other than '9'.";
}
