using SpockWallet.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Events
{
    public class TokenTransferAdded
    {
        public TokenTransferAdded(TokenTransfer tokenTransfer)
        {
            TokenTransfer = tokenTransfer;
        }

        public TokenTransfer TokenTransfer { get; set; }
    }
}
