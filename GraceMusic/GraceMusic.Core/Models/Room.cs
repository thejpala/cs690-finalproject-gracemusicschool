namespace GraceMusic.Core.Models;

public class Room
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; } = 1;
    public bool IsAvailable { get; set; } = true;

    public Room()
    {
    }

    public Room(string id, string name, int capacity, bool isAvailable = true)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        IsAvailable = isAvailable;
    }
}
