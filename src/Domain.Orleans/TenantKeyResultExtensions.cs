using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

namespace Continuum.Domain.Orleans;

/// <summary>Validators for identifiers stored as tenant-qualified Orleans keys, such as <see cref="TenantKeyValueObject{T}"/> values.</summary>
public static class TenantKeyResultExtensions
{
    private const string WrongTenant = "'{0}' does not belong to the current tenant.";

    /// <summary>
    ///     Ensures the value is a well-formed <see cref="TenantKey"/> that belongs to <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="result">The result holding the key to validate.</param>
    /// <param name="tenantId">The tenant the key must belong to, or <see langword="null"/> when the key must not be tenant qualified.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <returns>The key in the canonical format of <see cref="TenantKey.Create"/>, or a <see cref="ValidationError"/> when it is malformed or belongs to another tenant.</returns>
    public static Result<string> IsTenantKey(this Result<string> result, string? tenantId, string propertyName)
        => result.IsFailure ? result : Validate(result.Value, tenantId, guidKey: false, propertyName);

    /// <inheritdoc cref="IsTenantKey(Result{string}, string?, string)"/>
    /// <param name="result">The result holding the key to validate.</param>
    /// <param name="tenantId">The tenant the key must belong to, or <see langword="null"/> when the key must not be tenant qualified.</param>
    /// <param name="path">The path of the property being validated.</param>
    /// <param name="member">The member being validated.</param>
    public static Result<string> IsTenantKey(this Result<string> result, string? tenantId, PropertyPath path, string member)
        => result.IsFailure ? result : Validate(result.Value, tenantId, guidKey: false, path.Format(member));

    /// <summary>
    ///     Ensures the value is a well-formed <see cref="TenantKey"/> that belongs to <paramref name="tenantId"/> and whose
    ///     key within the tenant is a non-empty <see cref="Guid"/>.
    /// </summary>
    /// <param name="result">The result holding the key to validate.</param>
    /// <param name="tenantId">The tenant the key must belong to, or <see langword="null"/> when the key must not be tenant qualified.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <returns>
    ///     The key with the <see cref="Guid"/> in its canonical <c>D</c> format, or a <see cref="ValidationError"/> when it is
    ///     malformed, belongs to another tenant, or its key is not a non-empty <see cref="Guid"/>.
    /// </returns>
    public static Result<string> IsTenantGuidKey(this Result<string> result, string? tenantId, string propertyName)
        => result.IsFailure ? result : Validate(result.Value, tenantId, guidKey: true, propertyName);

    /// <inheritdoc cref="IsTenantGuidKey(Result{string}, string?, string)"/>
    /// <param name="result">The result holding the key to validate.</param>
    /// <param name="tenantId">The tenant the key must belong to, or <see langword="null"/> when the key must not be tenant qualified.</param>
    /// <param name="path">The path of the property being validated.</param>
    /// <param name="member">The member being validated.</param>
    public static Result<string> IsTenantGuidKey(this Result<string> result, string? tenantId, PropertyPath path, string member)
        => result.IsFailure ? result : Validate(result.Value, tenantId, guidKey: true, path.Format(member));

    private static Result<string> Validate(string value, string? tenantId, bool guidKey, string propertyName)
    {
        if (!TenantKey.TrySplit(value, out var actualTenantId, out var key))
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }
        if (!string.Equals(actualTenantId, tenantId, StringComparison.Ordinal))
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, WrongTenant, propertyName));
        }
        if (!guidKey)
        {
            return Result.Success(TenantKey.Create(tenantId, key));
        }
        if (!Guid.TryParse(key, out var guid))
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }
        return guid == Guid.Empty
            ? Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.NotEmpty, ValidatorErrorStrings.NotEmpty, propertyName))
            : Result.Success(TenantKey.Create(tenantId, guid.ToString("D")));
    }
}
