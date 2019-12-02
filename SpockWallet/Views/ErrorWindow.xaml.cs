using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpockWallet.Localizations;
using SpockWallet.ViewModels.TransactionModels;
using System.Web;

namespace SpockWallet.Views
{
    public class ErrorWindow : Window
    {
        private readonly string _errorMsg;

        public ErrorWindow(string errorMsg)
        {
            _errorMsg = errorMsg;

            this.InitializeComponent();
            this.FindControl<TextBox>("errBox").Text = errorMsg;
            this.FindControl<Button>("copyMsg").Click += ErrorWindow_CopyMsg_Click;
            this.FindControl<Button>("exit").Click += ErrorWindow_Exit_Click;
            this.FindControl<Button>("sendEmail").Click += ErrorWindow_SendEmail_Click; ;

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void ErrorWindow_SendEmail_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            TransactionViewModel.OpenBrowser("mailto:support@spock.network?subject=SpockWallet%20Error%20Message&body=" + HttpUtility.UrlEncode(
                "Client Version:" + App.Version + "\r\n" +
                "OS Version:" + System.Environment.OSVersion.VersionString + "\r\n" +
                _errorMsg));
        }

        private void ErrorWindow_Exit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ErrorWindow_CopyMsg_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await Application.Current.Clipboard.SetTextAsync(_errorMsg);

            var btn = (Button)sender;

            LocalizationAttachedPropertyHolder.SetUid(btn, "Copied");
            btn.Content = AppResources.Copied;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
