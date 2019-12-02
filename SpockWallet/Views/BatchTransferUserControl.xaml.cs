using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpockWallet.Views
{
    public class BatchTransferUserControl : UserControl
    {
        public BatchTransferUserControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
