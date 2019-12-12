using MessageBox.Avalonia;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3.Accounts;
using ReactiveUI;
using Serilog;
using SpockWallet.Events;
using SpockWallet.Localizations;
using SpockWallet.Services;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;

namespace SpockWallet.ViewModels.PoolModels
{
    public class PlotIdManagerWindowsViewModel : ViewModelBase
    {
        public const string PlotIdManagerABI = "[{\"type\":\"function\",\"name\":\"getBeneficiary\",\"inputs\":[{\"name\":\"plotID\",\"type\":\"uint64\",\"components\":null,\"indexed\":null}],\"outputs\":[{\"name\":\"\",\"type\":\"address\",\"components\":null}],\"payable\":false,\"stateMutability\":\"view\",\"constant\":true,\"anonymous\":null},{\"type\":\"function\",\"name\":\"setOwner\",\"inputs\":[{\"name\":\"plotID\",\"type\":\"uint64\",\"components\":null,\"indexed\":null},{\"name\":\"newOwner\",\"type\":\"address\",\"components\":null,\"indexed\":null}],\"outputs\":[],\"payable\":true,\"stateMutability\":\"payable\",\"constant\":false,\"anonymous\":null},{\"type\":\"function\",\"name\":\"setBeneficiary\",\"inputs\":[{\"name\":\"plotID\",\"type\":\"uint64\",\"components\":null,\"indexed\":null},{\"name\":\"beneficiary\",\"type\":\"address\",\"components\":null,\"indexed\":null}],\"outputs\":[],\"payable\":true,\"stateMutability\":\"payable\",\"constant\":false,\"anonymous\":null},{\"type\":\"function\",\"name\":\"getOwner\",\"inputs\":[{\"name\":\"plotID\",\"type\":\"uint64\",\"components\":null,\"indexed\":null}],\"outputs\":[{\"name\":\"\",\"type\":\"address\",\"components\":null}],\"payable\":false,\"stateMutability\":\"view\",\"constant\":true,\"anonymous\":null},{\"type\":\"constructor\",\"name\":null,\"inputs\":[],\"outputs\":null,\"payable\":false,\"stateMutability\":\"nonpayable\",\"constant\":null,\"anonymous\":null}]";
        public const string PlotIdManagerAddress = "0x1112b0cb6fe165dfb6726c5396b94ca35718a7f1";
        //public const string PlotIdManagerAddress = "0x46673fDb142B2B3Cdd24Ac7739c6143c2B30C78E";

        private readonly Account _account;

        private string beneficiary;

        public string Beneficiary
        {
            get { return beneficiary; }
            set
            {
                this.RaiseAndSetIfChanged(ref beneficiary, value);
            }
        }

        private string owner;

        public string Owner
        {
            get { return owner; }
            set
            {
                this.RaiseAndSetIfChanged(ref owner, value);
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

        private string plotId;

        public string PlotId
        {
            get { return plotId; }
            set
            {
                this.RaiseAndSetIfChanged(ref plotId, value);
            }
        }

        private bool canBindClick;

        public bool CanBindClick
        {
            get { return canBindClick; }
            set
            {
                this.RaiseAndSetIfChanged(ref canBindClick, value);
            }
        }

        private bool isBindOwner;

        public bool IsBindOwner
        {
            get { return isBindOwner; }
            set
            {
                this.RaiseAndSetIfChanged(ref isBindOwner, value);
            }
        }

        public ReactiveCommand<Unit, string> _executeBeneficiaryBindCommand { get; }
        public ReactiveCommand<Unit, string> ExecuteBeneficiaryBindCommand => _executeBeneficiaryBindCommand;

        public ReactiveCommand<Unit, string> _executeOwnerBindCommand { get; }
        public ReactiveCommand<Unit, string> ExecuteOwnerBindCommand => _executeOwnerBindCommand;

        public PlotIdManagerWindowsViewModel(Account account)
        {
            this._account = account;
            this.Address = this._account.Address.ConvertSpockAddress();
            this.PlotId = this._account.Address.ConvertPlotId();

            var canExecuteOwnerBind = this.WhenAnyValue(
                x => x.Owner,
                (onwer) => Web3Service.IsValidAddress(onwer));
            _executeOwnerBindCommand = ReactiveCommand.CreateFromTask(ExecuteOwnerBindAsync, canExecuteOwnerBind);

            var _executeBeneficiaryBind = this.WhenAnyValue(
                x => x.Beneficiary,
                (beneficiary) => Web3Service.IsValidAddress(beneficiary));
            _executeBeneficiaryBindCommand = ReactiveCommand.CreateFromTask(ExecuteBeneficiaryBindAsync, _executeBeneficiaryBind);

            try
            {
                var web3 = Web3Service.GetWeb3();

                var ownerHandler = web3.Eth.GetContractQueryHandler<GetOwnerFunction>();
                var owner = ownerHandler.QueryAsync<string>(
                    PlotIdManagerAddress, new GetOwnerFunction() { PlotId = BigInteger.Parse(this.PlotId) }).GetAwaiter().GetResult();

                isBindOwner = owner != "0x0000000000000000000000000000000000000000";

                if (isBindOwner)
                    this.Owner = owner.ConvertSpockAddress();
                else
                    this.Owner = this.Address;

                var beneficiaryHandler = web3.Eth.GetContractQueryHandler<GetBeneficiaryFunction>();
                var beneficiary = beneficiaryHandler.QueryAsync<string>(
                    PlotIdManagerAddress, new GetBeneficiaryFunction() { PlotId = BigInteger.Parse(this.PlotId) }).GetAwaiter().GetResult();
                if (beneficiary != "0x0000000000000000000000000000000000000000")
                    this.Beneficiary = beneficiary.ConvertSpockAddress();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取PlotId{PlotId}受益人或者所有者失败", this.PlotId);
            }
        }

        public async Task<string> ExecuteOwnerBindAsync()
        {
            var transferFunction = new SetOwnerFunction()
            {
                NewOwner = this.Owner.EnsureHexAddress(),
                PlotId = BigInteger.Parse(this.PlotId),
                FromAddress = this._account.Address
            };

            return await ContractFunctionCallAsync(transferFunction);

            //return await ContractFunctionCallAsync(
            //    "setOwner",
            //    BigInteger.Parse(this.PlotId),
            //    this.Owner.EnsureHexAddress());
        }

        public async Task<string> ExecuteBeneficiaryBindAsync()
        {
            var transferFunction = new SetBeneficiaryFunction()
            {
                Beneficiary = this.Beneficiary.EnsureHexAddress(),
                PlotId = BigInteger.Parse(this.PlotId),
                FromAddress = this._account.Address,
            };

            return await ContractFunctionCallAsync(transferFunction);

            //return await ContractFunctionCallAsync(
            //    "setBeneficiary",
            //    BigInteger.Parse(this.PlotId),
            //    this.Beneficiary.EnsureHexAddress());
        }

        private async Task<string> ContractFunctionCallAsync<T>(T transferFunction) where T : FunctionMessage, new()
        {
            var web3 = Web3Service.GetWeb3(this._account);

            try
            {
                var input = transferFunction.CreateTransactionInput(PlotIdManagerAddress);
                input.Gas = new HexBigInteger(BigInteger.Parse("3000000"));
                input.GasPrice = new HexBigInteger(Nethereum.Signer.SignedTransactionBase.DEFAULT_GAS_PRICE);
                input.Value = new HexBigInteger(Web3.Convert.ToWei(100));

                var contractHandler = web3.Eth.GetContractHandler(PlotIdManagerAddress);

                //var gas = await contractHandler.EstimateGasAsync(transferFunction);
                //input.Gas = gas;

                var txHash = await web3.Eth.TransactionManager
                    .SendTransactionAsync(input);

                //string txHash = tx.TransactionHash;

                var transaction =
                    await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);

                MessageBus.Current.SendMessage(new TransactionAdded(transaction));

                await new MessageBoxWindow(AppResources.ContractCallSuccess,
                    AppResources.ContractCallSuccessText).Show();

                if (transferFunction is SetOwnerFunction)
                    this.IsBindOwner = true;

                return txHash;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                await new MessageBoxWindow(AppResources.ContractCallFailed,
                    ex.Message).Show();
            }

            return null;
        }

        //使用无类型方式调用合约
        //private async Task<string> ContractFunctionCallAsync(string functionName, params object[] paramArgs)
        //{
        //    var web3 = Web3Service.GetWeb3(this.Account);

        //    try
        //    {
        //        var contract = web3.Eth.GetContract(PlotIdManagerABI, PlotIdManagerAddress);

        //        var function = contract.GetFunction(functionName);

        //        var hash = await function.SendTransactionAsync(
        //            this.Account.Address,
        //            new HexBigInteger(BigInteger.Parse("3000000")),
        //            new HexBigInteger(Nethereum.Signer.SignedTransactionBase.DEFAULT_GAS_PRICE),
        //            new HexBigInteger(new BigInteger(0)),
        //            paramArgs);

        //        var transaction =
        //             await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(hash);

        //        MessageBus.Current.SendMessage(new TransactionAdded(transaction));

        //        await new MessageBoxWindow(AppResources.ContractCallSuccess,
        //            AppResources.ContractCallSuccessText).Show();

        //        if (functionName == "setOwner")
        //            this.IsBindOwner = true;

        //        return hash;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex.Message);
        //        await new MessageBoxWindow(AppResources.ContractCallFailed,
        //            ex.Message).Show();
        //    }

        //    return null;
        //}
    }

    [Function("getOwner", "address")]
    public class GetOwnerFunction : FunctionMessage
    {
        [Parameter("uint64", "plotID", 1)]
        public BigInteger PlotId { get; set; }
    }

    [Function("getBeneficiary", "address")]
    public class GetBeneficiaryFunction : FunctionMessage
    {
        [Parameter("uint64", "plotID", 1)]
        public BigInteger PlotId { get; set; }
    }

    [Function("setOwner")]
    public class SetOwnerFunction : FunctionMessage
    {
        [Parameter("uint64", "plotID", 1)]
        public BigInteger PlotId { get; set; }

        [Parameter("address", "newOwner", 2)]
        public string NewOwner { get; set; }
    }

    [Function("setBeneficiary")]
    public class SetBeneficiaryFunction : FunctionMessage
    {
        [Parameter("uint64", "plotID", 1)]
        public BigInteger PlotId { get; set; }

        [Parameter("address", "beneficiary", 2)]
        public string Beneficiary { get; set; }
    }
}
