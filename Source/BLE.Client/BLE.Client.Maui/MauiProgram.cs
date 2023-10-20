using BLE.Client.Helpers;
using BLE.Client.Maui.Services;
namespace BLE.Client.Maui;

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
			});
        builder.Services.AddSingleton<IAlertService, AlertService>();

        AddPlatformSpecificItems(builder);



#if DEBUG
        //builder.Logging.AddDebug();
#endif

        return builder.Build();
	}

    public static bool IsAndroid => DeviceInfo.Current.Platform == DevicePlatform.Android;

    public static bool IsMacCatalyst => DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst;

    public static bool IsMacOS => DeviceInfo.Current.Platform == DevicePlatform.macOS;


    private static void AddPlatformSpecificItems(MauiAppBuilder builder)
    {
#if ANDROID
        builder.Services.AddSingleton<IPlatformHelpers, DroidPlatformHelpers>();
#elif IOS
        builder.Services.AddSingleton<IPlatformHelpers, iOSPlatformHelpers>();
#elif MACCATALYST
        builder.Services.AddSingleton<IPlatformHelpers, MacCatalystPlatformHelpers>();
#elif WINDOWS
        builder.Services.AddSingleton<IPlatformHelpers, WindowsPlatformHelpers>();
#endif
    }
}
