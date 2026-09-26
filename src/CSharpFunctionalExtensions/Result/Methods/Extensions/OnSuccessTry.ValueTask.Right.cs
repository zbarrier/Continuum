namespace Continuum.CSharpFunctionalExtensions.ValueTasks;

public static partial class ResultExtensions
{
    public static async ValueTask<Result> OnSuccessTry(this Result result, Func<ValueTask> func,
        Func<Exception, Error> errorHandler = null) 
        => result.IsFailure
            ? result
            : await Result.Try(func, errorHandler).DefaultAwait();

    public static async ValueTask<Result> OnSuccessTry<T>(this Result<T> result, Func<T, ValueTask> func,
        Func<Exception, Error> errorHandler = null) 
        => result.IsFailure
            ? Result.Failure(result.Error)
            : await Result.Try(() => func.Invoke(result.Value), errorHandler).DefaultAwait();
}
