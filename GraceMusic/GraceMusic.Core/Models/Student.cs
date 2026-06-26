namespace GraceMusic.Core.Models;
public class Student
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public Student() { }
    public Student(string id, string name, string phone = "")
    {
        Id = id; Name = name; Phone = phone;
    }
}