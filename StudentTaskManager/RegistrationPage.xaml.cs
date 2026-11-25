
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace StudentTaskManager;

public partial class RegistrationPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public RegistrationPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
    }

    // EMAIL VALIDATION
    private bool IsValidEmail(string email)
    {
        return new EmailAddressAttribute().IsValid(email);
    }

    // PASSWORD VALIDATION
    private bool IsValidPassword(string password)
    {
        return password.Length >= 8
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(c => !char.IsLetterOrDigit(c));
    }

    // EMAIL TEXT CHANGED
    private void Email_TextChanged(object sender, TextChangedEventArgs e)
    {
        emailErrorLabel.IsVisible = false;
        emailErrorLabel.Text = "";
        registerErrorLabel.IsVisible = false;

        emailBorder.Stroke = emailEntry.IsFocused
            ? Color.FromArgb("#0D6EFD")
            : Color.FromArgb("#CED4DA");
    }

    // PASSWORD TEXT CHANGED
    private void Password_TextChanged(object sender, TextChangedEventArgs e)
    {
        passwordErrorLabel.IsVisible = false;
        passwordErrorLabel.Text = "";
        registerErrorLabel.IsVisible = false;

        passwordBorder.Stroke = passwordEntry.IsFocused
            ? Color.FromArgb("#0D6EFD")
            : Color.FromArgb("#CED4DA");

        // Recheck confirm password when password changes
        if (!string.IsNullOrEmpty(confirmPasswordEntry.Text))
        {
            if (passwordEntry.Text != confirmPasswordEntry.Text)
            {
                confirmPasswordErrorLabel.Text = "Passwords do not match.";
                confirmPasswordErrorLabel.IsVisible = true;
                confirmPasswordBorder.Stroke = Color.FromArgb("#DC3545");
            }
            else
            {
                confirmPasswordErrorLabel.IsVisible = false;
                confirmPasswordErrorLabel.Text = "";

                confirmPasswordBorder.Stroke = confirmPasswordEntry.IsFocused
                    ? Color.FromArgb("#0D6EFD")
                    : Color.FromArgb("#CED4DA");
            }
        }
    }

    // CONFIRM PASSWORD TEXT CHANGED
    private void ConfirmPassword_TextChanged(object sender, TextChangedEventArgs e)
    {
        confirmPasswordErrorLabel.IsVisible = false;
        confirmPasswordErrorLabel.Text = "";
        registerErrorLabel.IsVisible = false;

        confirmPasswordBorder.Stroke = confirmPasswordEntry.IsFocused
            ? Color.FromArgb("#0D6EFD")
            : Color.FromArgb("#CED4DA");
    }

    // EMAIL FOCUS
    private void Email_Focused(object sender, FocusEventArgs e)
    {
        emailBorder.Stroke = Color.FromArgb("#0D6EFD");
    }

    private void Email_Unfocused(object sender, FocusEventArgs e)
    {
        emailBorder.Stroke = emailErrorLabel.IsVisible
            ? Color.FromArgb("#DC3545")
            : Color.FromArgb("#CED4DA");
    }

    // PASSWORD FOCUS
    private void Password_Focused(object sender, FocusEventArgs e)
    {
        passwordBorder.Stroke = Color.FromArgb("#0D6EFD");
    }

    private void Password_Unfocused(object sender, FocusEventArgs e)
    {
        passwordBorder.Stroke = passwordErrorLabel.IsVisible
            ? Color.FromArgb("#DC3545")
            : Color.FromArgb("#CED4DA");
    }

    // CONFIRM PASSWORD FOCUS
    private void ConfirmPassword_Focused(object sender, FocusEventArgs e)
    {
        confirmPasswordBorder.Stroke = Color.FromArgb("#0D6EFD");
    }

    private void ConfirmPassword_Unfocused(object sender, FocusEventArgs e)
    {
        confirmPasswordBorder.Stroke = confirmPasswordErrorLabel.IsVisible
            ? Color.FromArgb("#DC3545")
            : Color.FromArgb("#CED4DA");
    }

    // SHOW / HIDE PASSWORD
    private void TogglePasswordVisibility(object sender, EventArgs e)
    {
        passwordEntry.IsPassword = !passwordEntry.IsPassword;

        passwordToggleButton.Text = passwordEntry.IsPassword
            ? "⊘"
            : "◉";
    }

    // SHOW / HIDE CONFIRM PASSWORD
    private void ToggleConfirmPasswordVisibility(object sender, EventArgs e)
    {
        confirmPasswordEntry.IsPassword = !confirmPasswordEntry.IsPassword;

        confirmPasswordToggleButton.Text = confirmPasswordEntry.IsPassword
            ? "⊘"
            : "◉";
    }

    // GENERAL ERROR MESSAGE
    private void ShowRegisterError(string message)
    {
        registerErrorLabel.Text = "• " + message;
        registerErrorLabel.IsVisible = true;
    }

    // REGISTER BUTTON
    private async void Register_Clicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim() ?? "";
        string password = passwordEntry.Text ?? "";
        string confirmPassword = confirmPasswordEntry.Text ?? "";

        // Clear previous errors
        emailErrorLabel.IsVisible = false;
        passwordErrorLabel.IsVisible = false;
        confirmPasswordErrorLabel.IsVisible = false;
        registerErrorLabel.IsVisible = false;

        emailBorder.Stroke = Color.FromArgb("#CED4DA");
        passwordBorder.Stroke = Color.FromArgb("#CED4DA");
        confirmPasswordBorder.Stroke = Color.FromArgb("#CED4DA");

        bool isValid = true;

        // EMAIL VALIDATION
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

        // PASSWORD VALIDATION
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

        // CONFIRM PASSWORD VALIDATION
        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            confirmPasswordErrorLabel.Text = "Please confirm your password.";
            confirmPasswordErrorLabel.IsVisible = true;
            confirmPasswordBorder.Stroke = Color.FromArgb("#DC3545");
            isValid = false;
        }
        else if (password != confirmPassword)
        {
            confirmPasswordErrorLabel.Text = "Passwords do not match.";
            confirmPasswordErrorLabel.IsVisible = true;
            confirmPasswordBorder.Stroke = Color.FromArgb("#DC3545");
            isValid = false;
        }

        // Stop if validation fails
        if (!isValid)
            return;

        try
        {
            registerButton.IsEnabled = false;
            registerButton.Text = "Creating Account...";

            // Registration data
            var registerData = new
            {
                email = email,
                password = password,
                confirmPassword = confirmPassword
            };

            // Backend API
            string apiUrl = "http://localhost:5108/api/mobile/register";

            var response = await _httpClient.PostAsJsonAsync(
                apiUrl,
                registerData);

            string responseContent =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result =
                    await response.Content.ReadFromJsonAsync<RegisterResponse>();

                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                {
                    ShowRegisterError(
                        "Registration succeeded, but token was not received.");
                    return;
                }

                // Save JWT token
                await SecureStorage.SetAsync("auth_token", result.Token);

                // Save user information
                await SecureStorage.SetAsync(
                    "user_id",
                    result.UserId.ToString());

                await SecureStorage.SetAsync(
                    "user_role",
                    result.Role);

               

                await Shell.Current.GoToAsync(nameof(StudentDetailsPage));
            }
            else
            {
                ShowRegisterError(responseContent);
            }
        }
        catch (Exception)
        {
            ShowRegisterError(
                "Unable to connect to the server. Please try again.");
        }
        finally
        {
            registerButton.IsEnabled = true;
            registerButton.Text = "＋  Create Account";
        }
    }

    // NAVIGATE TO LOGIN
    private async void Login_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}
