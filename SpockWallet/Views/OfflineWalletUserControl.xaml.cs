using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpockWallet.Views
{
    public class OfflineWalletUserControl : UserControl
    {
        public OfflineWalletUserControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
