using System;
using System.Collections.Generic;
using System.Text;
using SpockWallet.Data;

namespace SpockWallet.Events
{
    public class ContractCreated
    {
        public ContractCreated(Contract contract)
        {
            this.Contract = contract;
        }

        public Contract Contract { get; set; }
    }
}
