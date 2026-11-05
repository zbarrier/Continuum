using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.TypeMapping;

using GetRDone;

using ManagedCode.Orleans.Identity.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Aspire-provided resources: Azure Table storage for Orleans clustering and KurrentDB for event logs.
builder.AddKeyedAzureTableServiceClient("clustering");
builder.AddKeyedKurrentDBClient(GetRDoneStorage.DefaultConnectionName);

builder.Services.AddDefaultTypeMapAttributeMappers(GetRDoneStorage.DefaultConnectionName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
builder.AddTypeMappedJsonGrainStorageSerializer(GetRDoneStorage.DefaultConnectionName, requireStoredType: true);

// Every tenant shares the default database for now. Give a tenant its own ConnectionName to move it to a dedicated one.
builder.Services.AddMultitenantKurrentDBBasedLogConsistencyProvider(GetRDoneStorage.EventLogProvider,
    (tenantId, options) => { },
    ob => ob.Configure(options => options.ConnectionName = GetRDoneStorage.DefaultConnectionName));

// The Orleans Identity grain filter evaluates [Authorize] attributes using ASP.NET Core authorization services.
builder.Services.AddAuthorization();

builder.UseOrleans(silo =>
{
    // Orleans resolves a grain storage provider when activating journaled grains even though events are
    // stored by the KurrentDB log-consistency provider, so a default (unused) provider is required.
    silo.AddMemoryGrainStorageAsDefault();
    silo.AddOrleansIdentity();
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
