using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.ViewModels.WalletModels
{
    public class AssetsViewModel : ViewModelBase
    {
        private string coinType;

        public string CoinType
        {
            get { return coinType; }
            set
            {
                this.RaiseAndSetIfChanged(ref coinType, value);
            }
        }

        private string coinName;

        public string CoinName
        {
            get { return coinName; }
            set
            {
                this.RaiseAndSetIfChanged(ref coinName, value);
            }
        }

        private string coinTypeAddress;

        public string CoinTypeAddress
        {
            get { return coinTypeAddress; }
            set
            {
                this.RaiseAndSetIfChanged(ref coinTypeAddress, value);
            }
        }

        private int coinTypeDecimal;

        public int CoinTypeDecimal
        {
            get { return coinTypeDecimal; }
            set
            {
                this.RaiseAndSetIfChanged(ref coinTypeDecimal, value);
            }
        }
    }
}
