using System.Text.Json.Serialization;

using Continuum;
using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.Serialization.Orleans;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Storage;

// Exercises Continuum.Core and Continuum.Core.Orleans under Native AOT. Publish with:
//   dotnet publish tests/Core.AotSmokeTest -c Release
// then run the produced executable. A non-zero exit code means a check failed.

var failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition) failures++;
}

// Source-generated registrations (no reflection scanning)
var registrations = TypeMapRegistry.GetRegistrations(TypeMapKinds.DomainEvent | TypeMapKinds.Snapshot);
Check("Generator registered mapped types", registrations.Any(r => r.Type == typeof(OrderPlaced)) && registrations.Any(r => r.Type == typeof(OrderSnapshot)));

// DI-built, frozen type mappers
const string connection = "store";
var services = new ServiceCollection()
    .AddDefaultTypeMapAttributeMappers(connection, TypeMapKinds.DomainEvent | TypeMapKinds.Snapshot | TypeMapKinds.Metadata)
    .AddTypeMappedJsonGrainStorageSerializer(connection, typeInfoResolver: SmokeJsonContext.Default)
    .BuildServiceProvider();

var mapper = services.GetRequiredKeyedService<ITypeMapper>(connection);
Check("Mapper is frozen", mapper.IsFrozen);
Check("Type -> name", mapper.GetTypeName<OrderPlaced>() == "smoke.order-placed");
Check("Name -> type", mapper.GetType("smoke.order-snapshot") == typeof(OrderSnapshot));
Check("Metadata mapped", mapper.GetTypeName<EventMetadata>() == "smoke.event-metadata");

var eventsOnly = services.GetRequiredKeyedService<ITypeMapper>(TypeMapKinds.DomainEvent.ToTypeMapperKey(connection));
Check("Lazy sub-mapper filters kinds", eventsOnly.IsTypeRegistered<OrderPlaced>() && !eventsOnly.IsTypeRegistered<OrderSnapshot>());

// Serializer round-trip via source-generated JSON metadata, resolving the CLR type from the mapper
var serializer = services.GetRequiredKeyedService<IGrainStorageSerializer>(connection);
object original = new OrderPlaced(NewId.NextGuid(), 42.5m, TimeSpan.FromMinutes(90));
var payload = serializer.Serialize(original);
var roundTripped = serializer.Deserialize<object>(new BinaryDataWithType(payload.ToMemory(), mapper.GetType(mapper.GetTypeName(original.GetType()))));
Check("Serializer round-trip", roundTripped is OrderPlaced placed && placed == (OrderPlaced)original);

var snapshot = new OrderSnapshot { Total = 10m, Lines = 3 };
var snapshotBack = serializer.Deserialize<OrderSnapshot>(new BinaryDataWithType(serializer.Serialize(snapshot).ToMemory(), typeof(OrderSnapshot)));
Check("Snapshot round-trip", snapshotBack is { Total: 10m, Lines: 3 });

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;

[DomainEventType("smoke.order-placed")]
internal sealed record OrderPlaced(Guid OrderId, decimal Amount, TimeSpan Duration);

[SnapshotType("smoke.order-snapshot")]
internal sealed class OrderSnapshot
{
    public decimal Total { get; set; }
    public int Lines { get; set; }
}

[MetadataType("smoke.event-metadata")]
internal sealed class EventMetadata;

[JsonSerializable(typeof(OrderPlaced))]
[JsonSerializable(typeof(OrderSnapshot))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext;
