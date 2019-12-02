using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class TransationReceiptsChanged
    {
        public string Hash { get; set; }

        public string BlockNumber { get; set; }

        public string Status { get; set; }
    }
}
