using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Grains
{
    public class FailedMessage : Grain, IFailedMessage
    {
        private readonly ILogger _logger;

        public FailedMessage(ILogger<FailedMessage> logger)
        {
            _logger = logger;
        }

        public Task<int> SaveFailedMessage(IMessage message)
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

                using (var db = new MessageContext())
                {
                    db.Messages.Add(message);
                    result = await db.SaveChangesAsync();
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
            public DbSet<IMessage> Messages { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(
                    @"Server=(localdb)\mssqllocaldb;Database=Blogging;Integrated Security=True");
            }
        }
    }
}