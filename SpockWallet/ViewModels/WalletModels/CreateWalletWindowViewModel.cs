using NBitcoin;
using Nethereum.Web3;
using ReactiveUI;
using Serilog;
using SpockWallet.Data;
using SpockWallet.Events;
using SpockWallet.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpockWallet.ViewModels.WalletModels
{
    public class CreateWalletWindowViewModel : ViewModelBase
    {
        public delegate void WindowsClose();

        public event WindowsClose OnWindowsClose;

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

        public async void CreateMnemonic()
        {
            Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);

            AccountMnemonic = string.Join(" ", mnemonic.Words);

            Nethereum.HdWallet.Wallet hdWallet = new Nethereum.HdWallet.Wallet(AccountMnemonic, MnemonicPassword);
            Nethereum.Web3.Accounts.Account account = hdWallet.GetAccount(0);

            Wallet wallet = new Wallet();

            if (account.Address.StartsWith("0x"))
                wallet.Address = "SPOCK-" + account.Address.Substring(2);
            else
                wallet.Address = account.Address;

            wallet.PrivateKey = account.PrivateKey;
            wallet.Balance = "0";
            wallet.StakingRequired = "0";
            wallet.PlotId = wallet.Address.ConvertPlotId();
            wallet.ScanLocation = await GetLatestBlock();

            MessageBus.Current.SendMessage(new WalletCreated()
            {
                Wallet = wallet,
                Account = new Nethereum.Web3.Accounts.Account(wallet.PrivateKey)
            });
        }

        private async Task<long> GetLatestBlock()
        {
            try
            {
                Web3 web3 = Web3Service.GetWeb3();
                var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                return Convert.ToInt64(blockNumber.Value.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateWallet - GetLatestBlock");
                return 0;
            }

        }

        public void Done()
        {
            OnWindowsClose?.Invoke();
        }
    }
}
