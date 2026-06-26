namespace GraceMusic.Core.Models;
public class Enrollment
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Instrument { get; set; } = string.Empty;
    public int Level { get; set; } = 1;

    public Enrollment() { }
    public Enrollment(string id, string studentId, string instrument, int level = 1)
    {
        Id = id; StudentId = studentId; Instrument = instrument; Level = level;
    }
}