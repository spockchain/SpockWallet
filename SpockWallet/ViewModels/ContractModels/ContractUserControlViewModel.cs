using System;
using SpockWallet.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Text;
using AvaloniaEdit;
using System.IO;
using ReactiveUI.Legacy;
using SpockWallet.ViewModels.ContractModels;
using ReactiveUI;
using SpockWallet.Data;
using System.Linq;
using SpockWallet.Localizations;
using Nethereum.Web3;
using System.Reactive;
using System.Reactive.Linq;
using SpockWallet.Events;
using SpockWallet.ViewModels.TransactionModels;

namespace SpockWallet.ViewModels.ContractModels
{
    public class ContractUserControlViewModel : ViewModelBase
    {
        public ReactiveCommand<Data.Contract, Unit> FilterContractCommand { get; }

        private ReactiveList<MenuItemViewModel> menuItems;

        public ReactiveList<MenuItemViewModel> MenuItems
        {
            get { return menuItems; }
            set
            {
                this.RaiseAndSetIfChanged(ref menuItems, value);
            }
        }

        private ContractViewModel contract;

        public ContractViewModel Contract
        {
            get { return contract; }
            set
            {
                this.RaiseAndSetIfChanged(ref contract, value);
            }
        }

        private ReactiveList<TokenTransferViewModel> tokenTransfers;

        public ReactiveList<TokenTransferViewModel> TokenTransfers
        {
            get { return tokenTransfers; }
            set
            {
                this.RaiseAndSetIfChanged(ref tokenTransfers, value);
            }
        }

        private bool showContractDetail;

        public bool ShowContractDetail
        {
            get { return showContractDetail; }
            set
            {
                this.RaiseAndSetIfChanged(ref showContractDetail, value);
            }
        }

        public ContractUserControlViewModel()
        {
            MenuItems = new ReactiveList<MenuItemViewModel>();
            FilterContractCommand = ReactiveCommand.Create<Data.Contract>(SelectContract);
            ShowContractDetail = false;

            MessageBus.Current.Listen<ContractCreated>().Subscribe(x =>
            {
                ShowContractDetail = true;

                var lastMenuItem = MenuItems[0].Items;
                lastMenuItem.Add(new MenuItemViewModel()
                {
                    Command = FilterContractCommand,
                    CommandParameter = x.Contract,
                    Header = $"{x.Contract.Symbol} ({x.Contract.TokenName})"
                });

                MenuItems = new ReactiveList<MenuItemViewModel>();
                MenuItems.Add(new MenuItemViewModel()
                {
                    Header = AppResources.SelectToken,
                    Items = lastMenuItem
                });

                if (Contract == null)
                    Contract = new ContractViewModel(x.Contract);
            });

            MessageBus.Current.Listen<TokenTransferAdded>().Subscribe(x =>
            {
                if (Contract != null && Contract.Address == x.TokenTransfer.ContractAddress)
                    TokenTransfers.Insert(0, new TokenTransferViewModel(x.TokenTransfer));
            });

            using (DataContext dataContext = new DataContext())
            {
                //加载合约
                var contracts = dataContext.Contracts.Where(p => !p.IsDelete).ToArray();
                if (contracts != null && contracts.Length > 0)
                {
                    MenuItems.Add(new MenuItemViewModel()
                    {
                        Header = AppResources.SelectToken,
                        Items = contracts.Select(p => new MenuItemViewModel()
                        {
                            Command = FilterContractCommand,
                            CommandParameter = p,
                            Header = $"{p.Symbol} ({p.TokenName})"
                        }).ToList()
                    });

                    ShowContractDetail = true;
                    var defaultContract = new ContractViewModel(contracts.FirstOrDefault());
                    Contract = defaultContract;

                    //如果存在则加载合约地址
                    var tokenTransfers = dataContext.TokenTransfers.Where(p => p.ContractAddress == contract.Address).ToArray();
                    if (tokenTransfers != null && tokenTransfers.Length > 0)
                        TokenTransfers = new ReactiveList<TokenTransferViewModel>(tokenTransfers.Select(p => new TokenTransferViewModel(p)));
                    else
                        TokenTransfers = new ReactiveList<TokenTransferViewModel>();

                    TokenTransfers.ChangeTrackingEnabled = true;
                }
                else
                {
                    MenuItems.Add(new MenuItemViewModel()
                    {
                        Header = AppResources.NoContractAvailable,
                        Items = new ReactiveList<MenuItemViewModel>()
                    });
                    TokenTransfers = new ReactiveList<TokenTransferViewModel>();
                    TokenTransfers.ChangeTrackingEnabled = true;
                }
            }
        }

        public void DeployConteact()
        {
            CreateContractWindow createContract = new CreateContractWindow();
            var createContractWindowViewModel = new CreateContractWindowViewModel(createContract.FindControl<TextEditor>("SourceCodeEditor"),
                createContract.FindControl<StackPanel>("constructorParamsBorder"),
                createContract.FindControl<TextEditor>("AbiEditor"));
            createContractWindowViewModel.OnWindowsClose += () => createContract.Close();
            createContract.DataContext = createContractWindowViewModel;
            createContract.Show();

            //价值合约简单示例
            if (File.Exists("simple-example.sol"))
            {
                string code = File.ReadAllText("simple-example.sol");
                createContractWindowViewModel.LoadSourceCode(code);
            }
        }

        public void SelectContract(Data.Contract contract)
        {
            if (Contract != null && Contract.Address == contract.Address)
                return;

            Contract = new ContractViewModel(contract);
            using (DataContext dataContext = new DataContext())
            {
                var tokenTransfers = dataContext.TokenTransfers.Where(p => p.ContractAddress == contract.Address).ToArray();
                if (tokenTransfers != null && tokenTransfers.Length > 0)
                    TokenTransfers = new ReactiveList<TokenTransferViewModel>(tokenTransfers.Select(p => new TokenTransferViewModel(p)));
                else
                    TokenTransfers = new ReactiveList<TokenTransferViewModel>();

                TokenTransfers.ChangeTrackingEnabled = true;
            }
        }

        public async void CopyAddress()
        {
            if (Contract != null && !string.IsNullOrEmpty(Contract.Address))
                await Application.Current.Clipboard.SetTextAsync(Contract.Address);
        }

        public void OpenExplorer()
        {
            if (Contract != null && !string.IsNullOrEmpty(Contract.Address))
                TransactionViewModel.OpenBrowser($"https://www.spock.network/token/{Contract.Address}");
        }
    }
}
