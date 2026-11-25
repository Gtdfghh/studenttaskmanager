using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class EditProfilePage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    public event EventHandler? ProfileSaved;
    public event EventHandler? CancelRequested;

    // DEPARTMENT LIST
    private readonly List<string> departments = new()
    {
        "Agricultural Engineering",
        "Aeronautical Engineering",
        "Aerospace Engineering",
        "Artificial Intelligence and Data Science (AI & DS)",
        "Artificial Intelligence and Machine Learning (AI/ML)",
        "Automobile Engineering",
        "Biotechnology",
        "Biomedical Engineering",
        "Blockchain Technology",
        "Chemical Engineering",
        "Civil Engineering",
        "Computer Engineering",
        "Computer Science and Engineering (CSE)",
        "Computer Science and Engineering (Artificial Intelligence)",
        "Computer Science and Engineering (Artificial Intelligence and Machine Learning)",
        "Computer Science and Engineering (Blockchain Technology)",
        "Computer Science and Engineering (Cloud Computing)",
        "Computer Science and Engineering (Cyber Security)",
        "Computer Science and Engineering (Data Science)",
        "Computer Science and Engineering (Internet of Things)",
        "Computer Science and Engineering (IoT)",
        "Computer Science and Engineering (IoT and Cyber Security with Blockchain Technology)",
        "Computer Science and Engineering (Machine Learning)",
        "Cyber Security",
        "Data Science",
        "Electrical and Electronics Engineering (EEE)",
        "Electrical Engineering",
        "Electronics and Communication Engineering (ECE)",
        "Electronics and Instrumentation Engineering",
        "Electronics and Telecommunication Engineering",
        "Electronics Engineering",
        "Environmental Engineering",
        "Food Processing Technology",
        "Food Technology",
        "Industrial Engineering",
        "Information Science and Engineering (ISE)",
        "Information Technology (IT)",
        "Instrumentation and Control Engineering",
        "Internet of Things (IoT)",
        "Manufacturing Engineering",
        "Marine Engineering",
        "Mechatronics Engineering",
        "Mechanical Engineering",
        "Metallurgical and Materials Engineering",
        "Mining Engineering",
        "Petroleum Engineering",
        "Printing Technology",
        "Production Engineering",
        "Robotics and Automation",
        "Robotics Engineering",
        "Software Engineering",
        "Structural Engineering",
        "Textile Engineering",
        "VLSI Design"
    };

    public EditProfilePage()
    {
        InitializeComponent();

        InitializeDropdowns();
    }

    // INITIALIZE DEPARTMENT AND YEAR PICKERS
    private void InitializeDropdowns()
    {
        departmentPicker.ItemsSource = departments;

        yearPicker.ItemsSource = new List<string>
        {
            "1st Year",
            "2nd Year",
            "3rd Year",
            "4th Year"
        };
    }

    // DEPARTMENT SELECTION
    private void DepartmentPicker_SelectedIndexChanged(
        object? sender, EventArgs e)
    {
        if (departmentPicker.SelectedIndex >= 0)
        {
            editDepartmentSelectedLabel.Text =
                departmentPicker.SelectedItem?.ToString() ?? "";
        }
    }

    // YEAR SELECTION
    private void YearPicker_SelectedIndexChanged(
        object? sender, EventArgs e)
    {
        if (yearPicker.SelectedIndex >= 0)
        {
            editYearSelectedLabel.Text =
                yearPicker.SelectedItem?.ToString() ?? "";
        }
    }

    // LOAD STUDENT DETAILS
    public async Task LoadStudentDetails()
    {
        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Error",
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
                        "Student details not found.",
                        "OK");

                    return;
                }

                nameEntry.Text = student.Name;

                dateOfBirthPicker.Date = student.DateOfBirth;

                phoneEntry.Text = student.Phone;

                int departmentIndex =
                    departments.IndexOf(student.Department);

                if (departmentIndex >= 0)
                {
                    departmentPicker.SelectedIndex =
                        departmentIndex;
                }
                else
                {
                    editDepartmentSelectedLabel.Text =
                        student.Department;
                }

                if (student.Year.HasValue &&
                    student.Year.Value >= 1 &&
                    student.Year.Value <= 4)
                {
                    yearPicker.SelectedIndex =
                        student.Year.Value - 1;
                }
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
                "Unable to load student details: " + ex.Message,
                "OK");
        }
    }

    // SAVE CHANGES
    private async void SaveChanges_Clicked(
        object? sender, EventArgs e)
    {
        try
        {
            // VALIDATE NAME
            string name = nameEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Validation Error",
                    "Please enter your full name.",
                    "OK");

                return;
            }

            // VALIDATE DATE OF BIRTH
            // VALIDATE DATE OF BIRTH
            DateTime? selectedDob = dateOfBirthPicker.Date;

            if (!selectedDob.HasValue)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Validation Error",
                    "Please select your date of birth.",
                    "OK");

                return;
            }

            DateTime dob = selectedDob.Value;

            if (dob.Date >= DateTime.Today)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Validation Error",
                    "Please select a valid date of birth.",
                    "OK");

                return;
            }

            // VALIDATE PHONE
            string phone = phoneEntry.Text?.Trim() ?? "";

            if (phone.Length != 10 ||
                !phone.All(char.IsDigit))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Validation Error",
                    "Phone number must contain exactly 10 digits.",
                    "OK");

                return;
            }

            // VALIDATE DEPARTMENT
            if (departmentPicker.SelectedIndex < 0)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Validation Error",
                    "Please select your department.",
                    "OK");

                return;
            }

            string department =
                departmentPicker.SelectedItem?.ToString() ?? "";

            // VALIDATE YEAR
            if (yearPicker.SelectedIndex < 0)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Validation Error",
                    "Please select your academic year.",
                    "OK");

                return;
            }

            int year = yearPicker.SelectedIndex + 1;

            // GET JWT TOKEN
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

            // PREPARE UPDATED DETAILS
            var updatedStudent = new
            {
                Name = name,
                DateOfBirth = dob.ToString("yyyy-MM-dd"),
                Phone = phone,
                Department = department,
                Year = year
            };

            // CREATE PUT REQUEST
            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                "http://localhost:5108/api/mobile/students/me");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(updatedStudent);

            // SEND REQUEST
            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            { 

                ProfileSaved?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await Shell.Current.DisplayAlertAsync(
                    "Update Failed",
                    error,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Error",
                "Unable to update profile: " + ex.Message,
                "OK");
        }
    }

    // CANCEL
    private void Cancel_Clicked(
        object? sender, EventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}