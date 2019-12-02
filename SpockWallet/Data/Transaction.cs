using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SpockWallet.Data
{
    public class Transaction
    {
        [Key]
        public string Hash { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string BlockHash { get; set; }

        public long BlockNumber { get; set; }

        public string Value { get; set; }

        public string Gas { get; set; }

        public string GasPrice { get; set; }

        public string Input { get; set; }

        public string Status { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
