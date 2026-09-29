namespace Continuum.CSharpFunctionalExtensions
{
    internal struct SerializationValue
    {
        public bool IsFailure { get; }
        public Error? Error { get; }

        internal SerializationValue(bool isFailure, Error? error)
        {
            IsFailure = isFailure;
            Error = error;
        }
    }
}
