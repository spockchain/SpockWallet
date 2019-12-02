using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpockWallet.Views
{
    public class ReceiveWindow : Window
    {
        public ReceiveWindow()
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
