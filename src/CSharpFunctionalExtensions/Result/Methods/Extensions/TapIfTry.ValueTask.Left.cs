namespace Continuum.CSharpFunctionalExtensions.ValueTasks;

public static partial class AsyncResultExtensionsLeftOperand
{
    /// <summary>
    ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result> TapIfTry(this ValueTask<Result> resultTask, bool condition, Action action)
    {
        var result = await resultTask.DefaultAwait();
        return result.TapIfTry(condition, action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result<T>> TapIfTry<T>(this ValueTask<Result<T>> resultTask, bool condition, Action action)
    {
        var result = await resultTask.DefaultAwait();
        return result.TapIfTry(condition, action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result<T>> TapIfTry<T>(this ValueTask<Result<T>> resultTask, bool condition, Action<T> action)
    {
        var result = await resultTask.DefaultAwait();
        return result.TapIfTry(condition, action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success and the predicate is true. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result<T>> TapIfTry<T>(this ValueTask<Result<T>> resultTask, Func<T, bool> predicate, Action action)
    {
        var result = await resultTask.DefaultAwait();
        return result.TapIfTry(predicate, action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success and the predicate is true. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result<T>> TapIfTry<T>(this ValueTask<Result<T>> resultTask, Func<T, bool> predicate, Action<T> action)
    {
        var result = await resultTask.DefaultAwait();
        return result.TapIfTry(predicate, action);
    }
}
