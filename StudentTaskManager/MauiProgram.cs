
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace StudentTaskManager
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
                })
                .ConfigureLifecycleEvents(lifecycle =>
                {
#if ANDROID
                    lifecycle.AddAndroid(android =>
                    {
                        android.OnCreate((activity, bundle) =>
                        {
                            HandleResetLink(activity.Intent);
                        });

                        android.OnNewIntent((activity, intent) =>
                        {
                            HandleResetLink(intent);
                        });
                    });
#endif
                });

            // HTTP CLIENT
            builder.Services.AddSingleton<HttpClient>();

            // PAGES
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<ForgotPasswordPage>();
            builder.Services.AddTransient<ResetPasswordPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

#if ANDROID
        private static void HandleResetLink(Android.Content.Intent? intent)
        {
            if (intent?.Action != Android.Content.Intent.ActionView)
                return;

            string? link = intent.Data?.ToString();

            if (string.IsNullOrWhiteSpace(link))
                return;

            Task.Run(() =>
            {
                if (Uri.TryCreate(link, UriKind.Absolute, out Uri? uri))
                {
                    App.Current?.SendOnAppLinkRequestReceived(uri);
                }
            });
        }
#endif
    }
}
