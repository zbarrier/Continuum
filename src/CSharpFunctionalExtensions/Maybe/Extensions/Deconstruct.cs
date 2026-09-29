namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Deconstructs the instance into a presence flag and its value.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="result">The source instance.</param>
        /// <param name="hasValue"><see langword="true"/> when a value is present.</param>
        /// <param name="value">The inner value, or <see langword="default"/> when empty.</param>
        public static void Deconstruct<T>(in this Maybe<T> result, out bool hasValue, out T? value)
        {
            hasValue = result.HasValue;
            value = result.GetValueOrDefault();
        }
    }
}
