using Continuum.CSharpFunctionalExtensions.Internal;

namespace Continuum.CSharpFunctionalExtensions
{
    public readonly partial struct Result : IResult, IError
    {
        public bool IsFailure { get; }

        public bool IsSuccess => !IsFailure;

        private readonly Error _error;

        public Error Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

        private Result(bool isFailure, Error error)
        {
            IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
            _error = error;
        }
    }
}
