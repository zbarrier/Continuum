namespace Orleans.Hosting;

/// <summary>
///     Records the name of each registered KurrentDB grain storage provider so startup checks can find them all.
/// </summary>
internal sealed record KurrentDBGrainStorageRegistration(string Name);
