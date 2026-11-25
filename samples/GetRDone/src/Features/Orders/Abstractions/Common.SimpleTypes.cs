using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

namespace GetRDone.Orders.Abstractions;

[Alias("Orders.OrderId.V1"), GenerateSerializer, Immutable]
public sealed record OrderId : TenantKeyValueObject<OrderId>
{
    public static Result<OrderId> Create(string? value, string? tenantId, string propertyName = nameof(OrderId))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsTenantGuidKey(tenantId, propertyName)
            .Map(validatedValue => new OrderId(validatedValue));
    }
    public static Result<OrderId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsTenantGuidKey(tenantId, propertyPath, member)
            .Map(validatedValue => new OrderId(validatedValue));
    }

    private OrderId(string value) : base(value) { }
}

[Alias("Orders.OrderNumber.V1"), GenerateSerializer, Immutable]
public sealed record OrderNumber : Int64ValueObject<OrderNumber>
{
    public const long MinValue = 100_000;
    public const long MaxValue = long.MaxValue;

    public static Result<OrderNumber> Create(string? value, string propertyName = nameof(OrderNumber))
    {
        return Result.IsInt64(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new OrderNumber(validatedValue));
    }
    public static Result<OrderNumber> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsInt64(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new OrderNumber(validatedValue));
    }
    public static Result<OrderNumber> Create(long? value, string propertyName = nameof(OrderNumber))
    {
        return Result.NotNull(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new OrderNumber(validatedValue));
    }
    public static Result<OrderNumber> Create(long? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new OrderNumber(validatedValue));
    }
    public static Result<OrderNumber> Create(long value, string propertyName = nameof(OrderNumber))
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyName)
            .Map(validatedValue => new OrderNumber(validatedValue));
    }
    public static Result<OrderNumber> Create(long value, PropertyPath propertyPath, string member)
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new OrderNumber(validatedValue));
    }

    private OrderNumber(long value) : base(value) { }
}

[Alias("Orders.ClientId.V1"), GenerateSerializer, Immutable]
public sealed record ClientId : TenantKeyValueObject<ClientId>
{
    public static Result<ClientId> Create(string? value, string? tenantId, string propertyName = nameof(ClientId))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsTenantKey(tenantId, propertyName)
            .Map(validatedValue => new ClientId(validatedValue));
    }
    public static Result<ClientId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsTenantKey(tenantId, propertyPath, member)
            .Map(validatedValue => new ClientId(validatedValue));
    }

    private ClientId(string value) : base(value) { }
}

[Alias("Orders.ServiceRequestTypeId.V1"), GenerateSerializer, Immutable]
public sealed record ServiceRequestTypeId : TenantKeyValueObject<ServiceRequestTypeId>
{
    public static Result<ServiceRequestTypeId> Create(string? value, string? tenantId, string propertyName = nameof(ServiceRequestTypeId))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsTenantKey(tenantId, propertyName)
            .Map(validatedValue => new ServiceRequestTypeId(validatedValue));
    }
    public static Result<ServiceRequestTypeId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsTenantKey(tenantId, propertyPath, member)
            .Map(validatedValue => new ServiceRequestTypeId(validatedValue));
    }

    private ServiceRequestTypeId(string value) : base(value) { }
}

[Alias("Orders.ServiceRequestTypeVersion.V1"), GenerateSerializer, Immutable]
public sealed record ServiceRequestTypeVersion : Int32ValueObject<ServiceRequestTypeVersion>
{
    public const int MinValue = 1;
    public const int MaxValue = int.MaxValue;

    public static Result<ServiceRequestTypeVersion> Create(string? value, string propertyName = nameof(ServiceRequestTypeVersion))
    {
        return Result.IsInt32(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new ServiceRequestTypeVersion(validatedValue));
    }
    public static Result<ServiceRequestTypeVersion> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsInt32(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new ServiceRequestTypeVersion(validatedValue));
    }
    public static Result<ServiceRequestTypeVersion> Create(int? value, string propertyName = nameof(ServiceRequestTypeVersion))
    {
        return Result.NotNull(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new ServiceRequestTypeVersion(validatedValue));
    }
    public static Result<ServiceRequestTypeVersion> Create(int? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new ServiceRequestTypeVersion(validatedValue));
    }
    public static Result<ServiceRequestTypeVersion> Create(int value, string propertyName = nameof(ServiceRequestTypeVersion))
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyName)
            .Map(validatedValue => new ServiceRequestTypeVersion(validatedValue));
    }
    public static Result<ServiceRequestTypeVersion> Create(int value, PropertyPath propertyPath, string member)
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new ServiceRequestTypeVersion(validatedValue));
    }

    private ServiceRequestTypeVersion(int value) : base(value) { }
}

[Alias("Orders.ServiceRequestTypeName.V1"), GenerateSerializer, Immutable]
public sealed record ServiceRequestTypeName : StringValueObject<ServiceRequestTypeName>
{
    public const int MaxLength = 30;

    public static Result<ServiceRequestTypeName> Create(string? value, string propertyName = nameof(ServiceRequestTypeName))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new ServiceRequestTypeName(validatedValue));
    }
    public static Result<ServiceRequestTypeName> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new ServiceRequestTypeName(validatedValue));
    }

    private ServiceRequestTypeName(string value) : base(value) { }
}

[Alias("Orders.FormTypeId.V1"), GenerateSerializer, Immutable]
public sealed record FormTypeId : TenantKeyValueObject<FormTypeId>
{
    public static Result<FormTypeId> Create(string? value, string? tenantId, string propertyName = nameof(FormTypeId))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsTenantKey(tenantId, propertyName)
            .Map(validatedValue => new FormTypeId(validatedValue));
    }
    public static Result<FormTypeId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsTenantKey(tenantId, propertyPath, member)
            .Map(validatedValue => new FormTypeId(validatedValue));
    }

    private FormTypeId(string value) : base(value) { }
}

[Alias("Orders.FormTypeVersion.V1"), GenerateSerializer, Immutable]
public sealed record FormTypeVersion : Int32ValueObject<FormTypeVersion>
{
    public const int MinValue = 1;
    public const int MaxValue = int.MaxValue;

    public static Result<FormTypeVersion> Create(string? value, string propertyName = nameof(FormTypeVersion))
    {
        return Result.IsInt32(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new FormTypeVersion(validatedValue));
    }
    public static Result<FormTypeVersion> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsInt32(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new FormTypeVersion(validatedValue));
    }
    public static Result<FormTypeVersion> Create(int? value, string propertyName = nameof(FormTypeVersion))
    {
        return Result.NotNull(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new FormTypeVersion(validatedValue));
    }
    public static Result<FormTypeVersion> Create(int? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new FormTypeVersion(validatedValue));
    }
    public static Result<FormTypeVersion> Create(int value, string propertyName = nameof(FormTypeVersion))
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyName)
            .Map(validatedValue => new FormTypeVersion(validatedValue));
    }
    public static Result<FormTypeVersion> Create(int value, PropertyPath propertyPath, string member)
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new FormTypeVersion(validatedValue));
    }

    private FormTypeVersion(int value) : base(value) { }
}

[Alias("Orders.FormTypeName.V1"), GenerateSerializer, Immutable]
public sealed record FormTypeName : StringValueObject<FormTypeName>
{
    public const int MaxLength = 50;

    public static Result<FormTypeName> Create(string? value, string propertyName = nameof(FormTypeName))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new FormTypeName(validatedValue));
    }
    public static Result<FormTypeName> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new FormTypeName(validatedValue));
    }

    private FormTypeName(string value) : base(value) { }
}

[Alias("Orders.PropertyStreet.V1"), GenerateSerializer, Immutable]
public sealed record PropertyStreet : StringValueObject<PropertyStreet>
{
    // The USPS allows up to 46 characters per line for address line 1 and address line 2.
    // This gives us a max length of 92 characters for the full street address (line 1 + line 2).
    // We bump this up to 94 characters to allow for a separator (e.g. newline, CRLF) between the
    // two lines, if needed.
    public const int MaxLength = 94;

    public static Result<PropertyStreet> Create(string? value, string propertyName = nameof(PropertyStreet))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new PropertyStreet(validatedValue));
    }
    public static Result<PropertyStreet> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new PropertyStreet(validatedValue));
    }

    private PropertyStreet(string value) : base(value) { }
}

[Alias("Orders.PropertyCity.V1"), GenerateSerializer, Immutable]
public sealed record PropertyCity : StringValueObject<PropertyCity>
{
    // The USPS allows up to 50 characters for city names.
    public const int MaxLength = 50;

    public static Result<PropertyCity> Create(string? value, string propertyName = nameof(PropertyCity))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new PropertyCity(validatedValue));
    }
    public static Result<PropertyCity> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new PropertyCity(validatedValue));
    }

    private PropertyCity(string value) : base(value) { }
}

[Alias("Orders.PropertyState.V1"), GenerateSerializer, Immutable]
public sealed record PropertyState : StringValueObject<PropertyState>
{
    public static Result<PropertyState> Create(string? value, string propertyName = nameof(PropertyState))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsUSPostalStateAbbreviation(propertyName)
            .Map(validatedValue => new PropertyState(validatedValue.ToUpperInvariant()));
    }
    public static Result<PropertyState> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsUSPostalStateAbbreviation(propertyPath, member)
            .Map(validatedValue => new PropertyState(validatedValue.ToUpperInvariant()));
    }

    private PropertyState(string value) : base(value) { }
}

[Alias("Orders.PropertyZip.V1"), GenerateSerializer, Immutable]
public sealed record PropertyZip : StringValueObject<PropertyZip>
{
    public static Result<PropertyZip> Create(string? value, string propertyName = nameof(PropertyZip))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsUSZipCode(propertyName)
            .Map(validatedValue => new PropertyZip(validatedValue));
    }
    public static Result<PropertyZip> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsUSZipCode(propertyPath, member)
            .Map(validatedValue => new PropertyZip(validatedValue));
    }

    private PropertyZip(string value) : base(value) { }
}

[Alias("Orders.PropertyType.V1"), GenerateSerializer, Immutable]
public sealed record PropertyType : StringValueObject<PropertyType>
{
    public const int MaxLength = 30;

    public static Result<PropertyType> Create(string? value, string propertyName = nameof(PropertyType))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new PropertyType(validatedValue));
    }
    public static Result<PropertyType> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new PropertyType(validatedValue));
    }

    private PropertyType(string value) : base(value) { }
}

[Alias("Orders.IntendedUseId.V1"), GenerateSerializer, Immutable]
public sealed record IntendedUseId : TenantKeyValueObject<IntendedUseId>
{
    public static Result<IntendedUseId> Create(string? value, string? tenantId, string propertyName = nameof(IntendedUseId))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsTenantKey(tenantId, propertyName)
            .Map(validatedValue => new IntendedUseId(validatedValue));
    }
    public static Result<IntendedUseId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsTenantKey(tenantId, propertyPath, member)
            .Map(validatedValue => new IntendedUseId(validatedValue));
    }

    private IntendedUseId(string value) : base(value) { }
}

[Alias("Orders.IntendedUseVersion.V1"), GenerateSerializer, Immutable]
public sealed record IntendedUseVersion : Int32ValueObject<IntendedUseVersion>
{
    public const int MinValue = 1;
    public const int MaxValue = int.MaxValue;

    public static Result<IntendedUseVersion> Create(string? value, string propertyName = nameof(IntendedUseVersion))
    {
        return Result.IsInt32(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new IntendedUseVersion(validatedValue));
    }
    public static Result<IntendedUseVersion> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsInt32(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new IntendedUseVersion(validatedValue));
    }
    public static Result<IntendedUseVersion> Create(int? value, string propertyName = nameof(IntendedUseVersion))
    {
        return Result.NotNull(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new IntendedUseVersion(validatedValue));
    }
    public static Result<IntendedUseVersion> Create(int? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new IntendedUseVersion(validatedValue));
    }
    public static Result<IntendedUseVersion> Create(int value, string propertyName = nameof(IntendedUseVersion))
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyName)
            .Map(validatedValue => new IntendedUseVersion(validatedValue));
    }
    public static Result<IntendedUseVersion> Create(int value, PropertyPath propertyPath, string member)
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new IntendedUseVersion(validatedValue));
    }

    private IntendedUseVersion(int value) : base(value) { }
}

[Alias("Orders.IntendedUseName.V1"), GenerateSerializer, Immutable]
public sealed record IntendedUseName : StringValueObject<IntendedUseName>
{
    public const int MaxLength = 30;

    public static Result<IntendedUseName> Create(string? value, string propertyName = nameof(IntendedUseName))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new IntendedUseName(validatedValue));
    }
    public static Result<IntendedUseName> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new IntendedUseName(validatedValue));
    }

    private IntendedUseName(string value) : base(value) { }
}

[Alias("Orders.LoanNumber.V1"), GenerateSerializer, Immutable]
public sealed record LoanNumber : StringValueObject<LoanNumber>
{
    public const int MaxLength = 30;

    public static Result<LoanNumber> Create(string? value, string propertyName = nameof(LoanNumber))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new LoanNumber(validatedValue));
    }
    public static Result<LoanNumber> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new LoanNumber(validatedValue));
    }

    private LoanNumber(string value) : base(value) { }
}

[Alias("Orders.BorrowerName.V1"), GenerateSerializer, Immutable]
public sealed record BorrowerName : StringValueObject<BorrowerName>
{
    public const int MaxLength = 100;

    public static Result<BorrowerName> Create(string? value, string propertyName = nameof(BorrowerName))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new BorrowerName(validatedValue));
    }
    public static Result<BorrowerName> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new BorrowerName(validatedValue));
    }

    private BorrowerName(string value) : base(value) { }
}

/// <summary>The name of the person who gives the appraiser access to the property (see <see cref="AccessContact"/>).</summary>
[Alias("Orders.AccessContactName.V1"), GenerateSerializer, Immutable]
public sealed record AccessContactName : StringValueObject<AccessContactName>
{
    public const int MaxLength = 100;

    public static Result<AccessContactName> Create(string? value, string propertyName = nameof(AccessContactName))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new AccessContactName(validatedValue));
    }
    public static Result<AccessContactName> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new AccessContactName(validatedValue));
    }

    private AccessContactName(string value) : base(value) { }
}

/// <summary>
///     The 10-digit US phone number of the property's access contact (see <see cref="AccessContact"/>), stored as digits
///     only (formatting such as <c>(540) 555-0142</c> is accepted and removed).
/// </summary>
[Alias("Orders.AccessContactPhone.V1"), GenerateSerializer, Immutable]
public sealed record AccessContactPhone : StringValueObject<AccessContactPhone>
{
    public static Result<AccessContactPhone> Create(string? value, string propertyName = nameof(AccessContactPhone))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsUSPhoneNumber(propertyName)
            .Map(validatedValue => new AccessContactPhone(validatedValue));
    }
    public static Result<AccessContactPhone> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsUSPhoneNumber(propertyPath, member)
            .Map(validatedValue => new AccessContactPhone(validatedValue));
    }

    private AccessContactPhone(string value) : base(value) { }
}

[Alias("Orders.DueDate.V1"), GenerateSerializer, Immutable]
public sealed record DueDate : DateOnlyValueObject<DueDate>
{
    public static Result<DueDate> Create(string? value, string propertyName = nameof(DueDate))
    {
        return Result.IsDateOnly(value, propertyName)
            .NotEmpty(propertyName)
            .Map(validatedValue => new DueDate(validatedValue));
    }
    public static Result<DueDate> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsDateOnly(value, propertyPath, member)
            .NotEmpty(propertyPath, member)
            .Map(validatedValue => new DueDate(validatedValue));
    }
    public static Result<DueDate> Create(DateOnly? value, string propertyName = nameof(DueDate))
    {
        return Result.NotNull(value, propertyName)
            .NotEmpty(propertyName)
            .Map(validatedValue => new DueDate(validatedValue));
    }
    public static Result<DueDate> Create(DateOnly? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .NotEmpty(propertyPath, member)
            .Map(validatedValue => new DueDate(validatedValue));
    }
    public static Result<DueDate> Create(DateOnly value, string propertyName = nameof(DueDate))
    {
        return Result.NotEmpty(value, propertyName)
            .Map(validatedValue => new DueDate(validatedValue));
    }
    public static Result<DueDate> Create(DateOnly value, PropertyPath propertyPath, string member)
    {
        return Result.NotEmpty(value, propertyPath, member)
            .Map(validatedValue => new DueDate(validatedValue));
    }

    private DueDate(DateOnly value) : base(value) { }
}

[Alias("Orders.UserId.V1"), GenerateSerializer, Immutable]
public sealed record UserId : TenantKeyValueObject<UserId>
{
    public static Result<UserId> Create(string? value, string? tenantId, string propertyName = nameof(UserId))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsTenantGuidKey(tenantId, propertyName)
            .Map(validatedValue => new UserId(validatedValue));
    }
    public static Result<UserId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsTenantGuidKey(tenantId, propertyPath, member)
            .Map(validatedValue => new UserId(validatedValue));
    }

    private UserId(string value) : base(value) { }
}

/// <summary>A user's email address, such as the address a notice is sent to.</summary>
[Alias("Orders.EmailAddress.V1"), GenerateSerializer, Immutable]
public sealed record EmailAddress : StringValueObject<EmailAddress>
{
    public static Result<EmailAddress> Create(string? value, string propertyName = nameof(EmailAddress))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsEmail(propertyName)
            .Map(validatedValue => new EmailAddress(validatedValue));
    }
    public static Result<EmailAddress> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsEmail(propertyPath, member)
            .Map(validatedValue => new EmailAddress(validatedValue));
    }

    private EmailAddress(string value) : base(value) { }
}

/// <summary>The subject of an email the app sends, such as a notice.</summary>
[Alias("Orders.EmailSubject.V1"), GenerateSerializer, Immutable]
public sealed record EmailSubject : StringValueObject<EmailSubject>
{
    public const int MaxLength = 200;

    public static Result<EmailSubject> Create(string? value, string propertyName = nameof(EmailSubject))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new EmailSubject(validatedValue));
    }
    public static Result<EmailSubject> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new EmailSubject(validatedValue));
    }

    private EmailSubject(string value) : base(value) { }
}

/// <summary>A supporting document attached to an order; the client generates it before uploading the file.</summary>
[Alias("Orders.DocumentId.V1"), GenerateSerializer, Immutable]
public sealed record DocumentId : TenantKeyValueObject<DocumentId>
{
    public static Result<DocumentId> Create(string? value, string? tenantId, string propertyName = nameof(DocumentId))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsTenantGuidKey(tenantId, propertyName)
            .Map(validatedValue => new DocumentId(validatedValue));
    }
    public static Result<DocumentId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsTenantGuidKey(tenantId, propertyPath, member)
            .Map(validatedValue => new DocumentId(validatedValue));
    }

    private DocumentId(string value) : base(value) { }
}

[Alias("Orders.DocumentTypeId.V1"), GenerateSerializer, Immutable]
public sealed record DocumentTypeId : TenantKeyValueObject<DocumentTypeId>
{
    public static Result<DocumentTypeId> Create(string? value, string? tenantId, string propertyName = nameof(DocumentTypeId))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .IsTenantKey(tenantId, propertyName)
            .Map(validatedValue => new DocumentTypeId(validatedValue));
    }
    public static Result<DocumentTypeId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .IsTenantKey(tenantId, propertyPath, member)
            .Map(validatedValue => new DocumentTypeId(validatedValue));
    }

    private DocumentTypeId(string value) : base(value) { }
}

[Alias("Orders.DocumentTypeVersion.V1"), GenerateSerializer, Immutable]
public sealed record DocumentTypeVersion : Int32ValueObject<DocumentTypeVersion>
{
    public const int MinValue = 1;
    public const int MaxValue = int.MaxValue;

    public static Result<DocumentTypeVersion> Create(string? value, string propertyName = nameof(DocumentTypeVersion))
    {
        return Result.IsInt32(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new DocumentTypeVersion(validatedValue));
    }
    public static Result<DocumentTypeVersion> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsInt32(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new DocumentTypeVersion(validatedValue));
    }
    public static Result<DocumentTypeVersion> Create(int? value, string propertyName = nameof(DocumentTypeVersion))
    {
        return Result.NotNull(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new DocumentTypeVersion(validatedValue));
    }
    public static Result<DocumentTypeVersion> Create(int? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new DocumentTypeVersion(validatedValue));
    }
    public static Result<DocumentTypeVersion> Create(int value, string propertyName = nameof(DocumentTypeVersion))
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyName)
            .Map(validatedValue => new DocumentTypeVersion(validatedValue));
    }
    public static Result<DocumentTypeVersion> Create(int value, PropertyPath propertyPath, string member)
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new DocumentTypeVersion(validatedValue));
    }

    private DocumentTypeVersion(int value) : base(value) { }
}

[Alias("Orders.DocumentTypeName.V1"), GenerateSerializer, Immutable]
public sealed record DocumentTypeName : StringValueObject<DocumentTypeName>
{
    public const int MaxLength = 50;

    public static Result<DocumentTypeName> Create(string? value, string propertyName = nameof(DocumentTypeName))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new DocumentTypeName(validatedValue));
    }
    public static Result<DocumentTypeName> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new DocumentTypeName(validatedValue));
    }

    private DocumentTypeName(string value) : base(value) { }
}

/// <summary>The name of an uploaded file, as it was on the user's computer.</summary>
[Alias("Orders.FileName.V1"), GenerateSerializer, Immutable]
public sealed record FileName : StringValueObject<FileName>
{
    // Windows, macOS and Linux file systems allow up to 255 characters for a file name.
    public const int MaxLength = 255;

    public static Result<FileName> Create(string? value, string propertyName = nameof(FileName))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new FileName(validatedValue));
    }
    public static Result<FileName> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new FileName(validatedValue));
    }

    private FileName(string value) : base(value) { }
}

/// <summary>The media type of an uploaded file, such as <c>application/pdf</c>.</summary>
[Alias("Orders.ContentType.V1"), GenerateSerializer, Immutable]
public sealed record ContentType : StringValueObject<ContentType>
{
    // RFC 6838 allows up to 127 characters for each of the type and subtype.
    public const int MaxLength = 255;

    public static Result<ContentType> Create(string? value, string propertyName = nameof(ContentType))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new ContentType(validatedValue));
    }
    public static Result<ContentType> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new ContentType(validatedValue));
    }

    private ContentType(string value) : base(value) { }
}

/// <summary>The size of an uploaded file in bytes; completed appraisals with their photos can be large, so up to 100 MB.</summary>
[Alias("Orders.FileSize.V1"), GenerateSerializer, Immutable]
public sealed record FileSize : Int64ValueObject<FileSize>
{
    public const long MinValue = 1;
    public const long MaxValue = 100L * 1024 * 1024;

    public static Result<FileSize> Create(string? value, string propertyName = nameof(FileSize))
    {
        return Result.IsInt64(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new FileSize(validatedValue));
    }
    public static Result<FileSize> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsInt64(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new FileSize(validatedValue));
    }
    public static Result<FileSize> Create(long? value, string propertyName = nameof(FileSize))
    {
        return Result.NotNull(value, propertyName)
            .InclusiveBetween(MinValue, MaxValue, propertyName)
            .Map(validatedValue => new FileSize(validatedValue));
    }
    public static Result<FileSize> Create(long? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .InclusiveBetween(MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new FileSize(validatedValue));
    }
    public static Result<FileSize> Create(long value, string propertyName = nameof(FileSize))
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyName)
            .Map(validatedValue => new FileSize(validatedValue));
    }
    public static Result<FileSize> Create(long value, PropertyPath propertyPath, string member)
    {
        return Result.InclusiveBetween(value, MinValue, MaxValue, propertyPath, member)
            .Map(validatedValue => new FileSize(validatedValue));
    }

    private FileSize(long value) : base(value) { }
}

/// <summary>What a supporting document holds, in the user's words. The description is optional.</summary>
[Alias("Orders.DocumentDescription.V1"), GenerateSerializer, Immutable]
public sealed record DocumentDescription : StringValueObject<DocumentDescription>
{
    public const int MaxLength = 500;

    public static Result<DocumentDescription> Create(string? value, string propertyName = nameof(DocumentDescription))
    {
        return Result.NotNullOrWhitespace(value, propertyName)
            .MaxLength(MaxLength, propertyName)
            .Map(validatedValue => new DocumentDescription(validatedValue));
    }
    public static Result<DocumentDescription> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNullOrWhitespace(value, propertyPath, member)
            .MaxLength(MaxLength, propertyPath, member)
            .Map(validatedValue => new DocumentDescription(validatedValue));
    }

    /// <summary>Creates the description, or returns <see langword="null"/> when it is empty.</summary>
    public static Result<DocumentDescription?> CreateOptional(string? value, string propertyName = nameof(DocumentDescription))
    {
        return string.IsNullOrWhiteSpace(value)
            ? Result.Success<DocumentDescription?>(null)
            : Create(value, propertyName).Map(description => (DocumentDescription?)description);
    }
    /// <summary>Creates the description, or returns <see langword="null"/> when it is empty.</summary>
    public static Result<DocumentDescription?> CreateOptional(string? value, PropertyPath propertyPath, string member)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Result.Success<DocumentDescription?>(null)
            : Create(value, propertyPath, member).Map(description => (DocumentDescription?)description);
    }

    private DocumentDescription(string value) : base(value) { }
}
