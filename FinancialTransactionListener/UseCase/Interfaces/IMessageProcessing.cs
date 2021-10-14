using FinancialTransactionListener.Boundary;
using System.Threading.Tasks;

namespace FinancialTransactionListener.UseCase.Interfaces
{
    public interface IMessageProcessing
    {
        Task ProcessMessageAsync(EntityEventSns message);
    }
}
