using System.Runtime.Serialization;

namespace Orleans.Streaming.KurrentDBStorage;

/// <summary>
///     Exception for throwing from KurrentDB stream storage.
/// </summary>
[GenerateSerializer]
public class KurrentDBStorageException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="KurrentDBStorageException" />.
    /// </summary>
    public KurrentDBStorageException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="KurrentDBStorageException" />.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public KurrentDBStorageException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="KurrentDBStorageException" />.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public KurrentDBStorageException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <inheritdoc />
    protected KurrentDBStorageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
