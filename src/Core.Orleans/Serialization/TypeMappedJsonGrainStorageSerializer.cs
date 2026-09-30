using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Continuum.Converters.Json;

using Orleans.Storage;

namespace Continuum.Serialization.Orleans;

/// <summary>
/// System.Text.Json grain storage serializer that deserializes to the runtime type carried by
/// <see cref="BinaryDataWithType"/>, as required by type-mapped event-sourcing storage.
/// </summary>
/// <remarks>
/// For trimming and Native AOT, supply an <see cref="IJsonTypeInfoResolver"/> (typically a source-generated
/// <see cref="System.Text.Json.Serialization.JsonSerializerContext"/>) covering every persisted type.
/// Without one, reflection-based metadata is used when reflection-based serialization is enabled.
/// </remarks>
public sealed class TypeMappedJsonGrainStorageSerializer : IGrainStorageSerializer
{
    /// <summary>Web defaults plus <see cref="NewIdConverter"/>.</summary>
    public static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new NewIdConverter()
        }
    };

    private readonly JsonSerializerOptions _options;

    /// <param name="options">The serializer options. They are copied, so the instance passed in is not modified.</param>
    /// <param name="typeInfoResolver">
    /// Optional resolver providing JSON metadata for persisted types. Required when reflection-based serialization is disabled (Native AOT).
    /// </param>
    public TypeMappedJsonGrainStorageSerializer(JsonSerializerOptions options, IJsonTypeInfoResolver? typeInfoResolver = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = new JsonSerializerOptions(options)
        {
            TypeInfoResolver = typeInfoResolver ?? options.TypeInfoResolver ?? CreateReflectionResolver()
        };
        _options.MakeReadOnly();
    }

    /// <summary>
    /// Serializes the specified value using its runtime type.
    /// </summary>
    /// <typeparam name="T">The declared type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized payload.</returns>
    public BinaryData Serialize<T>(T? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var data = JsonSerializer.SerializeToUtf8Bytes(value, _options.GetTypeInfo(value.GetType()));
        return new BinaryData(data);
    }

    /// <summary>
    /// Deserializes the specified input payload. The input must be of type <see cref="BinaryDataWithType" />.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized object.</typeparam>
    /// <param name="input">The input to be deserialized. It should be of type <see cref="BinaryDataWithType" />.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is not a <see cref="BinaryDataWithType"/> or deserializes to null.</exception>
    public T Deserialize<T>(BinaryData input)
    {
        if (input is not BinaryDataWithType dataWithType)
        {
            throw new ArgumentException($"Input type must be of type '{nameof(BinaryDataWithType)}'.", nameof(input));
        }

        var obj = JsonSerializer.Deserialize(input.ToMemory().Span, _options.GetTypeInfo(dataWithType.Type))
            ?? throw new ArgumentException("Deserialized object is null.", nameof(input));
        return (T)obj;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Only reached when JsonSerializer.IsReflectionEnabledByDefault is true, which the trimmer and AOT compiler honor as a feature switch.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Only reached when JsonSerializer.IsReflectionEnabledByDefault is true, which the trimmer and AOT compiler honor as a feature switch.")]
    private static IJsonTypeInfoResolver CreateReflectionResolver()
        => JsonSerializer.IsReflectionEnabledByDefault
            ? new DefaultJsonTypeInfoResolver()
            : throw new InvalidOperationException(
                $"Reflection-based serialization is disabled. Provide an {nameof(IJsonTypeInfoResolver)} (for example a JsonSerializerContext) to {nameof(TypeMappedJsonGrainStorageSerializer)}.");
}
