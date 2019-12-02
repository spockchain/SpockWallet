using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Nethereum.Web3;
using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.Events;
using SpockWallet.Localizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpockWallet.ViewModels.WalletModels
{
    public class DeleteWalletWindowViewModel : ViewModelBase
    {
        public delegate void WindowsClose();

        public event WindowsClose OnWindowsClose;

        private string deleteAddress;

        public string DeleteAddress
        {
            get => deleteAddress;
            set => this.RaiseAndSetIfChanged(ref deleteAddress, value);
        }

        private string deleteStatus;

        public string DeleteStatus
        {
            get => deleteStatus;
            set => this.RaiseAndSetIfChanged(ref deleteStatus, value);
        }

        public async void Delete()
        {
            using (DataContext dataContext = new DataContext())
            {
                var wallet = dataContext.Wallets.Where(q => q.Address == DeleteAddress).FirstOrDefault();
                if (wallet != null)
                {
                    if (string.IsNullOrEmpty(wallet.Balance) || wallet.Balance == "0")
                    {
                        DeleteWallet(dataContext, wallet);
                    }
                    else
                    {
                        decimal spok = Web3.Convert.FromWei(System.Numerics.BigInteger.Parse(wallet.Balance));

                        var result = new MessageBoxWindow(new MessageBoxParams
                        {
                            Button = ButtonEnum.YesNo,
                            ContentTitle = AppResources.DeleteWallet,
                            ContentMessage = string.Format(AppResources.DeleteWalletMessage, spok),
                            Icon = Icon.Warning,
                            Style = Style.Windows
                        });

                        if ("Yes" == await result.Show())
                        {
                            DeleteWallet(dataContext, wallet);
                        }
                    }

                    return;
                }
                DeleteStatus = AppResources.WalletNotFound;
            }
        }

        private void DeleteWallet(DataContext dataContext, Wallet wallet)
        {
            wallet.IsDelete = true;
            dataContext.SaveChanges();
            DeleteStatus = DeleteAddress + " " + AppResources.WalletHasDeleted;

            MessageBus.Current.SendMessage(new WalletDeleted() { Address = wallet.Address });

            DeleteAddress = string.Empty;
        }
    }
}
