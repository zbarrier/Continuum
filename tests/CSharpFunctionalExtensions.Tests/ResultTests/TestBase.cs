using System;
using System.Collections.Generic;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests
{
    public abstract class TestBase
    {
        protected const string ErrorMessageString = "Error Message";
        protected const string ErrorMessageString2 = "Error Message2";

        protected readonly static Error ErrorMessage = RequestErrors.NewUnknown("{0}", ErrorMessageString);
        protected readonly static Error ExceptionMessage = RequestErrors.NewUnknown("{0}", ErrorMessageString);
        protected readonly static Error ErrorMessage2 = RequestErrors.NewUnknown("{0}", ErrorMessageString2);

        protected readonly static Error RequestError = new RequestError(System.Net.HttpStatusCode.InternalServerError, Grpc.Core.StatusCode.Unknown, "{0}", "My Request Error.");
        protected readonly static Error ValidationError = new ValidationError(new List<ValidationErrorEntry>
        {
            new ValidationErrorEntry(ValidationSeverity.Error, "Property1", "code1", "{0}", new ErrorArgument[] { "Error Message 1" }),
            new ValidationErrorEntry(ValidationSeverity.Error, "Property2", "code2", "{0}", new ErrorArgument[] { "Error Message 2" }),
            new ValidationErrorEntry(ValidationSeverity.Error, "Property3", "code3", "{0}", new ErrorArgument[] { "Error Message 3" })
        });

        protected class T
        {
            public static readonly T Value = new T();

            public static readonly T Value2 = new T();
        }

        protected class K
        {
            public static readonly K Value = new K();
            
            public static readonly K Value2 = new K();
        }

        protected class E : Error
        {
            public static readonly E Value = new E();
            public static readonly E Value2 = new E();

            public E() : base(System.Net.HttpStatusCode.InternalServerError, Grpc.Core.StatusCode.Unknown, "test_error")
            {
            }

            public override string GetFormattedMessage() => Code;
            protected override bool EqualsCore(Error other) => false;
            protected override void AddHashCodeCore(ref HashCode hash) { }

            public override string ToString()
            {
                return this.PriorityCode.ToString();
            }
        }

        protected class E2 : Error
        {
            public static readonly E2 Value = new E2();

            public E2() : base(System.Net.HttpStatusCode.InternalServerError, Grpc.Core.StatusCode.Unknown, "test_error")
            {
            }

            public override string GetFormattedMessage() => Code;
            protected override bool EqualsCore(Error other) => false;
            protected override void AddHashCodeCore(ref HashCode hash) { }

            public override string ToString()
            {
                return this.PriorityCode.ToString();
            }
        }
    }
}
