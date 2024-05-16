using Crypto.Blazor.Core;
using Crypto.MauiBlazor.Service;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace Crypto.MauiBlazor
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            MagicCryptoServiceConfig.Configure(builder.Services);
            builder.Services.AddSingleton(new CoinMarketCapService(Path.Combine(AppContext.BaseDirectory, "coinmarketcap_data.json")));
            builder.Services.AddSingleton<SettingsService>();
            
            builder.Services.AddMudServices();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            var host = builder.Build();
            return host;
        }
    }
}
