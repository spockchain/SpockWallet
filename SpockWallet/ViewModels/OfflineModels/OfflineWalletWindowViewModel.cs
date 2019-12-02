using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.Events;
using SpockWallet.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpockWallet.ViewModels.OfflineModels
{
    public class OfflineWalletWindowViewModel : ViewModelBase
    {
        private OfflineWalletUserControlViewModel walletsUserControlViewModel;

        public OfflineWalletUserControlViewModel WalletsUserControlViewModel
        {
            get { return walletsUserControlViewModel; }
            set
            {
                this.RaiseAndSetIfChanged(ref walletsUserControlViewModel, value);
            }
        }

        public OfflineWalletWindowViewModel()
        {
            WalletsUserControlViewModel = new OfflineWalletUserControlViewModel();

            using (DataContext dataContext = new DataContext())
            {
                var wallets = dataContext.Wallets.ToArray();
                if (wallets.Any())
                {
                    foreach (Wallet item in wallets)
                    {
                        WalletsUserControlViewModel.Wallets.Add(new OfflineWalletViewModel(item));
                    }
                }
                else
                {
                    CreateWallet();
                }
            }

            MessageBus.Current.Listen<WalletCreated>().Subscribe(wallet =>
            {
                using (DataContext dataContext = new DataContext())
                {
                    var userWallet = dataContext.Wallets.Where(p => p.Address == wallet.Wallet.Address).FirstOrDefault();
                    if (userWallet == null)
                    {
                        dataContext.Wallets.Add(wallet.Wallet);
                        if (dataContext.SaveChanges() > 0)
                            WalletsUserControlViewModel.Wallets.Add(new OfflineWalletViewModel(wallet.Wallet));
                    }
                    else if (userWallet != null && userWallet.IsDelete)
                    {
                        wallet.Wallet.IsDelete = false;
                        dataContext.Update(wallet);

                        if (dataContext.SaveChanges() > 0)
                            WalletsUserControlViewModel.Wallets.Add(new OfflineWalletViewModel(wallet.Wallet));
                    }
                }
            });
        }

        private void ImportWallet()
        {
            ImportWalletWindow importPrivateKeyWindow = new ImportWalletWindow();
            importPrivateKeyWindow.Show();
        }

        private void CreateWallet()
        {
            CreateWalletWindow createWalletWindow = new CreateWalletWindow();
            createWalletWindow.Show();
        }
    }
}
