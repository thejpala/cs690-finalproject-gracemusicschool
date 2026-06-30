using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;

namespace GraceMusic.Core.Services;

public class ProgressionService
{
    private readonly IRepository<Enrollment> _enrollmentRepo;

    public ProgressionService(IRepository<Enrollment> enrollmentRepo)
    {
        _enrollmentRepo = enrollmentRepo;
    }

    // Resolves FR-4.3: Update Instructor Approval for Progression
    public void UpdateApprovalStatus(string enrollmentId, bool isApproved)
    {
        var enrollments = _enrollmentRepo.LoadAll();
        var target = enrollments.FirstOrDefault(e => e.Id == enrollmentId);
        
        if (target != null)
        {
            target.IsInstructorApproved = isApproved;
            _enrollmentRepo.SaveAll(enrollments);
        }
    }

    // Resolves FR-4.2: Enforcing instructor approval for tier promotion
    public bool TryPromoteStudent(string enrollmentId, out string message)
    {
        var enrollments = _enrollmentRepo.LoadAll();
        var target = enrollments.FirstOrDefault(e => e.Id == enrollmentId);

        if (target == null)
        {
            message = "Error: Enrollment record not found.";
            return false;
        }

        if (!target.IsInstructorApproved)
        {
            message = "Promotion Denied. Level advancement depends explicitly on instructor progress sign-off.";
            return false;
        }

        target.Level += 1;
        target.IsInstructorApproved = false; // Reset the flag for the next tier!
        
        _enrollmentRepo.SaveAll(enrollments);
        message = $"Success: Student successfully promoted to {target.Instrument} Level {target.Level}.";
        return true;
    }
}