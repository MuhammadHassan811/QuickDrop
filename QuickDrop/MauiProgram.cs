using Microsoft.Extensions.Logging;
using QuickDrop.Services;

namespace QuickDrop
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Inter-Regular.ttf", "Inter");
                    fonts.AddFont("Inter-Bold.ttf", "InterBold");
                    fonts.AddFont("MaterialSymbolsOutlined.ttf", "MaterialSymbols");
                    fonts.AddFont("MaterialSymbolsOutlined-Regular.ttf", "MaterialSymbols");
                });

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddTransient<Pages.HomePage>();
            builder.Services.AddTransient<Pages.CategoryPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}