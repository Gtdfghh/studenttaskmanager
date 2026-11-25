
using System;

namespace StudentTaskManager;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // LEARN MORE: Scroll to Features section
    private async void OnLearnMoreClicked(
        object sender, EventArgs e)
    {
        await homeScrollView.ScrollToAsync(
            featuresSection,
            ScrollToPosition.Start,
            true);
    }

    // GET STARTED: Open Login page
    private async void OnGetStartedClicked(
        object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(LoginPage));
    }
}
