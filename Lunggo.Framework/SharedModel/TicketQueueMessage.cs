﻿using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Lunggo.Framework.SharedModel
{
    public class TicketQueueMessage
    {
        public TicketPurpose TicketPurpose { get; set; }
        public object TicketObjectDetail { get; set; }
        public CloudQueueMessage SerializeToQueueMessage()
        {
            string classInJson = JsonConvert.SerializeObject(this);
            return new CloudQueueMessage(classInJson);
        }
    }

    public enum TicketPurpose
    {
        ApiBookingFailed,
        ApalagiGitu
    }
}
