using DynamicData;
using DynamicData.Binding;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using SpockWallet.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ReactiveUI.Legacy;

namespace SpockWallet.ViewModels.WalletModels
{
    public class WalletsUserControlViewModel : ReactiveObject
    {
        public ReactiveList<WalletViewModel> Wallets { get; set; }

        public WalletsUserControlViewModel()
        {
            Wallets = new ReactiveList<WalletViewModel>();

            Wallets.ChangeTrackingEnabled = true;
        }

        public void ChangeSymbol(string symbol)
        {
            if (Wallets != null)
            {
                for (int i = 0; i < Wallets.Count; i++)
                    Wallets[i].Symbol = symbol;
            }
        }
    }
}
