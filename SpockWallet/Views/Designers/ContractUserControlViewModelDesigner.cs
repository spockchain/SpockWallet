using SpockWallet.ViewModels.ContractModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.Views.Designers
{
    public class ContractUserControlViewModelDesigner : ContractUserControlViewModel
    {
        public ContractUserControlViewModelDesigner()
            : base()
        {
            this.Contract = new ViewModels.ContractModels.ContractViewModel()
            {
                Address = "SPOCK-5021bb5f12ecf3a9a57714dcb819da7a681b12f2",
                CreationTransaction = "0x29fa1f2d781c404e9baafc21329cc0b200968d6badf1a601968d3897d0228262",
                Decimals = 18,
                Owner = "SPOCK-b84a1184476eb54d6f579b8f6b54ce7f8ae29026",
                Symbol = "FBL",
                TokenName = "First Blood",
                TotalSupply = "1000000"
            };

            for (int i = 0; i < 3; i++)
            {
                this.TokenTransfers.Add(new ViewModels.ContractModels.TokenTransferViewModel()
                {
                    Symbol = "FBL",
                    BlockNumber = 1 * 1000,
                    Contract = "0x29fa1f2d781c404e9baafc21329cc0b200968d6badf1a601968d3897d0228262",
                    From = "SPOCK-b84a1184476eb54d6f579b8f6b54ce7f8ae29026",
                    Value = 100,
                    Hash = "0x29fa1f2d781c404e9baafc21329cc0b200968d6badf1a601968d3897d0228262",
                    To = "SPOCK-F7e39F2DaD56fb0437484d39a19592666dafd388",
                });
            }
        }
    }
}
