using System;
using System.Net;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinancialTransactionListener.Tests.Infrastructure.Exceptions
{
    public class GetTransactionExceptionTests
    {
        [Fact]
        public void GetPersonExceptionConstructorTest()
        {
            var transactionId = Guid.NewGuid();
            const HttpStatusCode statusCode = HttpStatusCode.OK;
            const string msg = "Some API error message";

            var ex = new GetTransactionException(transactionId, statusCode, msg);
            ex.TransactionId.Should().Be(transactionId);
            ex.StatusCode.Should().Be(statusCode);
            ex.ResponseBody.Should().Be(msg);
            ex.Message.Should().Be($"Failed to get transaction details for id {transactionId}. Status code: {statusCode}; Message: {msg}");
        }
    }
}
