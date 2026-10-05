namespace Continuum.EventSourcing.Orleans.CosmosDB;

/// <summary>
///     Exception for throwing from CosmosDB log consistent storage.
/// </summary>
[GenerateSerializer, Immutable]
public class CosmosDBLogConsistentStorageException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="CosmosDBLogConsistentStorageException" />.
    /// </summary>
    public CosmosDBLogConsistentStorageException() { }

    /// <summary>
    ///     Initializes a new instance of <see cref="CosmosDBLogConsistentStorageException" />.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CosmosDBLogConsistentStorageException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of <see cref="CosmosDBLogConsistentStorageException" />.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public CosmosDBLogConsistentStorageException(string message, Exception inner) : base(message, inner) { }
}
