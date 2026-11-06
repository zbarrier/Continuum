using System.Data.Common;
using System.Net.Mail;

using GetRDone.Identity.Data;

using Microsoft.AspNetCore.Identity;

namespace GetRDone.Identity.Email;

/// <summary>Sends account emails over SMTP. Locally this is Mailpit, configured by the "mailpit" connection string (Endpoint=smtp://host:port).</summary>
public sealed class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    private const string FromAddress = "no-reply@getrdone.local";

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your Get-R-Done account",
            $"<p>Hi {Encode(user.DisplayName)},</p><p>Please confirm your account by <a href=\"{Encode(confirmationLink)}\">clicking here</a>.</p>");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendAsync(email, "Reset your Get-R-Done password",
            $"<p>Hi {Encode(user.DisplayName)},</p><p>Reset your password by <a href=\"{Encode(resetLink)}\">clicking here</a>. If you didn't request this, you can ignore this email.</p>");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsync(email, "Your Get-R-Done password reset code",
            $"<p>Hi {Encode(user.DisplayName)},</p><p>Your password reset code is <strong>{Encode(resetCode)}</strong>.</p>");

    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        var endpoint = GetEndpoint();
        using var client = new SmtpClient(endpoint.Host, endpoint.Port);
        using var message = new MailMessage(FromAddress, to, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(message);
        logger.LogInformation("Sent account email '{Subject}'", subject);
    }

    private Uri GetEndpoint()
    {
        var connectionString = configuration.GetConnectionString("mailpit")
            ?? throw new InvalidOperationException("The 'mailpit' connection string was not provided.");
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        return builder.TryGetValue("Endpoint", out var endpoint) && Uri.TryCreate(endpoint?.ToString(), UriKind.Absolute, out var uri)
            ? uri
            : throw new InvalidOperationException("The 'mailpit' connection string must contain Endpoint=smtp://host:port.");
    }

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);
}
