using Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace Grains
{
    public class MessagePublishing : Grain, IPublishMessage
    {
        private readonly ILogger _logger;

        public MessagePublishing(ILogger<MessagePublishing> logger)
        {
            _logger = logger;
        }

        public async Task<IMessage> PublishFailed(IMessage message)
        {
            //todo: create failed node that saves to db

            _logger.LogError($"Sender: {message.SendingApplication} Payload: {message.Payload}");
            return await Task.FromResult(message); //todo: send to new failure node
        }

        public async Task PublishMessage(IMessage message)
        {
            _logger.LogInformation($"Sending without Ack: {message.SendingApplication} Payload: {message.Payload}");
            await SendMessage(message);
        }

        public async Task<bool> PublishMessageAck(IMessage message)
        {
            _logger.LogInformation($"Sending with Ack: {message.SendingApplication} Payload: {message.Payload}");
            var response = SendMessage(message);

            if (response.Result)
            {
                return response.Result;
            }
            else
            {
                await PublishFailed(message);
                return response.Result;
            }
        }

        private async Task<bool> SendMessage(IMessage message)
        {
            //todo: get ack from rabbitMQ

            try
            {
                string exchangeName = "/";
                string queueName = $"{message.SendingApplication} Logging";
                string routingKey = $"{message.SendingApplication}";

                ConnectionFactory factory = new ConnectionFactory();
                // "guest"/"guest" by default, limited to localhost connections
                factory.UserName = "guest";
                factory.Password = "guest";
                factory.VirtualHost = "/";
                factory.HostName = "my-rabbit";

                IConnection conn = factory.CreateConnection();
                IModel channel = conn.CreateModel();

                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);

                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message.ToString());
                await Task.Run(() => channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes));

                channel.Close();
                conn.Close();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error connecting to RabbitMQ. Exception: {ex.Message}");

                return false;
            }
        }
    }
}