using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class EditCoursePage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    private const string ApiUrl =
        "http://localhost:5108/api/mobile/admin/courses";

    private int _courseId;

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    public event EventHandler? CourseSaved;
    public event EventHandler? CancelRequested;

    public EditCoursePage()
    {
        InitializeComponent();
    }

    // LOAD SELECTED COURSE
    public async Task LoadCourseDetails(int id)
    {
        _courseId = id;

        try
        {
            if (_courseId <= 0)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    "Invalid course ID.",
                    "OK");
                return;
            }

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

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{ApiUrl}/{_courseId}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var course =
                    JsonSerializer.Deserialize<CourseDetailsResponse>(
                        responseBody, jsonOptions);

                if (course == null)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Error",
                        "Unable to read course details.",
                        "OK");
                    return;
                }

                courseNameEntry.Text = course.CourseName;
                descriptionEditor.Text = course.Description;
                durationEntry.Text = course.Duration;
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    "Unable to load course: " + responseBody,
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

    // SAVE CHANGES
    private async void SaveChanges_Clicked(
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

        if (_courseId <= 0)
        {
            await Shell.Current.DisplayAlertAsync(
                "Error",
                "Invalid course ID.",
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

            var updatedCourse = new
            {
                CourseName = courseName,
                Description = description,
                Duration = duration
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"{ApiUrl}/{_courseId}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(updatedCourse);

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
                    "Update Failed",
                    "HTTP Status: " + (int)response.StatusCode +
                    "\n" + responseBody,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    // CANCEL BUTTON
    private void Cancel_Clicked(
        object sender, EventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    // RESPONSE MODEL
    private class CourseDetailsResponse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Duration { get; set; } = "";
    }
}