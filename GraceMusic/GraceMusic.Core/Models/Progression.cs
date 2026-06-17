namespace GraceMusic.Core.Models;

public class Progression
{
    public string StudentId { get; set; } = string.Empty;
    public string Instrument { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public DateTime LastUpdated { get; set; } = DateTime.Today;

    public Progression()
    {
    }

    public Progression(string studentId, string instrument, int level, DateTime? lastUpdated = null)
    {
        StudentId = studentId;
        Instrument = instrument;
        Level = level;
        LastUpdated = lastUpdated ?? DateTime.Today;
    }
}
