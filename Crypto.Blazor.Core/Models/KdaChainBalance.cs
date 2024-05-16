using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Blazor.Core.Models
{
    public class KdaChainBalance
    {
        private string _chainIdString;

        public string ChainIdString
        {
            get { return _chainIdString; }
            set { _chainIdString = value; }
        }

        public int? ChainId
        {
            get
            {
                int parsedChainId;
                if (int.TryParse(_chainIdString, out parsedChainId))
                {
                    return parsedChainId;
                }
                else
                {
                    return null;
                }
            }
        }

        public double Balance { get; set; }
    }
}
