using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FinancialTransactionListener.Domain.Transaction;
using FinancialTransactionListener.Gateway;
using FluentAssertions;
using Moq;
using Nest;
using Xunit;

namespace FinancialTransactionListener.Tests.Gateway
{
    [Collection("ElasticSearch collection")]
    public sealed class EsGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly Mock<IElasticClient> _mockEsClient;
        private readonly EsGateway _sut;

        private readonly ElasticSearchFixture _testFixture;
        private readonly List<Action> _cleanup = new List<Action>();

        public EsGatewayTests(ElasticSearchFixture testFixture)
        {
            _testFixture = testFixture;

            _mockEsClient = new Mock<IElasticClient>();
            _sut = new EsGateway(_mockEsClient.Object);
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
            foreach (var action in _cleanup)
                action();

            _disposed = true;
        }

        private bool ValidateIndexRequest<T>(IndexRequest<T> actual, T obj) where T : class
        {
            actual.Document.Should().Be(obj);
            return true;
        }

        private Transaction CreateTransaction()
        {
            return _fixture.Build<Transaction>()
                           .Create();
        }

      

     

        [Fact]
        public void IndexPersonTestNullPersonThrows()
        {
            Func<Task<IndexResponse>> func = async () => await _sut.IndexTransaction(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task IndexPersonTestCallsEsClientUsingMocks()
        {
            var indexResponse = _fixture.Create<IndexResponse>();
            var transaction = CreateTransaction();
            _mockEsClient.Setup(x => x.IndexAsync(It.IsAny<IndexRequest<Transaction>>(), default(CancellationToken)))
                         .ReturnsAsync(indexResponse);
            var response = await _sut.IndexTransaction(transaction).ConfigureAwait(false);

            response.Should().Be(indexResponse);
            _mockEsClient.Verify(x => x.IndexAsync(It.Is<IndexRequest<Transaction>>(y => ValidateIndexRequest<Transaction>(y, transaction)),
                                                   default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async Task IndexTransactionTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var transaction = CreateTransaction();
            var response = await sut.IndexTransaction(transaction).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<Transaction>(transaction.Id, g => g.Index("transactions"))
                                           .ConfigureAwait(false);
            result.Source.Should().BeEquivalentTo(transaction);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("transactions", transaction.Id))
                                                                           .ConfigureAwait(false));
        }


       
    }
}
