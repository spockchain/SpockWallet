using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpockWallet.ViewModels;

namespace SpockWallet.Views
{
    public class SendTransactionWindows : Window
    {
        public SendTransactionWindows()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
