
namespace StudentTaskManager;

public partial class AddStudentPage : ContentPage
{
    public AddStudentPage()
    {
        InitializeComponent();
    }

    private async void OnSaveStudentClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text;
        string email = EmailEntry.Text;
        string age = AgeEntry.Text;

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(age))
        {
            await DisplayAlertAsync("Error", "Please fill all fields", "OK");
            return;
        }

        await DisplayAlertAsync(
            "Success",
            "Student details entered successfully!",
            "OK");
    }
}
