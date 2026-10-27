using Continuum.Orleans.Tenancy;

using Microsoft.Extensions.Logging;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

public class TenantStorageFailureLoggerTests
{
    private readonly ManualTimeProvider _time = new();
    private readonly CapturingLogger _logger = new();

    private TenantStorageFailureLogger CreateSut() => new(_logger, "provider", _time);

    [Fact]
    public void Every_failure_is_logged_as_an_error_during_the_full_logging_period()
    {
        var sut = CreateSut();

        for (var i = 0; i < 10; i++)
        {
            sut.Failed("acme", new InvalidOperationException("boom"));
            _time.Advance(TimeSpan.FromSeconds(1));
        }

        Assert.Equal(10, _logger.Entries.Count(e => e.Level == LogLevel.Error));
        Assert.All(_logger.Entries, e => Assert.Contains("acme", e.Message));
        Assert.All(_logger.Entries, e => Assert.IsType<InvalidOperationException>(e.Exception));
    }

    [Fact]
    public void Failures_are_throttled_after_the_full_logging_period()
    {
        var sut = CreateSut();
        sut.Failed("acme", new InvalidOperationException("boom"));
        _time.Advance(TenantStorageFailureLogger.FullLoggingPeriod);
        _logger.Entries.Clear();

        sut.Failed("acme", new InvalidOperationException("boom"));
        sut.Failed("acme", new InvalidOperationException("boom"));
        sut.Failed("acme", new InvalidOperationException("boom"));
        Assert.Single(_logger.Entries);

        _time.Advance(TenantStorageFailureLogger.ThrottledLoggingInterval);
        sut.Failed("acme", new InvalidOperationException("boom"));

        Assert.Equal(2, _logger.Entries.Count);
        Assert.Contains("2 not logged", _logger.Entries[1].Message);
    }

    [Fact]
    public void Tenants_are_throttled_independently()
    {
        var sut = CreateSut();
        sut.Failed("acme", new InvalidOperationException("boom"));
        _time.Advance(TenantStorageFailureLogger.FullLoggingPeriod);
        sut.Failed("acme", new InvalidOperationException("boom"));
        _logger.Entries.Clear();

        sut.Failed("acme", new InvalidOperationException("boom"));
        sut.Failed("contoso", new InvalidOperationException("boom"));
        sut.Failed("contoso", new InvalidOperationException("boom"));

        Assert.Equal(2, _logger.Entries.Count);
        Assert.All(_logger.Entries, e => Assert.Contains("contoso", e.Message));
    }

    [Fact]
    public void Success_logs_recovery_and_resets_the_failure_period()
    {
        var sut = CreateSut();
        sut.Failed("acme", new InvalidOperationException("boom"));
        _time.Advance(TenantStorageFailureLogger.FullLoggingPeriod);

        sut.Succeeded("acme");
        Assert.Equal(LogLevel.Information, _logger.Entries[^1].Level);

        _logger.Entries.Clear();
        sut.Failed("acme", new InvalidOperationException("boom"));
        sut.Failed("acme", new InvalidOperationException("boom"));
        Assert.Equal(2, _logger.Entries.Count);
    }

    [Fact]
    public void Success_without_prior_failure_does_not_log()
    {
        CreateSut().Succeeded("acme");

        Assert.Empty(_logger.Entries);
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

    private sealed class CapturingLogger : ILogger
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }
}
