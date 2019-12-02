using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SpockWallet.Data
{
    public class TokenTransfer
    {
        [Key]
        public string Hash { get; set; }

        public long BlockNumber { get; set; }

        public string Method { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string ContractAddress { get; set; }

        public string Symbol { get; set; }

        public decimal Value { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    }
}
