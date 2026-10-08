using System.Text.Json;
using System.Text.Json.Serialization;

using Continuum.Hosting.Orleans;
using Continuum.Serialization.Orleans;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Storage;

namespace Continuum.Orleans.Tests;

public sealed class StoredEvent
{
    public NewId Id { get; set; }
    public string Name { get; set; } = "";
    public TimeSpan Duration { get; set; }
}

[JsonSerializable(typeof(StoredEvent))]
public sealed partial class TestJsonContext : JsonSerializerContext;

public sealed class TypeMappedJsonGrainStorageSerializerTests
{
    private static readonly StoredEvent Sample = new() { Id = NewId.Next(), Name = "created", Duration = TimeSpan.FromMinutes(90) };

    private static StoredEvent RoundTrip(IGrainStorageSerializer serializer, object value)
    {
        var data = serializer.Serialize(value);
        return serializer.Deserialize<StoredEvent>(new BinaryDataWithType(data.ToMemory(), typeof(StoredEvent)))!;
    }

    private static void AssertEqual(StoredEvent expected, StoredEvent actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Duration, actual.Duration);
    }

    [Fact]
    public void RoundTrips_with_reflection_resolver()
    {
        var serializer = new TypeMappedJsonGrainStorageSerializer(TypeMappedJsonGrainStorageSerializer.DefaultOptions);

        AssertEqual(Sample, RoundTrip(serializer, Sample));
    }

    [Fact]
    public void RoundTrips_with_source_generated_resolver()
    {
        var serializer = new TypeMappedJsonGrainStorageSerializer(TypeMappedJsonGrainStorageSerializer.DefaultOptions, TestJsonContext.Default);

        AssertEqual(Sample, RoundTrip(serializer, Sample));
    }

    [Fact]
    public void Serializes_using_runtime_type()
    {
        var serializer = new TypeMappedJsonGrainStorageSerializer(TypeMappedJsonGrainStorageSerializer.DefaultOptions);

        var json = serializer.Serialize<object>(Sample).ToString();

        Assert.Contains("\"name\":\"created\"", json);
    }

    [Fact]
    public void Does_not_mutate_supplied_options()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        _ = new TypeMappedJsonGrainStorageSerializer(options, TestJsonContext.Default);

        Assert.Null(options.TypeInfoResolver);
        Assert.False(options.IsReadOnly);
    }

    [Fact]
    public void Serialize_null_throws()
    {
        var serializer = new TypeMappedJsonGrainStorageSerializer(TypeMappedJsonGrainStorageSerializer.DefaultOptions);

        Assert.Throws<ArgumentNullException>(() => serializer.Serialize<StoredEvent>(null));
    }

    [Fact]
    public void Deserialize_plain_BinaryData_uses_requested_type()
    {
        var serializer = new TypeMappedJsonGrainStorageSerializer(TypeMappedJsonGrainStorageSerializer.DefaultOptions);

        var restored = serializer.Deserialize<StoredEvent>(serializer.Serialize(Sample));

        Assert.Equal(Sample.Id, restored.Id);
        Assert.Equal(Sample.Name, restored.Name);
    }

    [Fact]
    public void Deserialize_requires_BinaryDataWithType_when_stored_type_required()
    {
        var serializer = new TypeMappedJsonGrainStorageSerializer(TypeMappedJsonGrainStorageSerializer.DefaultOptions, requireStoredType: true);

        Assert.Throws<ArgumentException>(() => serializer.Deserialize<StoredEvent>(serializer.Serialize(Sample)));
    }

    [Fact]
    public void Deserialize_null_payload_throws()
    {
        var serializer = new TypeMappedJsonGrainStorageSerializer(TypeMappedJsonGrainStorageSerializer.DefaultOptions);

        Assert.Throws<ArgumentException>(() => serializer.Deserialize<StoredEvent>(new BinaryDataWithType("null"u8.ToArray(), typeof(StoredEvent))));
    }

    [Fact]
    public void Extension_registers_keyed_serializer_with_resolver()
    {
        var provider = new ServiceCollection()
            .AddTypeMappedJsonGrainStorageSerializer("store", typeInfoResolver: TestJsonContext.Default)
            .BuildServiceProvider();

        var serializer = provider.GetRequiredKeyedService<IGrainStorageSerializer>("store");

        Assert.IsType<TypeMappedJsonGrainStorageSerializer>(serializer);
        AssertEqual(Sample, RoundTrip(serializer, Sample));
    }

    [Fact]
    public void Extension_creates_converters_from_service_provider()
    {
        IServiceProvider? received = null;
        var provider = new ServiceCollection()
            .AddTypeMappedJsonGrainStorageSerializer("store", sp =>
            {
                received = sp;
                return [];
            }, typeInfoResolver: TestJsonContext.Default)
            .BuildServiceProvider();

        var serializer = provider.GetRequiredKeyedService<IGrainStorageSerializer>("store");

        Assert.NotNull(received);
        AssertEqual(Sample, RoundTrip(serializer, Sample));
    }
}
