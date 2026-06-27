using System;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using GraceMusic.Infrastructure;
using GraceMusic.Infrastructure.Constants;
using GraceMusic.Infrastructure.Repositories;
using GraceMusic.UI.Screens;
using Spectre.Console;

namespace GraceMusic.UI;

public class ConsoleUI 
{
    private readonly FileRepository<Teacher> _teacherRepo;
    private readonly FileRepository<Room> _roomRepo;
    private readonly FileRepository<string> _instrumentRepo;
    private readonly FileRepository<Student> _studentRepo;
    private readonly FileRepository<Lesson> _lessonRepo;
    private readonly FileRepository<Enrollment> _enrollmentRepo;
    private readonly FileRepository<Payment> _paymentRepo;
    private readonly FileRepository<TeacherLeave> _leaveRepo;
    private readonly FileRepository<MakeupRequest> _makeupRepo;

    private readonly SchedulingService _schedulingService;
    private readonly PaymentService _paymentService;
    private readonly ScheduleReportingService _reportingService;

    public ConsoleUI()
    {
        _teacherRepo = new FileRepository<Teacher>(FilePaths.Teachers,
            line => { var p = CsvParser.Split(line); return new Teacher(p[0], p[1], p[2], decimal.Parse(p[3]), p.Length > 4 ? p[4] : "09:00-17:00"); },
            t => $"{t.Id},{CsvParser.Escape(t.Name)},{t.Specialty},{t.HourlyRate},{t.BaseHours}");

        _roomRepo = new FileRepository<Room>(FilePaths.Rooms,
            line => { var p = CsvParser.Split(line); return new Room(p[0], p[1], int.Parse(p[2])); },
            r => $"{r.Id},{CsvParser.Escape(r.Name)},{r.Capacity}");

        _instrumentRepo = new FileRepository<string>(FilePaths.Instruments, line => line, s => s);

        _studentRepo = new FileRepository<Student>(FilePaths.Students,
            line => { var p = CsvParser.Split(line); return new Student(p[0], p[1], p[2]); },
            s => $"{s.Id},{CsvParser.Escape(s.Name)},{s.Phone}");

        _lessonRepo = new FileRepository<Lesson>(FilePaths.Lessons,
            line => { var p = CsvParser.Split(line); return new Lesson(p[0], p[1], p[2], p[3], DateTime.Parse(p[4]), p[5]); },
            l => $"{l.Id},{l.EnrollmentId},{l.TeacherId},{l.RoomId},{l.LessonDate:O},{l.TimeSlot}");
        
        _enrollmentRepo = new FileRepository<Enrollment>(FilePaths.Enrollments,
            line => { var p = CsvParser.Split(line); return new Enrollment(p[0], p[1], p[2], int.Parse(p[3])); },
            e => $"{e.Id},{e.StudentId},{e.Instrument},{e.Level}");
        
        _paymentRepo = new FileRepository<Payment>(FilePaths.Payments,
            line => { var p = CsvParser.Split(line); return new Payment(p[0], p[1], decimal.Parse(p[2]), p[3], DateTime.Parse(p[4])); },
            p => $"{p.Id},{p.StudentId},{p.Amount},{p.CoverageMonth},{p.PaymentDate:O}");

        _leaveRepo = new FileRepository<TeacherLeave>(FilePaths.TeacherLeaves,
            line => { var p = CsvParser.Split(line); return new TeacherLeave(p[0], p[1], DateTime.Parse(p[2]), p[3]); },
            l => $"{l.Id},{l.TeacherId},{l.LeaveDate:O},{l.TimeSlot}");

        _makeupRepo = new FileRepository<MakeupRequest>(FilePaths.MakeupRequests,
            line => { var p = CsvParser.Split(line); return new MakeupRequest(p[0], p[1], p[2], DateTime.Parse(p[3])); },
            m => $"{m.Id},{m.StudentId},{m.EnrollmentId},{m.TargetDate:O}");

        _paymentService = new PaymentService(_paymentRepo, _enrollmentRepo);
        _reportingService = new ScheduleReportingService(_lessonRepo, _leaveRepo, _makeupRepo, _teacherRepo, _studentRepo);
        _schedulingService = new SchedulingService(_lessonRepo, _enrollmentRepo);
    }

    public void Run() 
    {
        bool running = true;
        while (running) 
        {
            UiRenderer.DrawHeader("GRACE'S MUSIC SCHOOL: MAIN MENU");
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an action:")
                    .AddChoices(
                        "1. Lesson Scheduling & Management",
                        "2. Manage School Registry & Enrollments",
                        "3. Student Payments Portal",
                        "4. View Daily Master Schedule Summary",
                        "9. Exit Application"
                    ));

            switch (choice[0].ToString()) 
            {
                case "1": new SchedulingScreen(_studentRepo, _teacherRepo, _roomRepo, _enrollmentRepo, _lessonRepo, _leaveRepo, _makeupRepo).Render(); break;
                case "2": new RegistryScreen(_teacherRepo, _roomRepo, _instrumentRepo, _studentRepo, _enrollmentRepo, _paymentService).Render(); break;
                case "3": new PaymentScreen(_paymentService, _studentRepo, _paymentRepo).Render(); break;
                case "4": new MasterScheduleScreen(_reportingService, _schedulingService, _teacherRepo, _studentRepo, _roomRepo, _lessonRepo, _makeupRepo, _enrollmentRepo).Render(); break;
                case "9": running = false; break;
            }
        }
    }
}