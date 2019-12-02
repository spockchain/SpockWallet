using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class SyncStoped
    {
        public string Address { get; set; }

        public long SyncBlocked { get; set; }
    }
}
