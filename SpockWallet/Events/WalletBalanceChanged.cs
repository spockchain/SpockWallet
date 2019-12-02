using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class WalletBalanceChanged
    {
        public string Address { get; set; }

        public decimal StakingRequired { get; set; }

        public decimal Balance { get; set; }
    }
}
