using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class StudentDashboardPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    private MyProfilePage? _profilePage;
    private AvailableCoursesPage? _coursesView;
    private MyEnrollmentsPage? _enrollmentsView;

    private View? _dashboardContent;

    public StudentDashboardPage()
    {
        InitializeComponent();

        // Store the original dashboard content.
        _dashboardContent = mainContentArea.Content;

        SetActiveButton("Dashboard");
    }

    // LOAD DASHBOARD COUNTS WHEN PAGE APPEARS
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDashboardCounts();
    }

    // SIDEBAR BLUE HIGHLIGHT
    private void SetActiveButton(string selected)
    {
        DashboardButton.BackgroundColor =
            selected == "Dashboard"
                ? Color.FromArgb("#347FF0")
                : Colors.Transparent;

        ProfileButton.BackgroundColor =
            selected == "Profile"
                ? Color.FromArgb("#347FF0")
                : Colors.Transparent;

        CoursesButton.BackgroundColor =
            selected == "Courses"
                ? Color.FromArgb("#347FF0")
                : Colors.Transparent;

        EnrollmentsButton.BackgroundColor =
            selected == "Enrollments"
                ? Color.FromArgb("#347FF0")
                : Colors.Transparent;
    }

    // LOAD AVAILABLE COURSES AND ENROLLMENT COUNTS
    private async Task LoadDashboardCounts()
    {
        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlertAsync(
                    "Session Expired",
                    "Please log in again.",
                    "OK");

                return;
            }

            // GET AVAILABLE COURSES COUNT
            int coursesCount = await GetCountAsync(
                "http://localhost:5108/api/mobile/courses",
                token);

            totalCoursesLabel.Text =
                coursesCount.ToString();

            // GET LOGGED-IN STUDENT ENROLLMENTS COUNT
            int enrollmentsCount = await GetCountAsync(
                "http://localhost:5108/api/mobile/enrollments/me",
                token);

            myEnrollmentsLabel.Text =
                enrollmentsCount.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Dashboard Error",
                "Unable to load dashboard counts: " + ex.Message,
                "OK");
        }
    }

    // SEND GET REQUEST AND COUNT THE RETURNED RECORDS
    private async Task<int> GetCountAsync(
        string apiUrl,
        string token)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            apiUrl);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(request);

        string responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                "API: " + apiUrl +
                "\nHTTP Status: " + (int)response.StatusCode +
                "\nResponse: " + responseBody);
        }

        using JsonDocument document =
            JsonDocument.Parse(responseBody);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new Exception(
                "The API did not return a list of records: " + apiUrl);
        }

        return document.RootElement.GetArrayLength();
    }

    // DASHBOARD
    private async void Dashboard_Clicked(
        object sender, EventArgs e)
    {
        mainContentArea.Content = _dashboardContent;

        SetActiveButton("Dashboard");

        // Refresh counts when returning to dashboard.
        await LoadDashboardCounts();
    }

    // MY PROFILE
    private async void MyProfile_Clicked(
        object sender, EventArgs e)
    {
        try
        {
            if (_profilePage == null)
            {
                _profilePage = new MyProfilePage();
            }

            mainContentArea.Content = _profilePage;

            SetActiveButton("Profile");

            await _profilePage.LoadStudentProfile();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                "Unable to open profile: " + ex.Message,
                "OK");
        }
    }

    // AVAILABLE COURSES
    private async void AvailableCourses_Clicked(
        object sender, EventArgs e)
    {
        try
        {
            if (_coursesView == null)
            {
                _coursesView = new AvailableCoursesPage();
            }

            mainContentArea.Content = _coursesView;

            SetActiveButton("Courses");

            await _coursesView.LoadCourses();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                "Unable to load courses: " + ex.Message,
                "OK");
        }
    }

    // MY ENROLLMENTS
    private async void MyEnrollments_Clicked(
        object sender, EventArgs e)
    {
        try
        {
            if (_enrollmentsView == null)
            {
                _enrollmentsView = new MyEnrollmentsPage();
            }

            mainContentArea.Content = _enrollmentsView;

            SetActiveButton("Enrollments");

            await _enrollmentsView.LoadEnrollments();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                "Unable to load enrollments: " + ex.Message,
                "OK");
        }
    }

    // LOGOUT
    private async void Logout_Clicked(
        object sender, EventArgs e)
    {
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("user_id");
        SecureStorage.Remove("user_role");

        await Shell.Current.GoToAsync(
            nameof(LoginPage));
    }
}