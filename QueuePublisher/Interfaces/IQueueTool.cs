using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Grains.Enums.Enums;

namespace Interfaces
{
    public interface IQueueTool
    {
        Task<bool> SendMessage(IMessage message);

        Task<bool> SendMessageAck(IMessage message);
    }
}