
namespace StudentTaskManager;

public class AdminStudentEditResponse
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = "";

    public DateTime DateOfBirth { get; set; }

    public string Phone { get; set; } = "";

    public string Department { get; set; } = "";

    public int? Year { get; set; }
}
