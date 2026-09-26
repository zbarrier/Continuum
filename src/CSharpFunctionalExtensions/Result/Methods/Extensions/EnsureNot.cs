namespace Continuum.CSharpFunctionalExtensions;
public static partial class ResultExtensions
{
    /// <summary>
    ///     Returns a new failure result if the predicate is true. Otherwise returns the starting result.
    /// </summary>
    public static Result<T> EnsureNot<T>(this Result<T> result, Func<T, bool> test, Error error) =>
        result.Ensure(v => !test(v), error);
}
