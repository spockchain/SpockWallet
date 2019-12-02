using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using SpockWallet.Events;
using SpockWallet.Services;
using SpockWallet.ViewModels.WalletModels;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SpockWallet.Views
{
    public class ExitApplicationWindows : Window
    {
        //要等待结束同步的任务
        private readonly int _syncingCount;
        private int StopedSync = 0;
        private readonly WalletWindowViewModel _walletWindowViewModel;
        private readonly IDisposable _timer;
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(1000);
        private readonly ProgressBar _progressBar;

        private bool isExiting = false;

        public ExitApplicationWindows(WalletWindowViewModel viewModel)
        {
            _walletWindowViewModel = viewModel;
            _syncingCount = viewModel.WalletsUserControlViewModel.Wallets.Count;

            this.InitializeComponent();
            this.Closed += ExitApplicationWindows_Closed;
            _progressBar = this.FindControl<ProgressBar>("Progress");

            this.Activated += ExitApplicationWindows_Activated;
#if DEBUG
            this.AttachDevTools();
#endif

            if (_syncingCount > 0)
            {
                MessageBus.Current.Listen<SyncStoped>().Subscribe(block =>
                {
                    StopedSync++;
                    if (StopedSync >= _syncingCount)
                    {
                        _walletWindowViewModel.UpdateWalletScanLocationCancellationToken.Cancel();
                    }
                });

                MessageBus.Current.Listen<SyncShutdown>().Subscribe(block =>
                {
                    if (isExiting)
                        return;

                    isExiting = true;

                    Program.NodeProcess?.StandardInput.WriteLine("exit");
                    Program.NodeProcess?.WaitForExit();

                    Program.NodeProcess?.Dispose();

                    Program.NodeProcess = null;

                    _progressBar.Value = 60;

                    _timer.Dispose();

                    this.Close();
                });
            }

            //初始化定时器
            _timer = Observable.Timer(_updateInterval, _updateInterval, RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    if (_progressBar.Value <= 55)
                        _progressBar.Value += 1;
                });
        }

        private void ExitApplicationWindows_Closed(object sender, EventArgs e)
        {
            if (isExiting)
                return;

            isExiting = true;

            Program.NodeProcess?.StandardInput.WriteLine("exit");
            Program.NodeProcess?.WaitForExit();

            Program.NodeProcess?.Dispose();

            Program.NodeProcess = null;
        }

        private void ExitApplicationWindows_Activated(object sender, System.EventArgs e)
        {
            if (_syncingCount <= 0)
            {
                this.Close();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
