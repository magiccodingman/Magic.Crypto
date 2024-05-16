using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Blazor.Core.Services
{
    public class BinanceApi
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private const string ApiUrl = "https://api.binance.us";
        private readonly HttpClient _httpClient;

        public BinanceApi(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _httpClient = new HttpClient();
        }

        public async Task<JObject> GetAccountInfoAsync()
        {
            var serverTime = await GetServerTimeAsync();
            var endpoint = "/api/v3/account";
            var requestUri = $"{ApiUrl}{endpoint}";

            var timestamp = serverTime;
            var queryString = $"timestamp={timestamp}";

            var signature = CreateSignature(queryString);
            var requestUriWithSignature = $"{requestUri}?{queryString}&signature={signature}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUriWithSignature);
            request.Headers.Add("X-MBX-APIKEY", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error retrieving account info: {await response.Content.ReadAsStringAsync()}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var accountInfo = JObject.Parse(content);
            return accountInfo;
        }

        private async Task<long> GetServerTimeAsync()
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}/api/v3/time");
            var content = await response.Content.ReadAsStringAsync();
            var serverTime = JObject.Parse(content)["serverTime"].ToObject<long>();
            return serverTime;
        }

        private string CreateSignature(string queryString)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_apiSecret);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
