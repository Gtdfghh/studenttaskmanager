using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StudentTaskManager;

public partial class AddCoursePage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    private const string ApiUrl =
        "http://localhost:5108/api/mobile/admin/courses";

    public event EventHandler? CourseSaved;
    public event EventHandler? CancelRequested;

    public AddCoursePage()
    {
        InitializeComponent();
    }

    private async void AddCourse_Clicked(
        object sender, EventArgs e)
    {
        string courseName =
            courseNameEntry.Text?.Trim() ?? "";

        string description =
            descriptionEditor.Text?.Trim() ?? "";

        string duration =
            durationEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(courseName))
        {
            await Shell.Current.DisplayAlertAsync(
                "Validation Error",
                "Please enter the course name.",
                "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            await Shell.Current.DisplayAlertAsync(
                "Validation Error",
                "Please enter the course description.",
                "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(duration))
        {
            await Shell.Current.DisplayAlertAsync(
                "Validation Error",
                "Please enter the course duration.",
                "OK");
            return;
        }

        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Login Required",
                    "Please log in again as Admin.",
                    "OK");
                return;
            }

            var courseData = new
            {
                CourseName = courseName,
                Description = description,
                Duration = duration
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post, ApiUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(courseData);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
               

                CourseSaved?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Add Course Failed",
                    "HTTP Status: " + (int)response.StatusCode +
                    "\n\nServer Response:\n" +
                    (string.IsNullOrWhiteSpace(responseBody)
                        ? "The server returned an empty response."
                        : responseBody),
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Connection Error",
                ex.Message,
                "OK");
        }
    }

    private void Cancel_Clicked(
        object sender, EventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}