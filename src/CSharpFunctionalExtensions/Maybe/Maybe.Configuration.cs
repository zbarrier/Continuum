namespace Continuum.CSharpFunctionalExtensions
{
	/// <summary>
	///     Represents an optional value: either a value of type <typeparamref name="T"/> or no value.
	/// </summary>
	public readonly partial struct Maybe<T>
	{
		private static class Configuration
		{
			public static string NoValueException = "Maybe has no value.";			
		}
	}
}
