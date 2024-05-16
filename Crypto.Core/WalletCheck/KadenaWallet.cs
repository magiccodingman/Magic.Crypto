using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Core.WalletCheck
{
    public class KadenaWallet
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<Dictionary<string, double>> GetWalletBalances(string server, string accountName)
        {
            var balances = new Dictionary<string, double>();
            var versionInfo = await GetVersion(server);
            if (versionInfo == null) return balances;

            var chainIds = versionInfo.Chains;
            var nv = versionInfo.NodeVersion;
            var host = (string chainId) => $"https://{server}/chainweb/0.0/{nv}/chain/{chainId}/pact";

            double balance = await GetBalance(host, "coin", accountName, "2");

            //var tasks = new List<Task>();

            //foreach (var chainId in chainIds)
            //{
            //    tasks.Add(Task.Run(async () =>
            //    {
            //        double balance = await GetBalance(host, "coin", accountName, chainId);
            //        lock (balances)
            //        {
            //            balances[chainId] = balance;
            //        }
            //    }));
            //}

            //await Task.WhenAll(tasks);
            return balances;
        }

        private async Task<VersionInfo> GetVersion(string server)
        {
            try
            {
                var response = await httpClient.GetStringAsync($"https://{server}/info");
                var resJSON = JsonConvert.DeserializeObject<NodeInfo>(response);
                var av = resJSON.NodeApiVersion;
                var nv = resJSON.NodeVersion;
                var chainIds = resJSON.NodeChains;

                return new VersionInfo
                {
                    NodeVersion = nv,
                    Chains = chainIds.Length != 10 ? await GetChainIds(server, av, nv) : chainIds
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching version info: {e.Message}");
                return null;
            }
        }

        private async Task<string[]> GetChainIds(string server, string av, string nv)
        {
            try
            {
                var cutResponse = await httpClient.GetStringAsync($"https://{server}/chainweb/{av}/{nv}/cut");
                var cutJSON = JsonConvert.DeserializeObject<CutInfo>(cutResponse);

                // Extract chain IDs from the "hashes" field
                var chainIds = cutJSON.Hashes.Keys.ToArray();

                return chainIds;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching chain IDs: {e.Message}");
                // Fallback to default chain IDs
                return new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            }
        }

        private async Task<double> GetBalance(Func<string, string> host, string token, string accountName, string chainId)
        {
            try
            {
                var pactCode = $"({token}.details {accountName})";
                var meta = DumMeta(chainId);

                var payload = new
                {
                    pactCode,
                    meta
                };

                var response = await httpClient.PostAsync(host(chainId), new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error fetching balance for chain {chainId}: {response.StatusCode} - {response.ReasonPhrase}");
                    return 0;
                }

                var result = JsonConvert.DeserializeObject<PactResponse>(responseString).Result;

                if (result.Status == "success" && result.Data != null)
                {
                    return result.Data.Balance;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching balance for chain {chainId}: {e.Message}");
            }

            return 0;
        }

        private Meta DumMeta(string chainId)
        {
            return new Meta
            {
                ChainId = chainId,
                Sender = "not-real",
                GasPrice = 0.00000001,
                GasLimit = 6000,
                CreationTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() - 15,
                Ttl = 600
            };
        }
    }

    public class VersionInfo
    {
        public string NodeVersion { get; set; }
        public string[] Chains { get; set; }
    }

    public class NodeInfo
    {
        [JsonProperty("nodeApiVersion")]
        public string NodeApiVersion { get; set; }

        [JsonProperty("nodeVersion")]
        public string NodeVersion { get; set; }

        [JsonProperty("nodeChains")]
        public string[] NodeChains { get; set; }
    }

    public class CutInfo
    {
        [JsonProperty("hashes")]
        public Dictionary<string, HashInfo> Hashes { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class HashInfo
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }

    public class Meta
    {
        [JsonProperty("chainId")]
        public string ChainId { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("gasPrice")]
        public double GasPrice { get; set; }

        [JsonProperty("gasLimit")]
        public long GasLimit { get; set; }

        [JsonProperty("creationTime")]
        public long CreationTime { get; set; }

        [JsonProperty("ttl")]
        public long Ttl { get; set; }
    }

    public class PactResponse
    {
        [JsonProperty("result")]
        public PactResult Result { get; set; }
    }

    public class PactResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public PactData Data { get; set; }
    }

    public class PactData
    {
        [JsonProperty("balance")]
        public double Balance { get; set; }
    }
}
