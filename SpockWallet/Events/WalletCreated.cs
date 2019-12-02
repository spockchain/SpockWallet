using Nethereum.Web3.Accounts;
using SpockWallet.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class WalletCreated
    {
        public Wallet Wallet { get; set; }

        public Account Account { get; set; }
    }
}
