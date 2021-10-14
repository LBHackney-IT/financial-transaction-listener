using System;
using System.Threading.Tasks;
using FinancialTransactionListener.Domain.Transaction;
using FinancialTransactionListener.Gateway.Interfaces;
using Hackney.Core.Logging;
using Nest;

namespace FinancialTransactionListener.Gateway
{
    public class EsGateway : IEsGateway
    {
        private readonly IElasticClient _elasticClient;

        private const string IndexNameTransactions = "transactions";
        public EsGateway(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        [LogCall]
        private async Task<IndexResponse> ESIndex<T>(T esObject, string indexName) where T : class
        {
            return await _elasticClient.IndexAsync(new IndexRequest<T>(esObject, indexName));
        }

        public async Task<IndexResponse> IndexTransaction(Transaction transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            return await ESIndex(transaction, IndexNameTransactions);
        }

        
    }
}
