namespace Interfaces
{
    public interface IMessage
    {
        string SendingApplication { get; set; }
        string Payload { get; set; }
        string Queue { get; set; }
    }
}