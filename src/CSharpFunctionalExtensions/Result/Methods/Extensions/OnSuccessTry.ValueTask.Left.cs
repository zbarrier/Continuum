namespace Continuum.CSharpFunctionalExtensions.ValueTasks;

public static partial class ResultExtensions
{
    public static async ValueTask<Result> OnSuccessTry(this ValueTask<Result> task, Action action,
        Func<Exception, Error> errorHandler = null)
    {
        var result = await task.DefaultAwait();
        return result.OnSuccessTry(action, errorHandler);
    }

    public static async ValueTask<Result> OnSuccessTry<T>(this ValueTask<Result<T>> task, Action<T> action,
        Func<Exception, Error> errorHandler = null)
    {
        var result = await task.DefaultAwait();
        return result.OnSuccessTry(action, errorHandler);
    }
}
