using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpockWallet.ViewModels.SettingModels;

namespace SpockWallet.Views
{
    public class SettingWindow : Window
    {
        public SettingWindow()
        {
            DataContext = new SettingWindowsViewModel();

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ((SettingWindowsViewModel)this.DataContext).OnWindowsClose += CreateWalletWindow_OnWindowsClose;
        }

        private void CreateWalletWindow_OnWindowsClose()
        {
            this.Close();
        }
    }
}
