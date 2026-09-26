#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid US Tax ID.
    /// </summary>
    public static Result<string> IsUSTaxId(string value, string propertyName)
    {
        // Check length
        bool isValidLength = value.Length == TaxIdUtil.EIN_Length || value.Length == TaxIdUtil.SSN_ITIN_Length;
        if (!isValidLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, TaxIdUtil.SimpleError, propertyName));
        }

        return value.Length == TaxIdUtil.EIN_Length
            ? TaxIdUtil.CheckEinHyphenPositions(value, TaxIdUtil.FormatError, propertyName)
                .Bind(() => TaxIdUtil.CheckEinDigits(value, TaxIdUtil.FormatError, propertyName))
            : TaxIdUtil.CheckSsnAndItinHyphenPositions(value, TaxIdUtil.FormatError, propertyName)
                .Bind(() => TaxIdUtil.CheckSsnOrItinDigits(value, TaxIdUtil.FormatError, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid US Tax ID.
    /// </summary>
    public static Result<string> IsUSTaxId(string value, string propertyNameFormat, params object[] arguments)
    {
        // Check length
        bool isValidLength = value.Length == TaxIdUtil.EIN_Length || value.Length == TaxIdUtil.SSN_ITIN_Length;
        if (!isValidLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, TaxIdUtil.SimpleError, propertyName));
        }

        return value.Length == TaxIdUtil.EIN_Length
            ? TaxIdUtil.CheckEinHyphenPositions(value, TaxIdUtil.FormatError, propertyNameFormat, arguments)
                .Bind(() => TaxIdUtil.CheckEinDigits(value, TaxIdUtil.FormatError, propertyNameFormat, arguments))
            : TaxIdUtil.CheckSsnAndItinHyphenPositions(value, TaxIdUtil.FormatError, propertyNameFormat, arguments)
                .Bind(() => TaxIdUtil.CheckSsnOrItinDigits(value, TaxIdUtil.FormatError, propertyNameFormat, arguments));
    }


    /// <summary>
    ///     Ensure value is valid US EIN Tax ID.
    /// </summary>
    public static Result<string> IsUSEinTaxId(string value, string propertyName)
    {
        // Check length
        bool isValidLength = value.Length == TaxIdUtil.EIN_Length;
        if (!isValidLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, TaxIdUtil.EinFormatError, propertyName));
        }

        return TaxIdUtil.CheckEinHyphenPositions(value, TaxIdUtil.EinFormatError, propertyName)
            .Bind(() => TaxIdUtil.CheckEinDigits(value, TaxIdUtil.EinFormatError, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid US EIN Tax ID.
    /// </summary>
    public static Result<string> IsUSEinTaxId(string value, string propertyNameFormat, params object[] arguments)
    {
        // Check length
        bool isValidLength = value.Length == TaxIdUtil.EIN_Length;
        if (!isValidLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, TaxIdUtil.EinFormatError, propertyName));
        }

        return TaxIdUtil.CheckEinHyphenPositions(value, TaxIdUtil.EinFormatError, propertyNameFormat, arguments)
            .Bind(() => TaxIdUtil.CheckEinDigits(value, TaxIdUtil.EinFormatError, propertyNameFormat, arguments));
    }


    /// <summary>
    ///     Ensure value is valid US SSN Tax ID.
    /// </summary>
    public static Result<string> IsUSSsnTaxId(string value, string propertyName)
    {
        // Check length
        bool isValidLength = value.Length == TaxIdUtil.SSN_ITIN_Length;
        if (!isValidLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, TaxIdUtil.SsnFormatError, propertyName));
        }

        return TaxIdUtil.CheckSsnAndItinHyphenPositions(value, TaxIdUtil.SsnFormatError, propertyName)
            .Bind(() => TaxIdUtil.CheckSsnDigits(value, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid US SSN Tax ID.
    /// </summary>
    public static Result<string> IsUSSsnTaxId(string value, string propertyNameFormat, params object[] arguments)
    {
        // Check length
        bool isValidLength = value.Length == TaxIdUtil.SSN_ITIN_Length;
        if (!isValidLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, TaxIdUtil.SsnFormatError, propertyName));
        }

        return TaxIdUtil.CheckSsnAndItinHyphenPositions(value, TaxIdUtil.SsnFormatError, propertyNameFormat, arguments)
            .Bind(() => TaxIdUtil.CheckSsnDigits(value, propertyNameFormat, arguments));
    }


    /// <summary>
    ///     Ensure value is valid US ITIN Tax ID.
    /// </summary>
    public static Result<string> IsUSItinTaxId(string value, string propertyName)
    {
        // Check length
        bool isValidLength = value.Length == TaxIdUtil.SSN_ITIN_Length;
        if (!isValidLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, TaxIdUtil.ItinFormatError, propertyName));
        }

        return TaxIdUtil.CheckSsnAndItinHyphenPositions(value, TaxIdUtil.ItinFormatError, propertyName)
            .Bind(() => TaxIdUtil.CheckItinDigits(value, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid US ITIN Tax ID.
    /// </summary>
    public static Result<string> IsUSItinTaxId(string value, string propertyNameFormat, params object[] arguments)
    {
        // Check length
        bool isValidLength = value.Length == TaxIdUtil.SSN_ITIN_Length;
        if (!isValidLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, TaxIdUtil.ItinFormatError, propertyName));
        }

        return TaxIdUtil.CheckSsnAndItinHyphenPositions(value, TaxIdUtil.ItinFormatError, propertyNameFormat, arguments)
            .Bind(() => TaxIdUtil.CheckItinDigits(value, propertyNameFormat, arguments));
    }

}
