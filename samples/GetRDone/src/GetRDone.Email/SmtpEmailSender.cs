using System.Data.Common;
using System.Net.Mail;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GetRDone.Email;

/// <summary>
///     Sends emails over SMTP. Locally this is Mailpit, configured by the <see cref="ConnectionName"/> connection string
///     (<c>Endpoint=smtp://host:port</c>) that the AppHost provides.
/// </summary>
public sealed partial class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    /// <summary>The name of the connection string that holds the SMTP endpoint.</summary>
    public const string ConnectionName = "mailpit";

    private const string FromAddress = "no-reply@getrdone.local";

    private readonly ILogger<SmtpEmailSender> _logger = logger;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var endpoint = GetEndpoint();
        using var client = new SmtpClient(endpoint.Host, endpoint.Port);
        using var mail = new MailMessage(FromAddress, message.To, message.Subject, message.HtmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(mail, cancellationToken);
        LogSent(message.Subject);
    }

    private Uri GetEndpoint()
    {
        var connectionString = configuration.GetConnectionString(ConnectionName)
            ?? throw new InvalidOperationException($"The '{ConnectionName}' connection string was not provided.");
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        return builder.TryGetValue("Endpoint", out var endpoint) && Uri.TryCreate(endpoint?.ToString(), UriKind.Absolute, out var uri)
            ? uri
            : throw new InvalidOperationException($"The '{ConnectionName}' connection string must contain Endpoint=smtp://host:port.");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Sent email '{Subject}'")]
    private partial void LogSent(string subject);
}
