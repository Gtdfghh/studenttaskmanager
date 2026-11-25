using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentTaskManager;

public partial class ManageCoursePage : ContentView
{
    private readonly HttpClient _httpClient = new HttpClient();

    private const string ApiUrl =
        "http://localhost:5108/api/mobile/admin/courses";

    private readonly JsonSerializerOptions jsonOptions =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    private AddCoursePage? _addCourseView;
    private EditCoursePage? _editCourseView;

    private int _pendingDeleteCourseId;

    public ManageCoursePage()
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
                await Shell.Current.DisplayAlertAsync(
                    "Login Required",
                    "Please log in as Admin again.",
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

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    "Unable to load courses: " + error,
                    "OK");

                return;
            }

            var courses =
                await response.Content.ReadFromJsonAsync
                <List<AdminCourseResponse>>(jsonOptions);

            DisplayCourses(
                courses ?? new List<AdminCourseResponse>());
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Connection Error",
                ex.Message,
                "OK");
        }
    }

    // DISPLAY COURSES
    private void DisplayCourses(
        List<AdminCourseResponse> courses)
    {
        coursesContainer.Children.Clear();

        noCoursesLabel.IsVisible = courses.Count == 0;

        foreach (var course in courses)
        {
            var row = new Grid
            {
                Padding = new Thickness(15, 18),
                ColumnDefinitions =
                {
                    new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength(1.5, GridUnitType.Star)
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength(3, GridUnitType.Star)
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength(1.5, GridUnitType.Star)
                    }
                }
            };

            row.Add(
                CreateCell(course.CourseId.ToString()), 0, 0);

            row.Add(
                CreateCell(course.CourseName), 1, 0);

            row.Add(
                CreateCell(course.Description), 2, 0);

            row.Add(
                CreateCell(course.Duration), 3, 0);

            // EDIT BUTTON
            var editButton = new Button
            {
                Text = "✏️",
                BackgroundColor = Color.FromArgb("#347FF0"),
                TextColor = Colors.White,
                WidthRequest = 44,
                HeightRequest = 38,
                Padding = 0,
                CornerRadius = 6
            };

            editButton.Clicked += async (sender, e) =>
            {
                await OpenEditCourse(course.CourseId);
            };

            // DELETE BUTTON
            var deleteButton = new Button
            {
                Text = "🗑",
                BackgroundColor = Color.FromArgb("#DC3545"),
                TextColor = Colors.White,
                WidthRequest = 44,
                HeightRequest = 38,
                Padding = 0,
                CornerRadius = 6
            };

            deleteButton.Clicked += (sender, e) =>
            {
                _pendingDeleteCourseId = course.CourseId;

                DeleteConfirmOverlay.IsVisible = true;
            };

            // ACTION BUTTONS
            var actionLayout = new HorizontalStackLayout
            {
                Spacing = 8,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            actionLayout.Children.Add(editButton);
            actionLayout.Children.Add(deleteButton);

            row.Add(actionLayout, 4, 0);

            // ROW BORDER
            var rowBorder = new Border
            {
                Content = row,
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#D9DDE2"),
                StrokeThickness = 0.5,
                Padding = 0
            };

            coursesContainer.Children.Add(rowBorder);
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
            VerticalTextAlignment = TextAlignment.Center
        };
    }

    // SEARCH COURSES
    private async void Search_Clicked(
        object sender,
        EventArgs e)
    {
        await LoadCourses(
            searchEntry.Text?.Trim() ?? "");
    }

    // OPEN ADD COURSE FORM
    private void AddCourse_Clicked(
        object sender,
        EventArgs e)
    {
        _addCourseView = new AddCoursePage();

        _addCourseView.CourseSaved +=
            CourseForm_CourseSaved;

        _addCourseView.CancelRequested +=
            CourseForm_CancelRequested;

        ShowCourseForm(_addCourseView);
    }

    // OPEN EDIT COURSE FORM
    private async Task OpenEditCourse(int courseId)
    {
        try
        {
            _editCourseView = new EditCoursePage();

            _editCourseView.CourseSaved +=
                CourseForm_CourseSaved;

            _editCourseView.CancelRequested +=
                CourseForm_CancelRequested;

            ShowCourseForm(_editCourseView);

            await _editCourseView.LoadCourseDetails(courseId);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Error",
                "Unable to open Edit Course: " + ex.Message,
                "OK");

            await ReturnToCourseList(false);
        }
    }

    // DISPLAY FORM IN SAME PANEL
    private void ShowCourseForm(ContentView form)
    {
        courseFormArea.Content = form;

        courseListView.IsVisible = false;
        courseFormArea.IsVisible = true;
    }

    // RETURN TO COURSE LIST
    private async Task ReturnToCourseList(bool reload)
    {
        courseFormArea.Content = null;
        courseFormArea.IsVisible = false;

        courseListView.IsVisible = true;

        if (_addCourseView != null)
        {
            _addCourseView.CourseSaved -=
                CourseForm_CourseSaved;

            _addCourseView.CancelRequested -=
                CourseForm_CancelRequested;
        }

        if (_editCourseView != null)
        {
            _editCourseView.CourseSaved -=
                CourseForm_CourseSaved;

            _editCourseView.CancelRequested -=
                CourseForm_CancelRequested;
        }

        _addCourseView = null;
        _editCourseView = null;

        if (reload)
        {
            await LoadCourses(
                searchEntry.Text?.Trim() ?? "");
        }
    }

    // AFTER ADD OR EDIT SUCCESS
    private async void CourseForm_CourseSaved(
        object? sender,
        EventArgs e)
    {
        await ReturnToCourseList(true);
    }

    // CANCEL ADD OR EDIT
    private async void CourseForm_CancelRequested(
        object? sender,
        EventArgs e)
    {
        await ReturnToCourseList(false);
    }

    // CANCEL DELETE
    private void CancelDelete_Clicked(
        object sender,
        EventArgs e)
    {
        DeleteConfirmOverlay.IsVisible = false;

        _pendingDeleteCourseId = 0;
    }

    // CONFIRM DELETE
    private async void ConfirmDelete_Clicked(
        object sender,
        EventArgs e)
    {
        DeleteConfirmOverlay.IsVisible = false;

        if (_pendingDeleteCourseId <= 0)
            return;

        int courseId = _pendingDeleteCourseId;

        _pendingDeleteCourseId = 0;

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
                HttpMethod.Delete,
                $"{ApiUrl}/{courseId}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                await LoadCourses(
                    searchEntry.Text?.Trim() ?? "");
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                await Shell.Current.DisplayAlertAsync(
                    "Delete Failed",
                    error,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }
}