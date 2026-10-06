using Azure.Data.Tables;

using Continuum.Streaming;
using Continuum.Streaming.Orleans.Azure.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Continuum.Orleans.Hosting;

public static class AzureTableCheckpointStoreBuilderExtensions
{
    public static IHostApplicationBuilder AddAzureTableCheckpointStore(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.Services.AddKeyedSingleton<ICheckpointStore<ulong>>(connectionName, (provider, key) =>
            {
                var client = provider.GetRequiredKeyedService<TableServiceClient>(connectionName);
                return new AzureTableCheckpointStore(client);
            });
        return builder;
    }

    public static IHostApplicationBuilder AddAzureTableCheckpointStoreAsDefault(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.AddAzureTableCheckpointStore(connectionName)
            .Services.AddSingleton<ICheckpointStore<ulong>>(provider =>
            {
                return provider.GetRequiredKeyedService<ICheckpointStore<ulong>>(connectionName);
            });
        return builder;
    }
}
