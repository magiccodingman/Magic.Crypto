using Crypto.Blazor.Core.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Blazor.Core
{
    public class CryptoService
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference _kdaModule;
        private readonly string _Server;

        /// <summary>
        /// Actual limit is 50 per 1 second. I chose to be extra conservative.
        /// </summary>
        private readonly int _rateLimit = 40; // Kadena ChainWeb API rate limit
        private readonly int _rateLimitInterval = 1300; // Interval in milliseconds
        private readonly ConcurrentQueue<DateTime> _requestTimestamps;
        private readonly SemaphoreSlim _semaphore;

        public CryptoService(IJSRuntime jsRuntime, string server = "api.chainweb.com")
        {
            _jsRuntime = jsRuntime;
            _Server = server;
            _requestTimestamps = new ConcurrentQueue<DateTime>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        private KdaVersion _KdaVersion { get; set; }

        private async Task InitializeAsync(string server)
        {
            _kdaModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Crypto.Blazor.Core/js/kdaBalanceChecker.js");
            await _kdaModule.InvokeAsync<object>("loadDependencies");
            _KdaVersion = await _kdaModule.InvokeAsync<KdaVersion>("getVersion", _Server);
        }

        public async Task UpdateKdaVersion()
        {
            await EnforceRateLimit();
            var kdaModule = await GetKdaModule();
            _KdaVersion = await kdaModule.InvokeAsync<KdaVersion>("getVersion", _Server);
        }

        public async Task<List<KdaWallet>> GetKdaWalletAsync(IEnumerable<string> walletAddr)
        {
            List<KdaWallet> wallets = new List<KdaWallet>();
            foreach (var waddress in walletAddr)
            {
                var wallet = await GetKdaWalletAsync(waddress);
                if (wallet != null)
                    wallets.Add(wallet);
            }
            return wallets;
        }
        public async Task<KdaWallet> GetKdaWalletAsync(string walletAddr)
        {
            var walletBalancesList = await GetKdaAllChainBalancesAsync(walletAddr);
            var wallet = new KdaWallet()
            {
                Address = walletAddr,
                ChainBalances = walletBalancesList
            };

            return wallet;
        }

        private async Task<List<KdaChainBalance>> GetKdaAllChainBalancesAsync(string walletAddr)
        {
            var kdaVersion = await GetKdaVersion();
            var tasks = kdaVersion.chainIds.Select(chain => GetKdaChainBalance(walletAddr, chain));
            var balances = await Task.WhenAll(tasks);

            return balances?.ToList() ?? new List<KdaChainBalance>();
        }

        public async Task<KdaChainBalance> GetKdaChainBalance(string walletAddr, int chainId)
        {
            return await GetKdaChainBalance(walletAddr, chainId.ToString());
        }

        public async Task<KdaChainBalance> GetKdaChainBalance(string walletAddr, string chainIdString)
        {
            await EnforceRateLimit();
            var kdaModule = await GetKdaModule();
            var result = await kdaModule.InvokeAsync<KdaChainBalanceResponse>("getChainBalanceResponse",
                _Server, (await GetKdaVersion()).nv,
                walletAddr, chainIdString);

            return new KdaChainBalance()
            {
                ChainIdString = chainIdString,
                Balance = result?.data?.balance??0
            };
        }

        private async Task<KdaVersion> GetKdaVersion()
        {
            await InitCryptoJs();
            return _KdaVersion;
        }

        private async Task<IJSObjectReference> GetKdaModule()
        {
            await InitCryptoJs();
            return _kdaModule;
        }

        private async Task InitCryptoJs(string server = "api.chainweb.com")
        {
            if (_kdaModule == null || _KdaVersion == null)
                await InitializeAsync(server);
        }

        private async Task EnforceRateLimit()
        {
            await _semaphore.WaitAsync();
            try
            {
                DateTime now = DateTime.UtcNow;
                _requestTimestamps.Enqueue(now);
                DateTime oldest;
                // Remove timestamps older than the interval
                while (_requestTimestamps.TryPeek(out oldest) && (now - oldest).TotalMilliseconds > _rateLimitInterval)
                {
                    _requestTimestamps.TryDequeue(out _);
                }

                // If we've reached the limit, delay
                if (_requestTimestamps.Count > _rateLimit)
                {
                    var delayStartTime = DateTime.UtcNow;
                    Console.WriteLine($"Rate limit hit at {delayStartTime}. Delaying...");

                    // Calculate how long to wait until we can proceed
                    var delay = _rateLimitInterval - (now - oldest).TotalMilliseconds;
                    await Task.Delay((int)delay);

                    var delayEndTime = DateTime.UtcNow;
                    var delayDuration = delayEndTime - delayStartTime;
                    Console.WriteLine($"Delay ended at {delayEndTime}. Total delay duration: {delayDuration.TotalMilliseconds} ms");

                    _requestTimestamps.Enqueue(DateTime.UtcNow); // Add new timestamp after delay
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
