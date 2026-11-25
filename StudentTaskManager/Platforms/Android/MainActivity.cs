
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace StudentTaskManager;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize |
                           ConfigChanges.Density,
    Exported = true)]

[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[]
    {
        Intent.CategoryDefault,
        Intent.CategoryBrowsable
    },
    DataScheme = "studenttaskmanager",
    DataHost = "reset-password")]

public class MainActivity : MauiAppCompatActivity
{
}
