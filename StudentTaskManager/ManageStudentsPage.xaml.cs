using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class ManageStudentsPage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    // Selected student ID for deletion.
    private int _pendingDeleteStudentId;

    // Sends selected student ID to AdminDashboardPage.
    public event Action<int>? EditRequested;

    public ManageStudentsPage()
    {
        InitializeComponent();
    }

    // DISPLAY ALERT
    private async Task ShowMessage(
        string title,
        string message)
    {
        var page = Shell.Current?.CurrentPage;

        if (page != null)
        {
            await page.DisplayAlertAsync(
                title,
                message,
                "OK");
        }
    }

    // LOAD STUDENTS
    public async Task LoadStudents(string search)
    {
        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await ShowMessage(
                    "Session Expired",
                    "Please log in again.");

                await Shell.Current.GoToAsync("//MainPage");
                return;
            }

            string url =
                "http://localhost:5108/api/mobile/admin/students";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "?search=" +
                    Uri.EscapeDataString(search);
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var students =
                    await response.Content.ReadFromJsonAsync
                    <List<AdminStudentResponse>>();

                studentsCollection.ItemsSource = students;
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await ShowMessage("Error", error);
            }
        }
        catch (Exception ex)
        {
            await ShowMessage(
                "Error",
                "Unable to load students: " + ex.Message);
        }
    }

    // SEARCH STUDENTS
    private async void Search_Clicked(
        object sender,
        EventArgs e)
    {
        string search =
            searchEntry.Text?.Trim() ?? "";

        await LoadStudents(search);
    }

    // EDIT STUDENT
    private void EditStudent_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter == null)
        {
            return;
        }

        int studentId =
            Convert.ToInt32(button.CommandParameter);

        // Notify AdminDashboardPage.
        EditRequested?.Invoke(studentId);
    }

    // OPEN DELETE CONFIRMATION
    private void DeleteStudent_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter == null)
        {
            return;
        }

        _pendingDeleteStudentId =
            Convert.ToInt32(button.CommandParameter);

        // Show custom popup.
        DeleteConfirmOverlay.IsVisible = true;
    }

    // CANCEL DELETE
    private void CancelDelete_Clicked(
        object sender,
        EventArgs e)
    {
        DeleteConfirmOverlay.IsVisible = false;

        _pendingDeleteStudentId = 0;
    }

    // CONFIRM DELETE
    private async void ConfirmDelete_Clicked(
        object sender,
        EventArgs e)
    {
        DeleteConfirmOverlay.IsVisible = false;

        if (_pendingDeleteStudentId <= 0)
        {
            return;
        }

        int studentId = _pendingDeleteStudentId;

        _pendingDeleteStudentId = 0;

        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await ShowMessage(
                    "Error",
                    "Please log in again.");

                return;
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"http://localhost:5108/api/mobile/admin/students/{studentId}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Refresh the student list.
                await LoadStudents(
                    searchEntry.Text?.Trim() ?? "");
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await ShowMessage(
                    "Delete Failed",
                    error);
            }
        }
        catch (Exception ex)
        {
            await ShowMessage(
                "Error",
                "Unable to delete student: " + ex.Message);
        }
    }
}