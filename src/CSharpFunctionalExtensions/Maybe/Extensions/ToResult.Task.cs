using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        public static async Task<Result<T>> ToResult<T>(this Task<Maybe<T>> maybeTask, Error error)
        {
            var maybe = await maybeTask.DefaultAwait();
            return maybe.ToResult(error);
        }

        public static async Task<Result<T>> ToResult<T>(this Task<Maybe<T>> maybeTask, Func<Error> errorFunc)
        {
            var maybe = await maybeTask.DefaultAwait();
            return maybe.ToResult(errorFunc);
        }
    }
}