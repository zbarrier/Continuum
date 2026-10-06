using System.ComponentModel.DataAnnotations;

using Continuum.Streaming.Orleans.KurrentDB.Monitors;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Continuum.Streaming.Orleans.KurrentDB.Configuration;

public class KurrentDBCatchupSubscriptionOptions
{
    [Required]
    public bool Enabled { get; set; } = false;

    [Required]
    public string ConnectionName { get; set; } = default!;

    [Required]
    public KurrentDBChannelBufferOptions BufferOptions { get; set; } = new();

    [Required]
    public KurrentDBCheckpointMonitorOptions CheckpointMonitorOptions { get; set; } = new();

    public KurrentDBSubscriptionCredentialsOptions Credentials { get; set; } = new();

    [Required]
    public string? CheckpointConnectionName { get; set; } = default!;

    [Required]
    public ICheckpointStore<ulong> CheckpointStore { get; set; } = default!;

    [Required]
    public IStreamedEventSerde StreamEventSerde { get; set; } = default!;

    [Required]
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent;

    [Required]
    public ITypeMapper TypeMapper { get; set; } = default!;
}

public class KurrentDBChannelBufferOptions
{
    [Required, Range(1, 100_000)]
    public int MaxEventsPerProcessLoop { get; set; } = 100;

    [Required, Range(1, 1_000_000)]
    public int MaxUnprocessedEventsBeforeStop { get; set; } = 1_000;
}



public class DefaultKurrentDBChannelCatchupSubscriptionOptionsConfigurator : IPostConfigureOptions<KurrentDBCatchupSubscriptionOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultKurrentDBChannelCatchupSubscriptionOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PostConfigure(string? name, KurrentDBCatchupSubscriptionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionName))
        {
            // Validator will throw an exception, so we will let it handle it.
            return;
        }
        if (options.CheckpointStore is null)
        {
            options.CheckpointStore = string.IsNullOrWhiteSpace(options.CheckpointConnectionName) 
                ? _serviceProvider.GetRequiredService<ICheckpointStore<ulong>>()
                : _serviceProvider.GetRequiredKeyedService<ICheckpointStore<ulong>>(options.CheckpointConnectionName);
        }
        if (options.StreamEventSerde is null)
        {
            options.StreamEventSerde = _serviceProvider.GetRequiredKeyedService<IStreamedEventSerde>(options.ConnectionName);
        }
        if (options.TypeMapper is null)
        {
            string key = options.TypeMapKinds.ToTypeMapperKey(options.ConnectionName);
            options.TypeMapper = _serviceProvider.GetRequiredKeyedService<ITypeMapper>(key);
        }
    }
}