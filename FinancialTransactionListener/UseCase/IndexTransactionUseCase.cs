using System;
using System.Threading.Tasks;
using FinancialTransactionListener.Boundary;
using FinancialTransactionListener.Domain.Transaction;
using FinancialTransactionListener.Gateway.Interfaces;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FinancialTransactionListener.UseCase.Interfaces;

namespace FinancialTransactionListener.UseCase
{
    public class IndexTransactionUseCase : IIndexTransactionUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly ITransactionApiGateway _transactionApiGateway;

        public IndexTransactionUseCase(IEsGateway esGateway, ITransactionApiGateway transactionApiGateway)
        {
            _esGateway = esGateway;
            _transactionApiGateway = transactionApiGateway;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Transaction from Transaction service API
            var transaction = await _transactionApiGateway.GetTransactionByIdAsync(message.EntityId)
                                         .ConfigureAwait(false);
            if (transaction is null) throw new EntityNotFoundException<Transaction>(message.EntityId);

            // 2. Create the ES index
            await _esGateway.IndexTransaction(transaction);
        }
    }
}
