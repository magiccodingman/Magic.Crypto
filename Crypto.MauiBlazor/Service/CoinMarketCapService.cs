using Crypto.MauiBlazor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.MauiBlazor.Service
{
    public class CoinMarketCapService
    {
        private readonly string _dataFilePath;
        private readonly HttpClient _httpClient;

        public CoinMarketCapService(string dataFilePath)
        {
            _dataFilePath = dataFilePath;
            _httpClient = new HttpClient();
        }

        public async Task<CoinMarketCapResponse> GetCryptoUsdListAsync(string apiKey)
        {
            var currentTime = DateTime.UtcNow;
            var lastCallData = await GetLastCallDataAsync();

            if (lastCallData != null && (currentTime - lastCallData._LastCall).TotalHours < 2)
            {
                return lastCallData.Obj;
            }

            var apiResponse = await MakeApiCallAsync(apiKey);
            await SaveResponseAsync(currentTime, apiResponse);
            return apiResponse;
        }

        private async Task<LastCall> GetLastCallDataAsync()
        {
            if (!File.Exists(_dataFilePath))
            {
                return null;
            }

            var fileContent = await File.ReadAllTextAsync(_dataFilePath);
            var jsonData = JObject.Parse(fileContent);
            return new LastCall
            {
                _LastCall = jsonData["lastCallTime"].ToObject<DateTime>(),
                Obj = jsonData["lastResponse"].ToObject<CoinMarketCapResponse>(),
            };
            //var lastCallTime = jsonData["lastCallTime"].ToObject<DateTime>();
            //var lastResponse = jsonData["lastResponse"].ToObject<CoinMarketCapResponse>();

            //return (lastCallTime, lastResponse);
        }

        private async Task<CoinMarketCapResponse> MakeApiCallAsync(string apiKey)
        {
            var requestUri = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest?start=1&limit=5000&convert=USD";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("X-CMC_PRO_API_KEY", apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error retrieving crypto USD list: {await response.Content.ReadAsStringAsync()}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CoinMarketCapResponse>(content);
        }

        private async Task SaveResponseAsync(DateTime callTime, CoinMarketCapResponse response)
        {
            var data = new JObject
        {
            { "lastCallTime", callTime },
            { "lastResponse", JObject.FromObject(response) }
        };

            var jsonData = data.ToString();
            await File.WriteAllTextAsync(_dataFilePath, jsonData);
        }
    }

    public class LastCall
    {
        public DateTime _LastCall { get; set; }
        public CoinMarketCapResponse Obj { get; set; }
    }
}
