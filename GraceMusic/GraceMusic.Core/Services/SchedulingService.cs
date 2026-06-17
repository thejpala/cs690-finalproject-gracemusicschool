using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;

namespace GraceMusic.Core.Services;

public class SchedulingService
{
    private readonly IRepository<Student> _studentRepository;
    private readonly IRepository<Teacher> _teacherRepository;
    private readonly IRepository<Room> _roomRepository;
    private readonly IRepository<Lesson> _lessonRepository;

    public SchedulingService(
        IRepository<Student> studentRepository,
        IRepository<Teacher> teacherRepository,
        IRepository<Room> roomRepository,
        IRepository<Lesson> lessonRepository)
    {
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _roomRepository = roomRepository;
        _lessonRepository = lessonRepository;
    }

    public SchedulingValidationResult ValidateBooking(
        string studentId,
        string teacherId,
        string roomId,
        string instrument,
        DateTime lessonDate,
        string timeSlot)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(studentId))
        {
            errors.Add("Student ID is required.");
        }

        if (string.IsNullOrWhiteSpace(teacherId))
        {
            errors.Add("Teacher ID is required.");
        }

        if (string.IsNullOrWhiteSpace(roomId))
        {
            errors.Add("Room ID is required.");
        }

        if (string.IsNullOrWhiteSpace(instrument))
        {
            errors.Add("Instrument is required.");
        }

        if (lessonDate < DateTime.Today)
        {
            errors.Add("Lesson date cannot be in the past.");
        }

        if (string.IsNullOrWhiteSpace(timeSlot))
        {
            errors.Add("Time slot is required.");
        }

        var student = _studentRepository.LoadAll().FirstOrDefault(s => s.Id == studentId);
        if (student == null)
        {
            errors.Add($"Student '{studentId}' was not found.");
        }

        var teacher = _teacherRepository.LoadAll().FirstOrDefault(t => t.Id == teacherId);
        if (teacher == null)
        {
            errors.Add($"Teacher '{teacherId}' was not found.");
        }

        var room = _roomRepository.LoadAll().FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            errors.Add($"Room '{roomId}' was not found.");
        }
        else if (!room.IsAvailable)
        {
            errors.Add($"Room '{roomId}' is currently unavailable.");
        }

        if (teacher != null && !string.IsNullOrWhiteSpace(instrument) &&
            !string.Equals(teacher.Specialty, instrument, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"Teacher '{teacher.Name}' does not teach {instrument}.");
        }

        if (student != null && !string.IsNullOrWhiteSpace(instrument) &&
            !string.Equals(student.Instrument, instrument, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"Student '{student.Name}' is not enrolled for {instrument}.");
        }

        var overlappingLesson = _lessonRepository.LoadAll().FirstOrDefault(l =>
            l.RoomId == roomId &&
            l.LessonDate.Date == lessonDate.Date &&
            l.TimeSlot == timeSlot &&
            l.Status != "Cancelled");

        if (overlappingLesson != null)
        {
            errors.Add("The selected room is already booked for that time.");
        }

        return new SchedulingValidationResult(errors.Count == 0, errors);
    }

    public Lesson CreateLesson(string studentId, string teacherId, string roomId, string instrument, DateTime lessonDate, string timeSlot)
    {
        var validation = ValidateBooking(studentId, teacherId, roomId, instrument, lessonDate, timeSlot);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join(" ", validation.Errors));
        }

        var lesson = new Lesson(
            id: $"LSN-{Guid.NewGuid().ToString("N")[..8]}",
            studentId: studentId,
            teacherId: teacherId,
            roomId: roomId,
            instrument: instrument,
            lessonDate: lessonDate,
            timeSlot: timeSlot);

        var lessons = _lessonRepository.LoadAll();
        lessons.Add(lesson);
        _lessonRepository.SaveAll(lessons);

        return lesson;
    }
}

public class SchedulingValidationResult
{
    public SchedulingValidationResult(bool isValid, List<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }
    public List<string> Errors { get; }
}
