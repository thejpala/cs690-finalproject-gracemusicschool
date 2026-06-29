using System;
using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;

namespace GraceMusic.Core.Services;

public class ScheduleReportingService
{
    private readonly IRepository<Lesson> _lessonRepo;
    private readonly IRepository<TeacherLeave> _leaveRepo;
    private readonly IRepository<MakeupRequest> _makeupRepo;
    private readonly IRepository<Teacher> _teacherRepo;
    private readonly IRepository<Student> _studentRepo;

    public ScheduleReportingService(
        IRepository<Lesson> lessonRepo, 
        IRepository<TeacherLeave> leaveRepo, 
        IRepository<MakeupRequest> makeupRepo,
        IRepository<Teacher> teacherRepo,
        IRepository<Student> studentRepo)
    {
        _lessonRepo = lessonRepo;
        _leaveRepo = leaveRepo;
        _makeupRepo = makeupRepo;
        _teacherRepo = teacherRepo;
        _studentRepo = studentRepo;
    }

    // Resolves FR-5.1: Displaying master schedules
    public List<Lesson> GetDailySchedule(DateTime targetDate)
    {
        return _lessonRepo.LoadAll()
            .Where(l => l.LessonDate.Date == targetDate.Date)
            .OrderBy(l => TimeSpan.Parse(l.TimeSlot)) // Chronological sort
            .ToList();
    }

    // Replace your existing GetPendingActionItems with this:
    public List<ConflictAlert> GetPendingActionItems(DateTime targetDate)
    {
        var alerts = new List<ConflictAlert>();
        var teachers = _teacherRepo.LoadAll();
        var students = _studentRepo.LoadAll();

        // Check for Unassigned Makeups
        var makeups = _makeupRepo.LoadAll().Where(m => m.TargetDate.Date == targetDate.Date).ToList();
        foreach (var makeup in makeups)
        {
            var studentName = students.FirstOrDefault(s => s.Id == makeup.StudentId)?.Name ?? "Unknown Student";
            alerts.Add(new ConflictAlert(
                "UNASSIGNED_MAKEUP", 
                $"Student {studentName} requires a teacher/room assignment.", 
                makeup.Id));
        }

        // Check for Teacher Conflicts
        var leaves = _leaveRepo.LoadAll().Where(l => l.LeaveDate.Date == targetDate.Date).ToList();
        var dailyLessons = GetDailySchedule(targetDate);

        foreach (var leave in leaves)
        {
            var teacherName = teachers.FirstOrDefault(t => t.Id == leave.TeacherId)?.Name ?? "Unknown Teacher";
            var conflictingLessons = dailyLessons.Where(l => 
                l.TeacherId == leave.TeacherId && 
                (leave.TimeSlot == "ALL" || leave.TimeSlot == l.TimeSlot)).ToList();

            foreach (var conflict in conflictingLessons)
            {
                alerts.Add(new ConflictAlert(
                    "TEACHER_CONFLICT", 
                    $"{teacherName} is on leave during scheduled {conflict.TimeSlot} slot.", 
                    conflict.Id));
            }
        }

        return alerts;
    }
}