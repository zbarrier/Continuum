using FluentAssertions;
using System.Runtime.Serialization;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests
{
    //public class SerializationTests
    //{
    //    private static readonly string _errorMessage = "this failed";
    //    private static readonly Error _error = RequestErrors.NewUnknown(_errorMessage);

    //    [Fact]
    //    public void GetObjectData_sets_correct_statuses_on_success_result()
    //    {
    //        Result okResult = Result.Success();
    //        ISerializable serializableObject = okResult;

    //        var serializationInfo = new SerializationInfo(typeof(Result), new FormatterConverter());
    //        serializableObject.GetObjectData(serializationInfo, new StreamingContext());

    //        serializationInfo.GetBoolean(nameof(Result.IsSuccess)).Should().BeTrue();
    //        serializationInfo.GetBoolean(nameof(Result.IsFailure)).Should().BeFalse();
    //    }

    //    [Fact]
    //    public void GetObjectData_sets_correct_statuses_on_failure_result()
    //    {
    //        Result failResult = Result.Failure(_error);
    //        ISerializable serializableObject = failResult;

    //        var serializationInfo = new SerializationInfo(typeof(Result), new FormatterConverter());
    //        serializableObject.GetObjectData(serializationInfo, new StreamingContext());

    //        serializationInfo.GetBoolean(nameof(Result.IsSuccess)).Should().BeFalse();
    //        serializationInfo.GetBoolean(nameof(Result.IsFailure)).Should().BeTrue();
    //    }

    //    [Fact]
    //    public void GetObjectData_adds_message_in_context_on_failure_result()
    //    {
    //        Result failResult = Result.Failure(_error);
    //        ISerializable serializableObject = failResult;

    //        var serializationInfo = new SerializationInfo(typeof(Result), new FormatterConverter());
    //        serializableObject.GetObjectData(serializationInfo, new StreamingContext());

    //        serializationInfo.GetString(nameof(Result.Error)).Should().Be(_errorMessage);
    //    }

    //    [Fact]
    //    public void GetObjectData_of_generic_result_adds_object_in_context_when_success_result()
    //    {
    //        TestObject language = new TestObject { Number = 232, String = "C#" };
    //        Result<TestObject> okResult = Result.Success(language);
    //        ISerializable serializableObject = okResult;

    //        var serializationInfo = new SerializationInfo(typeof(Result), new FormatterConverter());
    //        serializableObject.GetObjectData(serializationInfo, new StreamingContext());

    //        serializationInfo.GetValue(nameof(Result<TestObject>.Value), typeof(TestObject))
    //            .Should().Be(language);
    //    }

    //    private class TestObject
    //    {
    //        public string String { get; set; }
    //        public int Number { get; set; }
    //    }
    //}
}
