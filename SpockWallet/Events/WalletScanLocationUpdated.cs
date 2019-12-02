using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class WalletScanLocationUpdated
    {
        public string Address { get; set; }

        public long ScanLocation { get; set; }
    }
}
