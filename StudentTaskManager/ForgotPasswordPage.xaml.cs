
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace StudentTaskManager;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly HttpClient _httpClient;

    // Replace this with the actual forgot-password API route
    // implemented by your backend.
   
      private const string ApiUrl =
    "http://localhost:5108/api/mobile/forgot-password";

    public ForgotPasswordPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
    }

    private bool IsValidEmail(string email)
    {
        return new EmailAddressAttribute().IsValid(email);
    }

    private void Email_TextChanged(object sender, TextChangedEventArgs e)
    {
        emailErrorLabel.IsVisible = false;
        errorLabel.IsVisible = false;

        emailBorder.Stroke = emailEntry.IsFocused
            ? Color.FromArgb("#0D6EFD")
            : Color.FromArgb("#D5D5D5");
    }

    private void Email_Focused(object sender, FocusEventArgs e)
    {
        emailBorder.Stroke = Color.FromArgb("#0D6EFD");
    }

    private void Email_Unfocused(object sender, FocusEventArgs e)
    {
        emailBorder.Stroke = emailErrorLabel.IsVisible
            ? Color.FromArgb("#DC3545")
            : Color.FromArgb("#D5D5D5");
    }

    private async void SendResetLink_Clicked(
        object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim() ?? "";

        emailErrorLabel.IsVisible = false;
        errorLabel.IsVisible = false;

        emailBorder.Stroke = Color.FromArgb("#D5D5D5");

        if (string.IsNullOrWhiteSpace(email))
        {
            emailErrorLabel.Text = "Email address is required.";
            emailErrorLabel.IsVisible = true;
            emailBorder.Stroke = Color.FromArgb("#DC3545");
            return;
        }

        if (!IsValidEmail(email))
        {
            emailErrorLabel.Text = "Please enter a valid email address.";
            emailErrorLabel.IsVisible = true;
            emailBorder.Stroke = Color.FromArgb("#DC3545");
            return;
        }

        try
        {
            sendResetButton.IsEnabled = false;
            sendResetButton.Text = "Sending...";

            var request = new
            {
                email = email
            };

            var response = await _httpClient.PostAsJsonAsync(
                ApiUrl, request);

            if (response.IsSuccessStatusCode)
            {
                // Show success toast only after API success.
                successToast.IsVisible = true;
                await successToast.FadeTo(1, 250);

                await Task.Delay(3000);

                await successToast.FadeTo(0, 250);
                successToast.IsVisible = false;
            }
            else
            {
                errorLabel.Text =
                    "Unable to send reset link. Please try again.";

                errorLabel.IsVisible = true;
            }
        }
        catch (Exception)
        {
            errorLabel.Text =
                "Unable to connect to the server. Please try again.";

            errorLabel.IsVisible = true;
        }
        finally
        {
            sendResetButton.IsEnabled = true;
            sendResetButton.Text = "Send Reset Link";
        }
    }


    private async void BackToLogin_Clicked(
        object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(LoginPage));
    }
}
