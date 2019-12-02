using CsvHelper;
using Nethereum.Web3.Accounts;
using ReactiveUI;
using ReactiveUI.Legacy;
using SpockWallet.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SpockWallet.ViewModels.BatchModels
{
    public class BatchTransferWindowViewModel : ViewModelBase
    {
        private readonly BatchTransferWindow _batchTransferWindow;

        private BatchTransferUserControlViewModel batchTransferUserControlViewModel;

        public BatchTransferUserControlViewModel BatchTransferUserControlViewModel
        {
            get { return batchTransferUserControlViewModel; }
            set
            {
                this.RaiseAndSetIfChanged(ref batchTransferUserControlViewModel, value);
            }
        }

        private string address;

        public string Address
        {
            get { return address; }
            set
            {
                this.RaiseAndSetIfChanged(ref address, value);
            }
        }

        private string saveCsvFileName;

        public string SaveCsvFileName
        {
            get => saveCsvFileName;
            set => this.RaiseAndSetIfChanged(ref saveCsvFileName, value);
        }

        private string execCsvFileName;

        public string ExecCsvFileName
        {
            get => saveCsvFileName;
            set => this.RaiseAndSetIfChanged(ref execCsvFileName, value);
        }

        private bool showFileControl;

        public bool ShowFileControl
        {
            get { return showFileControl; }
            set
            {
                this.RaiseAndSetIfChanged(ref showFileControl, value);
            }
        }

        public BatchTransferWindowViewModel(string address, Account account, BatchTransferWindow batchTransferWindow)
        {
            ShowFileControl = true;
            Address = address;
            BatchTransferUserControlViewModel = new BatchTransferUserControlViewModel(account, batchTransferWindow);
            _batchTransferWindow = batchTransferWindow;
        }

        public void SaveCsvFile(string fileName)
        {
            File.WriteAllText(fileName, "SPOCK-0000000000000000000000000000000000000000,100\r\nSPOCK-0000000000000000000000000000000000000000,200");
        }

        public async void OpenCsvFile(string fileName)
        {
            var fileStream = File.OpenRead(fileName);
            var reader = new StreamReader(fileStream);
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HasHeaderRecord = false;
                var tranList = csv.GetRecords<TranModel>().ToArray();
                if (tranList != null && tranList.Length > 0)
                {
                    ShowFileControl = false;
                    BatchTransferUserControlViewModel.AddTransactions(tranList);
                }
                else
                {
                    await new MessageBox.Avalonia.MessageBoxWindow("没有交易",
                    "请确认你的csv文件是否正确并且有交易")
                    .Show();
                }
            }
        }
    }

    public class TranModel
    {
        public string Address { get; set; }

        public decimal Amount { get; set; }
    }
}
