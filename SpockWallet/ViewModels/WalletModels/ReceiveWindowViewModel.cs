using Avalonia;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.ViewModels.WalletModels
{
    public class ReceiveWindowViewModel : ViewModelBase
    {
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

        private Bitmap qrCode;

        public Bitmap QrCode
        {
            get { return qrCode; }
            set
            {
                this.RaiseAndSetIfChanged(ref qrCode, value);
            }
        }

        public async void CopyAddress()
        {
            await Application.Current.Clipboard.SetTextAsync(Address);
        }
    }
}
