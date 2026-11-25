
namespace StudentTaskManager;

public class AdminStudentResponse
{
    public int StudentId { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public DateTime DateOfBirth { get; set; }

    public string Phone { get; set; } = "";

    public string Department { get; set; } = "";

    public int? Year { get; set; }
}
