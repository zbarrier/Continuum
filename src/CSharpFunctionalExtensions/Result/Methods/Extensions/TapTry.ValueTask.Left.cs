namespace Continuum.CSharpFunctionalExtensions.ValueTasks;

public static partial class AsyncResultExtensionsLeftOperand
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result> TapTry(this ValueTask<Result> resultTask, Action action)
    {
        var result = await resultTask.DefaultAwait();
        return result.TapTry(action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result<T>> TapTry<T>(this ValueTask<Result<T>> resultTask, Action action)
    {
        var result = await resultTask.DefaultAwait();
        return result.TapTry(action);
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static async ValueTask<Result<T>> TapTry<T>(this ValueTask<Result<T>> resultTask, Action<T> action)
    {
        var result = await resultTask.DefaultAwait();
        return result.TapTry(action);
    }
}
