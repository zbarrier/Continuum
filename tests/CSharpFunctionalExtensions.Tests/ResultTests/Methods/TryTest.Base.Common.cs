using System;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Methods.Try
{
    public abstract class TryTestBaseCommon : TestBase
    {
        protected TryTestBaseCommon()
        {
            FuncExecuted = false;
        }

        protected static readonly Exception Exception = new Exception(ErrorMessageString);
        protected static readonly Error ExceptionError = RequestErrors.NewUnknown("{0}", ErrorMessageString);

        protected const string ErrorHandlerMessage = "Error message from error handler";
        protected static readonly Error ErrorHandlerError = RequestErrors.NewUnknown("{0}", ErrorHandlerMessage);
        protected static readonly Func<Exception, Error> ErrorHandler = exc => ErrorHandlerError;

        protected bool FuncExecuted;
    }
}