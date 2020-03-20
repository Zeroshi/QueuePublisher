using Orleans;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ISaveFailedMessage : IGrainWithStringKey
    {
        Task<int> Save(IMessage message);
    }
}