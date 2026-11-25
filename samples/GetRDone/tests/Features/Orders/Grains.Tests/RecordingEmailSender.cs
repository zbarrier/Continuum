using System.Collections.Concurrent;

using GetRDone.Email;

namespace GetRDone.Orders.Grains.Tests;

/// <summary>Stands in for the SMTP sender in the test silo and keeps every email it is asked to send.</summary>
public sealed class RecordingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _sent = new();

    public IReadOnlyCollection<EmailMessage> Sent => _sent;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _sent.Enqueue(message);
        return Task.CompletedTask;
    }
}
