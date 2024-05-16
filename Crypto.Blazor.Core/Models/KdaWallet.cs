using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Blazor.Core.Models
{
    public class KdaWallet
    {
        // List of chain balances
        public List<KdaChainBalance>? ChainBalances { get; set; }

        // Address of the wallet
        public string Address { get; set; }

        // Total balance across all chains
        public double TotalBalance
        {
            get
            {
                // Check if ChainBalances is not null
                if (ChainBalances == null)
                {
                    return 0;
                }

                // Sum the Balance of each chain
                double totalBalance = 0;
                foreach (var chainBalance in ChainBalances)
                {
                    totalBalance += chainBalance.Balance;
                }
                return totalBalance;
            }
        }

        // Number of chains with a balance greater than 0
        /// <summary>
        /// This is the number of chains that the total balance is spread across on this KDA wallet.
        /// </summary>
        public int SpreadAcross
        {
            get
            {
                // Check if ChainBalances is not null
                if (ChainBalances == null)
                {
                    return 0;
                }

                // Count the number of chains with a balance greater than 0
                int count = 0;
                foreach (var chainBalance in ChainBalances)
                {
                    if (chainBalance.Balance > 0)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
    }

}
