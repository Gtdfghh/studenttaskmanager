
namespace StudentTaskManager;

public partial class AppHeader : ContentView
{
    public AppHeader()
    {
        InitializeComponent();
    }

    // HOME NAVIGATION

    private async void Home_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PopToRootAsync();
    }

    // LOGIN NAVIGATION
    private async void Login_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }

    // REGISTER NAVIGATION
    private async void Register_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegistrationPage));
    }

    // HOME HOVER
    private void Home_PointerEntered(object sender, PointerEventArgs e)
    {
        homeButton.TextColor = Color.FromArgb("#0D6EFD");
    }

    private void Home_PointerExited(object sender, PointerEventArgs e)
    {
        homeButton.TextColor = Color.FromArgb("#333333");
    }

    // LOGIN HOVER
    private void Login_PointerEntered(object sender, PointerEventArgs e)
    {
        loginButton.TextColor = Color.FromArgb("#0D6EFD");
    }

    private void Login_PointerExited(object sender, PointerEventArgs e)
    {
        loginButton.TextColor = Color.FromArgb("#333333");
    }

    // REGISTER HOVER
    private void Register_PointerEntered(object sender, PointerEventArgs e)
    {
        registerButton.TextColor = Color.FromArgb("#0D6EFD");
    }

    private void Register_PointerExited(object sender, PointerEventArgs e)
    {
        registerButton.TextColor = Color.FromArgb("#333333");
    }
}
