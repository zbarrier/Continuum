namespace Continuum.CSharpFunctionalExtensions;
public static partial class ResultExtensions
{
    /// <summary>
    /// The optional value is required, so convert to a failed result if there's no value.
    /// </summary>
    public static Result<T> Required<T>(this Result<Maybe<T>> result, Error error) =>
        result.Bind(value => value.ToResult(error));
}
