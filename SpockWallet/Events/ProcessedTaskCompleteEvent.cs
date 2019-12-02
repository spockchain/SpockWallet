using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class ProcessedTaskCompleteEvent
    {
        public Guid TaskId { get; set; }
    }
}
