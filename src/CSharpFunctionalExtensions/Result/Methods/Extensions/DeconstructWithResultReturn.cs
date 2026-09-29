namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Extracts the success value (or <see langword="default"/> on failure) and returns the original result for further chaining.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="value">The success value, or <see langword="default"/> when the result is a failure.</param>
    /// <returns><paramref name="result"/>, unchanged.</returns>
    public static Result<T> Deconstruct<T>(this Result<T> result, out T? value)
    {
        value = result.IsSuccess ? result.Value : default;
        return result;
    }
}
