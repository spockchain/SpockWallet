using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SpockWallet.Data
{
    public class Contract
    {
        [Key]
        public string Address { get; set; }

        public long BlockNumber { get; set; }

        public string CreationTransaction { get; set; }

        public string TokenName { get; set; }

        public string Symbol { get; set; }

        public string Owner { get; set; }

        public string TotalSupply { get; set; }

        public int Decimals { get; set; }

        public bool IsDelete { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    }
}
