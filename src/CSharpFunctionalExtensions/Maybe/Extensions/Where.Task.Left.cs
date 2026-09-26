using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        public static async Task<Maybe<T>> Where<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate)
        {
            var maybe = await maybeTask.DefaultAwait();
            return maybe.Where(predicate);
        }
    }
}