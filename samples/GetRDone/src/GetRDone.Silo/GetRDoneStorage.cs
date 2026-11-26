namespace GetRDone.Silo;

/// <summary>
///     Storage names the silo configures. The event-sourced grains name the same provider and connection in their
///     <c>[LogConsistencyProvider]</c> attributes and constructors.
/// </summary>
public static class GetRDoneStorage
{
    /// <summary>Log-consistency provider used by event-sourced grains.</summary>
    public const string EventLogProvider = "EventLog";

    /// <summary>Connection name (and Aspire resource name) of the shared KurrentDB database.</summary>
    public const string DefaultConnectionName = "kurrentdb";
}
