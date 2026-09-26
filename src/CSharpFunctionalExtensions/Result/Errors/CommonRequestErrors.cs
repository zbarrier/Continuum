using System.Net;

namespace Continuum.CSharpFunctionalExtensions;

public static class RequestErrors
{
    public static RequestError New(int priorityCode, HttpStatusCode httpStatusCode, Grpc.Core.StatusCode grpcStatusCode)
        => new RequestError(priorityCode, httpStatusCode, grpcStatusCode, string.Empty, []);
    public static RequestError New(int priorityCode, HttpStatusCode httpStatusCode, Grpc.Core.StatusCode grpcStatusCode, string? format)
        => new RequestError(priorityCode, httpStatusCode, grpcStatusCode, format, []);
    public static RequestError New(int priorityCode, HttpStatusCode httpStatusCode, Grpc.Core.StatusCode grpcStatusCode, string? format, params object[] arguments) 
        => new RequestError(priorityCode, httpStatusCode, grpcStatusCode, format, arguments);
    public static RequestError New(int priorityCode, int httpStatusCode, int grpcStatusCode)
        => new RequestError(priorityCode, httpStatusCode, grpcStatusCode, string.Empty, []);
    public static RequestError New(int priorityCode, int httpStatusCode, int grpcStatusCode, string? format)
        => new RequestError(priorityCode, httpStatusCode, grpcStatusCode, format, []);
    public static RequestError New(int priorityCode, int httpStatusCode, int grpcStatusCode, string? format, params object[] arguments)
        => new RequestError(priorityCode, httpStatusCode, grpcStatusCode, format, arguments);

    public static RequestError NewAlreadyExists(string? format) =>
        new RequestError(ErrorPriorityCode.ALREADY_EXISTS, System.Net.HttpStatusCode.Conflict, Grpc.Core.StatusCode.AlreadyExists, format, []);
    public static RequestError NewAlreadyExists(string? format, params object[] arguments) =>
        new RequestError(ErrorPriorityCode.ALREADY_EXISTS, System.Net.HttpStatusCode.Conflict, Grpc.Core.StatusCode.AlreadyExists, format, arguments);

    public static RequestError NewDeadlineExceeded(string? format) =>
    new RequestError(ErrorPriorityCode.DEADLINE_EXCEEDED, System.Net.HttpStatusCode.GatewayTimeout, Grpc.Core.StatusCode.DeadlineExceeded, format, []);
    public static RequestError NewDeadlineExceeded(string? format, params object[] arguments) =>
        new RequestError(ErrorPriorityCode.DEADLINE_EXCEEDED, System.Net.HttpStatusCode.GatewayTimeout, Grpc.Core.StatusCode.DeadlineExceeded, format, arguments);

    public static RequestError NewFailedPrecondition(string? format) =>
        new RequestError(ErrorPriorityCode.FAILED_PRECONDITION, System.Net.HttpStatusCode.BadRequest, Grpc.Core.StatusCode.FailedPrecondition, format, []);
    public static RequestError NewFailedPrecondition(string? format, params object[] arguments) =>
        new RequestError(ErrorPriorityCode.FAILED_PRECONDITION, System.Net.HttpStatusCode.BadRequest, Grpc.Core.StatusCode.FailedPrecondition, format, arguments);

    public static RequestError NewInvalidArg(string? format) =>
        new RequestError(ErrorPriorityCode.INVALID_ARGUMENT, System.Net.HttpStatusCode.UnprocessableEntity, Grpc.Core.StatusCode.InvalidArgument, format, []);
    public static RequestError NewInvalidArg(string? format, params object[] arguments) =>
        new RequestError(ErrorPriorityCode.INVALID_ARGUMENT, System.Net.HttpStatusCode.UnprocessableEntity, Grpc.Core.StatusCode.InvalidArgument, format, arguments);

    public static RequestError NewNotFound(string? format) =>
        new RequestError(ErrorPriorityCode.NOT_FOUND, System.Net.HttpStatusCode.NotFound, Grpc.Core.StatusCode.NotFound, format, []);
    public static RequestError NewNotFound(string? format, params object[] arguments) =>
        new RequestError(ErrorPriorityCode.NOT_FOUND, System.Net.HttpStatusCode.NotFound, Grpc.Core.StatusCode.NotFound, format, arguments);

    public static RequestError NewNotImplemented(string? format = null) =>
        new RequestError(ErrorPriorityCode.UNIMPLEMENTED, System.Net.HttpStatusCode.NotImplemented, Grpc.Core.StatusCode.Unimplemented, 
            format ?? "Not Implemented.", []);
    public static RequestError NewNotImplemented(string? format, params object[] arguments) =>
        new RequestError(ErrorPriorityCode.UNIMPLEMENTED, System.Net.HttpStatusCode.NotImplemented, Grpc.Core.StatusCode.Unimplemented, format, arguments);

    public static RequestError NewUnavailable(string? format) =>
        new RequestError(ErrorPriorityCode.UNAVAILABLE, System.Net.HttpStatusCode.ServiceUnavailable, Grpc.Core.StatusCode.Unavailable, format, []);
    public static RequestError NewUnavailable(string? format, params object[] arguments) =>
        new RequestError(ErrorPriorityCode.UNAVAILABLE, System.Net.HttpStatusCode.ServiceUnavailable, Grpc.Core.StatusCode.Unavailable, format, arguments);

    public static RequestError NewUnknown(string? format) =>
        new RequestError(ErrorPriorityCode.UNKNOWN, System.Net.HttpStatusCode.InternalServerError, Grpc.Core.StatusCode.Unknown, format, []);
    public static RequestError NewUnknown(string? format, params object[] arguments) =>
        new RequestError(ErrorPriorityCode.UNKNOWN, System.Net.HttpStatusCode.InternalServerError, Grpc.Core.StatusCode.Unknown, format, arguments);

}
