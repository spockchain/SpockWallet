using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpockWallet.ViewModels.WalletModels;

namespace SpockWallet.Views
{
    public class ImportWalletWindow : Window
    {
        public ImportWalletWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ((ImportWalletWindowViewModel)this.DataContext).OnWindowsClose += CreateWalletWindow_OnWindowsClose;
        }

        private void CreateWalletWindow_OnWindowsClose()
        {
            this.Close();
        }
    }
}
