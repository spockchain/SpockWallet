using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpockWallet.Views
{
    public class SecuritySetWindow : Window
    {
        public SecuritySetWindow()
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
