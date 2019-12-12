using Avalonia;
using Avalonia.Logging.Serilog;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using ConsoleTables;
using Nethereum.Web3;
using Serilog;
using SpockWallet.Services;
using SpockWallet.ViewModels.TransactionModels;
using SpockWallet.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;

namespace SpockWallet
{
    class Program
    {
        public static Process NodeProcess;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\spockwallet.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Started - Args: {0}", string.Join(' ', args));

            //以错误模式启动
            if (args != null && args.Length >= 2 && args.Contains("--error"))
            {
                BuildAvaloniaApp().Start(AppError, args);

                Log.CloseAndFlush();

                return;
            }

            //以工具模式启动
            if (args != null && args.Length >= 1 && args.Contains("--tools"))
            {
                ToolsMode();

                Log.CloseAndFlush();

                return;
            }

            bool startNode = true;

            //检测是否不需要启动节点
            if (args != null && args.Length >= 1 && args.Contains("--nonode"))
                startNode = false;

            //正常模式启动
            try
            {
                if (startNode)
                {
                    bool hasNode = CheckHasNode().GetAwaiter().GetResult();
                    if (!hasNode)
                        TryRunNode();
                }

                //注册Sol的代码高亮
                if (File.Exists("Solidity-Mode.xshd"))
                    HighlightingManager.Instance.RegisterHighlighting("Solidity", new[] { ".sol" }, RegisterSolidityHighlighting());

                BuildAvaloniaApp().Start(AppMain, args);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "SpockWallet Exit!");

                StartError(ex.ToString());
            }

            Log.CloseAndFlush();

            NodeProcess?.StandardInput.WriteLine("exit");
            NodeProcess?.WaitForExit();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            app.Run(new MainWindow());
        }

        private static void AppError(Application app, string[] args)
        {
            app.Run(new ErrorWindow(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[1]))));
        }

        private static void StartError(string errorMsg)
        {
            //设置启动参数
            string execArg = string.Join(" ", new string[] { "--error", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(errorMsg)) });

            string pathAndCommand = "\"" + System.IO.Directory.GetCurrentDirectory() + "\" \"SpockWallet.exe\" " + execArg;

            Log.Information(pathAndCommand);

            //如果不是管理员，则启动UAC
            TransactionViewModel.OpenBrowser(pathAndCommand);
        }

        private static async Task<bool> CheckHasNode()
        {
            Web3 web3 = new Web3("http://127.0.0.1:9666");

            try
            {
                var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                if (blockNumber != null)
                    return true;
            }
            catch
            {
                Log.Information("Check Node Result: Not found!");
            }

            return false;
        }

        private static Func<IHighlightingDefinition> RegisterSolidityHighlighting()
        {
            IHighlightingDefinition Func()
            {
                XshdSyntaxDefinition xshd;
                using (var s = File.Open("Solidity-Mode.xshd", FileMode.Open))
                using (var reader = XmlReader.Create(s))
                {
                    // in release builds, skip validating the built-in highlightings
                    xshd = HighlightingLoader.LoadXshd(reader);
                }
                return HighlightingLoader.Load(xshd, HighlightingManager.Instance);
            }

            return Func;
        }

        private static void TryRunNode()
        {
            try
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    NodeProcess = Process.Start(new ProcessStartInfo("./Spock/spock_win64_v2.1.2.exe", "console")
                    {
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    NodeProcess = Process.Start(new ProcessStartInfo("./Spock/spock_linux_amd64_v2.1.2", "console")
                    {
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    //Not Support
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Start Node Failed!");
            }
        }

        static void ToolsMode()
        {
            Console.WriteLine("欢迎使用Spock Wallet工具");
            Console.WriteLine();
            Console.WriteLine("==========================");
            Console.WriteLine("请回复编号:");
            Console.WriteLine("1.批量生成钱包地址/控制台打印.");
            Console.WriteLine("2.批量生成钱包地址/导出csv文件.");
            Console.WriteLine("3.Plot Id碰撞.");

            string funNumber = Console.ReadLine();
            if (funNumber == "1")
                GenerateAddress(false);

            if (funNumber == "2")
                GenerateAddress(true);

            if (funNumber == "3")
                Collide();
        }

        static void GenerateAddress(bool isExportCSV)
        {
            Console.WriteLine("请输入你要生成的数量");

            string number = Console.ReadLine();
            if (int.TryParse(number, out int num) && num > 0)
            {
                var wallet = new ConsoleTable("Address", "PrivateKey", "Plot Id");
                string[] address = new string[num];
                for (int i = 0; i < address.Length; i++)
                {
                    var key = Nethereum.Signer.EthECKey.GenerateKey();
                    if (!isExportCSV)
                    {
                        address[i] = $"{key.GetPublicAddress().ConvertSpockAddress()},{key.GetPrivateKey()},{key.GetPublicAddress().ConvertPlotId()}";
                        wallet.AddRow(address[i].Split(','));
                    }
                }

                if (!isExportCSV)
                    wallet.Write(Format.Alternative);
                else
                {
                    string csvPath = "ExportGenerateAddress-" + DateTime.Now.Millisecond + ".csv";

                    string pathAndCommand = "\"" + Directory.GetCurrentDirectory() + "\" \"" + csvPath + "\" ";

                    File.WriteAllText(csvPath,
                        string.Join("\r\n", address));

                    TransactionViewModel.OpenBrowser(pathAndCommand);
                }
            }
            else
                Console.WriteLine("数量不正确");
        }

        static void Collide()
        {
            Console.WriteLine("Plot Id碰撞可以无限的生成地址，直到出现你指定的Plot Id地址，碰撞时间无法估计，也无法保证能碰撞到！");
            Console.WriteLine("=========================");
            Console.WriteLine("请输入Plot Id");

            string plotId = Console.ReadLine();
            int count = 1;
            while (true)
            {
                var key = Nethereum.Signer.EthECKey.GenerateKey();

                Console.WriteLine($"{count}/{key.GetPublicAddress().ConvertSpockAddress()},{key.GetPrivateKey()},{key.GetPublicAddress().ConvertPlotId()}");

                if (key.GetPublicAddress().ConvertPlotId() == plotId)
                {
                    Console.WriteLine("碰撞成功：");

                    var wallet = new ConsoleTable("Address", "PrivateKey", "Plot Id");
                    wallet.AddRow(key.GetPublicAddress().ConvertSpockAddress(),
                        key.GetPrivateKey(),
                        key.GetPublicAddress().ConvertPlotId());

                    wallet.Write(Format.Alternative);
                    break;
                }

                count++;
            }
        }
    }
}
