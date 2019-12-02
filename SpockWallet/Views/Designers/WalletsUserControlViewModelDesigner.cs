using SpockWallet.Services;
using SpockWallet.ViewModels.WalletModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Views.Designers
{
    public class WalletsUserControlViewModelDesigner : WalletsUserControlViewModel
    {
        public WalletsUserControlViewModelDesigner()
            : base()
        {
            for (int i = 0; i < 3; i++)
            {
                this.Wallets.Add(new WalletViewModel(Web3Service.CreateWallet()));
            }
        }
    }
}
