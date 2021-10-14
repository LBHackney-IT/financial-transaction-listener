using System;
using System.Threading.Tasks;
using FinancialTransactionListener.Domain.Transaction;

namespace FinancialTransactionListener.Gateway.Interfaces
{
    public interface ITransactionApiGateway
    {
        Task<Transaction> GetTransactionByIdAsync(Guid id);
    }
}
