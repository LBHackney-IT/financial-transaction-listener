using BaseListener.Boundary;
using System.Threading.Tasks;

namespace BaseListener.UseCase.Interfaces
{
    public interface IMessageProcessing
    {
        Task ProcessMessageAsync(EntityEventSns message);
    }
}
