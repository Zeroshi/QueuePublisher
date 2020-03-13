using Orleans;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IPublishMessage : IGrainWithStringKey
    {
        Task<bool> PublishMessageAck(IMessage message);

        Task PublishMessage(IMessage message);

        Task<IMessage> PublishFailed(IMessage message);
    }
}