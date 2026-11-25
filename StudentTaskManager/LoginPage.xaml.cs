
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

namespace StudentTaskManager;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public LoginPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
    }

    // EMAIL: WHEN USER STARTS TYPING
    private void Email_TextChanged(object sender, TextChangedEventArgs e)
    {
        emailErrorLabel.IsVisible = false;
        emailErrorLabel.Text = "";

        loginErrorLabel.IsVisible = false;

        emailBorder.Stroke = emailEntry.IsFocused
            ? Color.FromArgb("#0D6EFD")
            : Color.FromArgb("#D5D5D5");
    }

    // PASSWORD: WHEN USER STARTS TYPING
    private void Password_TextChanged(object sender, TextChangedEventArgs e)
    {
        passwordErrorLabel.IsVisible = false;
        passwordErrorLabel.Text = "";

        loginErrorLabel.IsVisible = false;

        passwordBorder.Stroke = passwordEntry.IsFocused
            ? Color.FromArgb("#0D6EFD")
            : Color.FromArgb("#D5D5D5");
    }

    // EMAIL FOCUS
    private void Email_Focused(object sender, FocusEventArgs e)
    {
        emailBorder.Stroke = Color.FromArgb("#0D6EFD");
    }

    // EMAIL UNFOCUS
    private void Email_Unfocused(object sender, FocusEventArgs e)
    {
        emailBorder.Stroke = emailErrorLabel.IsVisible
            ? Color.FromArgb("#DC3545")
            : Color.FromArgb("#D5D5D5");
    }

    // PASSWORD FOCUS
    private void Password_Focused(object sender, FocusEventArgs e)
    {
        passwordBorder.Stroke = Color.FromArgb("#0D6EFD");
    }

    // PASSWORD UNFOCUS
    private void Password_Unfocused(object sender, FocusEventArgs e)
    {
        passwordBorder.Stroke = passwordErrorLabel.IsVisible
            ? Color.FromArgb("#DC3545")
            : Color.FromArgb("#D5D5D5");
    }

    // EMAIL FORMAT VALIDATION
    private bool IsValidEmail(string email)
    {
        return new EmailAddressAttribute().IsValid(email);
    }

    // PASSWORD VALIDATION
    // Minimum 8 characters:
    // Uppercase + lowercase + number + special character
    private bool IsValidPassword(string password)
    {
        return password.Length >= 8
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(c => !char.IsLetterOrDigit(c));
    }

    // SHOW ERROR MESSAGE
    private void ShowLoginError(string message)
    {
        loginErrorLabel.Text = "• " + message;
        loginErrorLabel.IsVisible = true;
    }

    // LOGIN BUTTON
    private async void Login_Clicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim() ?? "";
        string password = passwordEntry.Text ?? "";

        // CLEAR OLD ERRORS
        emailErrorLabel.IsVisible = false;
        passwordErrorLabel.IsVisible = false;
        loginErrorLabel.IsVisible = false;

        emailBorder.Stroke = Color.FromArgb("#D5D5D5");
        passwordBorder.Stroke = Color.FromArgb("#D5D5D5");

        bool isValid = true;

        // VALIDATE EMAIL
        if (string.IsNullOrWhiteSpace(email))
        {
            emailErrorLabel.Text = "Email is required.";
            emailErrorLabel.IsVisible = true;
            emailBorder.Stroke = Color.FromArgb("#DC3545");

            isValid = false;
        }
        else if (!IsValidEmail(email))
        {
            emailErrorLabel.Text = "Please enter a valid email address.";
            emailErrorLabel.IsVisible = true;
            emailBorder.Stroke = Color.FromArgb("#DC3545");

            isValid = false;
        }

        // VALIDATE PASSWORD
        if (string.IsNullOrWhiteSpace(password))
        {
            passwordErrorLabel.Text = "Password is required.";
            passwordErrorLabel.IsVisible = true;
            passwordBorder.Stroke = Color.FromArgb("#DC3545");

            isValid = false;
        }
        else if (!IsValidPassword(password))
        {
            passwordErrorLabel.Text =
                "Password must contain at least 8 characters, including uppercase, lowercase, number and special character.";

            passwordErrorLabel.IsVisible = true;
            passwordBorder.Stroke = Color.FromArgb("#DC3545");

            isValid = false;
        }

        if (!isValid)
        {
            return;
        }

        try
        {
            loginButton.IsEnabled = false;
            loginButton.Text = "Logging in...";

            var loginData = new
            {
                email = email,
                password = password,
                rememberMe = rememberMeCheckBox.IsChecked
            };

            string apiUrl =
                "http://localhost:5108/api/mobile/login";

            var response = await _httpClient.PostAsJsonAsync(
                apiUrl, loginData);

            string responseContent =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result =
                    JsonSerializer.Deserialize<LoginResponse>(
                        responseContent,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (result == null ||
                    string.IsNullOrWhiteSpace(result.Token))
                {
                    ShowLoginError("Login token was not received.");
                    return;
                }

                // SAVE AUTHENTICATION INFORMATION
                await SecureStorage.SetAsync(
                    "auth_token", result.Token);

                await SecureStorage.SetAsync(
                    "user_id", result.UserId.ToString());

                await SecureStorage.SetAsync(
                    "user_role", result.Role);

                // STUDENT LOGIN
                if (string.Equals(
                    result.Role,
                    "Student",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await Shell.Current.GoToAsync(
                        nameof(StudentDashboardPage));
                }

                // ADMIN LOGIN
                else if (string.Equals(
                    result.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await Shell.Current.GoToAsync(
                        nameof(AdminDashboardPage));
                }

                // OTHER ROLES
                else
                {
                    SecureStorage.Remove("auth_token");
                    SecureStorage.Remove("user_id");
                    SecureStorage.Remove("user_role");

                    ShowLoginError(
                        "This role does not have access to the mobile application.");
                }
            }
            else
            {
                ShowLoginError(
                    "Invalid email or password. Please try again.");
            }
        }
        catch (Exception)
        {
            ShowLoginError(
                "Unable to connect to the server. Please try again.");
        }
        finally
        {
            loginButton.IsEnabled = true;
            loginButton.Text = "Login";
        }
    }

    // SHOW / HIDE PASSWORD
    private void TogglePasswordVisibility(object sender, EventArgs e)
    {
        passwordEntry.IsPassword = !passwordEntry.IsPassword;

        // Crossed eye = hidden
        // Normal eye = visible
        passwordToggleButton.Text = passwordEntry.IsPassword
            ? "⊘"
            : "◉";
    }

    // FORGOT PASSWORD
    private async void ForgotPassword_Clicked(
     object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(ForgotPasswordPage));
    }


    // REGISTER
    private async void Register_Clicked(
        object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(RegistrationPage));
    }
}
