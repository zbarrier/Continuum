namespace Continuum.Orleans.KurrentDB;

/// <summary>
/// Thrown when a Continuum KurrentDB Orleans provider fails to read from or write to KurrentDB.
/// </summary>
[Alias("Continuum.KurrentDBStorageException.V1"), GenerateSerializer, Immutable]
public class KurrentDBStorageException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KurrentDBStorageException"/> class.
    /// </summary>
    public KurrentDBStorageException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KurrentDBStorageException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public KurrentDBStorageException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KurrentDBStorageException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public KurrentDBStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
