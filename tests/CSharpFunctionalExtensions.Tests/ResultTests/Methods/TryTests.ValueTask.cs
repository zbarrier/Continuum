using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Methods.Try
{
    [Collection(NonParallelTestCollectionDefinition.Name)]
    public class TryTestBaseTestsValueTask : TryTestBaseValueTask
    {
        [Fact]
        public async Task Try_ValueTask_execute_action_success_without_error_handler_function_result_expected()
        {
            var result = await Result.Try(Func_ValueTask);

            result.IsSuccess.Should().BeTrue();    
            FuncExecuted.Should().BeTrue();
        }
        
        [Fact]
        public async Task Try_ValueTask_execute_action_failed_without_error_handler_failed_result_expected()
        {
            var result = await Result.Try(Throwing_Func_ValueTask);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ExceptionError);
        }
        
        [Fact]
        public async Task Try_ValueTask_execute_action_failed_with_error_handler_failed_result_expected()
        {
            var result = await Result.Try(Throwing_Func_ValueTask, ErrorHandler);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorHandlerError);
        }

        [Fact]        
        public async Task ResultTry_Async_execute_action_failed_with_configured_default_error_handler_failed_result_expected()
        {
            var defaultTryErrorHandler = Result.Configuration.DefaultTryErrorHandler;
            Result.Configuration.DefaultTryErrorHandler = ErrorHandler;
            var result = await Result.Try(Throwing_Func_ValueTask);
            Result.Configuration.DefaultTryErrorHandler = defaultTryErrorHandler;

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorHandlerError);
        }

        [Fact] 
        public async Task Try_ValueTask_T_execute_function_success_without_error_handler_function_result_expected()
        {
            var result = await Result.Try(Func_ValueTask_T);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(T.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact] 
        public async Task Try_ValueTask_T_execute_function_failed_without_error_handler_failed_result_expected()
        {
            var result = await Result.Try(Throwing_Func_ValueTask_T);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ExceptionError);
        }

        [Fact] 
        public async Task Try_ValueTask_T_execute_function_failed_with_error_handler_failed_result_expected()
        {
            var result = await Result.Try(Throwing_Func_ValueTask_T, ErrorHandler);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorHandlerError);
        }
    }
}