using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Errors
{
    public class ErrorHashCodeTests
    {
        [Fact]
        public void EqualRequestErrors_HaveEqualHashCodes()
        {
            var a = RequestErrors.NewNotFound("{0} {1}", "Order", 42);
            var b = RequestErrors.NewNotFound("{0} {1}", "Order", 42);

            a.Should().Be(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void EqualValidationErrors_HaveEqualHashCodes()
        {
            var a = new ValidationError("Name", ValidationErrorCodes.NotEmpty, "'{0}' must not be empty.", "Name");
            var b = new ValidationError("Name", ValidationErrorCodes.NotEmpty, "'{0}' must not be empty.", "Name");

            a.Should().Be(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
            a.Entries[0].GetHashCode().Should().Be(b.Entries[0].GetHashCode());
        }

        [Fact]
        public void EqualErrors_WorkAsHashSetKeys()
        {
            var set = new HashSet<Error>
            {
                RequestErrors.NewNotFound("{0}", "Order"),
                new ValidationError("Name", ValidationErrorCodes.NotEmpty, "'{0}' must not be empty.", "Name"),
            };

            set.Contains(RequestErrors.NewNotFound("{0}", "Order")).Should().BeTrue();
            set.Contains(new ValidationError("Name", ValidationErrorCodes.NotEmpty, "'{0}' must not be empty.", "Name")).Should().BeTrue();
        }
    }
}
