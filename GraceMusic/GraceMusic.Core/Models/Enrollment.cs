namespace GraceMusic.Core.Models;

public class Enrollment
{
    public string Id { get; set; }
    public string StudentId { get; set; }
    public string Instrument { get; set; }
    public int Level { get; set; }
    public bool IsInstructorApproved { get; set; } 
    
    public Enrollment() { }

    public Enrollment(string id, string studentId, string instrument, int level, bool isInstructorApproved = false)
    {
        Id = id;
        StudentId = studentId;
        Instrument = instrument;
        Level = level;
        IsInstructorApproved = isInstructorApproved;
    }
}