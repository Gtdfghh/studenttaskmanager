
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class AdminDashboardPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    private View? _dashboardContent;

    private ManageStudentsPage? _studentsView;
    private ManageCoursePage? _coursesView;

    private EditAdminStudentPage? _editStudentView;
    private AdminEnrollmentsPage? _enrollmentsView;
    public AdminDashboardPage()
    {
        InitializeComponent();

        // Save the original dashboard content.
        _dashboardContent = mainContentArea.Content;

        SetActiveButton("Dashboard");
    }

    // SIDEBAR BLUE HIGHLIGHT
    private void SetActiveButton(string selected)
    {
        DashboardButton.BackgroundColor =
            selected == "Dashboard"
                ? Color.FromArgb("#347FF0")
                : Colors.Transparent;

        StudentsButton.BackgroundColor =
            selected == "Students"
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

    // SHOW DASHBOARD
    private void Dashboard_Clicked(object sender, EventArgs e)
    {
        SetActiveButton("Dashboard");

        // Display the original dashboard in the right-side area.
        mainContentArea.Content = _dashboardContent;
    }

    // MANAGE STUDENTS
    private async void ManageStudents_Clicked(
        object sender, EventArgs e)
    {
        await ShowManageStudents();
    }

    // DISPLAY STUDENTS INSIDE DASHBOARD
    private async Task ShowManageStudents()
    {
        SetActiveButton("Students");

        try
        {
            if (_studentsView == null)
            {
                _studentsView = new ManageStudentsPage();

                // IMPORTANT:
                // Connect the Edit button event.
                _studentsView.EditRequested += OpenEditStudent;
            }

            // Show Manage Students without opening another page.
            mainContentArea.Content = _studentsView;

            await _studentsView.LoadStudents("");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                "Unable to open Manage Students: " + ex.Message,
                "OK");
        }
    }

    // OPEN EDIT STUDENT FORM
    private async void OpenEditStudent(int studentId)
    {
        try
        {
            _editStudentView = new EditAdminStudentPage();

            // When Cancel is clicked, return to student list.
            _editStudentView.CancelRequested +=
                EditStudent_CancelRequested;

            // When Save succeeds, return to student list.
            _editStudentView.StudentUpdated +=
                EditStudent_StudentUpdated;

            // Replace only the right-side content.
            // The Admin sidebar remains visible.
            mainContentArea.Content = _editStudentView;

            // Load the selected student's information.
            await _editStudentView.LoadStudentDetails(studentId);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                "Unable to open student details: " + ex.Message,
                "OK");
        }
    }

    // CANCEL EDITING
    private async void EditStudent_CancelRequested(
        object? sender, EventArgs e)
    {
        await ReturnToStudentsList();
    }

    // AFTER SUCCESSFUL UPDATE
    private async void EditStudent_StudentUpdated(
        object? sender, EventArgs e)
    {
        await ReturnToStudentsList();
    }

    // RETURN TO STUDENT LIST
    private async Task ReturnToStudentsList()
    {
        if (_editStudentView != null)
        {
            _editStudentView.CancelRequested -=
                EditStudent_CancelRequested;

            _editStudentView.StudentUpdated -=
                EditStudent_StudentUpdated;

            _editStudentView = null;
        }

        // Restore the student list.
        mainContentArea.Content = _studentsView;

        // Reload the latest student records.
        if (_studentsView != null)
        {
            await _studentsView.LoadStudents("");
        }

        SetActiveButton("Students");
    }

    // MANAGE COURSES
    // MANAGE COURSES
    private async void ManageCourses_Clicked(
        object sender, EventArgs e)
    {
        await ShowManageCourses();
    }

    // DISPLAY COURSES INSIDE ADMIN DASHBOARD
    private async Task ShowManageCourses()
    {
        SetActiveButton("Courses");

        try
        {
            if (_coursesView == null)
            {
                _coursesView = new ManageCoursePage();
            }

            mainContentArea.Content = _coursesView;

            await _coursesView.LoadCourses("");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                "Unable to open Manage Courses: " + ex.Message,
                "OK");
        }
    }

    // ENROLLMENTS
    // ENROLLMENTS
    private async void Enrollments_Clicked(
        object sender, EventArgs e)
    {
        await ShowEnrollments();
    }

    // DISPLAY ENROLLMENTS INSIDE ADMIN DASHBOARD
    private async Task ShowEnrollments()
    {
        SetActiveButton("Enrollments");

        try
        {
            if (_enrollmentsView == null)
            {
                _enrollmentsView = new AdminEnrollmentsPage();
            }

            mainContentArea.Content = _enrollmentsView;

            await _enrollmentsView.LoadEnrollments("");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                "Unable to open Enrollments: " + ex.Message,
                "OK");
        }
    }

    // LOAD DASHBOARD DATA
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDashboard();
    }

    private async Task LoadDashboard()
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

                await Shell.Current.GoToAsync("//MainPage");
                return;
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost:5108/api/mobile/admin/dashboard");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var dashboard =
                    await response.Content.ReadFromJsonAsync
                    <AdminDashboardResponse>();

                if (dashboard != null)
                {
                    totalStudentsLabel.Text =
                        dashboard.TotalStudents.ToString();

                    totalCoursesLabel.Text =
                        dashboard.TotalCourses.ToString();

                    totalEnrollmentsLabel.Text =
                        dashboard.TotalEnrollments.ToString();
                }
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await DisplayAlertAsync(
                    "Dashboard Error",
                    error,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                "Unable to load dashboard: " + ex.Message,
                "OK");
        }
    }

    // TOTAL STUDENTS CARD
    private async void TotalStudentsCard_Clicked(
        object sender, TappedEventArgs e)
    {
        await ShowManageStudents();
    }

    // TOTAL COURSES CARD

    private async void TotalCoursesCard_Clicked(
    object sender, TappedEventArgs e)
    {
        await ShowManageCourses();
    }
    // TOTAL ENROLLMENTS CARD
    private async void TotalEnrollmentsCard_Clicked(
     object sender, TappedEventArgs e)
    {
        await ShowEnrollments();
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
