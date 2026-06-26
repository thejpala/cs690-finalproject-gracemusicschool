using System;
using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;

namespace GraceMusic.Core.Services;

public class SchedulingService
{
    private readonly IRepository<Lesson> _lessonRepository;
    private readonly IRepository<Enrollment> _enrollmentRepository;

    public SchedulingService(IRepository<Lesson> lessonRepository, IRepository<Enrollment> enrollmentRepository)
    {
        _lessonRepository = lessonRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public List<string> GetAvailableTimeSlots(Teacher teacher, Room room, string studentId, DateTime date)
    {
        var availableSlots = new List<string>();
        
        if (!TryParseHours(teacher.BaseHours, out TimeSpan start, out TimeSpan end))
        {
            start = TimeSpan.FromHours(9); 
            end = TimeSpan.FromHours(17);  
        }

        var dailyLessons = _lessonRepository.LoadAll().Where(l => l.LessonDate.Date == date.Date).ToList();
        
        var studentEnrollmentIds = _enrollmentRepository.LoadAll()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.Id).ToList();

        for (var time = start; time < end; time = time.Add(TimeSpan.FromMinutes(30)))
        {
            var timeString = time.ToString(@"hh\:mm");
            bool teacherConflict = dailyLessons.Any(l => l.TeacherId == teacher.Id && l.TimeSlot == timeString);
            bool roomConflict = dailyLessons.Any(l => l.RoomId == room.Id && l.TimeSlot == timeString);
            bool studentConflict = dailyLessons.Any(l => studentEnrollmentIds.Contains(l.EnrollmentId) && l.TimeSlot == timeString);

            if (!teacherConflict && !roomConflict && !studentConflict) availableSlots.Add(timeString);
        }
        return availableSlots;
    }

    // NEW: Just checks the Teacher's schedule
    public List<string> GetTeacherOnlyAvailableSlots(Teacher teacher, DateTime date)
    {
        var availableSlots = new List<string>();
        if (!TryParseHours(teacher.BaseHours, out TimeSpan start, out TimeSpan end))
        {
            start = TimeSpan.FromHours(9); end = TimeSpan.FromHours(17);
        }

        var teacherLessons = _lessonRepository.LoadAll()
            .Where(l => l.LessonDate.Date == date.Date && l.TeacherId == teacher.Id).ToList();

        for (var time = start; time < end; time = time.Add(TimeSpan.FromMinutes(30)))
        {
            var timeString = time.ToString(@"hh\:mm");
            if (!teacherLessons.Any(l => l.TimeSlot == timeString)) availableSlots.Add(timeString);
        }
        return availableSlots;
    }

    public void BookLesson(string enrollmentId, string teacherId, string roomId, DateTime date, string timeSlot)
    {
        var lessons = _lessonRepository.LoadAll();
        var lesson = new Lesson(
            id: $"LSN-{Guid.NewGuid().ToString("N")[..8]}",
            enrollmentId: enrollmentId,
            teacherId: teacherId,
            roomId: roomId,
            lessonDate: date,
            timeSlot: timeSlot);

        lessons.Add(lesson);
        _lessonRepository.SaveAll(lessons);
    }

    // NEW: Removes a lesson
    public void CancelLesson(string lessonId)
    {
        var lessons = _lessonRepository.LoadAll();
        var target = lessons.FirstOrDefault(l => l.Id == lessonId);
        if (target != null)
        {
            lessons.Remove(target);
            _lessonRepository.SaveAll(lessons);
        }
    }

    private bool TryParseHours(string baseHours, out TimeSpan start, out TimeSpan end)
    {
        start = TimeSpan.Zero; end = TimeSpan.Zero;
        try {
            var parts = baseHours.Split('-');
            return TimeSpan.TryParse(parts[0], out start) && TimeSpan.TryParse(parts[1], out end);
        } catch { return false; }
    }
}