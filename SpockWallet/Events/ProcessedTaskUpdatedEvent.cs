using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class ProcessedTaskUpdatedEvent
    {
        public Guid TaskId { get; set; }

        public int Index { get; set; }

        public string Status { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public decimal Value { get; set; }

        public string TxHash { get; set; }
    }
}
