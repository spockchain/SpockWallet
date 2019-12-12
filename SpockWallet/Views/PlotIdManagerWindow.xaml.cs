using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nethereum.Web3.Accounts;
using SpockWallet.ViewModels.PoolModels;

namespace SpockWallet.Views
{
    public class PlotIdManagerWindow : Window
    {
        public PlotIdManagerWindow(Account account)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            DataContext = new PlotIdManagerWindowsViewModel(account);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
