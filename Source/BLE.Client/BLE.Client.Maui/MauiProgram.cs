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
		if (IsAndroid)
		{
			AddAndroidSpecificItems(builder);
        }

#if DEBUG
        //builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

    public static bool IsAndroid => DeviceInfo.Current.Platform == DevicePlatform.Android;

    private static void AddAndroidSpecificItems(MauiAppBuilder builder)
    {
#if ANDROID
        builder.Services.AddSingleton<IPlatformHelpers, DroidPlatformHelpers>();
#endif

	}
}

