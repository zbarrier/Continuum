using Microsoft.Extensions.DependencyInjection;

using Xunit.DependencyInjection.Logging;

namespace Continuum.CSharpFunctionalExtensions.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddLogging(lb => lb.AddXunitOutput());
    }
}
