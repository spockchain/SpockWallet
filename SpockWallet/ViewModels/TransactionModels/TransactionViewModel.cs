using Avalonia;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using ReactiveUI;
using SpockWallet.Localizations;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpockWallet.ViewModels.TransactionModels
{
    public class TransactionViewModel : ReactiveObject
    {
        public readonly static string STATUS_INPROGRESS;
        public readonly static string STATUS_COMPLETED;
        public readonly static string STATUS_FAILED;

        static TransactionViewModel()
        {
            STATUS_INPROGRESS = AppResources.TxStatusPending;
            STATUS_COMPLETED = AppResources.TxStatusCompleted;
            STATUS_FAILED = AppResources.TxStatusFailed;
        }

        public void Initialise(Transaction transaction)
        {
            TransactionHash = transaction.TransactionHash;
            BlockNumber = transaction.BlockNumber.Value.ToString();
            From = transaction.From;
            To = transaction.To;
            Gas = (ulong)transaction.Gas.Value;
            GasPrice = (ulong)transaction.GasPrice.Value;
            Data = transaction.Input;
            Value = Web3.Convert.FromWei(transaction.Value.Value).ToString();

            if (transaction.Value != null) Amount = Web3.Convert.FromWei(transaction.Value.Value);
        }

        private string _blockNumber;
        public string BlockNumber
        {
            get => _blockNumber;
            set => this.RaiseAndSetIfChanged(ref _blockNumber, value);
        }

        private string _transactionHash;
        public string TransactionHash
        {
            get => _transactionHash;
            set => this.RaiseAndSetIfChanged(ref _transactionHash, value);
        }

        private string _from;
        public string From
        {
            get => _from;
            set => this.RaiseAndSetIfChanged(ref _from, value);
        }

        private string _value;
        public string Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        private string _to;
        public string To
        {
            get => _to;
            set => this.RaiseAndSetIfChanged(ref _to, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => this.RaiseAndSetIfChanged(ref _amount, value);
        }

        private ulong? _gas;
        public ulong? Gas
        {
            get => _gas;
            set => this.RaiseAndSetIfChanged(ref _gas, value);
        }

        private string _data;
        public string Data
        {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value);
        }

        private ulong? _gasPrice;
        public ulong? GasPrice
        {
            get => _gasPrice;
            set => this.RaiseAndSetIfChanged(ref _gasPrice, value);
        }

        private string _status;

        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        private bool _isShow;

        public bool IsShow
        {
            get => _isShow;
            set => this.RaiseAndSetIfChanged(ref _isShow, value);
        }

        public void ViewExplorer()
        {
            OpenBrowser($"https://www.spock.network/tx/{TransactionHash}");
        }

        public async void CopyTxHash()
        {
            await Application.Current.Clipboard.SetTextAsync(TransactionHash);
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public static void ExecCommand(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }
}