namespace Orleans.Streaming.KurrentDBStorage;

/// <summary>
///     An interface defining the required properties for a stream state model.
///     Custom state model types must implement this interface.
/// </summary>
/// <remarks>
///     Two options exist for implementations of <see cref="IKurrentDBState" />:
///     Strongly typed custom state model classes.
/// </remarks>
public interface IKurrentDBState
{
    // /// <summary>
    // ///     The Id value for the state.
    // /// </summary>
    // Guid Id { get; set; }

    /// <summary>
    ///     The ETag value for the state.
    /// </summary>
    string ETag { get; set; }
}
