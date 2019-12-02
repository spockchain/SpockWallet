using Nethereum.Web3;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpockWallet.Data;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Linq;
using Nethereum.Util;
using Nethereum.Web3.Accounts;

namespace SpockWallet.Services
{
    public static class Web3Service
    {
        private readonly static object _locker = new object();

        private static AddressUtil _addressUtil = new AddressUtil();
        private static Web3 web3;
        private static string rpcUrl;

        public static Web3 GetWeb3()
        {
            lock (_locker)
            {
                if (web3 == null || string.IsNullOrEmpty(rpcUrl))
                {
                    using (DataContext dataContext = new DataContext())
                    {
                        var rpcUrlSetting = dataContext.Configs.Where(p => p.Key == Config.KEY_RPC_HOST).FirstOrDefault();
                        rpcUrl = rpcUrlSetting.Value;
                        web3 = new Web3(rpcUrl);
                    }

                    return web3;
                }
                else
                {
                    return web3;
                }
            }
        }

        public static Web3 GetWeb3(Account account)
        {
            lock (_locker)
            {
                if (string.IsNullOrEmpty(rpcUrl))
                {
                    using (DataContext dataContext = new DataContext())
                    {
                        var rpcUrlSetting = dataContext.Configs.Where(p => p.Key == Config.KEY_RPC_HOST).FirstOrDefault();
                        rpcUrl = rpcUrlSetting.Value;
                        return new Web3(account, rpcUrlSetting.Value);
                    }
                }
                else
                {
                    return new Web3(account, rpcUrl);
                }
            }
        }

        public static bool IsValidAddress(string address)
        {
            return !string.IsNullOrEmpty(address) && _addressUtil.IsValidAddressLength(address.Replace("SPOCK-", "0x").Replace("spock-", "0x"));
        }

        public static string ConvertPlotId(this string address)
        {
            var bytes = address.Substring(address.Length - 16).HexToByteArray();
            Array.Reverse(bytes);

            return BitConverter.ToUInt64(bytes, 0).ToString();
        }

        public static string ConvertSpockAddress(this string address)
        {
            return "SPOCK-" + address.Substring(2);
        }

        public static Wallet CreateWallet()
        {
            EthECKey key = EthECKey.GenerateKey();

            string address = key.GetPublicAddress();

            return new Wallet()
            {
                Address = address,
                Balance = "0",
                StakingRequired = "0",
                PrivateKey = key.GetPrivateKey(),
                ScanLocation = 0,
                PlotId = address.ConvertPlotId(),
            };
        }

        public async static Task<bool> TestRpc(string url)
        {
            try
            {
                Web3 web3 = new Web3(url);
                await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                web3 = null;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async static Task<bool> TestWs(string url)
        {
            try
            {
                StreamingWebSocketClient streamingWebSocketClient = new StreamingWebSocketClient(url);
                await streamingWebSocketClient.StartAsync();

                await streamingWebSocketClient.StopAsync();

                streamingWebSocketClient = null;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string EnsureHexAddress(this string value)
        {
            if (value.StartsWith("SPOCK-") || value.StartsWith("spock-"))
                return "0x" + value.Remove(0, 6);
            else
                return value;
        }

        public static bool CheckPublicNode()
        {
            //检查公共节点
            using (DataContext dataContext = new DataContext())
            {
                var rpcUrlSetting = dataContext.Configs.Where(p => p.Key == Config.KEY_RPC_HOST).FirstOrDefault();
                return CheckPublicNode(rpcUrlSetting.Value);
            }
        }

        public static bool CheckPublicNode(string rpcUrlSetting)
        {
            return rpcUrlSetting != null && !(rpcUrlSetting.StartsWith("http://127.")
                   || rpcUrlSetting.StartsWith("http://localhost")
                   || rpcUrlSetting.StartsWith("http://10.")
                   || rpcUrlSetting.StartsWith("http://172.")
                   || rpcUrlSetting.StartsWith("http://192."));
        }
    }
}
