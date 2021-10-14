using System.Threading.Tasks;
using FinancialTransactionListener.Domain.Transaction;
using Nest;

namespace FinancialTransactionListener.Gateway.Interfaces
{
    public interface IEsGateway
    {
        Task<IndexResponse> IndexTransaction(Transaction transaction);


    }
}
