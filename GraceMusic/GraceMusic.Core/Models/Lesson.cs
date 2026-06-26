using System;
namespace GraceMusic.Core.Models;
public class Lesson
{
    public string Id { get; set; } = string.Empty;
    public string EnrollmentId { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public DateTime LessonDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;

    public Lesson() { }
    public Lesson(string id, string enrollmentId, string teacherId, string roomId, DateTime lessonDate, string timeSlot)
    {
        Id = id; EnrollmentId = enrollmentId; TeacherId = teacherId; RoomId = roomId; LessonDate = lessonDate; TimeSlot = timeSlot;
    }
}