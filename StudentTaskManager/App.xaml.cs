
using System.Web;

namespace StudentTaskManager;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    // RECEIVE PASSWORD RESET LINK
    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        // Check the incoming link
        if (uri.Scheme != "studenttaskmanager" ||
            uri.Host != "reset-password")
        {
            return;
        }

        // Extract reset token
        string? token = HttpUtility
            .ParseQueryString(uri.Query)
            .Get("token");

        // Perform navigation on the UI thread
        await Dispatcher.DispatchAsync(async () =>
        {
            var shell = Windows.FirstOrDefault()?.Page as Shell;

            if (shell == null)
                return;

            if (string.IsNullOrWhiteSpace(token))
            {
                await shell.DisplayAlertAsync(
                    "Invalid Link",
                    "The password reset link is invalid.",
                    "OK");

                return;
            }

            // Navigate to Reset Password page
            await shell.GoToAsync(
                $"{nameof(ResetPasswordPage)}?token={Uri.EscapeDataString(token)}");
        });
    }
}
