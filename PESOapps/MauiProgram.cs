using Microsoft.Extensions.Logging;
using PESOapps.Services;
using PESOapps.Shared.Services;

namespace PESOapps
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

            // Add device-specific services used by the PESOapps.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

#if ANDROID
            // For development ONLY: accept all SSL certificates on Android
            builder.Services.AddScoped(sp => new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            }));
#else
            // Default for Windows, iOS, Mac
            builder.Services.AddScoped(sp => new HttpClient());
#endif

            return builder.Build();
        }
    }
}
