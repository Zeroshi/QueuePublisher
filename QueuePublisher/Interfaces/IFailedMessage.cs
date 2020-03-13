using Orleans;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IFailedMessage : IGrainWithStringKey
    {
        Task<int> SaveFailedMessage(IMessage message);
    }
}