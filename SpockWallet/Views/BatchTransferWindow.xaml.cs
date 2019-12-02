using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nethereum.Web3.Accounts;
using SpockWallet.ViewModels.BatchModels;
using SpockWallet.ViewModels.TransactionModels;

namespace SpockWallet.Views
{
    public class BatchTransferWindow : Window
    {
        public BatchTransferWindow(string address, Account account)
        {
            this.InitializeComponent();
            this.DataContext = new BatchTransferWindowViewModel(address, account, this);
#if DEBUG
            this.AttachDevTools();
#endif
            this.FindControl<Button>("execCsvBtn").Click += async delegate
            {
                var openfileDialog = new OpenFileDialog()
                {
                    Title = "Open file",
                    Filters = new System.Collections.Generic.List<FileDialogFilter>() {
                        new FileDialogFilter() {
                            Extensions = new System.Collections.Generic.List<string>() {
                                "scv"
                            },
                            Name = "SCV"
                        }
                    }
                };

                var files = await openfileDialog.ShowAsync(GetWindow());
                if (files != null && files.Length > 0)
                {
                    var vm = (BatchTransferWindowViewModel)DataContext;
                    if (vm != null)
                    {
                        vm.ExecCsvFileName = files[0];
                        vm.OpenCsvFile(files[0]);
                    }
                }
            };

            this.FindControl<Button>("saveCsvBtn").Click += async delegate
            {
                var saveFileDialog = new SaveFileDialog()
                {
                    Title = "Save file",
                    Filters = new System.Collections.Generic.List<FileDialogFilter>() {
                        new FileDialogFilter() {
                            Extensions = new System.Collections.Generic.List<string>() {
                                "scv"
                            },
                            Name = "SCV"
                        }
                    }
                };

                var files = await saveFileDialog.ShowAsync(GetWindow());
                if (files != null)
                {
                    var vm = (BatchTransferWindowViewModel)DataContext;
                    if (vm != null)
                    {
                        vm.SaveCsvFile(files);
                        TransactionViewModel.ExecCommand("notepad \"" + files + "\"");
                    }
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        Window GetWindow() => (Window)this.VisualRoot;
    }
}
