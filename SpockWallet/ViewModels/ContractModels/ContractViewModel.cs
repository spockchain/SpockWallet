using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.ViewModels.ContractModels
{
    public class ContractViewModel : ViewModelBase
    {
        public ContractViewModel(Data.Contract contract)
        {
            Address = contract.Address;
            CreationTransaction = contract.CreationTransaction;
            TokenName = contract.TokenName;
            Symbol = contract.Symbol;
            Owner = contract.Owner;
            TotalSupply = contract.TotalSupply;
            Decimals = contract.Decimals;
        }

        public ContractViewModel()
        {
        }

        private string address;

        public string Address
        {
            get { return address; }
            set
            {
                this.RaiseAndSetIfChanged(ref address, value);
            }
        }

        private string creationTransaction;

        public string CreationTransaction
        {
            get { return creationTransaction; }
            set
            {
                this.RaiseAndSetIfChanged(ref creationTransaction, value);
            }
        }

        private string tokenName;

        public string TokenName
        {
            get { return tokenName; }
            set
            {
                this.RaiseAndSetIfChanged(ref tokenName, value);
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

        private string owner;

        public string Owner
        {
            get { return owner; }
            set
            {
                this.RaiseAndSetIfChanged(ref owner, value);
            }
        }

        private string totalSupply;

        public string TotalSupply
        {
            get { return totalSupply; }
            set
            {
                this.RaiseAndSetIfChanged(ref totalSupply, value);
            }
        }

        private int decimals;

        public int Decimals
        {
            get { return decimals; }
            set
            {
                this.RaiseAndSetIfChanged(ref decimals, value);
            }
        }
    }
}
