namespace GraceMusic.Core.Models;

public class ConflictAlert
{
    public string Type { get; set; } // e.g., "TEACHER_CONFLICT" or "UNASSIGNED_MAKEUP"
    public string Message { get; set; }
    public string ReferenceId { get; set; } // Holds either the LessonId or the MakeupId

    public ConflictAlert(string type, string message, string referenceId)
    {
        Type = type;
        Message = message;
        ReferenceId = referenceId;
    }
}