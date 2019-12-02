using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpockWallet.ViewModels;
using SpockWallet.ViewModels.SecurityModels;
using System.IO;

namespace SpockWallet.Views
{
    public class MainWindow : Window
    {
        private static object Locker = new object();

        private bool IsClosed = false;

        private readonly MainWindowViewModel _mainWindowViewModel;

        public MainWindow()
        {
            _mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = _mainWindowViewModel;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            LoadPassword();
            _mainWindowViewModel.OnConfigDone += MainWindow_OnConfigDone;
        }

        private void MainWindow_OnConfigDone(object e)
        {
            this.Close();
        }

        private async void LoadPassword()
        {
            if (!File.Exists("wallet.db"))
            {
                SecuritySetWindowViewModel setWindowViewModel = new SecuritySetWindowViewModel();

                SecuritySetWindow securitySetWindow = new SecuritySetWindow();
                securitySetWindow.DataContext = setWindowViewModel;
                securitySetWindow.Closed += SecuritySetWindow_Closed;

                setWindowViewModel.OnPasswordEntered += () => securitySetWindow.Close();

                await securitySetWindow.ShowDialog(this);
            }
            else
            {
                SecurityWindowViewModel passwordWindowViewModel = new SecurityWindowViewModel();

                SecurityWindow securityWindow = new SecurityWindow();
                securityWindow.DataContext = passwordWindowViewModel;
                securityWindow.Closed += SecurityWindow_Closed; ;

                passwordWindowViewModel.OnPasswordEntered += () => securityWindow.Close();

                await securityWindow.ShowDialog(this);
            }
        }

        private void SecuritySetWindow_Closed(object sender, System.EventArgs e)
        {
            lock (Locker)
            {
                if (IsClosed) return;

                IsClosed = true;

                if (sender is SecuritySetWindow securitySetWindow)
                {
                    if (((SecuritySetWindowViewModel)securitySetWindow.DataContext).IsSuceess)
                    {
                        _mainWindowViewModel.Init();
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
        }

        private void SecurityWindow_Closed(object sender, System.EventArgs e)
        {
            lock (Locker)
            {
                if (IsClosed) return;

                IsClosed = true;

                if (sender is SecurityWindow securityWindow)
                {
                    if (((SecurityWindowViewModel)securityWindow.DataContext).IsSuceess)
                    {
                        _mainWindowViewModel.Init();
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
