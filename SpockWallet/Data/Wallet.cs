using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SpockWallet.Data
{
    public class Wallet
    {
        [Key]
        public string Address { get; set; }

        public string PrivateKey { get; set; }

        public string Balance { get; set; }

        public string StakingRequired { get; set; }

        public string PlotId { get; set; }

        public long ScanLocation { get; set; }

        public bool IsDelete { get; set; }
    }
}
