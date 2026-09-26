namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    public static Result<T> Deconstruct<T>(this Result<T> result, out T value)
    {
        value = result.IsSuccess ? result.Value : default;
        return result;
    }
}
