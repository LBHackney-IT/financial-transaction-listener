using FinancialTransactionListener.Infrastructure.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinancialTransactionListener.Tests.Infrastructure.Exceptions
{
    public class MissedEntityDataExceptionTest
    {
        [Fact]
        public void MissedEntityDataExceptionConstructorTest()
        {
            const string message = "Test error.";

            var ex = new MissedEntityDataException(message);
            ex.Message.Should().Be(message);
        }
    }
}
