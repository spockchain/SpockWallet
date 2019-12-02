using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SpockWallet.Data
{
    public class Config
    {
        public const string KEY_RPC_HOST = "rpc_host";
        public const string KEY_WS_HOST = "ws_host";

        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
