using Amazon.DynamoDBv2.DataModel;
using FinancialTransactionListener.Domain;
using FinancialTransactionListener.Infrastructure;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FluentAssertions;
using System;
using System.Threading.Tasks;

namespace FinancialTransactionListener.Tests.E2ETests.Steps
{
    public class DoSomethingUseCaseSteps : BaseSteps
    {
        public DoSomethingUseCaseSteps()
        { }

        public async Task WhenTheFunctionIsTriggered(Guid id)
        {
            await TriggerFunction(id).ConfigureAwait(false);
        }

        public async Task ThenTheEntityIsUpdated(DbEntity beforeChange, IDynamoDBContext dbContext)
        {
            var entityInDb = await dbContext.LoadAsync<DbEntity>(beforeChange.Id);

            entityInDb.Should().BeEquivalentTo(beforeChange,
                config => config.Excluding(y => y.Description)
                                .Excluding(z => z.VersionNumber));
            entityInDb.Description.Should().Be("Updated");
            entityInDb.VersionNumber.Should().Be(beforeChange.VersionNumber + 1);
        }

        public void ThenAnEntityNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<DomainEntity>));
            (_lastException as EntityNotFoundException<DomainEntity>).Id.Should().Be(id);
        }
    }
}
