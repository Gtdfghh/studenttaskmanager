
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class EditAdminStudentPage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    private int studentId;

    // Events used by AdminDashboardPage.
    public event EventHandler? CancelRequested;
    public event EventHandler? StudentUpdated;

    // DEPARTMENTS
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

    public EditAdminStudentPage()
    {
        InitializeComponent();

        // Populate department dropdown.
        foreach (string department in departments)
        {
            AddDepartmentOption(department);
        }

        // Populate year dropdown.
        AddYearOption("1st Year");
        AddYearOption("2nd Year");
        AddYearOption("3rd Year");
        AddYearOption("4th Year");
    }

    // DISPLAY ALERT
    private async Task ShowMessage(
        string title, string message)
    {
        Page? page = Application.Current?
            .Windows.FirstOrDefault()?.Page;

        if (page != null)
        {
            await page.DisplayAlertAsync(title, message, "OK");
        }
    }

    // LOAD SELECTED STUDENT
    public async Task LoadStudentDetails(int id)
    {
        studentId = id;

        await LoadStudentDetailsFromApi();
    }

    // DEPARTMENT DROPDOWN
    private void DepartmentDropdown_Tapped(
        object sender, TappedEventArgs e)
    {
        departmentOptionsBorder.IsVisible =
            !departmentOptionsBorder.IsVisible;

        yearOptionsBorder.IsVisible = false;
    }

    // YEAR DROPDOWN
    private void YearDropdown_Tapped(
        object sender, TappedEventArgs e)
    {
        yearOptionsBorder.IsVisible =
            !yearOptionsBorder.IsVisible;

        departmentOptionsBorder.IsVisible = false;
    }

    // ADD DEPARTMENT OPTIONS
    private void AddDepartmentOption(string department)
    {
        Border option = CreateOption(department);

        var tap = new TapGestureRecognizer();

        tap.Tapped += (sender, e) =>
        {
            departmentSelectedLabel.Text = department;

            departmentSelectedLabel.TextColor =
                Color.FromArgb("#212529");

            departmentOptionsBorder.IsVisible = false;
        };

        option.GestureRecognizers.Add(tap);

        departmentOptions.Children.Add(option);
    }

    // ADD YEAR OPTIONS
    private void AddYearOption(string year)
    {
        Border option = CreateOption(year);

        var tap = new TapGestureRecognizer();

        tap.Tapped += (sender, e) =>
        {
            yearSelectedLabel.Text = year;

            yearSelectedLabel.TextColor =
                Color.FromArgb("#212529");

            yearOptionsBorder.IsVisible = false;
        };

        option.GestureRecognizers.Add(tap);

        yearOptions.Children.Add(option);
    }

    // DROPDOWN OPTION DESIGN
    private Border CreateOption(string value)
    {
        var label = new Label
        {
            Text = value,
            FontSize = 14,
            TextColor = Color.FromArgb("#212529"),
            VerticalOptions = LayoutOptions.Center
        };

        return new Border
        {
            Content = label,
            Padding = new Thickness(12, 12),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 0.5
        };
    }

    // LOAD STUDENT DETAILS FROM API
    private async Task LoadStudentDetailsFromApi()
    {
        try
        {
            if (studentId <= 0)
            {
                await ShowMessage(
                    "Error",
                    "Invalid student ID.");

                return;
            }

            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await ShowMessage(
                    "Session Expired",
                    "Please log in again.");

                return;
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"http://localhost:5108/api/mobile/admin/students/{studentId}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await ShowMessage("Error", error);

                return;
            }

            var student =
                await response.Content.ReadFromJsonAsync
                <AdminStudentEditResponse>();

            if (student == null)
            {
                await ShowMessage(
                    "Error",
                    "Student details not found.");

                return;
            }

            // FILL NAME
            nameEntry.Text = student.Name;

            // FILL DATE OF BIRTH
            dateOfBirthPicker.Date = student.DateOfBirth;

            // FILL PHONE
            phoneEntry.Text = student.Phone;

            // FILL DEPARTMENT
            departmentSelectedLabel.Text = student.Department;

            departmentSelectedLabel.TextColor =
                Color.FromArgb("#212529");

            // FILL YEAR
            if (student.Year.HasValue &&
                student.Year.Value >= 1 &&
                student.Year.Value <= 4)
            {
                string[] years =
                {
                    "1st Year",
                    "2nd Year",
                    "3rd Year",
                    "4th Year"
                };

                yearSelectedLabel.Text =
                    years[student.Year.Value - 1];

                yearSelectedLabel.TextColor =
                    Color.FromArgb("#212529");
            }
        }
        catch (Exception ex)
        {
            await ShowMessage(
                "Error",
                "Unable to load student details: " + ex.Message);
        }
    }

    // SAVE CHANGES
    private async void SaveChanges_Clicked(
        object sender, EventArgs e)
    {
        string name = nameEntry.Text?.Trim() ?? "";
        string phone = phoneEntry.Text?.Trim() ?? "";
        string department = departmentSelectedLabel.Text ?? "";
        string selectedYear = yearSelectedLabel.Text ?? "";

        // VALIDATE NAME
        if (string.IsNullOrWhiteSpace(name))
        {
            await ShowMessage(
                "Validation Error",
                "Please enter the student's name.");

            return;
        }

        // VALIDATE DATE OF BIRTH
        DateTime? selectedDate = dateOfBirthPicker.Date;

        if (!selectedDate.HasValue ||
            selectedDate.Value.Date >= DateTime.Today)
        {
            await ShowMessage(
                "Validation Error",
                "Please select a valid date of birth.");

            return;
        }

        DateTime dob = selectedDate.Value;

        // VALIDATE PHONE
        if (phone.Length != 10 ||
            !phone.All(char.IsDigit))
        {
            await ShowMessage(
                "Validation Error",
                "Phone number must contain exactly 10 digits.");

            return;
        }

        // VALIDATE DEPARTMENT
        if (string.IsNullOrWhiteSpace(department) ||
            department == "Select Department")
        {
            await ShowMessage(
                "Validation Error",
                "Please select a department.");

            return;
        }

        // VALIDATE YEAR
        int year = selectedYear switch
        {
            "1st Year" => 1,
            "2nd Year" => 2,
            "3rd Year" => 3,
            "4th Year" => 4,
            _ => 0
        };

        if (year == 0)
        {
            await ShowMessage(
                "Validation Error",
                "Please select a year.");

            return;
        }

        try
        {
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await ShowMessage(
                    "Session Expired",
                    "Please log in again.");

                return;
            }

            // UPDATED STUDENT DATA
            var updatedStudent = new
            {
                Name = name,
                DateOfBirth = dob,
                Phone = phone,
                Department = department,
                Year = year
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"http://localhost:5108/api/mobile/admin/students/{studentId}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(updatedStudent);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                

                StudentUpdated?.Invoke(
                    this,
                    EventArgs.Empty);
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await ShowMessage(
                    "Update Failed",
                    error);
            }
        }
        catch (Exception ex)
        {
            await ShowMessage(
                "Error",
                "Unable to update student: " + ex.Message);
        }
    }

    // CANCEL
    private void Cancel_Clicked(
        object sender, EventArgs e)
    {
        CancelRequested?.Invoke(
            this,
            EventArgs.Empty);
    }
}
