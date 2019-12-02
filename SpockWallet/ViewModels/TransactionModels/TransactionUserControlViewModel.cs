using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Legacy;
using Nethereum.Web3;
using SpockWallet.Events;
using SpockWallet.Services;
using SpockWallet.CacheModel;
using Serilog;
using SpockWallet.Localizations;
using SpockWallet.Views;
using System.Reactive;
using SpockWallet.Data;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;

namespace SpockWallet.ViewModels.TransactionModels
{
    public class TransactionUserControlViewModel : ReactiveObject, IDisposable
    {
        public ReactiveList<TransactionViewModel> Transactions { get; set; } = new ReactiveList<TransactionViewModel>();
        public readonly List<TransactionCache> TransactionCache = new List<TransactionCache>();

        //public ReactiveList<MenuItemViewModel> MenuItems { get; set; } = new ReactiveList<MenuItemViewModel>();

        private List<string> WalletAddress { get; } = new List<string>();

        //public ReactiveCommand<string, Unit> FilterAddressCommand { get; }

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(App.SyncTimeSpan);
        private readonly IDisposable _timer;

        private readonly object _receiptsCheckLock = new object();

        private string _url;
        public string Url
        {
            get => _url;
            set => this.RaiseAndSetIfChanged(ref _url, value);
        }

        public TransactionUserControlViewModel(List<string> address, Data.Transaction[] transactions)
        {
            Transactions.ChangeTrackingEnabled = true;

            //初始化筛选地址
            //FilterAddressCommand = ReactiveCommand.Create<string>(FilterAddress);
            //address.Insert(0, AppResources.FilterAll);
            WalletAddress = address;
            //MenuItems.Add(new MenuItemViewModel()
            //{
            //    Header = AppResources.FilterAll,
            //    Items = WalletAddress.Select(p => new MenuItemViewModel()
            //    {
            //        Command = FilterAddressCommand,
            //        CommandParameter = p,
            //        Header = p
            //    }).ToList()
            //});

            //初始化交易
            InitTransaction(transactions);

            //初始化事件监听
            InitListen();

            //初始化定时器
            _timer = Observable.Timer(_updateInterval, _updateInterval, RxApp.TaskpoolScheduler)
                .Subscribe(async _ => await CheckReceiptsAsync());
        }

        public void InitTransaction(Data.Transaction[] trx)
        {
            foreach (Data.Transaction item in trx)
            {
                if (WalletAddress.Where(p => p.Equals(item.From, StringComparison.OrdinalIgnoreCase)).Any()
                    || WalletAddress.Where(p => p.Equals(item.To, StringComparison.OrdinalIgnoreCase)).Any())
                {
                    string status = GetStatusByCode(item.Status);

                    Transactions.Add(new TransactionViewModel()
                    {
                        BlockNumber = item.BlockNumber.ToString(),
                        Value = Web3.Convert.FromWei(BigInteger.Parse(item.Value)).ToString(),
                        From = item.From,
                        To = item.Input != null && item.Input.Length > 2 ? AppResources.Contract : item.To,
                        Status = status,
                        TransactionHash = item.Hash,
                        IsShow = true,
                    });

                    TransactionCache.Add(new TransactionCache() { Hash = item.Hash, Status = status });
                }
            }
        }

        private void InitListen()
        {
            MessageBus.Current.Listen<TransactionAdded>().Subscribe(x =>
            {
                if (!Transactions.Where(p => p.TransactionHash == x.RawTransaction.TransactionHash).Any())
                {
                    var transactionViewModel = new TransactionViewModel();
                    transactionViewModel.Initialise(x.RawTransaction);
                    transactionViewModel.Status = TransactionViewModel.STATUS_INPROGRESS;
                    transactionViewModel.To = transactionViewModel.Data != null && transactionViewModel.Data.Length > 2 ? AppResources.Contract : transactionViewModel.To;

                    //if (MenuItems[0].Header.StartsWith("SPOCK-") || MenuItems[0].Header.StartsWith("spock-"))
                    //    transactionViewModel.IsShow = transactionViewModel.From.Equals(MenuItems[0].Header, StringComparison.OrdinalIgnoreCase) || transactionViewModel.To.Equals(MenuItems[0].Header, StringComparison.OrdinalIgnoreCase);
                    //else
                    //    transactionViewModel.IsShow = true;

                    lock (_receiptsCheckLock)
                    {
                        Transactions.Insert(0, transactionViewModel);
                        TransactionCache.Add(new TransactionCache() { Hash = transactionViewModel.TransactionHash, Status = transactionViewModel.Status });
                    }
                }
            });

            //MessageBus.Current.Listen<WalletCreated>().Subscribe(x =>
            //{
            //    WalletAddress.Add(x.Wallet.Address);
            //    MenuItems[0].Items.Add(new MenuItemViewModel()
            //    {
            //        Command = FilterAddressCommand,
            //        CommandParameter = x.Wallet.Address,
            //        Header = x.Wallet.Address
            //    });

            //    var tran = GetTransactions(x.Wallet.Address);

            //    lock (_receiptsCheckLock)
            //    {
            //        //从数据库中添加已有的交易
            //        foreach (var item in tran)
            //        {
            //            if (!Transactions.Where(p => p.TransactionHash == item.Hash).Any())
            //            {
            //                Transactions.Add(new TransactionViewModel()
            //                {
            //                    BlockNumber = item.BlockNumber.ToString(),
            //                    Value = Web3.Convert.FromWei(BigInteger.Parse(item.Value)).ToString(),
            //                    From = item.From,
            //                    To = item.To,
            //                    Status = TransactionViewModel.STATUS_INPROGRESS,
            //                    TransactionHash = item.Hash,
            //                    IsShow = true,
            //                });
            //            }
            //        }
            //        //重新排序
            //        Transactions = new ReactiveList<TransactionViewModel>(Transactions.OrderByDescending(p => Convert.ToInt64(p.BlockNumber)));

            //        //从数据库中添加已有的交易到缓存
            //        foreach (var item in tran)
            //        {
            //            if (!TransactionCache.Where(p => p.Hash == item.Hash).Any())
            //                TransactionCache.Add(new TransactionCache() { Hash = item.Hash, Status = TransactionViewModel.STATUS_INPROGRESS });
            //        }
            //    }
            //});

            //MessageBus.Current.Listen<WalletDeleted>().Subscribe(x =>
            //{
            //    //移除下拉列表
            //    WalletAddress.Remove(x.Address);
            //    MenuItems[0].Items?.Clear();
            //    MenuItems[0].Items = WalletAddress.Select(p => new MenuItemViewModel()
            //    {
            //        Command = FilterAddressCommand,
            //        CommandParameter = p,
            //        Header = p
            //    }).ToList();

            //    //移除指定地址的交易
            //    lock (_receiptsCheckLock)
            //    {
            //        var addTransactions = Transactions
            //        .Where(p => (!p.From.Equals(x.Address, StringComparison.OrdinalIgnoreCase) && !p.To.Equals(x.Address, StringComparison.OrdinalIgnoreCase)) || (WalletAddress.Contains(p.From) || WalletAddress.Contains(p.To)))
            //        .ToList();
            //        var transactionHashs = addTransactions.Select(p => p.TransactionHash);

            //        Transactions.Clear();
            //        Transactions.AddRange(addTransactions);

            //        TransactionCache.Clear();
            //        TransactionCache.AddRange(addTransactions.Select(p => new TransactionCache() { Hash = p.TransactionHash, Status = p.Status }));
            //    }

            //    //如果移除的正好是当前筛选的地址，列表则显示全部
            //    if (MenuItems[0].Header.Equals(x.Address, StringComparison.OrdinalIgnoreCase))
            //        FilterAddress(AppResources.FilterAll);
            //});

            MessageBus.Current.Listen<TransationReceiptsChanged>().Subscribe(x =>
            {
                var viewModelTransactions = Transactions.Where(p => p.TransactionHash == x.Hash).FirstOrDefault();
                if (viewModelTransactions != null)
                {
                    viewModelTransactions.BlockNumber = x.BlockNumber;
                    viewModelTransactions.Status = x.Status;
                }

                lock (_receiptsCheckLock)
                {
                    var transactionCache = TransactionCache.Where(p => p.Hash == x.Hash).FirstOrDefault();
                    if (transactionCache != null)
                    {
                        transactionCache.Status = x.Status;
                    }
                }
            });
        }

        public async Task CheckReceiptsAsync()
        {
            IEnumerable<TransactionCache> transactionsInProgress = new List<TransactionCache>();

            lock (_receiptsCheckLock)
            {
                transactionsInProgress =
                    TransactionCache.Where(x => x.Status == TransactionViewModel.STATUS_INPROGRESS).ToArray();
            }

            var web3 = Web3Service.GetWeb3();
            foreach (var transaction in transactionsInProgress)
            {
                try
                {
                    var receipt = await web3.Eth.Transactions.GetTransactionReceipt
                                .SendRequestAsync(transaction.Hash);

                    if (receipt != null && receipt.Status != null)
                    {
                        string status = nameof(TransactionViewModel.STATUS_INPROGRESS).ToLower();

                        TransationReceiptsChanged transationReceiptsChanged = new TransationReceiptsChanged();
                        transationReceiptsChanged.Hash = transaction.Hash;

                        if (receipt.Succeeded())
                        {
                            status = nameof(TransactionViewModel.STATUS_COMPLETED).ToLower();
                            transationReceiptsChanged.Status = TransactionViewModel.STATUS_COMPLETED;
                            transationReceiptsChanged.BlockNumber = receipt.BlockNumber.Value.ToString();
                        }
                        else
                        {
                            status = nameof(TransactionViewModel.STATUS_FAILED).ToLower();
                            transationReceiptsChanged.Status = TransactionViewModel.STATUS_FAILED;
                        }

                        using (DataContext dataContext = new DataContext())
                        {
                            var tx = dataContext.Transactions.FirstOrDefault(p => p.Hash == transaction.Hash);
                            if (tx != null)
                            {
                                tx.Status = status;
                                dataContext.Update(tx);
                                dataContext.SaveChanges();
                            }
                        }

                        RxApp.MainThreadScheduler.Schedule(transationReceiptsChanged, (schedule, tx) =>
                        {
                            MessageBus.Current.SendMessage(tx);

                            return new ScanDisposable();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Transactions - CheckReceipts");
                }
            }
        }

        private string GetStatusByReceipt(TransactionReceipt transactionReceipt)
        {
            if (transactionReceipt == null)
            {
                var hasError = transactionReceipt.HasErrors();
                if (hasError == null)
                    return TransactionViewModel.STATUS_INPROGRESS;

                if (hasError.Value)
                    return TransactionViewModel.STATUS_COMPLETED;
                else
                    return TransactionViewModel.STATUS_FAILED;
            }
            else
                return TransactionViewModel.STATUS_INPROGRESS;
        }

        private string GetStatusByCode(string status)
        {
            switch (status)
            {
                case "status_completed":
                    return TransactionViewModel.STATUS_COMPLETED;
                case "status_failed":
                    return TransactionViewModel.STATUS_FAILED;
                default:
                    return TransactionViewModel.STATUS_INPROGRESS;
            }
        }

        //public void FilterAddress(string address)
        //{
        //    MenuItems[0].Header = address;

        //    if (address.StartsWith("SPOCK-") || address.StartsWith("spock-"))
        //    {
        //        lock (_receiptsCheckLock)
        //        {
        //            foreach (var item in Transactions)
        //            {
        //                if (item.From.Equals(address, StringComparison.OrdinalIgnoreCase) || item.To.Equals(address, StringComparison.OrdinalIgnoreCase))
        //                    item.IsShow = true;
        //                else
        //                    item.IsShow = false;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        lock (_receiptsCheckLock)
        //        {
        //            foreach (var item in Transactions)
        //            {
        //                item.IsShow = true;
        //            }
        //        }
        //    }
        //}

        //public Transaction[] GetTransactions(string address)
        //{
        //    using (DataContext dataContext = new DataContext())
        //    {
        //        return dataContext.Transactions
        //        .Where(p => p.From.Equals(address, StringComparison.OrdinalIgnoreCase) || p.To.Equals(address, StringComparison.OrdinalIgnoreCase))
        //        .ToArray();
        //    }
        //}

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}