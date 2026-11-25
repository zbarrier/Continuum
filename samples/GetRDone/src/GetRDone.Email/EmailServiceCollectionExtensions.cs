using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GetRDone.Email;

public static class EmailServiceCollectionExtensions
{
    /// <summary>Registers <see cref="SmtpEmailSender"/> as the <see cref="IEmailSender"/>.</summary>
    public static IServiceCollection AddSmtpEmailSender(this IServiceCollection services)
    {
        services.TryAddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
