using System.Reactive.Linq;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using ReactiveUI;
using Nethereum.Web3;
using SpockWallet.Services;
using SpockWallet.Events;
using MessageBox.Avalonia;
using SpockWallet.Localizations;
using SpockWallet.Views;
using SpockWallet.ViewModels.WalletModels;
using ReactiveUI.Legacy;
using System.Reactive;
using SpockWallet.Data;
using System.Collections.Generic;
using System.Linq;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using System.Numerics;
using Serilog;
using System;
using Nethereum.Contracts;

namespace SpockWallet.ViewModels.TransactionModels
{
    public class SendTransactionViewModel : SendTransactionBaseViewModel
    {
        public delegate void WindowsClose();

        public event WindowsClose OnWindowsClose;

        private bool IsTokenTransfer = false;

        private AssetsViewModel currentAssetsViewModel;

        public AssetsViewModel CurrentAssetsViewModel
        {
            get { return currentAssetsViewModel; }
            set
            {
                this.RaiseAndSetIfChanged(ref currentAssetsViewModel, value);
            }
        }

        public ReactiveCommand<AssetsViewModel, Unit> FilterAssetsCommand { get; }

        private ReactiveList<MenuItemViewModel> menuItems;

        public ReactiveList<MenuItemViewModel> MenuItems
        {
            get { return menuItems; }
            set
            {
                this.RaiseAndSetIfChanged(ref menuItems, value);
            }
        }

        private decimal balance;

        public decimal Balance
        {
            get { return balance; }
            set
            {
                this.RaiseAndSetIfChanged(ref balance, value);
            }
        }

        private bool showAdvanced;

        public bool ShowAdvanced
        {
            get { return showAdvanced; }
            set
            {
                this.RaiseAndSetIfChanged(ref showAdvanced, value);
            }
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

        public void SwitchAdvanced()
        {
            ShowAdvanced = !ShowAdvanced;
        }

        public SendTransactionViewModel()
        {
            Gas = (ulong)Nethereum.Signer.SignedTransactionBase.DEFAULT_GAS_LIMIT;
            GasPrice = (ulong)Nethereum.Signer.SignedTransactionBase.DEFAULT_GAS_PRICE;

            var canExecuteTransaction = this.WhenAnyValue(
                x => x.AddressTo,
                x => x.AmountInEther,
                x => x.Account,
                x => x.Data,
                (addressTo, amountInEther, account, input) =>
                        (Web3Service.IsValidAddress(addressTo) || !string.IsNullOrEmpty(input) && input.StartsWith("0x") && input.Length > 2) &&
                        amountInEther != null &&
                        account != null);

            _executeTrasnactionCommand = ReactiveCommand.CreateFromTask(ExecuteAsync, canExecuteTransaction);
            FilterAssetsCommand = ReactiveCommand.Create<AssetsViewModel>(SelectAssets);

            List<Data.Contract> contracts = new List<Data.Contract>();
            using (DataContext dataContext = new DataContext())
                contracts = dataContext.Contracts
                    .Where(p => !p.IsDelete)
                    .ToList();

            MenuItems = new ReactiveList<MenuItemViewModel>();
            MenuItems.Add(new MenuItemViewModel()
            {
                Header = AppResources.SelectAssets,
                Items = contracts?.Select(p => new MenuItemViewModel()
                {
                    Command = FilterAssetsCommand,
                    CommandParameter = new AssetsViewModel()
                    {
                        CoinType = p.Symbol,
                        CoinName = p.TokenName,
                        CoinTypeAddress = p.Address,
                        CoinTypeDecimal = p.Decimals
                    },
                    Header = $"{p.Symbol} ({p.TokenName})"
                }).ToList()
            });

            MenuItems[0].Items.Insert(0,
                new MenuItemViewModel()
                {
                    Header = "SPOK",
                    Command = FilterAssetsCommand,
                    CommandParameter = new AssetsViewModel()
                    {
                        CoinName = "Spock Network",
                        CoinType = "SPOK",
                        CoinTypeDecimal = 18,
                    }
                });

            CurrentAssetsViewModel = (AssetsViewModel)MenuItems[0].Items[0].CommandParameter;
        }

        public async Task<string> ExecuteAsync()
        {
            var web3 = Web3Service.GetWeb3(Account);

            if (IsTokenTransfer)
                return await TokenTransferAsync(web3);
            else
                return await SendSpokAsync(web3);
        }

        private async void SelectAssets(AssetsViewModel assets)
        {
            if (assets.CoinTypeAddress != null && assets.CoinTypeAddress == CurrentAssetsViewModel.CoinTypeAddress)
                return;

            CurrentAssetsViewModel = assets;

            Web3 web3 = Web3Service.GetWeb3();

            if (assets.CoinType == "SPOK" && assets.CoinTypeAddress == null)
            {
                IsTokenTransfer = false;
                var spokBalance = await web3.Eth.GetBalance.SendRequestAsync(Account.Address.EnsureHexAddress());
                Balance = Web3.Convert.FromWei(spokBalance.Value);
            }
            else
            {
                IsTokenTransfer = true;
                var tokenBalanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
                var tokenBalance = await tokenBalanceHandler.QueryAsync<BigInteger>(assets.CoinTypeAddress.EnsureHexAddress(), new BalanceOfFunction()
                {
                    Owner = Account.Address,
                });

                Balance = Web3.Convert.FromWei(tokenBalance, assets.CoinTypeDecimal);
            }
        }

        private async Task<string> TokenTransferAsync(Web3 web3)
        {
            var transferFunction = new TransferFunction()
            {
                To = AddressTo.EnsureHexAddress(),
                Value = Web3.Convert.ToWei(AmountInEther.Value, CurrentAssetsViewModel.CoinTypeDecimal),
                FromAddress = web3.TransactionManager.Account.Address.EnsureHexAddress(),
            };

            var input = transferFunction.CreateTransactionInput(CurrentAssetsViewModel.CoinTypeAddress.EnsureHexAddress());
            var contractHandler = web3.Eth.GetContractHandler(CurrentAssetsViewModel.CoinTypeAddress.EnsureHexAddress());

            try
            {
                var gas = await contractHandler.EstimateGasAsync(transferFunction);
                input.Gas = gas;

                string txHash = await web3.Eth.TransactionManager
                    .SendTransactionAsync(input);

                var transaction =
                    await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);

                MessageBus.Current.SendMessage(new TransactionAdded(transaction));

                OnWindowsClose?.Invoke();
                return txHash;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                await new MessageBoxWindow(AppResources.SendFailed,
                    ex.Message).Show();
            }

            return null;
        }

        private async Task<string> SendSpokAsync(Web3 web3)
        {
            try
            {
                var transactionInput =
                    new TransactionInput
                    {
                        Value = new HexBigInteger(Web3.Convert.ToWei(AmountInEther.Value)),
                        To = AddressTo?.EnsureHexAddress(),
                        From = web3.TransactionManager.Account.Address.EnsureHexAddress()
                    };
                if (Gas != null) transactionInput.Gas = new HexBigInteger(Gas.Value);
                if (GasPrice != null) transactionInput.GasPrice = new HexBigInteger(GasPrice.Value);
                if (Nonce != null) transactionInput.Nonce = new HexBigInteger(Nonce.Value);
                if (!string.IsNullOrEmpty(Data)) transactionInput.Data = Data;

                var transactionHash = await web3.TransactionManager.SendTransactionAsync(transactionInput);
                var transaction =
                    await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionHash);

                MessageBus.Current.SendMessage(new TransactionAdded(transaction));

                OnWindowsClose?.Invoke();
                return transactionHash;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                await new MessageBoxWindow(AppResources.SendFailed,
                    ex.Message).Show();

                return null;
            }
        }

        public void BatchTransfer()
        {
            BatchTransferWindow batchTransferWindow = new BatchTransferWindow(Address, Account);
            batchTransferWindow.Show();

            OnWindowsClose?.Invoke();
        }
    }
}