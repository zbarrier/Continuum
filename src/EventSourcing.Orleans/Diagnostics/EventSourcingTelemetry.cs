namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     Names of the telemetry sources published by Continuum event-sourcing storage providers.
///     Register them with OpenTelemetry (for example in Aspire service defaults) to collect the data:
///     <code>
///     builder.Services.AddOpenTelemetry()
///         .WithMetrics(metrics =&gt; metrics.AddMeter(EventSourcingTelemetry.MeterName))
///         .WithTracing(tracing =&gt; tracing.AddSource(EventSourcingTelemetry.ActivitySourceName));
///     </code>
/// </summary>
public static class EventSourcingTelemetry
{
    /// <summary>
    ///     The name of the <see cref="System.Diagnostics.Metrics.Meter"/> that publishes storage metrics.
    /// </summary>
    public const string MeterName = "Continuum.EventSourcing";

    /// <summary>
    ///     The name of the <see cref="System.Diagnostics.ActivitySource"/> that publishes storage spans.
    /// </summary>
    public const string ActivitySourceName = "Continuum.EventSourcing";
}
