using Orleans;

namespace Continuum.Orleans;

/// <summary>
///     Orleans surrogate for <see cref="NewId"/>.
/// </summary>
[GenerateSerializer, Immutable, Alias("Continuum.NewId")]
public struct NewIdSurrogate
{
	/// <summary>The identifier as a sequential (lexicographically ordered) <see cref="Guid"/>.</summary>
	[Id(0)] public Guid SequentialGuid;
}

/// <summary>
///     Converts between <see cref="NewId"/> and <see cref="NewIdSurrogate"/>.
/// </summary>
[RegisterConverter]
public sealed class NewIdSurrogateConverter : IConverter<NewId, NewIdSurrogate>
{
	/// <inheritdoc/>
	public NewId ConvertFromSurrogate(in NewIdSurrogate surrogate) =>
		NewId.FromSequentialGuid(surrogate.SequentialGuid);

	/// <inheritdoc/>
	public NewIdSurrogate ConvertToSurrogate(in NewId value) =>
		new() { SequentialGuid = value.ToSequentialGuid() };
}
