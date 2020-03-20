using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Grains.Schema
{
    public class FailedMessages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FailedMessageId { get; set; }

        public string SendingApplication { get; set; }
        public string QueueName { get; set; }
        public string Message { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
        public DateTime LastRetryTimeStamp { get; set; }
        public int RetryTicker { get; set; }
        public int SecondsDelayForRetry { get; set; }
        public EmailContactAlerts EmailContactAlerts { get; set; }
    }

    public class EmailContactAlerts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmailContactAlertsId { get; set; }

        public string Email { get; set; }
        public string Description { get; set; }

        //[ForeignKey("StandardRefId")]
        public ICollection<FailedMessages> FailedMessages { get; set; }
    }
}