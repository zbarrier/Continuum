namespace Continuum.EventSourcing.Orleans.KurrentDB;

/// <summary>
///     Exception for throwing from KurrentDB log consistent storage.
/// </summary>
[GenerateSerializer, Immutable]
public class KurrentDBLogConsistentStorageException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="KurrentDBLogConsistentStorageException" />.
    /// </summary>
    public KurrentDBLogConsistentStorageException() { }

    /// <summary>
    ///     Initializes a new instance of <see cref="KurrentDBLogConsistentStorageException" />.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public KurrentDBLogConsistentStorageException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of <see cref="KurrentDBLogConsistentStorageException" />.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public KurrentDBLogConsistentStorageException(string message, Exception inner) : base(message, inner) { }
}
