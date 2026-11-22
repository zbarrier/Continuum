using Continuum.CSharpFunctionalExtensions;

namespace GetRDone.Orders.Abstractions;

// Compound value objects group simple value objects that are only meaningful together. Events still store the flat
// primitive values. Each Create validates every part and reports each invalid part under its own flat name (e.g.,
// PropertyStreet), so the errors line up with the command's properties. The PropertyPath overloads prefix those
// names with the path to the object that holds the values.

/// <summary>The location of the property being appraised.</summary>
[Alias("Orders.PropertyAddress.V1"), GenerateSerializer, Immutable]
public sealed record PropertyAddress
{
    [Id(0)] public PropertyStreet Street { get; }
    [Id(1)] public PropertyCity City { get; }
    [Id(2)] public PropertyState State { get; }
    [Id(3)] public PropertyZip Zip { get; }

    public static Result<PropertyAddress> Create(string? street, string? city, string? state, string? zip)
    {
        return Result.CombineForValidation(
                PropertyStreet.Create(street).Deconstruct(out var validStreet),
                PropertyCity.Create(city).Deconstruct(out var validCity),
                PropertyState.Create(state).Deconstruct(out var validState),
                PropertyZip.Create(zip).Deconstruct(out var validZip))
            .Map(() => new PropertyAddress(validStreet!, validCity!, validState!, validZip!));
    }
    public static Result<PropertyAddress> Create(string? street, string? city, string? state, string? zip, PropertyPath propertyPath)
    {
        return Result.CombineForValidation(
                PropertyStreet.Create(street, propertyPath, nameof(PropertyStreet)).Deconstruct(out var validStreet),
                PropertyCity.Create(city, propertyPath, nameof(PropertyCity)).Deconstruct(out var validCity),
                PropertyState.Create(state, propertyPath, nameof(PropertyState)).Deconstruct(out var validState),
                PropertyZip.Create(zip, propertyPath, nameof(PropertyZip)).Deconstruct(out var validZip))
            .Map(() => new PropertyAddress(validStreet!, validCity!, validState!, validZip!));
    }

    private PropertyAddress(PropertyStreet street, PropertyCity city, PropertyState state, PropertyZip zip)
    {
        Street = street;
        City = city;
        State = state;
        Zip = zip;
    }
}

/// <summary>
///     The person who gives the appraiser access to the property. The access contact is optional, but its name and phone
///     number go together: when either is given, both are required.
/// </summary>
[Alias("Orders.AccessContact.V1"), GenerateSerializer, Immutable]
public sealed record AccessContact
{
    [Id(0)] public AccessContactName Name { get; }
    [Id(1)] public AccessContactPhone Phone { get; }

    /// <summary>Creates the access contact, or returns <see langword="null"/> when both the name and phone are empty.</summary>
    public static Result<AccessContact?> CreateOptional(string? name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phone))
            return Result.Success<AccessContact?>(null);

        return Result.CombineForValidation(
                AccessContactName.Create(name).Deconstruct(out var validName),
                AccessContactPhone.Create(phone).Deconstruct(out var validPhone))
            .Map(() => (AccessContact?)new AccessContact(validName!, validPhone!));
    }
    /// <summary>Creates the access contact, or returns <see langword="null"/> when both the name and phone are empty.</summary>
    public static Result<AccessContact?> CreateOptional(string? name, string? phone, PropertyPath propertyPath)
    {
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phone))
            return Result.Success<AccessContact?>(null);

        return Result.CombineForValidation(
                AccessContactName.Create(name, propertyPath, nameof(AccessContactName)).Deconstruct(out var validName),
                AccessContactPhone.Create(phone, propertyPath, nameof(AccessContactPhone)).Deconstruct(out var validPhone))
            .Map(() => (AccessContact?)new AccessContact(validName!, validPhone!));
    }

    private AccessContact(AccessContactName name, AccessContactPhone phone)
    {
        Name = name;
        Phone = phone;
    }
}

/// <summary>A selected version of a service request type, identified by its ID, version, and name.</summary>
[Alias("Orders.ServiceRequestTypeRef.V1"), GenerateSerializer, Immutable]
public sealed record ServiceRequestTypeRef
{
    [Id(0)] public ServiceRequestTypeId Id { get; }
    [Id(1)] public ServiceRequestTypeVersion Version { get; }
    [Id(2)] public ServiceRequestTypeName Name { get; }

    public static Result<ServiceRequestTypeRef> Create(string? id, int version, string? name, string? tenantId)
    {
        return Result.CombineForValidation(
                ServiceRequestTypeId.Create(id, tenantId).Deconstruct(out var validId),
                ServiceRequestTypeVersion.Create(version).Deconstruct(out var validVersion),
                ServiceRequestTypeName.Create(name).Deconstruct(out var validName))
            .Map(() => new ServiceRequestTypeRef(validId!, validVersion!, validName!));
    }
    public static Result<ServiceRequestTypeRef> Create(string? id, int version, string? name, string? tenantId, PropertyPath propertyPath)
    {
        return Result.CombineForValidation(
                ServiceRequestTypeId.Create(id, tenantId, propertyPath, nameof(ServiceRequestTypeId)).Deconstruct(out var validId),
                ServiceRequestTypeVersion.Create(version, propertyPath, nameof(ServiceRequestTypeVersion)).Deconstruct(out var validVersion),
                ServiceRequestTypeName.Create(name, propertyPath, nameof(ServiceRequestTypeName)).Deconstruct(out var validName))
            .Map(() => new ServiceRequestTypeRef(validId!, validVersion!, validName!));
    }

    private ServiceRequestTypeRef(ServiceRequestTypeId id, ServiceRequestTypeVersion version, ServiceRequestTypeName name)
    {
        Id = id;
        Version = version;
        Name = name;
    }
}

/// <summary>A selected version of a form type, identified by its ID, version, and name.</summary>
[Alias("Orders.FormTypeRef.V1"), GenerateSerializer, Immutable]
public sealed record FormTypeRef
{
    [Id(0)] public FormTypeId Id { get; }
    [Id(1)] public FormTypeVersion Version { get; }
    [Id(2)] public FormTypeName Name { get; }

    public static Result<FormTypeRef> Create(string? id, int version, string? name, string? tenantId)
    {
        return Result.CombineForValidation(
                FormTypeId.Create(id, tenantId).Deconstruct(out var validId),
                FormTypeVersion.Create(version).Deconstruct(out var validVersion),
                FormTypeName.Create(name).Deconstruct(out var validName))
            .Map(() => new FormTypeRef(validId!, validVersion!, validName!));
    }
    public static Result<FormTypeRef> Create(string? id, int version, string? name, string? tenantId, PropertyPath propertyPath)
    {
        return Result.CombineForValidation(
                FormTypeId.Create(id, tenantId, propertyPath, nameof(FormTypeId)).Deconstruct(out var validId),
                FormTypeVersion.Create(version, propertyPath, nameof(FormTypeVersion)).Deconstruct(out var validVersion),
                FormTypeName.Create(name, propertyPath, nameof(FormTypeName)).Deconstruct(out var validName))
            .Map(() => new FormTypeRef(validId!, validVersion!, validName!));
    }

    private FormTypeRef(FormTypeId id, FormTypeVersion version, FormTypeName name)
    {
        Id = id;
        Version = version;
        Name = name;
    }
}

/// <summary>A selected version of an intended use, identified by its ID, version, and name.</summary>
[Alias("Orders.IntendedUseRef.V1"), GenerateSerializer, Immutable]
public sealed record IntendedUseRef
{
    [Id(0)] public IntendedUseId Id { get; }
    [Id(1)] public IntendedUseVersion Version { get; }
    [Id(2)] public IntendedUseName Name { get; }

    public static Result<IntendedUseRef> Create(string? id, int version, string? name, string? tenantId)
    {
        return Result.CombineForValidation(
                IntendedUseId.Create(id, tenantId).Deconstruct(out var validId),
                IntendedUseVersion.Create(version).Deconstruct(out var validVersion),
                IntendedUseName.Create(name).Deconstruct(out var validName))
            .Map(() => new IntendedUseRef(validId!, validVersion!, validName!));
    }
    public static Result<IntendedUseRef> Create(string? id, int version, string? name, string? tenantId, PropertyPath propertyPath)
    {
        return Result.CombineForValidation(
                IntendedUseId.Create(id, tenantId, propertyPath, nameof(IntendedUseId)).Deconstruct(out var validId),
                IntendedUseVersion.Create(version, propertyPath, nameof(IntendedUseVersion)).Deconstruct(out var validVersion),
                IntendedUseName.Create(name, propertyPath, nameof(IntendedUseName)).Deconstruct(out var validName))
            .Map(() => new IntendedUseRef(validId!, validVersion!, validName!));
    }

    private IntendedUseRef(IntendedUseId id, IntendedUseVersion version, IntendedUseName name)
    {
        Id = id;
        Version = version;
        Name = name;
    }
}
