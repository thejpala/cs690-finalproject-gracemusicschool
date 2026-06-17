namespace GraceMusic.Core.Models;

public class Student
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Instrument { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public string Phone { get; set; } = string.Empty;

    public Student()
    {
    }

    public Student(string id, string name, string instrument, int level, string phone = "")
    {
        Id = id;
        Name = name;
        Instrument = instrument;
        Level = level;
        Phone = phone;
    }
}
