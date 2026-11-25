
using System.Net.Http.Json;
using System.Text.Json;

namespace StudentTaskManager;

[QueryProperty(nameof(Token), "token")]
public partial class ResetPasswordPage : ContentPage
{
    private readonly HttpClient _httpClient;

    private const string ApiUrl =
        "http://localhost:5108/api/mobile/forgot-password/reset";

    public string Token { get; set; } = string.Empty;

    public ResetPasswordPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
    }

    // RESET PASSWORD BUTTON
    private async void ResetPassword_Clicked(
        object sender, EventArgs e)
    {
        string newPassword = newPasswordEntry.Text?.Trim() ?? "";
        string confirmPassword = confirmPasswordEntry.Text?.Trim() ?? "";

        newPasswordErrorLabel.IsVisible = false;
        confirmPasswordErrorLabel.IsVisible = false;
        generalErrorLabel.IsVisible = false;

        // CHECK RESET TOKEN
        if (string.IsNullOrWhiteSpace(Token))
        {
            generalErrorLabel.Text =
                "Invalid or missing reset token.";

            generalErrorLabel.IsVisible = true;
            return;
        }

        // NEW PASSWORD VALIDATION
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            newPasswordErrorLabel.Text =
                "New password is required.";

            newPasswordErrorLabel.IsVisible = true;
            return;
        }

        if (newPassword.Length < 8 ||
            !newPassword.Any(char.IsUpper) ||
            !newPassword.Any(char.IsLower) ||
            !newPassword.Any(char.IsDigit) ||
            !newPassword.Any(c => !char.IsLetterOrDigit(c)))
        {
            newPasswordErrorLabel.Text =
                "Password must contain at least 8 characters, including uppercase, lowercase, number and special character.";

            newPasswordErrorLabel.IsVisible = true;
            return;
        }

        // CONFIRM PASSWORD VALIDATION
        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            confirmPasswordErrorLabel.Text =
                "Please confirm your password.";

            confirmPasswordErrorLabel.IsVisible = true;
            return;
        }

        if (newPassword != confirmPassword)
        {
            confirmPasswordErrorLabel.Text =
                "Passwords do not match.";

            confirmPasswordErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            resetButton.IsEnabled = false;
            resetButton.Text = "Resetting...";

            var request = new
            {
                token = Token,
                newPassword = newPassword,
                confirmPassword = confirmPassword
            };

            var response = await _httpClient.PostAsJsonAsync(
                ApiUrl, request);

            if (response.IsSuccessStatusCode)
            {
                // SHOW SUCCESS TOAST
                successToast.IsVisible = true;
                await successToast.FadeTo(1, 250);

                await Task.Delay(2000);

                await successToast.FadeTo(0, 250);
                successToast.IsVisible = false;

                // GO BACK TO LOGIN
                await Shell.Current.GoToAsync(
                    nameof(LoginPage));
            }
            else
            {
                string responseText =
                    await response.Content.ReadAsStringAsync();

                string message =
                    "Reset link is invalid or expired.";

                try
                {
                    using var json =
                        JsonDocument.Parse(responseText);

                    if (json.RootElement.TryGetProperty(
                        "message", out var messageElement))
                    {
                        message =
                            messageElement.GetString() ?? message;
                    }
                }
                catch (JsonException)
                {
                    // Use default error message.
                }

                generalErrorLabel.Text = message;
                generalErrorLabel.IsVisible = true;
            }
        }
        catch (Exception)
        {
            generalErrorLabel.Text =
                "Unable to connect to the server. Please try again.";

            generalErrorLabel.IsVisible = true;
        }
        finally
        {
            resetButton.IsEnabled = true;
            resetButton.Text = "Reset Password";
        }
    }

    // BACK TO LOGIN
    private async void BackToLogin_Clicked(
        object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(LoginPage));
    }
}
