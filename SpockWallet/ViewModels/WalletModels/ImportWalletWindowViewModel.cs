using MessageBox.Avalonia;
using Nethereum.Web3.Accounts;
using ReactiveUI;
using Serilog;
using SpockWallet.Data;
using SpockWallet.Events;
using SpockWallet.Localizations;
using SpockWallet.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.ViewModels.WalletModels
{
    public class ImportWalletWindowViewModel : ViewModelBase
    {
        public delegate void WindowsClose();

        public event WindowsClose OnWindowsClose;

        private string privateKey;

        public string PrivateKey
        {
            get { return privateKey; }
            set
            {
                this.RaiseAndSetIfChanged(ref privateKey, value);
            }
        }

        private string mnemonicPassword;

        public string MnemonicPassword
        {
            get { return mnemonicPassword; }
            set
            {
                this.RaiseAndSetIfChanged(ref mnemonicPassword, value);
            }
        }

        private string accountMnemonic;

        public string AccountMnemonic
        {
            get { return accountMnemonic; }
            set
            {
                this.RaiseAndSetIfChanged(ref accountMnemonic, value);
            }
        }

        public async void LoadMnemonic()
        {
            if (string.IsNullOrWhiteSpace(AccountMnemonic))
                return;

            string[] mnemonic = AccountMnemonic.Split(' ');
            if (mnemonic.Length < 12)
            {
                await new MessageBoxWindow(AppResources.MnemonicInvalid,
                    AppResources.InvalidInputMnemonic)
                    .Show();

                return;
            }

            if (string.IsNullOrWhiteSpace(AccountMnemonic))
                return;

            Nethereum.HdWallet.Wallet hdWallet = new Nethereum.HdWallet.Wallet(AccountMnemonic, MnemonicPassword);

            LoadUserPrivateKey(hdWallet.GetAccount(0));
        }

        public async void LoadPrivateKey()
        {
            if (string.IsNullOrWhiteSpace(PrivateKey))
                return;

            try
            {
                LoadUserPrivateKey(new Account(PrivateKey.Trim()));
            }
            catch (Exception ex)
            {
                await new MessageBoxWindow(AppResources.PrivateKeyError,
                    AppResources.PrivateKeyError)
                    .Show();

                Log.Error(ex, "ImportWalletWindowViewModel - LoadPrivateKey");
            }
        }

        private void LoadUserPrivateKey(Account account)
        {
            Wallet wallet = new Wallet();

            if (account.Address.StartsWith("0x"))
                wallet.Address = "SPOCK-" + account.Address.Substring(2);
            else
                wallet.Address = account.Address;

            wallet.PrivateKey = account.PrivateKey;
            wallet.Balance = "0";
            wallet.StakingRequired = "0";
            wallet.PlotId = wallet.Address.ConvertPlotId();

            MessageBus.Current.SendMessage(new WalletCreated()
            {
                Wallet = wallet,
                Account = new Account(wallet.PrivateKey)
            });

            OnWindowsClose?.Invoke();
        }
    }
}
