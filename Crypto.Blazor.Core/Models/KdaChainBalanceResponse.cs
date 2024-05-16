using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Blazor.Core.Models
{
    public class KdaChainBalanceResponse
    {
        public string status { get; set; }
        public Data data { get; set; }
    }

    public class Guard
    {
        public string pred { get; set; }
        public List<string> keys { get; set; }
    }

    public class Data
    {
        public Guard guard { get; set; }
        public double balance { get; set; }
        public string account { get; set; }
    }
}
