using Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using static Grains.Enums.Enums;

namespace Grains
{
    public class MessagePublishing : Grain, IPublishMessage
    {
        private readonly ILogger _logger;
        private readonly ConnectionFactory _connectionFactory = null;
        private string _exchangeName = null;
        private string _queryName = null;
        private string _routingKey = null;

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
            var response = await SendMessage(message);

            if (response)
            {
                return response;
            }
            else
            {
                await PublishFailed(message);
                return response;
            }
        }

        private void SetupRabbitMq(IMessage message)
        {
            //todo: add to secrets
            _exchangeName = "/";
            _queryName = $"{message.SendingApplication} Logging";
            _routingKey = $"{message.SendingApplication}";

            ConnectionFactory factory = new ConnectionFactory();
            // "guest"/"guest" by default, limited to localhost connections
            factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
            factory.AutomaticRecoveryEnabled = true;

            factory.UserName = "guest";
            factory.Password = "guest";
            factory.VirtualHost = "/";
            factory.HostName = "my-rabbit";
        }

        private async Task<bool> SendMessage(IMessage message)
        {
            try
            {
                if (_connectionFactory == null)
                {
                    SetupRabbitMq(message);
                }

                IConnection conn = _connectionFactory.CreateConnection();
                IModel channel = conn.CreateModel();

                channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
                channel.QueueDeclare(_queryName, false, false, false, null);
                channel.QueueBind(_queryName, _exchangeName, _routingKey, null);

                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message.ToString());
                await Task.Run(() => channel.BasicPublish(_exchangeName, _routingKey, null, messageBodyBytes));

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