using Grains.Schema;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Grains
{
    public class SaveFailedMessage : Grain, ISaveFailedMessage
    {
        private readonly ILogger _logger;

        public SaveFailedMessage(ILogger<SaveFailedMessage> logger)
        {
            _logger = logger;
        }

        public Task<int> Save(IMessage message)
        {
            var response = SaveToDatabase(message).GetAwaiter().GetResult();
            return Task.FromResult(response);
        }

        private async Task<int> SaveToDatabase(IMessage message)
        {
            try
            {
                int result = 0;
                _logger.LogInformation($"Saving to Database. Sending Application: {message.SendingApplication} Payload: {message.Payload}");

                var emailContactAlerts = new EmailContactAlerts();
                emailContactAlerts.EmailContactAlertsId = 1;

                var failedMessage = new FailedMessages();
                failedMessage.CreatedTimeStamp = DateTime.Now;
                failedMessage.EmailContactAlerts = emailContactAlerts;
                failedMessage.LastRetryTimeStamp = DateTime.Now;
                failedMessage.Message = message.Payload;
                failedMessage.QueueName = message.Queue;
                failedMessage.RetryTicker = 0;
                failedMessage.SecondsDelayForRetry = 10;
                failedMessage.SendingApplication = message.SendingApplication;

                using (var db = new MessageContext())
                {
                    db.FailedMessages.Add(failedMessage);
                    await db.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sending Application: {message.SendingApplication} Payload: {message.Payload} Exception: {ex.Message}");

                return -1;
            }
        }

        public class MessageContext : DbContext
        {
            public DbSet<FailedMessages> FailedMessages { get; set; }
            public DbSet<EmailContactAlerts> EmailContactAlerts { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseMySql(
                    Environment.GetEnvironmentVariable("ConnectionString"));
            }
        }
    }
}