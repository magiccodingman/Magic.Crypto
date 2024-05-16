using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Blazor.Core
{
    public class MagicCryptoServiceConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddScoped<CryptoService>();
        }
    }
}
