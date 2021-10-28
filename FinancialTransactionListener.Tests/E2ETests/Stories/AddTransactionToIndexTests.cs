using System;
using FinancialTransactionListener.Tests.E2ETests.Fixtures;
using FinancialTransactionListener.Tests.E2ETests.Steps;
using TestStack.BDDfy;
using Xunit;

namespace FinancialTransactionListener.Tests.E2ETests.Stories
{
    [Story(
        AsA = "SQS Transaction Listener",
        IWant = "a function to process the transaction created and updated messages",
        SoThat = "The Transaction details are set in the index")]
    [Collection("ElasticSearch collection")]
    public sealed class AddTransactionToIndexTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly TransactionApiFixture _transactionApiFixture;

        private readonly AddTransactionToIndexSteps _steps;

        public AddTransactionToIndexTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _transactionApiFixture = new TransactionApiFixture();

            _steps = new AddTransactionToIndexSteps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;
            _transactionApiFixture.Dispose();

            _disposed = true;
        }

        //[Theory]
        //[InlineData(EventTypes.TransactionCreatedEvent)]
        //[InlineData(EventTypes.TransactionUpdatedEvent)]
        //public void TransactionNotFound(string eventType)
        //{
        //    var transactionId = Guid.NewGuid();
        //    this.Given(g => _transactionApiFixture.GivenTheTransactionDoesNotExist(transactionId))
        //        .When(w => _steps.WhenTheFunctionIsTriggered(transactionId, eventType))
        //        .Then(t => _steps.ThenATransactionNotFoundExceptionIsThrown(transactionId))
        //        .BDDfy();
        //}

        //[Fact]
        //public void TransactionCreatedAddedToIndex()
        //{
        //    var transactionId = Guid.NewGuid();
        //    this.Given(g => _transactionApiFixture.GivenTheTransactionExists(transactionId))
        //        .And(h => _esFixture.GivenATransactionIsNotIndexed(_transactionApiFixture.ResponseObject))
        //        .When(w => _steps.WhenTheFunctionIsTriggered(transactionId, EventTypes.TransactionCreatedEvent))
        //        .Then(t => _steps.ThenTheIndexIsCreatedWithTheTransaction(_transactionApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
        //        .BDDfy();
        //}

        //[Fact]
        //public void TransactionUpdateInIndex()
        //{
        //    var transactionId = Guid.NewGuid();
        //    this.Given(g => _transactionApiFixture.GivenTheTransactionExists(transactionId))
        //        .And(h => _esFixture.GivenATransactionIsIndexedWithDifferentInfo(_transactionApiFixture.ResponseObject))
        //        .When(w => _steps.WhenTheFunctionIsTriggered(transactionId, EventTypes.TransactionUpdatedEvent))
        //        .Then(t => _steps.ThenTheIndexIsCreatedWithTheTransaction(_transactionApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
        //        .BDDfy();
        //}
    }
}
