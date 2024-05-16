using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.MauiBlazor.Models
{
    public class AppSettings
    {
        public List<KdaAddress> KdaWalletAddresses { get; set; } = new List<KdaAddress>();

        public BinanceApiInfo? Binance { get; set; }

        public string CoinMarketApiKey { get; set; }
    }

    public class KdaAddress
    {
        public string Address { get; set; }
        public string Name { get; set; }
    }

    public class BinanceApiInfo
    {
        public string Key { get; set; }
        public string Secret { get; set; }
    }
}