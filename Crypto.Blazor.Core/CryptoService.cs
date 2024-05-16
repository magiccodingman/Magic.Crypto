using Crypto.Blazor.Core.Models;
using Microsoft.JSInterop;
using System;
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
        public CryptoService(IJSRuntime jsRuntime, string server = "api.chainweb.com")
        {
            _jsRuntime = jsRuntime;
            _Server = server;
        }

        private KdaVersion _KdaVersion { get; set; }

        public async Task InitializeAsync(string server)
        {
            _kdaModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Crypto.Blazor.Core/js/kdaBalanceChecker.js");
            await _kdaModule.InvokeAsync<object>("loadDependencies");
            _KdaVersion = await _kdaModule.InvokeAsync<KdaVersion>("getVersion", _Server);
        }

        public async Task<List<KdaChainBalance>> GetBalancesAsync(string walletAddr)
        {
            await InitSafety();

            var tasks = _KdaVersion.chainIds.Select(chain => GetKdaChainBalance(walletAddr, chain));
            var balances = await Task.WhenAll(tasks);

            return balances?.ToList() ?? new List<KdaChainBalance>();
        }

        private async Task<KdaChainBalance> GetKdaChainBalance(string walletAddr, string chainIdString)
        {
            var result = await _kdaModule.InvokeAsync<KdaChainBalanceResponse>("getChainBalanceResponse",
                _Server, _KdaVersion.nv,
                walletAddr, chainIdString);

            return new KdaChainBalance()
            {
                ChainIdString = chainIdString,
                Balance = result?.data?.balance??0
            };
        }

        private async Task InitSafety(string server = "api.chainweb.com")
        {
            if (_kdaModule == null)
                await InitializeAsync(server);
        }
    }
}
