namespace GetRDone;

/// <summary>Storage names shared by the Silo host and grain implementations.</summary>
public static class GetRDoneStorage
{
    /// <summary>Log-consistency provider used by event-sourced grains.</summary>
    public const string EventLogProvider = "EventLog";

    /// <summary>Connection name (and Aspire resource name) of the shared KurrentDB database.</summary>
    public const string DefaultConnectionName = "kurrentdb";
}
