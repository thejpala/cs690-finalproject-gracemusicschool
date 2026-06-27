using System;

namespace GraceMusic.Core.Models;

public class TeacherLeave
{
    public string Id { get; set; }
    public string TeacherId { get; set; }
    public DateTime LeaveDate { get; set; }
    public string TimeSlot { get; set; } // Can be a specific time (e.g., "16:00") or "ALL" for a full day

    public TeacherLeave() { }

    public TeacherLeave(string id, string teacherId, DateTime leaveDate, string timeSlot = "ALL")
    {
        Id = id;
        TeacherId = teacherId;
        LeaveDate = leaveDate;
        TimeSlot = timeSlot;
    }
}