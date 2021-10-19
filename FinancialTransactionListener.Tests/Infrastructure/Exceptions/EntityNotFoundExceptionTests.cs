using System;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FluentAssertions;
using Xunit;

namespace FinancialTransactionListener.Tests.Infrastructure.Exceptions
{
    public class EntityNotFoundExceptionTests
    {
        [Fact]
        public void EntityNotFoundExceptionConstructorTest()
        {
            var id = Guid.NewGuid();

            var ex = new EntityNotFoundException<SomeEntity>(id);
            ex.Id.Should().Be(id);

            var typeName = nameof(SomeEntity);
            ex.EntityName.Should().Be(typeName);
            ex.Message.Should().Be($"{typeName} with id {id} not found.");
        }
    }

    public class SomeEntity { }
}
