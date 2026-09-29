using Continuum.CSharpFunctionalExtensions.Internal;

namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Represents the outcome of an operation that does not return a value: success, or an <see cref="CSharpFunctionalExtensions.Error"/> on failure.
    /// </summary>
    public readonly partial struct Result : IResult, IError
    {
        /// <inheritdoc/>
        public bool IsFailure { get; }

        /// <inheritdoc/>
        public bool IsSuccess => !IsFailure;

        private readonly Error? _error;

        /// <summary>
        ///     Gets the error of a failed result.
        /// </summary>
        /// <exception cref="ResultSuccessException">The result is successful.</exception>
        public Error Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

        private Result(bool isFailure, Error? error)
        {
            IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
            _error = error;
        }
    }
}
