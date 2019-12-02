using ReactiveUI.Legacy;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.ViewModels.OfflineModels
{
    public class OfflineWalletUserControlViewModel : ViewModelBase
    {
        public ReactiveList<OfflineWalletViewModel> Wallets { get; set; }

        public OfflineWalletUserControlViewModel()
        {
            Wallets = new ReactiveList<OfflineWalletViewModel>();

            Wallets.ChangeTrackingEnabled = true;
        }
    }
}
