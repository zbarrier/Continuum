namespace GetRDone.Email;

/// <summary>Sends the app's emails: account emails from GetRDone.Identity and notifications from the silo.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

/// <summary>An email to one recipient, with an HTML body.</summary>
public sealed record EmailMessage(string To, string Subject, string HtmlBody);
