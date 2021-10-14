using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FinancialTransactionListener.Boundary;
using FinancialTransactionListener.Domain.Transaction;
using FinancialTransactionListener.Gateway.Interfaces;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FinancialTransactionListener.UseCase;
using FluentAssertions;
using Moq;
using Xunit;

namespace FinancialTransactionListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class IndexTransactionUseCaseTests
    {
        private readonly Mock<ITransactionApiGateway> _mockTransactionApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
       // private readonly IESEntityFactory _esEntityFactory;
        private readonly IndexTransactionUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly Transaction _transaction;

        private readonly Fixture _fixture;

        public IndexTransactionUseCaseTests()
        {
            _fixture = new Fixture();

            _mockTransactionApi = new Mock<ITransactionApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _sut = new IndexTransactionUseCase(_mockEsGateway.Object,
                _mockTransactionApi.Object);

            _message = CreateMessage();
            _transaction = CreateTransaction(_message.EntityId);
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.TransactionCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .Create();
        }

        private Transaction CreateTransaction(Guid entityId)
        {
            
            return _fixture.Build<Transaction>()
                           //.With(x => x.Id, entityId.ToString())
                           //.With(x => x.Tenures, tenures)
                           //.With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                           .Create();
        }

        private bool VerifyTransactionIndexed(Transaction transaction)
        {
            transaction.Should().BeEquivalentTo(CreateTransaction(transaction.Id));
            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTransactionExceptionThrown()
        {
            const string exMsg = "This is an error";
            _mockTransactionApi.Setup(x => x.GetTransactionByIdAsync(_message.EntityId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTransactionReturnsNullThrows()
        {
            _mockTransactionApi.Setup(x => x.GetTransactionByIdAsync(_message.EntityId))
                                       .ReturnsAsync((Transaction)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<Transaction>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexTransactionExceptionThrows()
        {
            const string exMsg = "This is the last error";
            _mockTransactionApi.Setup(x => x.GetTransactionByIdAsync(_message.EntityId))
                                       .ReturnsAsync(_transaction);
            _mockEsGateway.Setup(x => x.IndexTransaction(It.IsAny<Transaction>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Theory]
        [InlineData(EventTypes.TransactionCreatedEvent)]
        [InlineData(EventTypes.TransactionUpdatedEvent)]
        public async Task ProcessMessageAsyncTestIndexTransactionSuccess(string eventType)
        {
            _message.EventType = eventType;

            _mockTransactionApi.Setup(x => x.GetTransactionByIdAsync(_message.EntityId))
                                       .ReturnsAsync(_transaction);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexTransaction(It.Is<Transaction>(y => VerifyTransactionIndexed(y))), Times.Once);
        }
    }
}
