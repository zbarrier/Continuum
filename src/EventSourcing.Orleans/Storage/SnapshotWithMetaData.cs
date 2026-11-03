using System.Diagnostics.CodeAnalysis;

namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     A class that extends snapshot state with versioning metadata, so that a grain with log-view consistency
///     can use a standard storage provider via <see cref="LogViewAdaptor{TView,TEntry}" />
/// </summary>
/// <typeparam name="TView">The type used for log view</typeparam>
[GenerateSerializer]
public sealed class SnapshotWithMetaDataAndETag<TView> : IGrainState<SnapshotWithMetaData<TView>>
    where TView : class, new()
{
    /// <summary>
    ///     Initializes a new instance of SnapshotWithMetaDataAndETag class
    /// </summary>
    public SnapshotWithMetaDataAndETag()
    {
        State = new SnapshotWithMetaData<TView>();
    }

    /// <summary>
    ///     Initialize a new instance of SnapshotWithMetaDataAndETag class with an initialView
    /// </summary>
    /// <param name="initialView"></param>
    public SnapshotWithMetaDataAndETag(TView initialView)
    {
        State = new SnapshotWithMetaData<TView>(initialView);
    }

    /// <inheritdoc />
    [Id(0)]
    public string? ETag { get; set; }

    /// <inheritdoc />
    [Id(1)]
    public bool RecordExists { get; set; }

    /// <inheritdoc />
    // Orleans may assign null when a provider clears state; normalize to a fresh snapshot so State is never null.
    [Id(2)]
    [AllowNull]
    public SnapshotWithMetaData<TView> State
    {
        get => _state;
        set => _state = value ?? new SnapshotWithMetaData<TView>();
    }

    [NonSerialized]
    private SnapshotWithMetaData<TView> _state = null!;

    /// <summary>
    ///     Convert current SnapshotWithMetaDataAndETag object information to a string.
    /// </summary>
    public override string ToString()
    {
        return $"v{State.SnapshotVersion} ETag={ETag} Data={State.Snapshot}";
    }
}

/// <summary>
///     A class that extends grain state with versioning metadata, so that a log-consistent grain
///     can use a standard storage provider via <see cref="LogViewAdaptor{TView,TEntry}" />
/// </summary>
/// <typeparam name="TView"></typeparam>
[GenerateSerializer]
public sealed class SnapshotWithMetaData<TView>
    where TView : class, new()
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SnapshotWithMetaData{TView}" /> class.
    /// </summary>
    public SnapshotWithMetaData()
    {
        Snapshot = new TView();
        SnapshotVersion = -1;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SnapshotWithMetaData{TView}" /> class.
    /// </summary>
    public SnapshotWithMetaData(TView initialState)
    {
        Snapshot = initialState;
        SnapshotVersion = -1;
    }

    /// <summary>
    ///     The stored snapshot view of the log.
    /// </summary>
    [Id(0)]
    public TView Snapshot { get; set; }

    /// <summary>
    ///     The snapshot length of the log.
    /// </summary>
    [Id(1)]
    public int SnapshotVersion { get; set; }
}
