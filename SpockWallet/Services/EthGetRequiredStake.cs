using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpockWallet.Services
{
    public class EthGetRequiredStake : RpcRequestResponseHandler<HexBigInteger>
    {
        public EthGetRequiredStake(IClient client) : base(client, "eth_getRequiredStake")
        {
        }

        public Task<HexBigInteger> SendRequestAsync(string address, object id = null)
        {
            return base.SendRequestAsync(id, address);
        }

        public RpcRequest BuildRequest(string address, object id = null)
        {
            return base.BuildRequest(id, address);
        }
    }
}
