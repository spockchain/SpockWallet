using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpockWallet.ViewModels.WalletModels;

namespace SpockWallet.Views
{
    public class CreateWalletWindow : Window
    {
        public CreateWalletWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ((CreateWalletWindowViewModel)this.DataContext).OnWindowsClose += CreateWalletWindow_OnWindowsClose;
        }

        private void CreateWalletWindow_OnWindowsClose()
        {
            this.Close();
        }
    }
}
