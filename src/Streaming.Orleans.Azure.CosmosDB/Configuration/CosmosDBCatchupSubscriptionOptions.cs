using System.ComponentModel.DataAnnotations;

using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.Configuration;

public enum StartingPositionType
{
    BeginningOfTime,
    FromDateTime,
    Latest
}

public class CosmosDBCatchupSubscriptionOptions
{
    [Required]
    public bool Enabled { get; set; } = false;

    [Required]
    public string ConnectionName { get; set; } = default!;

    [Required]
    public string DatabaseName { get; set; } = default!;

    [Required]
    public string LeaseContainerName { get; set; } = default!;

    [Required]
    public string MonitoredContainerName { get; set; } = default!;

    [Required]
    public string InstanceName { get; set; } = default!;

    [Required]
    public StartingPositionType StartingPositionType { get; set; } = StartingPositionType.BeginningOfTime;

    public DateTime? StartingPositionDateTime { get; set; } = null;

    [Required, Range(100, 60000)]
    public int PollingIntervalInMilliseconds { get; set; } = 5000;

    [Required, Range(1, 10000)]
    public int MaxItemsPerPoll { get; set; } = 500;

    [Required]
    public bool IgnoreHeaderEvents { get; set; } = true;

    [Required]
    public JsonSerializer JsonSerializer { get; set; } = default!;

    [Required]
    public IStreamedNameParser StreamNameParser { get; set; } = default!;

    [Required]
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent | TypeMapKinds.Metadata;

    [Required]
    public ITypeMapper TypeMapper { get; set; } = default!;
}

public class DefaultCosmosDBCatchupSubscriptionOptionsConfigurator : IPostConfigureOptions<CosmosDBCatchupSubscriptionOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultCosmosDBCatchupSubscriptionOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PostConfigure(string? name, CosmosDBCatchupSubscriptionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionName))
        {
            // Validator will throw an exception, so we will let it handle it.
            return;
        }
        if (options.JsonSerializer is null)
        {
            options.JsonSerializer = JsonSerializer.CreateDefault();
        }
        if (options.StreamNameParser is null)
        {
            options.StreamNameParser = _serviceProvider.GetKeyedService<IStreamedNameParser>(options.ConnectionName) ?? new DefaultCosmosDBStreamNameParser();
        }
        if (options.TypeMapper is null)
        {
            string key = options.TypeMapKinds.ToTypeMapperKey(options.ConnectionName);
            options.TypeMapper = _serviceProvider.GetRequiredKeyedService<ITypeMapper>(key);
        }
    }
}