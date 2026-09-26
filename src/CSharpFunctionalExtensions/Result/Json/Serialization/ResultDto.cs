#nullable enable
using System.Text.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

//internal partial record ResultDto<TError>(bool IsSuccess, TError? Error)
//    where TError : Error
//{
//    public static ResultDto<TError> Success() => new(true, null);
//    public static ResultDto<TError> Failure(TError error) => new(false, error);

//    public static ResultDto<TValue, TError> Success<TValue>(TValue value) => new(true, value, null);
//    public static ResultDto<TValue, TError> Failure<TValue>(TError error) => new(false, default!, error);
//}

//internal record ResultDto<TValue, TError>(bool IsSuccess, TValue Value, TError? Error)
//    where TError : Error;
