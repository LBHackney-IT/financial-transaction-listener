using FinancialTransactionListener.Boundary;
using FinancialTransactionListener.Domain;
using FinancialTransactionListener.Gateway.Interfaces;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FinancialTransactionListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System;
using System.Threading.Tasks;

namespace FinancialTransactionListener.UseCase
{
    public class DoSomethingUseCase : IDoSomethingUseCase
    {
        private readonly IDbEntityGateway _gateway;

        public DoSomethingUseCase(IDbEntityGateway gateway)
        {
            _gateway = gateway;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // TODO - Implement use case logic
            DomainEntity entity = await _gateway.GetEntityAsync(message.EntityId).ConfigureAwait(false);
            if (entity is null) throw new EntityNotFoundException<DomainEntity>(message.EntityId);

            entity.Description = "Updated";

            // Save updated entity
            await _gateway.SaveEntityAsync(entity).ConfigureAwait(false);
        }
    }
}
