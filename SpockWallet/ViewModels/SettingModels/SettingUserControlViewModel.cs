using MessageBox.Avalonia;
using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.Events;
using SpockWallet.Localizations;
using SpockWallet.Services;
using SpockWallet.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpockWallet.ViewModels.SettingModels
{
    public class SettingUserControlViewModel : ViewModelBase
    {
        private bool rpcTesting = false;

        private string rpcStatus;

        public string RpcStatus
        {
            get => rpcStatus;
            set => this.RaiseAndSetIfChanged(ref rpcStatus, value);
        }

        private string rpcUrl;

        public string RpcUrl
        {
            get => rpcUrl;
            set => this.RaiseAndSetIfChanged(ref rpcUrl, value);
        }

        private string doneBtnText;

        public string DoneBtnText
        {
            get => doneBtnText;
            set => this.RaiseAndSetIfChanged(ref doneBtnText, value);
        }

        private string version;

        public string Version
        {
            get { return version; }
            set => this.RaiseAndSetIfChanged(ref version, value);
        }

        public SettingUserControlViewModel()
        {
            using (DataContext dataContext = new DataContext())
            {
                var rpcUrlSetting = dataContext.Configs.Where(p => p.Key == Config.KEY_RPC_HOST).FirstOrDefault();
                RpcUrl = rpcUrlSetting.Value;
            }

            DoneBtnText = AppResources.Save;
            Version = App.Version;
        }

        public async void TestRpc()
        {
            if (rpcTesting)
                return;

            if (Uri.TryCreate(rpcUrl, UriKind.RelativeOrAbsolute, out Uri _))
            {
                rpcTesting = true;
                RpcStatus = AppResources.RpcStatusTesting;

                if (await Web3Service.TestRpc(rpcUrl))
                    RpcStatus = AppResources.RpcStatusTestSuccess;
                else
                    RpcStatus = AppResources.RpcStatusTestFailed;

                rpcTesting = false;

                return;
            }

            RpcStatus = AppResources.RpcStatusInvalidUrl;
        }

        private void DeleteWallet()
        {
            DeleteWalletWindow deleteWalletWindow = new DeleteWalletWindow();
            deleteWalletWindow.Show();
        }

        public async void SaveSetting()
        {
            //if (wsTesting)
            //    return;

            if (rpcTesting)
                return;


            if (/*string.IsNullOrEmpty(WsUrl) || */string.IsNullOrEmpty(rpcUrl))
            {
                await new MessageBoxWindow(AppResources.PleaseEnterURL,
                    AppResources.PleaseEnterRpcUrlAddresses)
                    .Show();

                return;
            }

            DoneBtnText = AppResources.TestingAndSaving;

            if (await Web3Service.TestRpc(rpcUrl))
            {
                using (DataContext dataContext = new DataContext())
                {
                    //保存
                    var rpcHost = dataContext.Configs.Where(p => p.Key == Config.KEY_RPC_HOST).FirstOrDefault();
                    if (rpcHost != null)
                    {
                        rpcHost.Value = rpcUrl;
                        dataContext.Update(rpcHost);
                    }
                    else
                        dataContext.Configs.Add(new Config() { Key = Config.KEY_RPC_HOST, Value = rpcUrl });

                    //var wsHost = _dataContext.Settings.Where(p => p.Key == Setting.KEY_WS_HOST).FirstOrDefault();
                    //if (wsHost != null)
                    //{
                    //    wsHost.Value = wsUrl;
                    //    _dataContext.Update(wsHost);
                    //}
                    //else
                    //    _dataContext.Settings.Add(new Setting() { Key = Setting.KEY_WS_HOST, Value = wsUrl });

                    dataContext.SaveChanges();
                }

                await new MessageBoxWindow(AppResources.SaveSuccessful,
                    AppResources.SaveSuccessfulMessage)
                    .Show();

                DoneBtnText = AppResources.Save;
            }
            else
            {
                await new MessageBoxWindow(AppResources.AddressUnavailable,
                    AppResources.AddressUnavailableMessage)
                    .Show();

                DoneBtnText = "Save";

                return;
            }
        }
    }
}
