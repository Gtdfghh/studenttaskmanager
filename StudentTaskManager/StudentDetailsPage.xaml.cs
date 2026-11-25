
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StudentTaskManager;

public partial class StudentDetailsPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    private const string ApiUrl =
        "http://localhost:5108/api/mobile/students/register";

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
        "Computer Science and Engineering (IoT)",
        "Computer Science and Engineering (IoT and Cyber Security with Blockchain Technology)",
        "Cyber Security",
        "Data Science",
        "Electrical and Electronics Engineering (EEE)",
        "Electronics and Communication Engineering (ECE)",
        "Information Science and Engineering (ISE)",
        "Information Technology (IT)",
        "Mechanical Engineering"
    };

    private readonly List<string> years = new()
    {
        "1", "2", "3", "4"
    };

    public StudentDetailsPage()
    {
        InitializeComponent();

        dateOfBirthPicker.MaximumDate = DateTime.Today;

        foreach (string department in departments)
        {
            AddDepartmentOption(department);
        }

        foreach (string year in years)
        {
            AddYearOption(year);
        }
    }

    // COMMON COLORS
    private Color BlueColor => Color.FromArgb("#0D6EFD");
    private Color GreyColor => Color.FromArgb("#CED4DA");
    private Color RedColor => Color.FromArgb("#DC3545");

    // FULL NAME TEXT CHANGED
    private void Name_TextChanged(object sender, TextChangedEventArgs e)
    {
        nameErrorLabel.IsVisible = false;
        nameErrorLabel.Text = "";

        nameBorder.Stroke = nameEntry.IsFocused
            ? BlueColor
            : GreyColor;

        registerErrorLabel.IsVisible = false;
    }

    // PHONE TEXT CHANGED
    private void Phone_TextChanged(object sender, TextChangedEventArgs e)
    {
        phoneErrorLabel.IsVisible = false;
        phoneErrorLabel.Text = "";

        phoneBorder.Stroke = phoneEntry.IsFocused
            ? BlueColor
            : GreyColor;

        registerErrorLabel.IsVisible = false;
    }

    // DATE SELECTED
    private void Date_Selected(object sender, DateChangedEventArgs e)
    {
        dateErrorLabel.IsVisible = false;
        dateErrorLabel.Text = "";

        dateBorder.Stroke = dateOfBirthPicker.IsFocused
            ? BlueColor
            : GreyColor;

        registerErrorLabel.IsVisible = false;
    }

    // NAME FOCUS
    private void Name_Focused(object sender, FocusEventArgs e)
    {
        nameBorder.Stroke = BlueColor;
    }

    private void Name_Unfocused(object sender, FocusEventArgs e)
    {
        nameBorder.Stroke = nameErrorLabel.IsVisible
            ? RedColor
            : GreyColor;
    }

    // DATE FOCUS
    private void Date_Focused(object sender, FocusEventArgs e)
    {
        dateBorder.Stroke = BlueColor;
    }

    private void Date_Unfocused(object sender, FocusEventArgs e)
    {
        dateBorder.Stroke = dateErrorLabel.IsVisible
            ? RedColor
            : GreyColor;
    }

    // PHONE FOCUS
    private void Phone_Focused(object sender, FocusEventArgs e)
    {
        phoneBorder.Stroke = BlueColor;
    }

    private void Phone_Unfocused(object sender, FocusEventArgs e)
    {
        phoneBorder.Stroke = phoneErrorLabel.IsVisible
            ? RedColor
            : GreyColor;
    }

    // DEPARTMENT DROPDOWN
    private void DepartmentDropdown_Tapped(
        object sender, TappedEventArgs e)
    {
        departmentOptionsBorder.IsVisible =
            !departmentOptionsBorder.IsVisible;

        yearOptionsBorder.IsVisible = false;

        departmentBorder.Stroke =
            departmentOptionsBorder.IsVisible
                ? BlueColor
                : (departmentErrorLabel.IsVisible ? RedColor : GreyColor);

        yearBorder.Stroke = yearErrorLabel.IsVisible
            ? RedColor
            : GreyColor;
    }

    // YEAR DROPDOWN
    private void YearDropdown_Tapped(
        object sender, TappedEventArgs e)
    {
        yearOptionsBorder.IsVisible =
            !yearOptionsBorder.IsVisible;

        departmentOptionsBorder.IsVisible = false;

        yearBorder.Stroke =
            yearOptionsBorder.IsVisible
                ? BlueColor
                : (yearErrorLabel.IsVisible ? RedColor : GreyColor);

        departmentBorder.Stroke = departmentErrorLabel.IsVisible
            ? RedColor
            : GreyColor;
    }

    // ADD DEPARTMENT OPTION
    private void AddDepartmentOption(string department)
    {
        Border option = CreateOption(department);

        var tap = new TapGestureRecognizer();

        tap.Tapped += (s, e) =>
        {
            departmentSelectedLabel.Text = department;
            departmentSelectedLabel.TextColor =
                Color.FromArgb("#212529");

            departmentOptionsBorder.IsVisible = false;
            departmentErrorLabel.IsVisible = false;
            departmentErrorLabel.Text = "";

            departmentBorder.Stroke = GreyColor;
            registerErrorLabel.IsVisible = false;
        };

        option.GestureRecognizers.Add(tap);
        departmentOptions.Children.Add(option);
    }

    // ADD YEAR OPTION
    private void AddYearOption(string year)
    {
        Border option = CreateOption(year);

        var tap = new TapGestureRecognizer();

        tap.Tapped += (s, e) =>
        {
            yearSelectedLabel.Text = year;
            yearSelectedLabel.TextColor =
                Color.FromArgb("#212529");

            yearOptionsBorder.IsVisible = false;
            yearErrorLabel.IsVisible = false;
            yearErrorLabel.Text = "";

            yearBorder.Stroke = GreyColor;
            registerErrorLabel.IsVisible = false;
        };

        option.GestureRecognizers.Add(tap);
        yearOptions.Children.Add(option);
    }

    // CREATE DROPDOWN OPTION
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
            Padding = new Thickness(14, 12),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 0.5
        };
    }

    // SHOW FIELD ERROR
    private void ShowFieldError(
        Label errorLabel,
        Border fieldBorder,
        string message)
    {
        errorLabel.Text = message;
        errorLabel.IsVisible = true;
        fieldBorder.Stroke = RedColor;
    }

    // REGISTER STUDENT BUTTON
    private async void RegisterStudent_Clicked(
        object sender, EventArgs e)
    {
        string name = nameEntry.Text?.Trim() ?? "";
        string phone = phoneEntry.Text?.Trim() ?? "";
        string department = departmentSelectedLabel.Text ?? "";
        string selectedYear = yearSelectedLabel.Text ?? "";

        // CLEAR PREVIOUS ERRORS
        nameErrorLabel.IsVisible = false;
        dateErrorLabel.IsVisible = false;
        phoneErrorLabel.IsVisible = false;
        departmentErrorLabel.IsVisible = false;
        yearErrorLabel.IsVisible = false;
        registerErrorLabel.IsVisible = false;

        nameBorder.Stroke = GreyColor;
        dateBorder.Stroke = GreyColor;
        phoneBorder.Stroke = GreyColor;
        departmentBorder.Stroke = GreyColor;
        yearBorder.Stroke = GreyColor;

        bool isValid = true;

        // VALIDATE NAME
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowFieldError(
                nameErrorLabel,
                nameBorder,
                "Full name is required.");

            isValid = false;
        }
        else if (name.Length < 2 ||
                 !name.Any(char.IsLetter))
        {
            ShowFieldError(
                nameErrorLabel,
                nameBorder,
                "Please enter a valid full name.");

            isValid = false;
        }

        // VALIDATE DATE OF BIRTH
        if (dateOfBirthPicker.Date >= DateTime.Today)
        {
            ShowFieldError(
                dateErrorLabel,
                dateBorder,
                "Please select a valid date of birth.");

            isValid = false;
        }

        // VALIDATE PHONE
        if (phone.Length != 10 ||
            !phone.All(char.IsDigit))
        {
            ShowFieldError(
                phoneErrorLabel,
                phoneBorder,
                "Please enter a valid 10-digit phone number.");

            isValid = false;
        }

        // VALIDATE DEPARTMENT
        if (string.IsNullOrWhiteSpace(department) ||
            department == "-- Select Department --")
        {
            ShowFieldError(
                departmentErrorLabel,
                departmentBorder,
                "Please select your department.");

            isValid = false;
        }

        // VALIDATE YEAR
        if (!int.TryParse(selectedYear, out int year) ||
            year < 1 || year > 4)
        {
            ShowFieldError(
                yearErrorLabel,
                yearBorder,
                "Please select your year.");

            isValid = false;
        }

        // STOP IF VALIDATION FAILS
        if (!isValid)
            return;

        try
        {
            registerButton.IsEnabled = false;
            registerButton.Text = "Registering...";

            // GET JWT TOKEN
            string? token =
                await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlertAsync(
                    "Login Required",
                    "Your login session was not found. Please log in again.",
                    "OK");

                await Shell.Current.GoToAsync(
                    nameof(LoginPage));

                return;
            }

            // CREATE API REQUEST
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                ApiUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // STUDENT DETAILS
            request.Content = JsonContent.Create(new
            {
                name = name,

                dateOfBirth = dateOfBirthPicker.Date.Value
    .ToString("yyyy-MM-dd"),

                phone = phone,
                department = department,
                year = year
            });

            // SEND TO BACKEND
            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            string responseContent =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // DIRECT NAVIGATION - NO SUCCESS ALERT
                await Shell.Current.GoToAsync(
                    nameof(StudentDashboardPage));
            }
            else
            {
                ShowGeneralError(
                    "Registration failed: " + responseContent);
            }
        }
        catch (Exception)
        {
            ShowGeneralError(
                "Unable to register student details. Please try again.");
        }
        finally
        {
            registerButton.IsEnabled = true;
            registerButton.Text = "Register";
        }
    }

    // GENERAL API ERROR
    private void ShowGeneralError(string message)
    {
        registerErrorLabel.Text = "• " + message;
        registerErrorLabel.IsVisible = true;
    }
}
