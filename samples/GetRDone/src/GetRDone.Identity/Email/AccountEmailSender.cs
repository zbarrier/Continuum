using System.Net;

using GetRDone.Email;
using GetRDone.Identity.Data;

using Microsoft.AspNetCore.Identity;

namespace GetRDone.Identity.Email;

/// <summary>Writes the account emails ASP.NET Core Identity sends and sends them with the app's <see cref="IEmailSender"/> (Mailpit locally).</summary>
public sealed class AccountEmailSender(IEmailSender emailSender) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your Get-R-Done account",
            $"<p>Hi {Encode(user.DisplayName)},</p><p>Please confirm your account by <a href=\"{Encode(confirmationLink)}\">clicking here</a>.</p>");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendAsync(email, "Reset your Get-R-Done password",
            $"<p>Hi {Encode(user.DisplayName)},</p><p>Reset your password by <a href=\"{Encode(resetLink)}\">clicking here</a>. If you didn't request this, you can ignore this email.</p>");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsync(email, "Your Get-R-Done password reset code",
            $"<p>Hi {Encode(user.DisplayName)},</p><p>Your password reset code is <strong>{Encode(resetCode)}</strong>.</p>");

    private Task SendAsync(string to, string subject, string htmlBody) => emailSender.SendAsync(new EmailMessage(to, subject, htmlBody));

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
