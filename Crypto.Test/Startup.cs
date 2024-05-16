using Crypto.Core.WalletCheck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Test
{
    internal class Startup
    {
        private static readonly HttpClient client = new HttpClient();
        public async Task Main()
        {
            try
            {
                //var response1 = await new KadenaWallet().GetKdaBalanceAsync(2, "k:33f3d382bb505b384031eaf4d8d91e6e93fa84f539c8a27d2e2c4de9e51d330d");
                // var response = await new KadenaWallet().GetWalletBalances("api.chainweb.com", "k:33f3d382bb505b384031eaf4d8d91e6e93fa84f539c8a27d2e2c4de9e51d330d");

                //GetKdaBalancesAsync

                var server = "api.chainweb.com";  // You should fetch this from your UI or config
                var token = "coin";  // Ditto
                var account = "k:33f3d382bb505b384031eaf4d8d91e6e93fa84f539c8a27d2e2c4de9e51d330d";  // Ditto

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(account))
                {
                    Console.Error.WriteLine("Please make sure the server, token, and account fields are filled out.");
                    return;
                }

                var info = await GetVersionAsync(server);
                if (info == null)
                {
                    return;
                }

                string host = GetHost(server, info.NodeVersion, "2"); // Chain ID is hardcoded to "2"
                var balance = await GetBalanceAsync(host, token, account, "2");
                if (balance != null)
                {
                    Console.WriteLine($"Balance for chain 2: {balance}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private static async Task<VersionInfo> GetVersionAsync(string server)
        {
            try
            {
                var response = await client.GetStringAsync($"https://{server}/info");
                var json = JObject.Parse(response);
                string nodeVersion = json["nodeVersion"].ToString();
                return new VersionInfo { NodeVersion = nodeVersion };
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Unable to fetch version from server: {e}");
                return null;
            }
        }

        private static string GetHost(string server, string nodeVersion, string chainId)
        {
            return $"https://{server}/chainweb/0.0/{nodeVersion}/chain/{chainId}/pact";
        }

        private static async Task<decimal?> GetBalanceAsync(string host, string token, string account, string chainId)
        {
            try
            {
                string pactCode = $"({token}.details \"{account}\")";
                var meta = CreateMeta(chainId);

                var payload = new
                {
                    pactCode = pactCode,
                    meta = meta
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(host, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var resultJson = JObject.Parse(responseBody);

                var result = resultJson["result"];
                if (result != null && result["data"] != null)
                {
                    if (result["data"]["balance"] != null)
                    {
                        if (result["data"]["balance"].Type == JTokenType.Float)
                        {
                            return result["data"]["balance"].Value<decimal>();
                        }
                        if (result["data"]["balance"]["decimal"] != null)
                        {
                            return result["data"]["balance"]["decimal"].Value<decimal>();
                        }
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error fetching balance: {e}");
                return null;
            }
        }

        private static object CreateMeta(string chainId)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new
            {
                sender = "not-real",
                chainId = chainId,
                gasPrice = 0.00000001,
                gasLimit = 6000,
                creationTime = currentTime - 15,
                ttl = 600
            };
        }

        private class VersionInfo
        {
            public string NodeVersion { get; set; }
        }
    }
}
