using Nethereum.Web3;
using ReactiveUI;
using SpockWallet.Data;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;
using SpockWallet.Services;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using SpockWallet.Views;
using ZXing;
using ZXing.QrCode;
using SpockWallet.Converters;
using System.Drawing;
using Avalonia.Platform;
using Avalonia;
using SkiaSharp;
using Avalonia.Skia;
using SpockWallet.Events;
using SpockWallet.ViewModels.TransactionModels;
using Nethereum.Web3.Accounts;

namespace SpockWallet.ViewModels.WalletModels
{
    public class WalletViewModel : ReactiveObject
    {
        private readonly Wallet _wallet;
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public WalletViewModel()
        {
            Symbol = "SPOK";
        }

        public WalletViewModel(Wallet wallet) : this()
        {
            CancellationTokenSource = new CancellationTokenSource();

            _wallet = wallet;
            address = wallet.Address;
            plotid = wallet.PlotId;
            balance = Web3.Convert.FromWei(BigInteger.Parse(wallet.Balance));
            stakingRequired = Web3.Convert.FromWei(BigInteger.Parse(wallet.StakingRequired));

            RxApp.TaskpoolScheduler.Schedule(wallet, (schedule, state) =>
             {
                 Scan scan = new Scan();
                 scan.StartAsync(state, CancellationTokenSource);

                 return scan;
             });
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

        private decimal stakingRequired;

        public decimal StakingRequired
        {
            get { return stakingRequired; }
            set
            {
                this.RaiseAndSetIfChanged(ref stakingRequired, value);
            }
        }

        private string plotid;

        public string Plotid
        {
            get { return plotid; }
            set
            {
                this.RaiseAndSetIfChanged(ref plotid, value);
            }
        }

        private string symbol;

        public string Symbol
        {
            get { return symbol; }
            set
            {
                this.RaiseAndSetIfChanged(ref symbol, value);
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

        private bool isUnSafe;

        public bool IsUnSafe
        {
            get { return isUnSafe; }
            set
            {
                this.RaiseAndSetIfChanged(ref isUnSafe, value);
            }
        }

        public async void CopyAddress()
        {
            if (IsUnSafe)
                await Application.Current.Clipboard.SetTextAsync(_wallet.PrivateKey);
            else
                await Application.Current.Clipboard.SetTextAsync(Address);
        }

        public async void CopyPlotid()
        {
            await Application.Current.Clipboard.SetTextAsync(Plotid);
        }

        public void BindPlot()
        {
            PlotIdManagerWindow plotIdManagerWindows = new PlotIdManagerWindow(new Account(_wallet.PrivateKey));
            plotIdManagerWindows.Show();
        }

        public async void Send()
        {
            var spokBalance = await Web3Service.GetWeb3().Eth.GetBalance.SendRequestAsync(Address.EnsureHexAddress());

            SendTransactionViewModel sendTransactionViewModel = new SendTransactionViewModel()
            {
                Account = new Nethereum.Web3.Accounts.Account(_wallet.PrivateKey),
                Address = Address,
                Balance = Web3.Convert.FromWei(spokBalance.Value)
            };

            SendTransactionWindows sendTransactionWindows = new SendTransactionWindows();
            sendTransactionWindows.DataContext = sendTransactionViewModel;
            sendTransactionViewModel.OnWindowsClose += () => sendTransactionWindows.Close();

            sendTransactionWindows.Show();
        }

        public void Receive()
        {
            ReceiveWindow receiveWindow = new ReceiveWindow();
            receiveWindow.DataContext = new ReceiveWindowViewModel()
            {
                Address = address,
                Plotid = plotid,
                QrCode = Generate2D(address, 200, 200)
            };
            receiveWindow.Show();
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public static Avalonia.Media.Imaging.Bitmap Generate2D(string text, int width, int height)
        {
            BarcodeWriter<SKBitmap> writer = new BarcodeWriter<SKBitmap>();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Renderer = new SKBitmapRenderer();
            writer.Options = new QrCodeEncodingOptions()
            {
                DisableECI = true,//设置内容编码
                CharacterSet = "UTF-8",  //设置二维码的宽度和高度
                Width = width,
                Height = height,
                Margin = 1//设置二维码的边距,单位不是固定像素
            };

            var bitmap = writer.Write(text);

            return new Avalonia.Media.Imaging.Bitmap(
                    bitmap.ColorType.ToPixelFormat(),
                    bitmap.GetPixels(),
                    new PixelSize(bitmap.Width, bitmap.Height),
                         SkiaPlatform.DefaultDpi,
                    bitmap.RowBytes);
        }
    }
}
