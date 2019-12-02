using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using SpockWallet.Data;
using SpockWallet.Localizations;
using SpockWallet.Services;
using SpockWallet.ViewModels.TransactionModels;
using SpockWallet.ViewModels.WalletModels;
using System;
using System.Linq;

namespace SpockWallet.Views
{
    public class WalletWindow : Window
    {
        public WalletWindow()
        {
            this.InitializeComponent();
            this.Closing += WalletWindow_Closing;
            this.Opened += WalletWindow_Opened;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private async void WalletWindow_Opened(object sender, EventArgs e)
        {
            if (Web3Service.CheckPublicNode())
            {
                var result = await new MessageBox.Avalonia.MessageBoxWindow(new MessageBoxParams
                {
                    Button = ButtonEnum.YesNo,
                    ContentTitle = AppResources.NetworkWarning,
                    ContentMessage = AppResources.NetworkWarningMessage,
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    Style = Style.Windows
                }).ShowDialog(this);

                if (result == "Yes")
                    TransactionViewModel.OpenBrowser("https://github.com/spockchain/spock/releases");
            }
        }

        private void WalletWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DataContext is WalletWindowViewModel viewModel)
            {
                ExitApplicationWindows exitApplicationWindows = new ExitApplicationWindows(viewModel);
                exitApplicationWindows.Show();

                viewModel.Dispose();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
