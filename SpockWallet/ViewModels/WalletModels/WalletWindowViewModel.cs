using SpockWallet.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Nethereum.Signer;
using SpockWallet.Services;
using ReactiveUI;
using DynamicData;
using SpockWallet.Views;
using SpockWallet.Events;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Nethereum.Web3;
using System.Numerics;
using SpockWallet.CacheModel;
using Serilog;
using Avalonia.Media;
using System.Threading;
using ReactiveUI.Legacy;
using System.Reactive;
using SpockWallet.Localizations;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using SpockWallet.ViewModels.SettingModels;
using SpockWallet.ViewModels.TransactionModels;

namespace SpockWallet.ViewModels.WalletModels
{
    public class WalletWindowViewModel : ViewModelBase, IDisposable
    {
        public ScanLocationUpdate UpdateWalletScanLocation { get; set; }

        public CancellationTokenSource UpdateWalletScanLocationCancellationToken { get; set; }

        public double AngleSync { get; set; }

        private readonly List<WalletCache> _walletCaches = new List<WalletCache>();

        private readonly object _walletUpdateLock = new object();

        private readonly IDisposable _talkTimer;

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(App.SyncTimeSpan);

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

        private WalletsUserControlViewModel walletsUserControlViewModel;

        public WalletsUserControlViewModel WalletsUserControlViewModel
        {
            get { return walletsUserControlViewModel; }
            set
            {
                this.RaiseAndSetIfChanged(ref walletsUserControlViewModel, value);
            }
        }

        private SettingUserControlViewModel settingUserControlViewModel;
        public SettingUserControlViewModel SettingUserControlViewModel
        {
            get { return settingUserControlViewModel; }
            set
            {
                this.RaiseAndSetIfChanged(ref settingUserControlViewModel, value);
            }
        }

        private TransactionUserControlViewModel _transactionsViewModel;
        public TransactionUserControlViewModel TransactionsViewModel
        {
            get { return _transactionsViewModel; }
            set
            {
                this.RaiseAndSetIfChanged(ref _transactionsViewModel, value);
            }
        }

        private AssetsViewModel currentAssetsViewModel;

        public AssetsViewModel CurrentAssetsViewModel
        {
            get { return currentAssetsViewModel; }
            set
            {
                this.RaiseAndSetIfChanged(ref currentAssetsViewModel, value);
            }
        }

        private string blockHeight;

        public string BlockHeight
        {
            get { return blockHeight; }
            set
            {
                this.RaiseAndSetIfChanged(ref blockHeight, value);
            }
        }

        private string totalBalance;

        public string TotalBalance
        {
            get { return totalBalance; }
            set
            {
                this.RaiseAndSetIfChanged(ref totalBalance, value);
            }
        }

        private RotateTransform rotateTransform;

        public RotateTransform RotateTransform
        {
            get { return rotateTransform; }
            set
            {
                this.RaiseAndSetIfChanged(ref rotateTransform, value);
            }
        }

        private bool isSyncFailed;

        public bool IsSyncFailed
        {
            get { return isSyncFailed; }
            set
            {
                this.RaiseAndSetIfChanged(ref isSyncFailed, value);
            }
        }

        private bool isSyncLostBlock;

        public bool IsSyncLostBlock
        {
            get { return isSyncLostBlock; }
            set
            {
                this.RaiseAndSetIfChanged(ref isSyncLostBlock, value);
            }
        }

        public WalletWindowViewModel()
        {
            UpdateWalletScanLocation = new ScanLocationUpdate();
            UpdateWalletScanLocationCancellationToken = new CancellationTokenSource();
            FilterAssetsCommand = ReactiveCommand.Create<AssetsViewModel>(SelectAssets);

            var (wallets, trans, contracts) = InitWalletData();

            WalletsUserControlViewModel = new WalletsUserControlViewModel();
            Array.ForEach(wallets, p =>
            {
                _walletCaches.Add(new WalletCache() { Address = p.Address });
                WalletsUserControlViewModel.Wallets.Add(new WalletViewModel(p));
            });

            MenuItems = new ReactiveList<MenuItemViewModel>();
            MenuItems.Add(new MenuItemViewModel()
            {
                Header = AppResources.SelectAssets,
                Items = contracts.Select(p => new MenuItemViewModel()
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

            SettingUserControlViewModel = new SettingUserControlViewModel();
            TransactionsViewModel = new TransactionUserControlViewModel(_walletCaches.Select(p => p.Address).ToList(), trans);

            InitEventListen();

            _talkTimer = Observable.Timer(_updateInterval, _updateInterval, RxApp.TaskpoolScheduler)
                .Subscribe(async _ => await CheckWalletBalanceAsync()); ;

            BlockHeight = "--";
            RotateTransform = new RotateTransform() { Angle = AngleSync };
            IsSyncFailed = false;

            if (!wallets.Any())
                CreateWallet();

            //开启队列
            Task.Factory.StartNew(() => UpdateWalletScanLocation.Start(UpdateWalletScanLocationCancellationToken.Token));
        }

        private void InitEventListen()
        {
            MessageBus.Current.Listen<WalletCreated>().Subscribe(async wallet =>
            {
                using (DataContext dataContext = new DataContext())
                {
                    WalletViewModel walletViewModel = new WalletViewModel(wallet.Wallet);
                    if(CurrentAssetsViewModel.CoinTypeAddress != null)
                    {
                        walletViewModel.Symbol = CurrentAssetsViewModel.CoinType;

                        var tokenBalanceHandler = Web3Service.GetWeb3().Eth.GetContractQueryHandler<BalanceOfFunction>();
                        var tokenBalance = await tokenBalanceHandler.QueryAsync<BigInteger>(CurrentAssetsViewModel.CoinTypeAddress.EnsureHexAddress(), new BalanceOfFunction()
                        {
                            Owner = wallet.Wallet.Address.EnsureHexAddress(),
                        });

                        walletViewModel.Balance = Web3.Convert.FromWei(tokenBalance, CurrentAssetsViewModel.CoinTypeDecimal);
                    }

                    var userWallet = dataContext.Wallets.Where(p => p.Address == wallet.Wallet.Address).FirstOrDefault();
                    if (userWallet == null)
                    {
                        lock (_walletUpdateLock)
                        {
                            _walletCaches.Add(new WalletCache() { Address = wallet.Wallet.Address });
                        }

                        dataContext.Wallets.Add(wallet.Wallet);
                        if (dataContext.SaveChanges() > 0)
                            WalletsUserControlViewModel.Wallets.Add(walletViewModel);
                    }
                    else if (userWallet != null && userWallet.IsDelete)
                    {
                        lock (_walletUpdateLock)
                        {
                            _walletCaches.Add(new WalletCache() { Address = wallet.Wallet.Address });
                        }

                        userWallet.IsDelete = false;
                        dataContext.Update(userWallet);

                        if (dataContext.SaveChanges() > 0)
                            WalletsUserControlViewModel.Wallets.Add(walletViewModel);
                    }
                }
            });

            MessageBus.Current.Listen<ContractCreated>().Subscribe(x =>
            {
                var lastMenuItem = MenuItems[0].Items;
                lastMenuItem.Add(new MenuItemViewModel()
                {
                    Command = FilterAssetsCommand,
                    CommandParameter = new AssetsViewModel()
                    {
                        CoinType = x.Contract.Symbol,
                        CoinName = x.Contract.TokenName,
                        CoinTypeAddress = x.Contract.Address,
                        CoinTypeDecimal = x.Contract.Decimals
                    },
                    Header = $"{x.Contract.Symbol} ({x.Contract.TokenName})"
                });

                MenuItems = new ReactiveList<MenuItemViewModel>();
                MenuItems.Add(new MenuItemViewModel()
                {
                    Header = AppResources.SelectAssets,
                    Items = lastMenuItem
                });
            });

            MessageBus.Current.Listen<WalletDeleted>().Subscribe(state =>
            {
                var wallet = WalletsUserControlViewModel.Wallets.Where(p => p.Address == state.Address).FirstOrDefault();
                if (wallet != null)
                {
                    wallet.CancellationTokenSource?.Cancel();

                    WalletsUserControlViewModel.Wallets.Remove(wallet);
                }
            });

            MessageBus.Current.Listen<WalletBalanceChanged>().Subscribe(wallet =>
            {
                var walletViewModel = WalletsUserControlViewModel.Wallets.Where(p => p.Address == wallet.Address).FirstOrDefault();

                if (walletViewModel != null)
                {
                    walletViewModel.Balance = wallet.Balance;
                    walletViewModel.StakingRequired = wallet.StakingRequired;
                }
            });

            MessageBus.Current.Listen<BlockUpdated>().Subscribe(block =>
            {
                BlockHeight = block.BlockHeight;
                TotalBalance = WalletsUserControlViewModel.Wallets.Sum(p => p.Balance).ToString();
            });

            MessageBus.Current.Listen<BlockSynced>().Subscribe(block =>
            {
                IsSyncFailed = !block.IsSuccess;

                if (block.IsSuccess)
                {
                    RotateTransform.Angle += 22;
                    if (RotateTransform.Angle >= 199705)
                        RotateTransform.Angle = 0;
                }
            });

            MessageBus.Current.Listen<SyncLostBlock>().Subscribe(block =>
            {
                //显示同步丢失区块提示
                if (block.IsLost != IsSyncLostBlock)
                    IsSyncLostBlock = block.IsLost;
            });
        }

        private (Wallet[] wallets, Data.Transaction[] trans, Data.Contract[] contract) InitWalletData()
        {
            using (DataContext dataContext = new DataContext())
            {
                var wallets = dataContext.Wallets.Where(p => !p.IsDelete).ToArray();
                var transactions = dataContext.Transactions.OrderByDescending(p => p.BlockNumber).ToArray();
                var contracts = dataContext.Contracts.OrderByDescending(p => !p.IsDelete).ToArray();

                return (wallets, transactions, contracts);
            }
        }

        private async void SelectAssets(AssetsViewModel assets)
        {
            if (assets.CoinTypeAddress != null && assets.CoinTypeAddress == CurrentAssetsViewModel.CoinTypeAddress)
                return;

            CurrentAssetsViewModel = assets;
            await CheckWalletBalanceAsync();
            WalletsUserControlViewModel?.ChangeSymbol(assets.CoinType);
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

        public async Task CheckWalletBalanceAsync()
        {
            IEnumerable<WalletCache> walletCaches = new List<WalletCache>();
            AssetsViewModel currentAssets;

            lock (_walletUpdateLock)
            {
                currentAssets = CurrentAssetsViewModel;
                walletCaches = _walletCaches.ToArray();
            }

            Web3 web3 = Web3Service.GetWeb3();

            foreach (var item in walletCaches)
            {
                try
                {
                    using (DataContext dataContext = new DataContext())
                    {
                        WalletBalanceChanged balanceChanged = new WalletBalanceChanged();

                        var wallet = dataContext.Wallets.Where(p => p.Address == item.Address).FirstOrDefault();
                        balanceChanged.Address = wallet.Address;

                        EthGetRequiredStake ethGetRequiredStake = new EthGetRequiredStake(web3.Client);
                        var stake = await ethGetRequiredStake.SendRequestAsync(item.Address.EnsureHexAddress());

                        wallet.StakingRequired = stake.Value.ToString();
                        balanceChanged.StakingRequired = Web3.Convert.FromWei(stake.Value);

                        var balance = await web3.Eth.GetBalance.SendRequestAsync(item.Address.EnsureHexAddress());
                        wallet.Balance = balance.Value.ToString();
                        if (currentAssets.CoinType == "SPOK" && currentAssets.CoinTypeAddress == null)
                        {
                            balanceChanged.Balance = Web3.Convert.FromWei(balance.Value);
                        }
                        else if (!string.IsNullOrEmpty(currentAssets.CoinTypeAddress))
                        {
                            var tokenBalanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
                            var tokenBalance = await tokenBalanceHandler.QueryAsync<BigInteger>(currentAssets.CoinTypeAddress.EnsureHexAddress(), new BalanceOfFunction()
                            {
                                Owner = balanceChanged.Address.EnsureHexAddress(),
                            });

                            balanceChanged.Balance = Web3.Convert.FromWei(tokenBalance, currentAssets.CoinTypeDecimal);
                        }

                        dataContext.Update(wallet);
                        dataContext.SaveChanges();

                        var blockHeight = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                        BlockUpdated blockUpdated = new BlockUpdated
                        {
                            BlockHeight = blockHeight.Value.ToString()
                        };

                        var msg = (balanceChanged, blockUpdated);

                        RxApp.MainThreadScheduler.Schedule(msg, (schedule, state) =>
                        {
                            MessageBus.Current.SendMessage(state.balanceChanged);
                            MessageBus.Current.SendMessage(state.blockUpdated);

                            return new ScanDisposable();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Wallet - CheckWalletBalance");
                }
            }
        }

        public void Dispose()
        {
            //取消所有正在同步的钱包
            foreach (var item in WalletsUserControlViewModel.Wallets)
            {
                item?.CancellationTokenSource?.Cancel();
            }

            //停止所有正在检查的交易收据
            TransactionsViewModel?.Dispose();

            _talkTimer.Dispose();
        }
    }
}
