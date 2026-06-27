namespace GraceMusic.Core.Models;
public class Teacher
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string BaseHours { get; set; } = "09:00-17:00"; 

    public Teacher() { }
    public Teacher(string id, string name, string specialty, decimal hourlyRate, string baseHours = "09:00-17:00")
    {
        Id = id; Name = name; Specialty = specialty; HourlyRate = hourlyRate; BaseHours = baseHours;
    }
}