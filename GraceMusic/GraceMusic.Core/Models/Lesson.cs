namespace GraceMusic.Core.Models;

public class Lesson
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string Instrument { get; set; } = string.Empty;
    public DateTime LessonDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string Status { get; set; } = "Scheduled";

    public Lesson()
    {
    }

    public Lesson(string id, string studentId, string teacherId, string roomId, string instrument, DateTime lessonDate, string timeSlot)
    {
        Id = id;
        StudentId = studentId;
        TeacherId = teacherId;
        RoomId = roomId;
        Instrument = instrument;
        LessonDate = lessonDate;
        TimeSlot = timeSlot;
    }
}
