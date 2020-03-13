using Interfaces;

namespace Grains
{
    public class Message : IMessage
    {
        public string SendingApplication { get; set; }
        public string Payload { get; set; }
        public string Queue { get; set; }
    }
}