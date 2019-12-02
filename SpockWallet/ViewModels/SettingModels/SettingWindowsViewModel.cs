using MessageBox.Avalonia;
using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.Localizations;
using SpockWallet.Services;
using SpockWallet.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpockWallet.ViewModels.SettingModels
{
    public class SettingWindowsViewModel : ViewModelBase
    {
        public delegate void WindowsClose();

        public event WindowsClose OnWindowsClose;

        private bool rpcTesting = false;
        //private bool wsTesting = false;

        #region 绑定属性
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

        //private string wsStatus;

        //public string WsStatus
        //{
        //    get => wsStatus;
        //    set => this.RaiseAndSetIfChanged(ref wsStatus, value);
        //}

        //private string wsUrl;

        //public string WsUrl
        //{
        //    get => wsUrl;
        //    set => this.RaiseAndSetIfChanged(ref wsUrl, value);
        //}

        private string doneBtnText;

        public string DoneBtnText
        {
            get => doneBtnText;
            set => this.RaiseAndSetIfChanged(ref doneBtnText, value);
        }
        #endregion

        public SettingWindowsViewModel()
        {
            RpcUrl = "http://localhost:9666";
            RpcStatus = AppResources.RpcStatusWaitingTest;
            //WsStatus = "Status: waiting test...";
            DoneBtnText = AppResources.Save;
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

        //public async void TestWs()
        //{
        //    if (wsTesting)
        //        return;

        //    if (Uri.TryCreate(wsUrl, UriKind.RelativeOrAbsolute, out Uri _))
        //    {
        //        wsTesting = false;
        //        WsStatus = "Status: testing...";

        //        if (await Web3Service.TestWs(wsUrl))
        //            WsStatus = "Status: test succeed";
        //        else
        //            WsStatus = "Status：test failed";

        //        wsTesting = false;

        //        return;
        //    }

        //    WsStatus = "Warning：RPC address is not a valid URI address";
        //}

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

            if (/*await Web3Service.TestWs(wsUrl) &&*/ await Web3Service.TestRpc(rpcUrl))
            {
                //保存
                using (DataContext dataContext = new DataContext())
                {
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

                    //检查是不是公共节点
                    if (Web3Service.CheckPublicNode(rpcUrl))
                        App.SyncTimeSpan = App.SlowSyncTime;
                    else
                        App.SyncTimeSpan = App.MediumSyncTime;

                    dataContext.SaveChanges();
                }

                WalletWindow walletWindow = new WalletWindow();
                walletWindow.Show();

                OnWindowsClose?.Invoke();
            }
            else
            {
                await new MessageBoxWindow(AppResources.AddressUnavailable,
                    AppResources.AddressUnavailableMessage)
                    .Show();

                DoneBtnText = AppResources.Save;

                return;
            }
        }

        public void UseOfflineWallet()
        {
            OfflineWalletWindow offlineWindow = new OfflineWalletWindow();
            offlineWindow.Show();

            OnWindowsClose?.Invoke();
        }
    }
}
