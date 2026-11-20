using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

namespace GetRDone.Orders.Abstractions;

[Alias("Orders.OrderId.V1"), GenerateSerializer, Immutable]
public sealed record OrderId : GuidValueObject<OrderId>
{
    public static Result<OrderId> Create(string? value, string propertyName = nameof(OrderId))
    {
        return Result.IsGuidAndNotEmpty(value, propertyName)
            .Map(validatedValue => new OrderId(validatedValue));
    }
    public static Result<OrderId> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsGuidAndNotEmpty(value, propertyPath, member)
            .Map(validatedValue => new OrderId(validatedValue));
    }
    public static Result<OrderId> Create(Guid? value, string propertyName = nameof(OrderId))
    {
        return Result.NotNull(value, propertyName)
            .Map(validatedValue => new OrderId(validatedValue));
    }
    public static Result<OrderId> Create(Guid? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .Map(validatedValue => new OrderId(validatedValue));
    }
    public static Result<OrderId> Create(Guid value, string propertyName = nameof(OrderId))
    {
        return Result.NotEmpty(value, propertyName)
            .Map(validatedValue => new OrderId(validatedValue));
    }
    public static Result<OrderId> Create(Guid value, PropertyPath propertyPath, string member)
    {
        return Result.NotEmpty(value, propertyPath, member)
            .Map(validatedValue => new OrderId(validatedValue));
    }

    private OrderId(Guid value) : base(value) { }
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

[Alias("Orders.ServiceRequestTypeId.V1"), GenerateSerializer, Immutable]
public sealed record ServiceRequestTypeId : GuidValueObject<ServiceRequestTypeId>
{
    public static Result<ServiceRequestTypeId> Create(string? value, string propertyName = nameof(ServiceRequestTypeId))
    {
        return Result.IsGuidAndNotEmpty(value, propertyName)
            .Map(validatedValue => new ServiceRequestTypeId(validatedValue));
    }
    public static Result<ServiceRequestTypeId> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsGuidAndNotEmpty(value, propertyPath, member)
            .Map(validatedValue => new ServiceRequestTypeId(validatedValue));
    }
    public static Result<ServiceRequestTypeId> Create(Guid? value, string propertyName = nameof(ServiceRequestTypeId))
    {
        return Result.NotNull(value, propertyName)
            .NotEmpty(propertyName)
            .Map(validatedValue => new ServiceRequestTypeId(validatedValue));
    }
    public static Result<ServiceRequestTypeId> Create(Guid? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .NotEmpty(propertyPath, member)
            .Map(validatedValue => new ServiceRequestTypeId(validatedValue));
    }
    public static Result<ServiceRequestTypeId> Create(Guid value, string propertyName = nameof(ServiceRequestTypeId))
    {
        return Result.NotEmpty(value, propertyName)
            .Map(validatedValue => new ServiceRequestTypeId(validatedValue));
    }
    public static Result<ServiceRequestTypeId> Create(Guid value, PropertyPath propertyPath, string member)
    {
        return Result.NotEmpty(value, propertyPath, member)
            .Map(validatedValue => new ServiceRequestTypeId(validatedValue));
    }

    private ServiceRequestTypeId(Guid value) : base(value) { }
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

[Alias("Orders.FormTypeId.V1"), GenerateSerializer, Immutable]
public sealed record FormTypeId : GuidValueObject<FormTypeId>
{
    public static Result<FormTypeId> Create(string? value, string propertyName = nameof(FormTypeId))
    {
        return Result.IsGuidAndNotEmpty(value, propertyName)
            .Map(validatedValue => new FormTypeId(validatedValue));
    }
    public static Result<FormTypeId> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsGuidAndNotEmpty(value, propertyPath, member)
            .Map(validatedValue => new FormTypeId(validatedValue));
    }
    public static Result<FormTypeId> Create(Guid? value, string propertyName = nameof(FormTypeId))
    {
        return Result.NotNull(value, propertyName)
            .NotEmpty(propertyName)
            .Map(validatedValue => new FormTypeId(validatedValue));
    }
    public static Result<FormTypeId> Create(Guid? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .NotEmpty(propertyPath, member)
            .Map(validatedValue => new FormTypeId(validatedValue));
    }
    public static Result<FormTypeId> Create(Guid value, string propertyName = nameof(FormTypeId))
    {
        return Result.NotEmpty(value, propertyName)
            .Map(validatedValue => new FormTypeId(validatedValue));
    }
    public static Result<FormTypeId> Create(Guid value, PropertyPath propertyPath, string member)
    {
        return Result.NotEmpty(value, propertyPath, member)
            .Map(validatedValue => new FormTypeId(validatedValue));
    }

    private FormTypeId(Guid value) : base(value) { }
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
public sealed record IntendedUseId : GuidValueObject<IntendedUseId>
{
    public static Result<IntendedUseId> Create(string? value, string propertyName = nameof(IntendedUseId))
    {
        return Result.IsGuidAndNotEmpty(value, propertyName)
            .Map(validatedValue => new IntendedUseId(validatedValue));
    }
    public static Result<IntendedUseId> Create(string? value, PropertyPath propertyPath, string member)
    {
        return Result.IsGuidAndNotEmpty(value, propertyPath, member)
            .Map(validatedValue => new IntendedUseId(validatedValue));
    }
    public static Result<IntendedUseId> Create(Guid? value, string propertyName = nameof(IntendedUseId))
    {
        return Result.NotNull(value, propertyName)
            .NotEmpty(propertyName)
            .Map(validatedValue => new IntendedUseId(validatedValue));
    }
    public static Result<IntendedUseId> Create(Guid? value, PropertyPath propertyPath, string member)
    {
        return Result.NotNull(value, propertyPath, member)
            .NotEmpty(propertyPath, member)
            .Map(validatedValue => new IntendedUseId(validatedValue));
    }
    public static Result<IntendedUseId> Create(Guid value, string propertyName = nameof(IntendedUseId))
    {
        return Result.NotEmpty(value, propertyName)
            .Map(validatedValue => new IntendedUseId(validatedValue));
    }
    public static Result<IntendedUseId> Create(Guid value, PropertyPath propertyPath, string member)
    {
        return Result.NotEmpty(value, propertyPath, member)
            .Map(validatedValue => new IntendedUseId(validatedValue));
    }

    private IntendedUseId(Guid value) : base(value) { }
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
