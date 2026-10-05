using Microsoft.Extensions.DependencyInjection;

using Xunit.DependencyInjection.Logging;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddLogging(lb => lb.AddXunitOutput());
    }
}
