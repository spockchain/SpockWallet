using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class BlockSynced
    {
        public bool IsSuccess { get; set; }

        public string Address { get; set; }

        public long BlockNumber { get; set; }
    }
}
