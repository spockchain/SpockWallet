using Avalonia;
using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.ViewModels.WalletModels;
using SpockWallet.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.ViewModels.OfflineModels
{
    public class OfflineWalletViewModel : ViewModelBase
    {
        private readonly Wallet _wallet;

        public OfflineWalletViewModel()
        {

        }

        public OfflineWalletViewModel(Wallet wallet)
        {
            _wallet = wallet;
            address = wallet.Address;
            plotid = wallet.PlotId;
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

        private string plotid;

        public string Plotid
        {
            get { return plotid; }
            set
            {
                this.RaiseAndSetIfChanged(ref plotid, value);
            }
        }

        public async void CopyAddress()
        {
            await Application.Current.Clipboard.SetTextAsync(
#if DEBUG
        Address + ":" + _wallet.PrivateKey
#else
        Address
#endif
                );
        }

        public async void CopyPlotid()
        {
            await Application.Current.Clipboard.SetTextAsync(Plotid);
        }

        public void Receive()
        {
            ReceiveWindow receiveWindow = new ReceiveWindow();
            receiveWindow.DataContext = new ReceiveWindowViewModel()
            {
                Address = address,
                Plotid = plotid,
                QrCode = WalletViewModel.Generate2D(address, 200, 200)
            };
            receiveWindow.Show();
        }
    }
}
