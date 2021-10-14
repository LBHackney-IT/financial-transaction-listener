using FinancialTransactionListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinancialTransactionListener.Gateway.Interfaces
{
    public interface IDbEntityGateway
    {
        Task<DomainEntity> GetEntityAsync(Guid id);
        Task SaveEntityAsync(DomainEntity entity);
    }
}
