using System.Net;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization
{
    internal static class DtoMessages
    {
        public static readonly Error HttpResponseMessageIsNull = RequestErrors.NewUnknown("HttpResponseMessage is null.");
        public static readonly Error ContentJsonNotResult = RequestErrors.NewUnknown("The response content is not a Result.");
        public static readonly Error ContentJsonIsFailedResultWithoutError = RequestErrors.NewUnknown("Result was not successful and Error is null.");
        public static readonly Error ContentJsonIsSuccessfulResultWithoutValue = RequestErrors.NewUnknown("Result was successful and Value is null.");

        public static Error NotSuccessStatusCodeFormat(HttpStatusCode statusCode, string content,
            Func<HttpStatusCode, Grpc.Core.StatusCode>? errorCodeMapper = null)
        {
            var grpcStatusCode = errorCodeMapper is null
                ? DefaultErrorCodeMapper(statusCode)
                : errorCodeMapper(statusCode);

            return RequestErrors.NewUnknown(
                "HttpStatus code is {0}, GrpcStatusCode is {1}, Content {2}",
                statusCode.ToString(), grpcStatusCode.ToString(), content);
        }

        static Grpc.Core.StatusCode DefaultErrorCodeMapper(HttpStatusCode httpStatusCode)
        {
            return httpStatusCode switch
            {
                //300s
                HttpStatusCode.MovedPermanently => Grpc.Core.StatusCode.Unimplemented,
                HttpStatusCode.Found => Grpc.Core.StatusCode.Unavailable,
                HttpStatusCode.TemporaryRedirect => Grpc.Core.StatusCode.Unavailable,
                HttpStatusCode.PermanentRedirect => Grpc.Core.StatusCode.Unimplemented,
                //400s
                HttpStatusCode.BadRequest => Grpc.Core.StatusCode.FailedPrecondition,
                HttpStatusCode.Unauthorized => Grpc.Core.StatusCode.Unauthenticated,
                HttpStatusCode.PaymentRequired => Grpc.Core.StatusCode.FailedPrecondition,
                HttpStatusCode.Forbidden => Grpc.Core.StatusCode.PermissionDenied,
                HttpStatusCode.NotFound => Grpc.Core.StatusCode.NotFound,
                HttpStatusCode.MethodNotAllowed => Grpc.Core.StatusCode.Unimplemented,
                HttpStatusCode.NotAcceptable => Grpc.Core.StatusCode.InvalidArgument,
                HttpStatusCode.ProxyAuthenticationRequired => Grpc.Core.StatusCode.Unauthenticated,
                HttpStatusCode.RequestTimeout => Grpc.Core.StatusCode.DeadlineExceeded,
                HttpStatusCode.Conflict => Grpc.Core.StatusCode.FailedPrecondition,
                HttpStatusCode.Gone => Grpc.Core.StatusCode.NotFound,
                HttpStatusCode.LengthRequired => Grpc.Core.StatusCode.InvalidArgument,
                HttpStatusCode.PreconditionFailed => Grpc.Core.StatusCode.FailedPrecondition,
                HttpStatusCode.RequestEntityTooLarge => Grpc.Core.StatusCode.InvalidArgument,
                HttpStatusCode.RequestUriTooLong => Grpc.Core.StatusCode.InvalidArgument,
                HttpStatusCode.UnsupportedMediaType => Grpc.Core.StatusCode.InvalidArgument,
                HttpStatusCode.RequestedRangeNotSatisfiable => Grpc.Core.StatusCode.OutOfRange,
                HttpStatusCode.ExpectationFailed => Grpc.Core.StatusCode.InvalidArgument,
                HttpStatusCode.MisdirectedRequest => Grpc.Core.StatusCode.Unimplemented,
                HttpStatusCode.UnprocessableEntity => Grpc.Core.StatusCode.InvalidArgument,
                HttpStatusCode.Locked => Grpc.Core.StatusCode.FailedPrecondition,
                HttpStatusCode.FailedDependency => Grpc.Core.StatusCode.FailedPrecondition,
                HttpStatusCode.UpgradeRequired => Grpc.Core.StatusCode.Unimplemented,
                HttpStatusCode.PreconditionRequired => Grpc.Core.StatusCode.FailedPrecondition,
                HttpStatusCode.TooManyRequests => Grpc.Core.StatusCode.ResourceExhausted,
                HttpStatusCode.RequestHeaderFieldsTooLarge => Grpc.Core.StatusCode.InvalidArgument,
                HttpStatusCode.UnavailableForLegalReasons => Grpc.Core.StatusCode.PermissionDenied,
                //500s
                HttpStatusCode.InternalServerError => Grpc.Core.StatusCode.Internal,
                HttpStatusCode.NotImplemented => Grpc.Core.StatusCode.Unimplemented,
                HttpStatusCode.BadGateway => Grpc.Core.StatusCode.Unavailable,
                HttpStatusCode.ServiceUnavailable => Grpc.Core.StatusCode.Unavailable,
                HttpStatusCode.GatewayTimeout => Grpc.Core.StatusCode.DeadlineExceeded,
                HttpStatusCode.HttpVersionNotSupported => Grpc.Core.StatusCode.Unimplemented,
                HttpStatusCode.VariantAlsoNegotiates => Grpc.Core.StatusCode.Unimplemented,
                HttpStatusCode.InsufficientStorage => Grpc.Core.StatusCode.OutOfRange,
                HttpStatusCode.LoopDetected => Grpc.Core.StatusCode.Internal,
                HttpStatusCode.NotExtended => Grpc.Core.StatusCode.Unknown,
                HttpStatusCode.NetworkAuthenticationRequired => Grpc.Core.StatusCode.Unauthenticated,
                _ => Grpc.Core.StatusCode.Unknown
            };
        }

        static int DefaultPriorityCodeMapper(Grpc.Core.StatusCode grpcStatusCode)
        {
            return grpcStatusCode switch
            {
                Grpc.Core.StatusCode.Cancelled => ErrorPriorityCode.Cancelled,
                Grpc.Core.StatusCode.Unknown => ErrorPriorityCode.Unknown,
                Grpc.Core.StatusCode.InvalidArgument => ErrorPriorityCode.InvalidArgument,
                Grpc.Core.StatusCode.DeadlineExceeded => ErrorPriorityCode.DeadlineExceeded,
                Grpc.Core.StatusCode.NotFound => ErrorPriorityCode.NotFound,
                Grpc.Core.StatusCode.AlreadyExists => ErrorPriorityCode.AlreadyExists,
                Grpc.Core.StatusCode.PermissionDenied => ErrorPriorityCode.PermissionDenied,
                Grpc.Core.StatusCode.ResourceExhausted => ErrorPriorityCode.ResourceExhausted,
                Grpc.Core.StatusCode.FailedPrecondition => ErrorPriorityCode.FailedPrecondition,
                Grpc.Core.StatusCode.Aborted => ErrorPriorityCode.Aborted,
                Grpc.Core.StatusCode.OutOfRange => ErrorPriorityCode.OutOfRange,
                Grpc.Core.StatusCode.Unimplemented => ErrorPriorityCode.Unimplemented,
                Grpc.Core.StatusCode.Internal => ErrorPriorityCode.Internal,
                Grpc.Core.StatusCode.Unavailable => ErrorPriorityCode.Unavailable,
                Grpc.Core.StatusCode.DataLoss => ErrorPriorityCode.DataLoss,
                Grpc.Core.StatusCode.Unauthenticated => ErrorPriorityCode.Unauthenticated,
                _ => throw new ArgumentException(nameof(grpcStatusCode))
            };
        }
    }
}
