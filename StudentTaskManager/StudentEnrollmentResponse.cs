
namespace StudentTaskManager;

public class StudentEnrollmentResponse
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public string CourseName { get; set; } = "";

    public string Description { get; set; } = "";

    public string Duration { get; set; } = "";

    public DateTime EnrollmentDate { get; set; }
}
