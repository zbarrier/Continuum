using Continuum.EventSourcing.Orleans;
using Continuum.EventSourcing.Orleans.KurrentDB.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Hosting;

// Exercises the Continuum.EventSourcing providers under Native AOT. Publish with:
//   dotnet publish tests/EventSourcing.AotSmokeTest -c Release
// then run the produced executable. A non-zero exit code means a check failed.
// Orleans DeepCopier/serializer setup is not Native AOT compatible upstream, so no silo is started here; the
// publish itself verifies the provider code is free of trim/AOT warnings.

var failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition) failures++;
}

// KurrentDB metadata codec round-trip (System.Text.Json reader/writer).
var metadata = KurrentDBEventMetadata.Create([new KeyValuePair<string, string?>("tenant", "acme")]);
var bytes = KurrentDBEventMetadataCodec.Write(metadata);
var read = bytes is null ? null : KurrentDBEventMetadataCodec.Read(bytes);
Check("KurrentDB metadata round-trip", read is not null && read.TryGetValue("tenant", out var tenant) && tenant == "acme");
Check("KurrentDB empty metadata reads", KurrentDBEventMetadataCodec.Read([]).Count == 0);

// Provider registration. Resolving the options needs Orleans silo services (serializer, type mapper),
// so only the registrations are checked here.
var services = new ServiceCollection();
services.AddAzureCosmosDBBasedLogConsistencyProvider("cosmos", o =>
{
    o.ConnectionName = "cosmos";
    o.DatabaseName = "orleans";
    o.ContainerName = "events";
});
services.AddKurrentDBBasedLogConsistencyProvider("kurrent", o => o.ConnectionName = "kurrent");
Check("CosmosDB provider registered", services.Any(s => s.ServiceType == typeof(IConfigureOptions<CosmosDBLogConsistentStorageOptions>)));
Check("KurrentDB provider registered", services.Any(s => s.ServiceType == typeof(IConfigureOptions<KurrentDBLogConsistentStorageOptions>)));

Check("Telemetry meter name", EventSourcingTelemetry.MeterName == "Continuum.EventSourcing");

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;
