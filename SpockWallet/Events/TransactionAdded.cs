using System;
using System.Collections.Generic;
using Nethereum.RPC.Eth.DTOs;

namespace SpockWallet.Events
{
    public class TransactionAdded
    {
        public TransactionAdded(Transaction transaction)
        {
            RawTransaction = transaction;
        }

        public Transaction RawTransaction { get; set; }
    }
}
