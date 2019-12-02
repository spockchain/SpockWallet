using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using ReactiveUI;
using ReactiveUI.Legacy;
using SpockWallet.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SpockWallet.Events;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Avalonia.Controls;
using System.IO;
using SpockWallet.Views;
using SpockWallet.ViewModels.BatchModels;

namespace SpockWallet.ViewModels.BatchModels
{
    public class BatchTransferUserControlViewModel : ViewModelBase
    {
        private readonly BatchTransferWindow _batchTransferWindow;
        private readonly Guid _taskId;

        private CancellationTokenSource CancellationTokenSource;

        private readonly Account _account;

        private string totalAmount;

        public string TotalAmount
        {
            get { return totalAmount; }
            set
            {
                this.RaiseAndSetIfChanged(ref totalAmount, value);
            }
        }

        private bool showStop;

        public bool ShowStop
        {
            get { return showStop; }
            set
            {
                this.RaiseAndSetIfChanged(ref showStop, value);
            }
        }

        private bool showStart;

        public bool ShowStart
        {
            get { return showStart; }
            set
            {
                this.RaiseAndSetIfChanged(ref showStart, value);
            }
        }

        private ReactiveList<BatchTransactionViewModel> transactions;

        public ReactiveList<BatchTransactionViewModel> Transactions
        {
            get { return transactions; }
            set
            {
                this.RaiseAndSetIfChanged(ref transactions, value);
            }
        }

        private ReactiveList<ProcessedTransaction> processedTransactions;

        public ReactiveList<ProcessedTransaction> ProcessedTransactions
        {
            get { return processedTransactions; }
            set
            {
                this.RaiseAndSetIfChanged(ref processedTransactions, value);
            }
        }

        public const string STATUS_PENDDING = "待发送";
        public const string STATUS_QUEUEING = "队列中";
        public const string STATUS_SENDED = "已发送";
        public const string STATUS_FAILED = "出错";
        public const string STATUS_STOPED = "已暂停";

        public BatchTransferUserControlViewModel(Account account, BatchTransferWindow batchTransferWindow)
        {
            Transactions = new ReactiveList<BatchTransactionViewModel>();
            ProcessedTransactions = new ReactiveList<ProcessedTransaction>();
            ShowStart = false;
            ShowStop = false;
            _taskId = Guid.NewGuid();
            _account = account;
            _batchTransferWindow = batchTransferWindow;

            MessageBus.Current.Listen<ProcessedTaskUpdatedEvent>().Subscribe(data =>
            {
                if (data.TaskId == _taskId && Transactions != null && Transactions.Count >= data.Index)
                {
                    Transactions[data.Index].Status = data.Status;
                    if (data.Status == STATUS_SENDED)
                        ProcessedTransactions.Add(new ProcessedTransaction() { From = data.From, To = data.To, TxHash = data.TxHash, Value = data.Value });
                }
            });

            MessageBus.Current.Listen<ProcessedTaskCompleteEvent>().Subscribe(async data =>
            {
                if (data.TaskId == _taskId)
                {
                    ShowStart = false;
                    ShowStop = false;

                    var result = new MessageBox.Avalonia.MessageBoxWindow(new MessageBoxParams
                    {
                        Button = ButtonEnum.YesNo,
                        ContentTitle = "批量转账完成",
                        ContentMessage = $"应发送{Transactions.Count}笔交易，实际发送{ProcessedTransactions.Count}笔交易，是否保存报告？",
                        Icon = Icon.Success,
                        Style = Style.Windows
                    });

                    if ("Yes" == await result.Show())
                    {
                        var saveFileDialog = new SaveFileDialog()
                        {
                            Title = "Save file",
                            Filters = new List<FileDialogFilter>() {
                                new FileDialogFilter() {
                            Extensions = new List<string>() {
                                "scv"
                            },
                            Name = "SCV"
                        }
                    }
                        };

                        var files = await saveFileDialog.ShowAsync(_batchTransferWindow);
                        if (files != null)
                        {
                            string[] txLineHeader = new string[] { "Transaction Hash,From,To,Value" };
                            var txLines = ProcessedTransactions.Select(p => $"{p.TxHash},{p.From},{p.To},{p.Value}");
                            File.WriteAllText(files, string.Join("\r\n", txLineHeader.Union(txLines)));

                            _batchTransferWindow.Close();
                        }
                    }
                }
            });
        }

        public async void AddTransactions(IEnumerable<TranModel> tranModels)
        {
            if (CancellationTokenSource != null)
            {
                await new MessageBox.Avalonia.MessageBoxWindow("批量转账正在进行中",
                    "请暂停正在进行的批量转账任务然后再试。")
                    .Show();
                return;
            }

            string from = "0x" + _account.Address.Substring(2);

            HexBigInteger txResult = await Web3Service.GetWeb3().Eth.Transactions.GetTransactionCount.SendRequestAsync(from);
            BigInteger txCount = txResult.Value;

            //加载用户显示的交易列表
            Transactions.Clear();
            Transactions.AddRange(tranModels.Select(p => new BatchTransactionViewModel()
            {
                From = from,
                To = p.Address,
                Status = STATUS_PENDDING,
                Value = p.Amount,
                TransactionInput = new TransactionInput()
                {
                    From = from,
                    To = "0x" + p.Address.Substring(6),
                    Value = new HexBigInteger(Web3.Convert.ToWei(p.Amount)),
                    Gas = new HexBigInteger(Nethereum.Signer.SignedTransactionBase.DEFAULT_GAS_LIMIT),
                    GasPrice = new HexBigInteger(Nethereum.Signer.SignedTransactionBase.DEFAULT_GAS_PRICE),
                    Nonce = new HexBigInteger(txCount++)
                }
            }));

            ShowStop = false;
            ShowStart = true;
        }

        public void Start()
        {
            ShowStart = false;
            ShowStop = true;
            CancellationTokenSource = new CancellationTokenSource();

            foreach (var item in Transactions)
            {
                if (item.Status == STATUS_PENDDING || item.Status == STATUS_STOPED)
                    item.Status = STATUS_QUEUEING;
            }

            ProcessedTaskState processedTaskState = new ProcessedTaskState()
            {
                TaskId = _taskId,
                BatchTransactions = Transactions.ToList(),
                Account = _account
            };

            Task.Factory.StartNew(async (state) =>
            {
                var taskState = (ProcessedTaskState)state;

                var web3 = Web3Service.GetWeb3(taskState.Account);

                for (int i = 0; i < taskState.BatchTransactions.Count; i++)
                {
#if DEBUG
                    await Task.Delay(5000);
#endif

                    if (taskState.BatchTransactions[i].Status != STATUS_QUEUEING)
                        continue;

                    try
                    {
                        string txHash = await web3.TransactionManager.Account.TransactionManager.SendTransactionAsync(taskState.BatchTransactions[i].TransactionInput);

                        RxApp.MainThreadScheduler.Schedule(new ProcessedTaskUpdatedEvent()
                        {
                            From = taskState.BatchTransactions[i].From,
                            Status = STATUS_SENDED,
                            Index = i,
                            TaskId = taskState.TaskId,
                            To = taskState.BatchTransactions[i].To,
                            Value = taskState.BatchTransactions[i].Value,
                            TxHash = txHash
                        }, (schedule, scheduleState) =>
                        {
                            MessageBus.Current.SendMessage(scheduleState);

                            return new ScanDisposable();
                        });
                    }
                    catch (Exception)
                    {
                        RxApp.MainThreadScheduler.Schedule(new ProcessedTaskUpdatedEvent()
                        {
                            From = taskState.BatchTransactions[i].From,
                            Status = STATUS_FAILED,
                            Index = i,
                            TaskId = taskState.TaskId,
                            To = taskState.BatchTransactions[i].To,
                            Value = taskState.BatchTransactions[i].Value,
                        }, (schedule, scheduleState) =>
                        {
                            MessageBus.Current.SendMessage(scheduleState);

                            return new ScanDisposable();
                        });
                    }
                }

                RxApp.MainThreadScheduler.Schedule(new ProcessedTaskCompleteEvent() { TaskId = _taskId }, (schedule, scheduleState) =>
                {
                    MessageBus.Current.SendMessage(scheduleState);

                    return new ScanDisposable();
                });
            }, processedTaskState, CancellationTokenSource.Token);
        }

        public void Pause()
        {
            CancellationTokenSource.Cancel();

            foreach (var item in Transactions)
            {
                if (item.Status == STATUS_QUEUEING)
                    item.Status = STATUS_STOPED;
            }

            ShowStart = true;
            ShowStop = false;
        }
    }

    public class ProcessedTransaction
    {
        public string TxHash { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public decimal Value { get; set; }
    }

    public class ProcessedTaskState
    {
        public Account Account { get; set; }

        public List<BatchTransactionViewModel> BatchTransactions { get; set; }

        public Guid TaskId { get; set; }
    }
}
