using Avalonia.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using Nethereum.Web3;
using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.Localizations;
using SpockWallet.Services;
using SpockWallet.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpockWallet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public delegate void ConfigDone(object e);

        public event ConfigDone OnConfigDone;

        private DataContext _dataContext;

        private string greeting;

        public string Greeting
        {
            get => greeting;
            set => this.RaiseAndSetIfChanged(ref greeting, value);
        }

        public async void Init()
        {
            //初始化数据库
            Greeting = AppResources.Loading;

            Greeting = AppResources.StartInitDb;
            await Task.Delay(700);

            _dataContext = new DataContext();
            _dataContext.Database.Migrate();

            Greeting = AppResources.InitDbDone;
            await Task.Delay(700);

            Greeting = AppResources.ConnectToNode;
            await Task.Delay(700);

            //检测rpc连接
            //如果RPC设置不存在则使用默认的试试
            if (await TestNetwork(Config.KEY_RPC_HOST, "http://localhost:9666", true))
            {
                Greeting = AppResources.ConnectedWelcome;
                await Task.Delay(500);

                _dataContext.Dispose();

                WalletWindow walletWindow = new WalletWindow();
                walletWindow.Show();

                OnConfigDone?.Invoke(this);
            }
            else
            {
                _dataContext.Dispose();

                //如果本地或已配置rpc地址没有开8545端口说明没有装Spock则让用户自己配置
                SettingWindow settingWindows = new SettingWindow();
                settingWindows.Show();

                OnConfigDone?.Invoke(this);
            }
        }

        private async Task<bool> TestNetwork(string settingKey, string localUrl, bool isRpc = true)
        {
            //检测rpc连接
            bool isLocal = false;

            Config testSetting = _dataContext.Configs.Where(p => p.Key == settingKey).FirstOrDefault();
            if (testSetting == null)
            {
                isLocal = true;
                testSetting = new Config() { Key = settingKey, Value = localUrl };
            }

            if (isRpc)
            {
                if (await Web3Service.TestRpc(testSetting.Value))
                {
                    if (isLocal)
                    {
                        //本地测试通过
                        App.SyncTimeSpan = App.MediumSyncTime;

                        _dataContext.Configs.Add(testSetting);
                        _dataContext.SaveChanges();
                    }
                    else
                    {
                        //检查是不是公共节点
                        if (Web3Service.CheckPublicNode(testSetting.Value))
                            App.SyncTimeSpan = App.SlowSyncTime;
                        else
                            App.SyncTimeSpan = App.MediumSyncTime;
                    }

                    return true;
                }

                return false;
            }
            else
            {
                if (await Web3Service.TestWs(testSetting.Value))
                {
                    if (isLocal)
                    {
                        //本地测试通过
                        _dataContext.Configs.Add(testSetting);
                        _dataContext.SaveChanges();
                    }

                    return true;
                }

                return false;
            }
        }
    }
}
