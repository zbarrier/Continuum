namespace Orleans.Hosting;

/// <summary>
///     Configures a KurrentDB stream for a named service.
/// </summary>
public interface IKurrentDBStreamConfigurator : INamedServiceConfigurator
{
}

/// <summary>
///     Configures a KurrentDB stream for a silo and provides recovery options.
/// </summary>
public interface ISiloKurrentDBStreamConfigurator : IKurrentDBStreamConfigurator, ISiloRecoverableStreamConfigurator
{
}

/// <summary>
///     Configures a KurrentDB stream for a cluster client and provides persistent stream options.
/// </summary>
public interface IClusterClientKurrentDBStreamConfigurator : IKurrentDBStreamConfigurator, IClusterClientPersistentStreamConfigurator
{
}
