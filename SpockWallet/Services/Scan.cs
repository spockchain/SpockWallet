using Nethereum.JsonRpc.Client.Streaming;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Subscriptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using ReactiveUI;
using Nethereum.Web3;
using System.Threading;
using SpockWallet.Events;
using SpockWallet.Data;
using System.Linq;
using Serilog;
using Nethereum.Contracts;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Microsoft.EntityFrameworkCore;

namespace SpockWallet.Services
{
    public class Scan : IDisposable
    {
        private readonly Web3 _web3;

        public string Address;
        private BigInteger CurrentBlock = 0;
        private BigInteger SyncLatestBlock = 0;

        private CancellationTokenSource _cancellationTokenSource;

        public Scan()
        {
            _web3 = Web3Service.GetWeb3();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
        }

        public async void StartAsync(Wallet wallet, CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;

            CurrentBlock = wallet.ScanLocation;
            Address = wallet.Address;

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Wallet:{Address} 已开始扫描");
#endif

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                    if (blockNumber.Value != SyncLatestBlock)
                    {
                        //检查与上次同步的区块是否差距太大
                        if ((blockNumber.Value - SyncLatestBlock) >= 6)
                        {
                            RxApp.MainThreadScheduler.Schedule(new SyncLostBlock() { IsLost = true }, (schedule, synced) =>
                            {
                                MessageBus.Current.SendMessage(synced);

                                return new ScanDisposable();
                            });
                        }
                        else
                        {
                            RxApp.MainThreadScheduler.Schedule(new SyncLostBlock() { IsLost = false }, (schedule, synced) =>
                            {
                                MessageBus.Current.SendMessage(synced);

                                return new ScanDisposable();
                            });
                        }

                        SyncLatestBlock = blockNumber.Value;

                        await SyncBlock(blockNumber.Value);
                    }

                    ScanLocationUpdate.Queue.Enqueue(new WalletScanLocationUpdated()
                    {
                        Address = Address,
                        ScanLocation = Convert.ToInt64(CurrentBlock.ToString())
                    });

                    BlockSynced blockSynced = new BlockSynced();
                    blockSynced.Address = Address;
                    blockSynced.BlockNumber = Convert.ToInt64(CurrentBlock.ToString());
                    blockSynced.IsSuccess = true;

                    RxApp.MainThreadScheduler.Schedule(blockSynced, (schedule, synced) =>
                    {
                        MessageBus.Current.SendMessage(synced);

                        return new ScanDisposable();
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Scan - GetBlockNumber");

                    BlockSynced blockSynced = new BlockSynced();
                    blockSynced.Address = Address;
                    blockSynced.IsSuccess = false;

                    RxApp.MainThreadScheduler.Schedule(blockSynced, (schedule, synced) =>
                    {
                        MessageBus.Current.SendMessage(synced);

                        return new ScanDisposable();
                    });
                }

                if (!_cancellationTokenSource.IsCancellationRequested)
                    await Task.Delay(App.SyncTimeSpan);
            }

            SyncStoped syncStoped = new SyncStoped();
            syncStoped.Address = Address;
            syncStoped.SyncBlocked = Convert.ToInt64(CurrentBlock.ToString());

            RxApp.MainThreadScheduler.Schedule(syncStoped, (schedule, state) =>
            {
                MessageBus.Current.SendMessage(state);

                return new ScanDisposable();
            });

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Wallet:{Address} 已停止扫描（Block:{CurrentBlock}）");
#endif
        }

        private async Task SyncBlock(BigInteger trackingBlock)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"trackingBlock:{trackingBlock},currentBlock:{CurrentBlock},Wallet:{Address}");
#endif

            for (BigInteger i = CurrentBlock + 1; i <= trackingBlock; i++)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    break;

                BlockWithTransactions transactions;

                try
                {
                    transactions = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber
                                .SendRequestAsync(new BlockParameter(new Nethereum.Hex.HexTypes.HexBigInteger(i)));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Scan - SyncBlock");
                    i--;
                    continue;
                }

                if (transactions != null && transactions.Transactions != null)
                {
                    if (transactions.Transactions.Length > 0)
                    {
                        foreach (var item in transactions.Transactions)
                        {
                            TransactionReceipt receipt = await _web3.Eth.Transactions.GetTransactionReceipt
                                .SendRequestAsync(item.TransactionHash);

                            if (!string.IsNullOrEmpty(item.Input) && item.Input.Length > 2)
                                SyncContact(item, receipt);
                            else
                                SyncTransaction(item, receipt);
                        }
                    }
                }
                else
                {
                    i--;
                }

                CurrentBlock = i;
            }
        }

        private async void SyncContact(Nethereum.RPC.Eth.DTOs.Transaction transaction, TransactionReceipt transactionReceipt)
        {
            string tokenTranTo = null;

            if (transaction.IsForContractCreation(transactionReceipt))
            {
                //看看合约是不是当前用户创建的
                if (!transaction.From.Equals(Address, StringComparison.CurrentCultureIgnoreCase))
                    return;

                bool? hasError = transactionReceipt.HasErrors();
                if (hasError == null || !hasError.Value)
                {
                    using (DataContext dataContext = new DataContext())
                        await AddContractAsync(dataContext, transaction, transactionReceipt);
                }
            }
            else
            {
                string methodCode = transaction.Input.Substring(0, 10);

                TokenTransfer tokenTransfer = new TokenTransfer();
                tokenTransfer.BlockNumber = Convert.ToInt64(transaction.BlockNumber.Value.ToString());
                tokenTransfer.ContractAddress = transaction.To;
                tokenTransfer.CreateTime = DateTime.UtcNow;
                tokenTransfer.Hash = transaction.TransactionHash;

                BigInteger trabsferValue;

                if (methodCode == "0xa9059cbb")
                {
                    tokenTransfer.Method = "transfer";
                    tokenTransfer.From = transaction.From;
                    tokenTransfer.To = "SPOCK-" + transaction.Input.Substring(34, 40);
                    trabsferValue = new HexBigInteger(transaction.Input.Substring(74)).Value;
                }
                else if (methodCode == "0xa978501e")
                {
                    tokenTransfer.Method = "transferFrom";
                    tokenTransfer.From = "SPOCK-" + transaction.Input.Substring(34, 40);
                    tokenTransfer.To = "SPOCK-" + transaction.Input.Substring(74, 40);
                    trabsferValue = new HexBigInteger(transaction.Input.Substring(114)).Value;
                }
                else return;

                if (string.Equals(tokenTransfer.From, Address, StringComparison.CurrentCultureIgnoreCase) ||
                                string.Equals(tokenTransfer.To, Address, StringComparison.CurrentCultureIgnoreCase))
                {
                    tokenTranTo = tokenTransfer.To;

                    using (DataContext dataContext = new DataContext())
                    {
                        Data.Contract contract = null;

                        //检查合约存不存在
                        if (dataContext.Contracts.Any(p => p.Address == transaction.To))
                            contract = dataContext.Contracts.FirstOrDefault(p => p.Address == transaction.To);
                        else
                            contract = await AddContractAsync(dataContext, transaction, transactionReceipt);

                        if (contract != null)
                        {
                            tokenTransfer.Symbol = contract.Symbol;
                            tokenTransfer.Value = Web3.Convert.FromWei(trabsferValue, contract.Decimals);

                            try
                            {
                                dataContext.TokenTransfers.Add(tokenTransfer);
                                dataContext.SaveChanges();

                                RxApp.MainThreadScheduler.Schedule(tokenTransfer, (schedule, tx) =>
                                {
                                    MessageBus.Current.SendMessage(new TokenTransferAdded(tx));

                                    return new ScanDisposable();
                                });
                            }
                            catch (DbUpdateException ex)
                            {
                                Log.Error(ex, "Sync Token Transfer");
                            }
                        }
                    }
                }
            }

            SyncTransaction(transaction, transactionReceipt, tokenTranTo);
        }

        private void SyncTransaction(Nethereum.RPC.Eth.DTOs.Transaction transaction,
            TransactionReceipt transactionReceipt,
            string tokenTranTo = null)
        {
            if (string.Equals(transaction.From, Address, StringComparison.CurrentCultureIgnoreCase) ||
                string.Equals(transaction.To, Address, StringComparison.CurrentCultureIgnoreCase) ||
                (tokenTranTo != null && string.Equals(tokenTranTo, Address, StringComparison.CurrentCultureIgnoreCase)))
            {
                using (DataContext dataContext = new DataContext())
                {
                    if (!dataContext.Transactions.Where(p => p.Hash == transaction.TransactionHash).Any())
                    {
                        dataContext.Transactions.Add(new Data.Transaction()
                        {
                            BlockHash = transaction.BlockHash,
                            Hash = transaction.TransactionHash,
                            BlockNumber = Convert.ToInt64(transaction.BlockNumber.Value.ToString()),
                            CreateTime = DateTime.UtcNow,
                            Value = transaction.Value.Value.ToString(),
                            From = transaction.From,
                            Gas = transaction.Gas.Value.ToString(),
                            GasPrice = transaction.GasPrice.ToString(),
                            Input = transaction.Input,
                            Status = transactionReceipt?.Status.Value.ToString(),
                            To = transaction.To
                        });

                        dataContext.SaveChanges();

                        RxApp.MainThreadScheduler.Schedule(transaction, (schedule, tx) =>
                        {
                            MessageBus.Current.SendMessage(new TransactionAdded(tx));

                            return new ScanDisposable();
                        });
                    }
                }
            }
        }

        private async Task<Data.Contract> AddContractAsync(DataContext dataContext, Nethereum.RPC.Eth.DTOs.Transaction transaction, TransactionReceipt transactionReceipt)
        {
            try
            {
                string fixAddress = transactionReceipt.ContractAddress == null
                    ? transaction.To.EnsureHexAddress() : transactionReceipt.ContractAddress.EnsureHexAddress();

                Data.Contract contract = new Data.Contract();
                contract.Address = "SPOCK-" + fixAddress.Substring(2);
                contract.CreationTransaction = transaction.TransactionHash;
                contract.BlockNumber = Convert.ToInt64(transaction.BlockNumber.Value.ToString());
                contract.Owner = Address;
                contract.CreateTime = DateTime.UtcNow;

                var nameHandler = _web3.Eth.GetContractQueryHandler<NameFunction>();
                var name = await nameHandler.QueryAsync<string>(fixAddress, new NameFunction());
                contract.TokenName = name;

                var symbolHandler = _web3.Eth.GetContractQueryHandler<SymbolFunction>();
                var symbol = await symbolHandler.QueryAsync<string>(fixAddress, new SymbolFunction());
                contract.Symbol = symbol;

                var decimalsHandler = _web3.Eth.GetContractQueryHandler<DecimalsFunction>();
                var decimals = await decimalsHandler.QueryAsync<BigInteger>(fixAddress, new DecimalsFunction());
                contract.Decimals = Convert.ToInt32(decimals.ToString());

                var totalSupplyHandler = _web3.Eth.GetContractQueryHandler<TotalSupplyFunction>();
                var totalSupply = await totalSupplyHandler.QueryAsync<BigInteger>(fixAddress, new TotalSupplyFunction());
                contract.TotalSupply = Web3.Convert.FromWei(totalSupply, contract.Decimals).ToString();

                if (!dataContext.Contracts.Any(p => p.Address == transactionReceipt.ContractAddress))
                {
                    dataContext.Contracts.Add(contract);
                    dataContext.SaveChanges();

                    RxApp.MainThreadScheduler.Schedule(contract, (schedule, contractMsg) =>
                    {
                        MessageBus.Current.SendMessage(new ContractCreated(contractMsg));

                        return new ScanDisposable();
                    });
                }

                return contract;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Scan - SyncContact:{ContactAddress}", transactionReceipt.ContractAddress);
            }

            return null;
        }
    }

    public class ScanDisposable : IDisposable
    {
        public void Dispose()
        {
            //Nothing to do.
        }
    }
}
