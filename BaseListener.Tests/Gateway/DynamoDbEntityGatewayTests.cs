using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using BaseListener.Domain;
using BaseListener.Factories;
using BaseListener.Gateway;
using BaseListener.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BaseListener.Tests.Gateway
{
    [Collection("DynamoDb collection")]
    public class DynamoDbEntityGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ILogger<DynamoDbEntityGateway>> _logger;
        private readonly DynamoDbEntityGateway _classUnderTest;
        private DynamoDbFixture _dbTestFixture;
        private IDynamoDBContext DynamoDb => _dbTestFixture.DynamoDbContext;
        private readonly List<Action> _cleanup = new List<Action>();

        public DynamoDbEntityGatewayTests(DynamoDbFixture dbTestFixture)
        {
            _dbTestFixture = dbTestFixture;
            _logger = new Mock<ILogger<DynamoDbEntityGateway>>();
            _classUnderTest = new DynamoDbEntityGateway(DynamoDb, _logger.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var action in _cleanup)
                    action();

                if (_dbTestFixture != null)
                {
                    _dbTestFixture.Dispose();
                    _dbTestFixture = null;
                }

                _disposed = true;
            }
        }

        private async Task InsertDatatoDynamoDB(DomainEntity entity)
        {
            await DynamoDb.SaveAsync(entity.ToDatabase()).ConfigureAwait(false);
            _cleanup.Add(async () => await DynamoDb.DeleteAsync<DbEntity>(entity.Id).ConfigureAwait(false));
        }

        private DomainEntity ConstructDomainEntity()
        {
            var entity = _fixture.Build<DomainEntity>()
                                 .With(x => x.VersionNumber, (int?) null)
                                 .Create();
            return entity;
        }

        [Fact]
        public async Task GetEntityAsyncTestReturnsRecord()
        {
            var domainEntity = ConstructDomainEntity();
            await InsertDatatoDynamoDB(domainEntity).ConfigureAwait(false);

            var result = await _classUnderTest.GetEntityAsync(domainEntity.Id).ConfigureAwait(false);

            result.Should().BeEquivalentTo(domainEntity, (e) => e.Excluding(y => y.VersionNumber));
            result.VersionNumber.Should().Be(0);

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for id {domainEntity.Id}", Times.Once());
        }

        [Fact]
        public async Task GetEntityAsyncTestReturnsNullWhenNotFound()
        {
            var id = Guid.NewGuid();
            var result = await _classUnderTest.GetEntityAsync(id).ConfigureAwait(false);

            result.Should().BeNull();

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for id {id}", Times.Once());
        }

        [Fact]
        public async Task SaveEntityAsyncTestUpdatesDatabase()
        {
            var domainEntity = ConstructDomainEntity();
            await InsertDatatoDynamoDB(domainEntity).ConfigureAwait(false);

            domainEntity.Name = "New name";
            domainEntity.Description = "New description";
            domainEntity.VersionNumber = 0;
            await _classUnderTest.SaveEntityAsync(domainEntity).ConfigureAwait(false);

            var updatedInDB = await DynamoDb.LoadAsync<DbEntity>(domainEntity.Id).ConfigureAwait(false);
            updatedInDB.ToDomain().Should().BeEquivalentTo(domainEntity, (e) => e.Excluding(y => y.VersionNumber));
            updatedInDB.VersionNumber.Should().Be(domainEntity.VersionNumber + 1);

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync for id {domainEntity.Id}", Times.Once());
        }
    }
}
