
namespace StudentTaskManager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(LoginPage),
            typeof(LoginPage));

        Routing.RegisterRoute(
            nameof(RegistrationPage),
            typeof(RegistrationPage));

        Routing.RegisterRoute(
            nameof(StudentDetailsPage),
            typeof(StudentDetailsPage));

        Routing.RegisterRoute(
            nameof(StudentDashboardPage),
            typeof(StudentDashboardPage));

        Routing.RegisterRoute(
            nameof(EditProfilePage),
            typeof(EditProfilePage));

        Routing.RegisterRoute(
            nameof(AdminDashboardPage),
            typeof(AdminDashboardPage));

        Routing.RegisterRoute(
            nameof(ForgotPasswordPage),
            typeof(ForgotPasswordPage));

        // RESET PASSWORD PAGE
        Routing.RegisterRoute(
            nameof(ResetPasswordPage),
            typeof(ResetPasswordPage));
    }
}
