
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace StudentTaskManager;

public partial class MyEnrollmentsPage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    // CARD COLORS
    private readonly List<Color> cardColors = new()
    {
        Color.FromArgb("#FFC914"), // Yellow
        Color.FromArgb("#60A5FA"), // Blue
        Color.FromArgb("#22C55E"), // Green
        Color.FromArgb("#A78BFA")  // Purple
    };

    public MyEnrollmentsPage()
    {
        InitializeComponent();
    }

    // LOAD ENROLLMENTS FROM DATABASE
    public async Task LoadEnrollments(string searchString = "")
    {
        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Session Expired",
                    "Please log in again.",
                    "OK");

                return;
            }

            string url =
                "http://localhost:5108/api/mobile/enrollments/me";

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                url += "?searchString=" +
                    Uri.EscapeDataString(searchString);
            }

            using var request =
                new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var enrollments =
                    await response.Content.ReadFromJsonAsync<
                        List<StudentEnrollmentResponse>>();

                DisplayEnrollments(enrollments);
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    "Unable to load enrollments: " + error,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Connection Error",
                "Unable to connect to backend: " + ex.Message,
                "OK");
        }
    }

    // DISPLAY ENROLLMENT CARDS
    private void DisplayEnrollments(
        List<StudentEnrollmentResponse>? enrollments)
    {
        enrollmentsContainer.Children.Clear();

        if (enrollments == null || !enrollments.Any())
        {
            emptyState.IsVisible = true;
            return;
        }

        emptyState.IsVisible = false;

        int index = 0;

        foreach (var enrollment in enrollments)
        {
            // SELECT CARD COLOR
            Color cardColor =
                cardColors[index % cardColors.Count];

            // COURSE NAME
            var courseName = new Label
            {
                Text = enrollment.CourseName,
                FontSize = 28,
                TextColor = Color.FromArgb("#172033")
            };

            // COURSE DESCRIPTION
            var description = new Label
            {
                Text = enrollment.Description,
                FontSize = 16,
                TextColor = Color.FromArgb("#172033"),
                LineBreakMode = LineBreakMode.WordWrap
            };

            // DIVIDER
            var divider = new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromArgb("#64748B"),
                Margin = new Thickness(0, 5, 0, 5)
            };

            // DURATION
            var duration = new Label
            {
                Text = "🕒  Duration: " +
                       enrollment.Duration,
                FontSize = 15,
                TextColor = Color.FromArgb("#172033")
            };

            // ENROLLMENT DATE
            var enrollmentDate = new Label
            {
                Text = "🗓️  Enrolled On: " +
                       enrollment.EnrollmentDate.ToString("dd-MM-yyyy"),
                FontSize = 15,
                TextColor = Color.FromArgb("#172033")
            };

            // CARD CONTENT
            var cardContent = new VerticalStackLayout
            {
                Spacing = 16,

                Children =
                {
                    courseName,
                    description,
                    divider,
                    duration,
                    enrollmentDate
                }
            };

            // ROUNDED CARD WITH SHADOW
            var card = new Border
            {
                Content = cardContent,

                BackgroundColor = cardColor,

                Stroke = Colors.Transparent,
                StrokeThickness = 0,

                WidthRequest = 275,
                HeightRequest = 293,

                Padding = new Thickness(30),

                Margin = new Thickness(0, 0, 25, 20),

                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 15
                },

                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Offset = new Point(0, 4),
                    Opacity = 0.15f,
                    Radius = 8
                }
            };

            enrollmentsContainer.Children.Add(card);

            index++;
        }
    }

    // SEARCH BUTTON
    private async void Search_Clicked(
        object sender,
        EventArgs e)
    {
        string searchString =
            searchEntry.Text?.Trim() ?? "";

        await LoadEnrollments(searchString);
    }
}
