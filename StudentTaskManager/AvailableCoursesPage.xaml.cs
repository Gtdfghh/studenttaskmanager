
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class AvailableCoursesPage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    private const string ApiUrl =
        "http://localhost:5108/api/mobile/courses";

    private const string EnrollmentApiUrl =
        "http://localhost:5108/api/mobile/enrollments";

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    // ASP.NET STYLE CARD COLOURS
    private readonly string[] cardColors =
    {
        "#20BF62", // Green
        "#FFCA18", // Yellow
        "#60A5FA"  // Blue
    };

    public AvailableCoursesPage()
    {
        InitializeComponent();
    }

    // LOAD COURSES
    public async Task LoadCourses(string searchString = "")
    {
        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                ShowEnrollmentMessage(
                    "Please log in again.", false);
                return;
            }

            string requestUrl = ApiUrl;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                requestUrl += "?searchString=" +
                    Uri.EscapeDataString(searchString);
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get, requestUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var courses =
                    await response.Content.ReadFromJsonAsync<
                        List<AvailableCourseResponse>>(jsonOptions);

                DisplayCourses(
                    courses ?? new List<AvailableCourseResponse>());
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                ShowEnrollmentMessage(
                    "Unable to load courses: " + error, false);
            }
        }
        catch (Exception ex)
        {
            ShowEnrollmentMessage(
                "Connection Error: " + ex.Message, false);
        }
    }

    // SHOW SUCCESS OR ERROR MESSAGE
    private void ShowEnrollmentMessage(
        string message, bool success)
    {
        enrollmentMessageLabel.Text = message;

        if (success)
        {
            enrollmentMessageBorder.BackgroundColor =
                Color.FromArgb("#D1FAE5");

            enrollmentMessageBorder.Stroke =
                Color.FromArgb("#86EFAC");

            enrollmentMessageLabel.TextColor =
                Color.FromArgb("#166534");
        }
        else
        {
            enrollmentMessageBorder.BackgroundColor =
                Color.FromArgb("#FEE2E2");

            enrollmentMessageBorder.Stroke =
                Color.FromArgb("#FECACA");

            enrollmentMessageLabel.TextColor =
                Color.FromArgb("#DC2626");
        }

        enrollmentMessageBorder.IsVisible = true;
    }

    // DISPLAY COURSE CARDS
    private void DisplayCourses(
        List<AvailableCourseResponse> courses)
    {
        coursesContainer.Children.Clear();

        emptyStateBorder.IsVisible = courses.Count == 0;

        for (int i = 0; i < courses.Count; i++)
        {
            var course = courses[i];

            Color cardColor = Color.FromArgb(
                cardColors[i % cardColors.Length]);

            // COURSE NAME
            var nameLabel = new Label
            {
                Text = course.CourseName,
                FontSize = 28,
                TextColor = Color.FromArgb("#172033"),
                FontAttributes = FontAttributes.Bold
            };

            // COURSE DESCRIPTION
            var descriptionLabel = new Label
            {
                Text = course.Description,
                FontSize = 15,
                TextColor = Color.FromArgb("#172033"),
                LineBreakMode = LineBreakMode.WordWrap
            };

            // COURSE DURATION
            var durationLabel = new Label
            {
                Text = "Duration: " + course.Duration,
                FontSize = 15,
                TextColor = Color.FromArgb("#172033"),
                FontAttributes = FontAttributes.Bold
            };

            // ENROLL BUTTON
            var enrollButton = new Button
            {
                Text = "Enroll",
                BackgroundColor = Color.FromArgb("#1877ED"),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 58,
                CornerRadius = 7
            };

            enrollButton.Clicked += async (sender, e) =>
            {
                await Enroll_Clicked(course, enrollButton);
            };

            // CARD LAYOUT
            var cardContent = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition
                    {
                        Height = GridLength.Auto
                    },
                    new RowDefinition
                    {
                        Height = GridLength.Star
                    },
                    new RowDefinition
                    {
                        Height = GridLength.Auto
                    },
                    new RowDefinition
                    {
                        Height = GridLength.Auto
                    }
                },

                RowSpacing = 14
            };

            cardContent.Add(nameLabel, 0, 0);
            cardContent.Add(descriptionLabel, 0, 1);
            cardContent.Add(durationLabel, 0, 2);
            cardContent.Add(enrollButton, 0, 3);

            // COURSE CARD

            var card = new Border
            {
                Content = cardContent,

                BackgroundColor = cardColor,

                Stroke = Colors.Transparent,
                StrokeThickness = 0,

                Padding = new Thickness(30),

                WidthRequest = 307,
                HeightRequest = 295,

                Margin = new Thickness(0, 0, 28, 20),

                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(12)
                },

                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Offset = new Point(0, 3),
                    Radius = 6,
                    Opacity = 0.12f
                }
            };

            coursesContainer.Children.Add(card);
        }
    }

    // ENROLL IN COURSE
    private async Task Enroll_Clicked(
        AvailableCourseResponse course,
        Button enrollButton)
    {
        try
        {
            enrollmentMessageBorder.IsVisible = false;

            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                ShowEnrollmentMessage(
                    "Please log in again.", false);
                return;
            }

            if (course.CourseId <= 0)
            {
                ShowEnrollmentMessage(
                    "Invalid course ID.", false);
                return;
            }

            enrollButton.IsEnabled = false;
            enrollButton.Text = "Enrolling...";

            using var request = new HttpRequestMessage(
                HttpMethod.Post, EnrollmentApiUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(new
            {
                courseId = course.CourseId
            });

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            string responseContent =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                ShowEnrollmentMessage(
                    "You have successfully enrolled in the course.",
                    true);
            }
            else if (responseContent.Contains(
                "already enrolled",
                StringComparison.OrdinalIgnoreCase))
            {
                ShowEnrollmentMessage(
                    "You are already enrolled in this course.",
                    false);
            }
            else
            {
                ShowEnrollmentMessage(
                    string.IsNullOrWhiteSpace(responseContent)
                        ? "Enrollment failed. Please try again."
                        : responseContent,
                    false);
            }
        }
        catch (Exception ex)
        {
            ShowEnrollmentMessage(
                "Unable to enroll: " + ex.Message,
                false);
        }
        finally
        {
            enrollButton.IsEnabled = true;
            enrollButton.Text = "Enroll";
        }
    }

    // SEARCH COURSES
    private async void Search_Clicked(
        object sender, EventArgs e)
    {
        enrollmentMessageBorder.IsVisible = false;

        await LoadCourses(
            searchEntry.Text?.Trim() ?? "");
    }
}

// COURSE RESPONSE MODEL
public class AvailableCourseResponse
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = "";

    public string Description { get; set; } = "";

    public string Duration { get; set; } = "";
}
