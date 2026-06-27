using System;

namespace GraceMusic.Core.Models;

public class MakeupRequest
{
    public string Id { get; set; }
    public string StudentId { get; set; }
    public string EnrollmentId { get; set; }
    public DateTime TargetDate { get; set; }

    public MakeupRequest() { }

    public MakeupRequest(string id, string studentId, string enrollmentId, DateTime targetDate)
    {
        Id = id;
        StudentId = studentId;
        EnrollmentId = enrollmentId;
        TargetDate = targetDate;
    }
}