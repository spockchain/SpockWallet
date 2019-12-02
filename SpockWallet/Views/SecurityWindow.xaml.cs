using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpockWallet.ViewModels.SecurityModels;

namespace SpockWallet.Views
{
    public class SecurityWindow : Window
    {
        public SecurityWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<TextBox>("passwordKey").KeyDown += SecurityWindow_KeyDown;
        }

        private void SecurityWindow_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if(e.Key == Avalonia.Input.Key.Enter)
            {
                ((SecurityWindowViewModel)this.DataContext).Enter();
            }
        }
    }
}
