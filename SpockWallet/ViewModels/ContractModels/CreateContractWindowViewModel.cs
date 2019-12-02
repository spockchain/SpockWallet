using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Legacy;
using SolcNet;
using SolcNet.DataDescription.Input;
using SolcNet.Legacy;
using SolcNet.NativeLib;
//using SolcNet;
//using SolcNet.DataDescription.Input;
using SpockWallet.Data;
using SpockWallet.Events;
using SpockWallet.Localizations;
using SpockWallet.Services;
using SpockWallet.ViewModels.TransactionModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace SpockWallet.ViewModels.ContractModels
{
    public class CreateContractWindowViewModel : SendTransactionBaseViewModel
    {
        public delegate void WindowsClose();

        public event WindowsClose OnWindowsClose;

        private readonly TextEditor _sourceCodeEditor;
        private readonly TextEditor _abiEditor;
        private readonly StackPanel _constructorParamsBorder;

        private SolcLibDefaultProvider solcLibDefaultProvider;
        private string abiJsonString;
        private string[] constructorTypes;

        private bool hasError;

        public bool HasError
        {
            get { return hasError; }
            set
            {
                this.RaiseAndSetIfChanged(ref hasError, value);
            }
        }

        private string errorMsg;

        public string ErrorMsg
        {
            get { return errorMsg; }
            set
            {
                this.RaiseAndSetIfChanged(ref errorMsg, value);
            }
        }

        private string contractName;

        public string ContractName
        {
            get { return contractName; }
            set
            {
                this.RaiseAndSetIfChanged(ref contractName, value);
            }
        }

        private string bytecode;

        public string Bytecode
        {
            get { return bytecode; }
            set
            {
                this.RaiseAndSetIfChanged(ref bytecode, value);
            }
        }

        private string from;

        public string From
        {
            get { return from; }
            set
            {
                this.RaiseAndSetIfChanged(ref from, value);
            }
        }

        private string currentCompiledVersion;

        public string CurrentCompiledVersion
        {
            get { return currentCompiledVersion; }
            set
            {
                this.RaiseAndSetIfChanged(ref currentCompiledVersion, value);
            }
        }

        private bool showConstructorParams;

        public bool ShowConstructorParams
        {
            get { return showConstructorParams; }
            set
            {
                this.RaiseAndSetIfChanged(ref showConstructorParams, value);
            }
        }

        public ReactiveCommand<Wallet, Unit> FilterAddressCommand { get; }

        public ReactiveList<MenuItemViewModel> MenuItems { get; set; } = new ReactiveList<MenuItemViewModel>();

        public CreateContractWindowViewModel(TextEditor sourceCodeEditor, StackPanel stackPanel, TextEditor abiEditor)
        {
            Gas = (ulong)Nethereum.Signer.SignedTransactionBase.DEFAULT_GAS_LIMIT * 100;
            GasPrice = (ulong)Nethereum.Signer.SignedTransactionBase.DEFAULT_GAS_PRICE;
            AmountInEther = 0;
            ContractName = "TokenERC20";
            solcLibDefaultProvider = new SolcLibDefaultProvider(LibPath.GetLibPath(SolcVersion.v0_4_25));
            CurrentCompiledVersion = $"v0.4.25 {AppResources.Default}";

            ShowConstructorParams = false;
            _sourceCodeEditor = sourceCodeEditor;
            _constructorParamsBorder = stackPanel;
            _abiEditor = abiEditor;

            FilterAddressCommand = ReactiveCommand.Create<Wallet>(SelectWallet);

            using (DataContext dataContext = new DataContext())
            {
                var wallets = dataContext.Wallets.Where(p => !p.IsDelete && p.Balance != "0").ToArray();
                if (wallets != null && wallets.Length > 0)
                {
                    MenuItems.Add(new MenuItemViewModel()
                    {
                        Header = AppResources.WalletSelect,
                        Items = wallets.Select(p => new MenuItemViewModel()
                        {
                            Command = FilterAddressCommand,
                            CommandParameter = p,
                            Header = $"{p.Address} ({Web3.Convert.FromWei(BigInteger.Parse(p.Balance))} SPOK)"
                        }).ToList()
                    });
                }
                else
                {
                    MenuItems.Add(new MenuItemViewModel()
                    {
                        Header = AppResources.NoWalletAvailable
                    });
                }
            }

            var canExecuteTransaction = this.WhenAnyValue(
                x => x.AmountInEther,
                x => x.Account,
                x => x.Data,
                (amountInEther, account, data) =>
                        amountInEther != null &&
                        account != null &&
                        !string.IsNullOrEmpty(data));

            _executeTrasnactionCommand = ReactiveCommand.CreateFromTask(ExecuteAsync, canExecuteTransaction);
        }

        public async void CopyErrorMsg()
        {
            await Application.Current.Clipboard.SetTextAsync(ErrorMsg);
        }

        public async void SelectWallet(Wallet wallet)
        {
            //检查余额
            var balance = Web3.Convert.FromWei(BigInteger.Parse(wallet.Balance));
            if (balance < 100000)
            {
                await new MessageBox.Avalonia.MessageBoxWindow(AppResources.TxStatusFailed,
                    AppResources.DeployLimitTip).Show();

                return;
            }

            From = wallet.Address;
            Account = new Account(wallet.PrivateKey);
        }

        public void LoadSourceCode(string code)
        {
            _sourceCodeEditor.Text = code;
        }

        public async void Compile()
        {
            if (string.IsNullOrWhiteSpace(_sourceCodeEditor.Text) ||
                string.IsNullOrEmpty(ContractName))
            {
                await new MessageBox.Avalonia.MessageBoxWindow(AppResources.CompileFailed,
                    AppResources.CompileFailedMessage).Show();

                return;
            }

            const string compileFileName = "SimpleContract.sol";

            File.WriteAllText(compileFileName, _sourceCodeEditor.Text);

            //var libPath = Legacy.LibPath.GetLibPath(solcVersion);
            //var libProvider = new SolcLibDefaultProvider(libPath);
            var solcLib = new SolcLib(solcLibDefaultProvider);
            try
            {
                var result = solcLib.Compile(new[] { compileFileName },
                        new[] {
                    OutputType.Abi,
                    OutputType.Ast,
                    OutputType.EvmMethodIdentifiers,
                    OutputType.EvmAssembly,
                    OutputType.EvmBytecode,
                    OutputType.IR,
                    OutputType.DevDoc,
                    OutputType.UserDoc,
                    OutputType.Metadata
                        });

                HasError = result.Errors != null && result.Errors.Length > 0;
                if (HasError)
                {
                    ShowConstructorParams = false;
                    ErrorMsg = string.Empty;

                    foreach (var item in result.Errors)
                        ErrorMsg += $"{item.Message}\r\n";

                    return;
                }

                if (result.Contracts.ContainsKey(compileFileName) &&
                    result.Contracts[compileFileName].ContainsKey(ContractName))
                {
                    var contracts = result.Contracts[compileFileName][ContractName];

                    Bytecode = contracts.Evm.Bytecode.Object;
                    ShowConstructorParams = CheckCodeConstructor(JsonConvert.DeserializeObject<AbiApi[]>(contracts.AbiJsonString));
                    Data = Bytecode;
                    abiJsonString = contracts.AbiJsonString;

                    _abiEditor.Text = ConvertJsonString(abiJsonString);
                }
                else
                {
                    await new MessageBox.Avalonia.MessageBoxWindow(AppResources.ContractNameNotFound,
                            AppResources.ContractNameNotFoundMessage).Show();
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMsg = ex.Message;
                ShowConstructorParams = false;
            }
        }

        public async Task<string> ExecuteAsync()
        {
            var result = new MessageBoxWindow(new MessageBoxParams
            {
                Button = ButtonEnum.YesNo,
                ContentTitle = AppResources.Warning,
                ContentMessage = AppResources.DeployTokenWarning,
                Icon = Icon.Warning,
                Style = Style.Windows
            });

            if ("Yes" != await result.Show())
                return null;

            var web3 = Web3Service.GetWeb3(Account);

            object[] codeParams = null;

            //检查构造函数是否都已经填了
            if (ShowConstructorParams && constructorTypes != null && _constructorParamsBorder.Children.Count > 0)
            {
                codeParams = new object[_constructorParamsBorder.Children.Count];
                for (int i = 0; i < _constructorParamsBorder.Children.Count; i++)
                {
                    if (constructorTypes[i] == "string")
                        codeParams[i] = ((TextBox)_constructorParamsBorder.Children[i]).Text;
                    else
                    {
                        string paramValueString = ((TextBox)_constructorParamsBorder.Children[i]).Text;
                        if (paramValueString.Contains('.'))
                            codeParams[i] = BigDecimal.Parse(paramValueString);
                        else
                            codeParams[i] = BigInteger.Parse(paramValueString);
                    }
                }
            }

            try
            {
                var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abiJsonString,
                    Data,
                    web3.TransactionManager.Account.Address.EnsureHexAddress(),
                    new HexBigInteger(Gas.Value),
                    new HexBigInteger(GasPrice.Value),
                    new HexBigInteger(Web3.Convert.ToWei(AmountInEther.Value)),
                    codeParams);

                var transaction =
                    await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionHash);

                MessageBus.Current.SendMessage(new TransactionAdded(transaction));

                OnWindowsClose?.Invoke();
                return transactionHash;
            }
            catch (Exception ex)
            {
                await new MessageBox.Avalonia.MessageBoxWindow(AppResources.SendFailed,
                    ex.Message).Show();

                return null;
            }
        }

        public void SelectCompiledVersion(string version)
        {
            var libPath = LibPath.GetLibPath(Enum.Parse<SolcVersion>(version.Replace('.','_')));
            solcLibDefaultProvider = new SolcLibDefaultProvider(libPath);
            CurrentCompiledVersion = version;
        }

        public async void CopyBytecode()
        {
            await Application.Current.Clipboard.SetTextAsync(Bytecode);
        }

        /// <summary>
        /// 检查代码的构造函数，然后设置参数
        /// </summary>
        /// <param name="abiApis"></param>
        private bool CheckCodeConstructor(AbiApi[] abiApis)
        {
            var api = abiApis.FirstOrDefault(p => p.type == "constructor");
            if (api != null && api.inputs != null && api.inputs.Length > 0)
            {
                _constructorParamsBorder.Children.Clear();

                constructorTypes = new string[api.inputs.Length];

                //动态生成构造函数的输入控件
                for (int i = 0; i < api.inputs.Length; i++)
                {
                    constructorTypes[i] = api.inputs[i].type;

                    TextBox param = new TextBox();
                    param.Watermark = api.inputs[i].name;
                    param.Margin = new Thickness(0, 5, 0, 0);
                    _constructorParamsBorder.Children.Add(param);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
    }

    public class AbiApi
    {
        public string type { get; set; }
        public object name { get; set; }
        public Input[] inputs { get; set; }
        public object outputs { get; set; }
        public bool? payable { get; set; }
        public string stateMutability { get; set; }
        public object constant { get; set; }
        public object anonymous { get; set; }
    }

    public class Input
    {
        public string name { get; set; }
        public string type { get; set; }
        public object components { get; set; }
        public object indexed { get; set; }
    }
}