using Nethereum.Web3;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using SpockWallet.ViewModels.TransactionModels;

namespace SpockWallet.ViewModels.ContractModels
{
    public class TokenTransferViewModel : ViewModelBase
    {
        public TokenTransferViewModel()
        {
        }

        public TokenTransferViewModel(Data.TokenTransfer tokenTransfer)
        {
            Hash = tokenTransfer.Hash;
            BlockNumber = tokenTransfer.BlockNumber;
            From = tokenTransfer.From;
            To = tokenTransfer.To;
            Contract = tokenTransfer.ContractAddress;
            Symbol = tokenTransfer.Symbol;
            Value = tokenTransfer.Value;
        }

        private string hash;

        public string Hash
        {
            get { return hash; }
            set
            {
                this.RaiseAndSetIfChanged(ref hash, value);
            }
        }

        private long blockNumber;

        public long BlockNumber
        {
            get { return blockNumber; }
            set
            {
                this.RaiseAndSetIfChanged(ref blockNumber, value);
            }
        }

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

        private string contract;

        public string Contract
        {
            get { return contract; }
            set
            {
                this.RaiseAndSetIfChanged(ref contract, value);
            }
        }

        private string symbol;

        public string Symbol
        {
            get { return symbol; }
            set
            {
                this.RaiseAndSetIfChanged(ref symbol, value);
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

        public void OpenExplorer()
        {
            TransactionViewModel.OpenBrowser($"https://www.spock.network/tx/{Hash}");
        }
    }
}
