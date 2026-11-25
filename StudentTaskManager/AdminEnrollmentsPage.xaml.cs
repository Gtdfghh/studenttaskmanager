
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class AdminEnrollmentsPage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    private const string ApiUrl =
        "http://localhost:5108/api/mobile/admin/enrollments";

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    public AdminEnrollmentsPage()
    {
        InitializeComponent();
    }

    // LOAD ENROLLMENTS
    // Public so AdminDashboardPage can call it.
    public async Task LoadEnrollments(string searchString = "")
    {
        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Login Required",
                    "Please login as Admin again.",
                    "OK");

                return;
            }

            string requestUrl = ApiUrl;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                requestUrl += "?searchString=" +
                    Uri.EscapeDataString(searchString);
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                requestUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var enrollments =
                    await response.Content.ReadFromJsonAsync
                    <List<AdminEnrollmentResponse>>(jsonOptions);

                DisplayEnrollments(
                    enrollments ?? new List<AdminEnrollmentResponse>());
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

    // DISPLAY ENROLLMENTS
    private void DisplayEnrollments(
        List<AdminEnrollmentResponse> enrollments)
    {
        enrollmentsContainer.Children.Clear();

        noEnrollmentsLabel.IsVisible =
            enrollments.Count == 0;

        foreach (var enrollment in enrollments)
        {
            var row = new Grid
            {
                Padding = new Thickness(15, 15),
                ColumnDefinitions =
                {
                    new ColumnDefinition
                    {
                        Width = new GridLength(1.2, GridUnitType.Star)
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength(1.2, GridUnitType.Star)
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength(1.5, GridUnitType.Star)
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength(1.5, GridUnitType.Star)
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength(1.2, GridUnitType.Star)
                    }
                }
            };

            row.Add(
                CreateCell(enrollment.EnrollmentId.ToString()), 0, 0);

            row.Add(
                CreateCell(enrollment.StudentId.ToString()), 1, 0);

            row.Add(
                CreateCell(enrollment.StudentName), 2, 0);

            row.Add(
                CreateCell(enrollment.CourseName), 3, 0);

            row.Add(
                CreateCell(enrollment.Duration), 4, 0);

            var rowBorder = new Border
            {
                Content = row,
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#D9DDE2"),
                StrokeThickness = 0.5,
                Padding = 0
            };

            enrollmentsContainer.Children.Add(rowBorder);
        }
    }

    // CREATE TABLE CELL
    private Label CreateCell(string value)
    {
        return new Label
        {
            Text = value,
            FontSize = 14,
            TextColor = Color.FromArgb("#212529"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        };
    }

    // SEARCH BUTTON
    private async void Search_Clicked(
        object sender, EventArgs e)
    {
        string searchText =
            searchEntry.Text?.Trim() ?? "";

        await LoadEnrollments(searchText);
    }
}
