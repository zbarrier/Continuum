namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     Creates the transaction ID that is stored on every entry of one log append.
/// </summary>
/// <remarks>
///     A new ID is created for every append attempt. It lets a writer tell whether an append whose outcome is unknown
///     (for example, after a timeout) was actually committed. Register a custom implementation in the service collection
///     to replace the default, which uses <see cref="Guid.CreateVersion7()" />.
/// </remarks>
public interface ITransactionIdGenerator
{
    /// <summary>
    ///     Creates a new, unique transaction ID.
    /// </summary>
    Guid NewTransactionId();
}

/// <summary>
///     The default <see cref="ITransactionIdGenerator" />, which uses <see cref="Guid.CreateVersion7()" />.
/// </summary>
public sealed class DefaultTransactionIdGenerator : ITransactionIdGenerator
{
    /// <inheritdoc />
    public Guid NewTransactionId() => Guid.CreateVersion7();
}
