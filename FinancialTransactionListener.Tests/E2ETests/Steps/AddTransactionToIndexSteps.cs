using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using AutoFixture;
using FinancialTransactionListener.Boundary;
using FinancialTransactionListener.Domain.Transaction;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;
using Nest;
using Xunit;

namespace FinancialTransactionListener.Tests.E2ETests.Steps
{
    public class AddTransactionToIndexSteps : BaseSteps
    {
        private readonly Fixture _fixture = new Fixture();
        //private readonly ESEntityFactory _entityFactory = new ESEntityFactory();
        private Exception _lastException;

        public AddTransactionToIndexSteps()
        { }

        private SQSEvent.SQSMessage CreateMessage(Guid transactionId, string eventType = EventTypes.TransactionCreatedEvent)
        {
            var personSns = _fixture.Build<EntityEventSns>()
                                    .With(x => x.EntityId, transactionId)
                                    .With(x => x.EventType, eventType)
                                    .Create();

            var msgBody = JsonSerializer.Serialize(personSns, JsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        public async Task WhenTheFunctionIsTriggered(Guid personId, string eventType)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { CreateMessage(personId, eventType) })
                                   .Create();

            async Task Func()
            {
                var fn = new SqsFunction();
                await fn.FunctionHandler(sqsEvent, lambdaContext).ConfigureAwait(false);
            }

            _lastException = await Record.ExceptionAsync(Func);
        }

        public void ThenATransactionNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Person>));
            (_lastException as EntityNotFoundException<Person>)?.Id.Should().Be(id);
        }
        public async Task ThenTheIndexIsCreatedWithTheTransaction(
            Transaction transaction, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<Transaction>(transaction.Id, g => g.Index("transaction"))
                .ConfigureAwait(false);

            var transactionInIndex = result.Source;
            transactionInIndex.Should().BeEquivalentTo(transaction);

            _cleanup.Add(async () => await esClient.DeleteAsync(new DeleteRequest("transaction", transactionInIndex.Id))
                .ConfigureAwait(false));
        }

    }
}
