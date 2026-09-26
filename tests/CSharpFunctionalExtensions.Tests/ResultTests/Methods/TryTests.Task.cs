using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Methods.Try
{
    [Collection(NonParallelTestCollectionDefinition.Name)]
    public class TryTestBaseTestsTask : TryTestBaseTask
    {
        [Fact]
        public async Task Try_Task_execute_action_success_without_error_handler_function_result_expected()
        {
            var result = await Result.Try(Func_Task);

            result.IsSuccess.Should().BeTrue();    
            FuncExecuted.Should().BeTrue();
        }
        
        [Fact]
        public async Task Try_Task_execute_action_failed_without_error_handler_failed_result_expected()
        {
            var result = await Result.Try(Throwing_Func_Task);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ExceptionError);
        }
        
        [Fact]
        public async Task Try_Task_execute_action_failed_with_error_handler_failed_result_expected()
        {
            var result = await Result.Try(Throwing_Func_Task, ErrorHandler);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorHandlerError);
        }

        [Fact]        
        public async Task ResultTry_Async_execute_action_failed_with_configured_default_error_handler_failed_result_expected()
        {
            var defaultTryErrorHandler = Result.Configuration.DefaultTryErrorHandler;
            Result.Configuration.DefaultTryErrorHandler = ErrorHandler;
            var result = await Result.Try(Throwing_Func_Task);
            Result.Configuration.DefaultTryErrorHandler = defaultTryErrorHandler;

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorHandlerError);
        }

        [Fact] 
        public async Task Try_Task_T_execute_function_success_without_error_handler_function_result_expected()
        {
            var result = await Result.Try(Func_Task_T);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(T.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact] 
        public async Task Try_Task_T_execute_function_failed_without_error_handler_failed_result_expected()
        {
            var result = await Result.Try(Throwing_Func_Task_T);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ExceptionError);
        }

        [Fact] 
        public async Task Try_Task_T_execute_function_failed_with_error_handler_failed_result_expected()
        {
            var result = await Result.Try(Throwing_Func_Task_T, ErrorHandler);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorHandlerError);
        }
    }
}