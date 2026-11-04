#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     US tax ID (EIN, SSN, ITIN) validators exposed as static members of <see cref="Result"/>.
/// </summary>
public static partial class USTaxIdValidators
{
    extension(Result)
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
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.TaxId, USValidatorErrorStrings.TaxId, propertyName));
            }

            return value.Length == TaxIdUtil.EIN_Length
                ? TaxIdUtil.CheckEinHyphenPositions(value, USValidatorErrorStrings.TaxIdFormat, propertyName)
                    .Bind(() => TaxIdUtil.CheckEinDigits(value, USValidatorErrorStrings.TaxIdFormat, propertyName))
                : TaxIdUtil.CheckSsnAndItinHyphenPositions(value, USValidatorErrorStrings.TaxIdFormat, propertyName)
                    .Bind(() => TaxIdUtil.CheckSsnOrItinDigits(value, USValidatorErrorStrings.TaxIdFormat, propertyName));
        }

        /// <summary>
        ///     Ensure value is valid US Tax ID.
        /// </summary>
        public static Result<string> IsUSTaxId(string value, PropertyPath path, string member)
        {
            // Check length
            bool isValidLength = value.Length == TaxIdUtil.EIN_Length || value.Length == TaxIdUtil.SSN_ITIN_Length;
            if (!isValidLength)
            {
                var propertyName = path.Format(member);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.TaxId, USValidatorErrorStrings.TaxId, propertyName));
            }

            return value.Length == TaxIdUtil.EIN_Length
                ? TaxIdUtil.CheckEinHyphenPositions(value, USValidatorErrorStrings.TaxIdFormat, path, member)
                    .Bind(() => TaxIdUtil.CheckEinDigits(value, USValidatorErrorStrings.TaxIdFormat, path, member))
                : TaxIdUtil.CheckSsnAndItinHyphenPositions(value, USValidatorErrorStrings.TaxIdFormat, path, member)
                    .Bind(() => TaxIdUtil.CheckSsnOrItinDigits(value, USValidatorErrorStrings.TaxIdFormat, path, member));
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
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.EinFormat, USValidatorErrorStrings.EinFormat, propertyName));
            }

            return TaxIdUtil.CheckEinHyphenPositions(value, USValidatorErrorStrings.EinFormat, propertyName)
                .Bind(() => TaxIdUtil.CheckEinDigits(value, USValidatorErrorStrings.EinFormat, propertyName));
        }

        /// <summary>
        ///     Ensure value is valid US EIN Tax ID.
        /// </summary>
        public static Result<string> IsUSEinTaxId(string value, PropertyPath path, string member)
        {
            // Check length
            bool isValidLength = value.Length == TaxIdUtil.EIN_Length;
            if (!isValidLength)
            {
                var propertyName = path.Format(member);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.EinFormat, USValidatorErrorStrings.EinFormat, propertyName));
            }

            return TaxIdUtil.CheckEinHyphenPositions(value, USValidatorErrorStrings.EinFormat, path, member)
                .Bind(() => TaxIdUtil.CheckEinDigits(value, USValidatorErrorStrings.EinFormat, path, member));
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
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.SsnFormat, USValidatorErrorStrings.SsnFormat, propertyName));
            }

            return TaxIdUtil.CheckSsnAndItinHyphenPositions(value, USValidatorErrorStrings.SsnFormat, propertyName)
                .Bind(() => TaxIdUtil.CheckSsnDigits(value, propertyName));
        }

        /// <summary>
        ///     Ensure value is valid US SSN Tax ID.
        /// </summary>
        public static Result<string> IsUSSsnTaxId(string value, PropertyPath path, string member)
        {
            // Check length
            bool isValidLength = value.Length == TaxIdUtil.SSN_ITIN_Length;
            if (!isValidLength)
            {
                var propertyName = path.Format(member);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.SsnFormat, USValidatorErrorStrings.SsnFormat, propertyName));
            }

            return TaxIdUtil.CheckSsnAndItinHyphenPositions(value, USValidatorErrorStrings.SsnFormat, path, member)
                .Bind(() => TaxIdUtil.CheckSsnDigits(value, path, member));
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
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.ItinFormat, USValidatorErrorStrings.ItinFormat, propertyName));
            }

            return TaxIdUtil.CheckSsnAndItinHyphenPositions(value, USValidatorErrorStrings.ItinFormat, propertyName)
                .Bind(() => TaxIdUtil.CheckItinDigits(value, propertyName));
        }

        /// <summary>
        ///     Ensure value is valid US ITIN Tax ID.
        /// </summary>
        public static Result<string> IsUSItinTaxId(string value, PropertyPath path, string member)
        {
            // Check length
            bool isValidLength = value.Length == TaxIdUtil.SSN_ITIN_Length;
            if (!isValidLength)
            {
                var propertyName = path.Format(member);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.ItinFormat, USValidatorErrorStrings.ItinFormat, propertyName));
            }

            return TaxIdUtil.CheckSsnAndItinHyphenPositions(value, USValidatorErrorStrings.ItinFormat, path, member)
                .Bind(() => TaxIdUtil.CheckItinDigits(value, path, member));
        }
    }
}
