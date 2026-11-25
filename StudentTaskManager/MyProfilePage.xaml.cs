using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class MyProfilePage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    private EditProfilePage? _editProfileView;

    // Store the original profile content.
    private View? _profileView;

    public MyProfilePage()
    {
        InitializeComponent();

        // Save the original profile ScrollView.
        _profileView = profileContentArea.Content;
    }

    // LOAD STUDENT PROFILE
    public async Task LoadStudentProfile()
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

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost:5108/api/mobile/students/me");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var student =
                    await response.Content.ReadFromJsonAsync<StudentProfileResponse>();

                if (student == null)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Error",
                        "Profile data not received.",
                        "OK");

                    return;
                }

                profileNameLabel.Text = student.Name;

                headerStudentIdLabel.Text =
                    $"Student ID: {student.StudentId}";

                studentIdLabel.Text =
                    student.StudentId.ToString();

                fullNameLabel.Text =
                    student.Name;

                dateOfBirthLabel.Text =
                    student.DateOfBirth.ToString("dd-MM-yyyy");

                phoneLabel.Text =
                    student.Phone;

                departmentLabel.Text =
                    student.Department;

                yearLabel.Text = student.Year.HasValue
                    ? student.Year.Value.ToString()
                    : "Not specified";
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await Shell.Current.DisplayAlertAsync(
                    "Profile Error",
                    error,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Error",
                "Could not load student profile: " + ex.Message,
                "OK");
        }
    }

    // OPEN EDIT PROFILE INSIDE THE SAME PANEL
    private async void EditProfile_Clicked(
        object sender, EventArgs e)
    {
        try
        {
            _editProfileView = new EditProfilePage();

            _editProfileView.ProfileSaved +=
                EditProfile_ProfileSaved;

            _editProfileView.CancelRequested +=
                EditProfile_CancelRequested;

            profileContentArea.Content = _editProfileView;

            await _editProfileView.LoadStudentDetails();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Error",
                "Unable to open Edit Profile: " + ex.Message,
                "OK");
        }
    }

    // AFTER PROFILE IS SUCCESSFULLY UPDATED
    private async void EditProfile_ProfileSaved(
        object? sender, EventArgs e)
    {
        await ReturnToProfile(true);
    }

    // WHEN CANCEL IS CLICKED
    private async void EditProfile_CancelRequested(
        object? sender, EventArgs e)
    {
        await ReturnToProfile(false);
    }

    // RETURN TO PROFILE VIEW
    private async Task ReturnToProfile(bool reload)
    {
        if (_editProfileView != null)
        {
            _editProfileView.ProfileSaved -=
                EditProfile_ProfileSaved;

            _editProfileView.CancelRequested -=
                EditProfile_CancelRequested;

            _editProfileView = null;
        }

        // Restore the original profile content.
        profileContentArea.Content = _profileView;

        // Reload updated information after saving.
        if (reload)
        {
            await LoadStudentProfile();
        }
    }
}