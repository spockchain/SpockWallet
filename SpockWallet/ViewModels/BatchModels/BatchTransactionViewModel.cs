using Nethereum.RPC.Eth.DTOs;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.ViewModels.BatchModels
{
    public class BatchTransactionViewModel : ViewModelBase
    {
        private string from;

        public string From
        {
            get { return from; }
            set
            {
                this.RaiseAndSetIfChanged(ref from, value);
            }
        }

        private string to;

        public string To
        {
            get { return to; }
            set
            {
                this.RaiseAndSetIfChanged(ref to, value);
            }
        }

        private decimal _value;

        public decimal Value
        {
            get { return _value; }
            set
            {
                this.RaiseAndSetIfChanged(ref _value, value);
            }
        }

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                this.RaiseAndSetIfChanged(ref status, value);
            }
        }

        public TransactionInput TransactionInput { get; set; }
    }
}
