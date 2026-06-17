using System;
using GraceMusic.Core.Models;
using GraceMusic.Infrastructure.Repositories;
using GraceMusic.Infrastructure.Constants;
using GraceMusic.UI.Screens;

namespace GraceMusic.UI;

public class ConsoleUI 
{
    // 1. Declare the repositories
    private readonly FileRepository<Teacher> _teacherRepo;
    private readonly FileRepository<Room> _roomRepo;
    private readonly FileRepository<string> _instrumentRepo;
    private readonly FileRepository<Student> _studentRepo;
    private readonly FileRepository<Lesson> _lessonRepo;
    private readonly FileRepository<Progression> _progressionRepo;
    private readonly FileRepository<Payment> _paymentsRepo;

    public ConsoleUI()
    {
        // 2. Initialize Repositories with specific Parsing/Serializing logic
        _teacherRepo = new FileRepository<Teacher>(FilePaths.Teachers,
            line => { 
                var p = line.Split(','); 
                return new Teacher(p[0], p[1], p[2], decimal.Parse(p[3])); 
            },
            t => $"{t.Id},{t.Name},{t.Specialty},{t.HourlyRate}"
        );

        _roomRepo = new FileRepository<Room>(
            FilePaths.Rooms,
            line => { 
                var p = line.Split(','); 
                return new Room(p[0], p[1], int.Parse(p[2]), bool.Parse(p[3])); 
            },
            r => $"{r.Id},{r.Name},{r.Capacity},{r.IsAvailable}"
        );

        _instrumentRepo = new FileRepository<string>(
            FilePaths.Instruments,
            line => line, // Instruments are just strings
            s => s
        );

        _studentRepo = new FileRepository<Student>(
            FilePaths.Students,
            line => { 
                var p = line.Split(','); 
                return new Student(p[0], p[1], p[2], int.Parse(p[3]), p[4]); 
            },
            s => $"{s.Id},{s.Name},{s.Instrument},{s.Level},{s.Phone}"
        );

        _lessonRepo = new FileRepository<Lesson>(
            FilePaths.Lessons,
            line => {
                var p = line.Split(',');
                return new Lesson(p[0], p[1], p[2], p[3], p[4], DateTime.Parse(p[5]), p[6]);
            },
            l => $"{l.Id},{l.StudentId},{l.TeacherId},{l.RoomId},{l.Instrument},{l.LessonDate:O},{l.TimeSlot}"
        );
        
        _progressionRepo = new FileRepository<Progression>(
            FilePaths.Progressions,
            line => {
                var p = line.Split(',');
                return new Progression(p[0], p[1], int.Parse(p[2])) ;
            },
            p => $"{p.StudentId},{p.Instrument},{p.Level}"
        );

        _paymentsRepo = new FileRepository<Payment>(
            FilePaths.Payments,
            line => {
                var p = line.Split(',');
                return new Payment(p[0], p[1], p[2], decimal.Parse(p[3]), DateTime.Parse(p[4]));
            },
            p => $"{p.Id},{p.StudentId},{p.Month},{p.Amount},{p.PaidDate}"
        );
    }

    public void Run() 
    {
        bool running = true;
        while (running) 
        {
            UiRenderer.DrawHeader("GRACE'S MUSIC SCHOOL: MAIN MENU");
            Console.WriteLine("  1. Manage Schedule (Bookings)");
            Console.WriteLine("  2. Manage Student Progression");
            Console.WriteLine("  3. Manage School Records");
            Console.WriteLine("  4. Process Payments");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("  9. Exit Application");
            Console.WriteLine("============================================================");
            Console.Write("Enter Selection [1-9]: ");

            string choice = Console.ReadLine() ?? string.Empty;

            switch (choice) 
            {
                case "1": 
                    // Injecting dependencies into SchedulingScreen
                    new SchedulingScreen(_studentRepo, _teacherRepo, _roomRepo, _instrumentRepo, _lessonRepo).Render(); 
                    break;
                case "2": 
                    // Note: Ensure ProgressionScreen constructor is updated to take IRepository dependencies
                    new ProgressionScreen(_progressionRepo).Render(); 
                    break;
                case "3": 
                    // Injecting dependencies into RegistryScreen
                    new RegistryScreen(_teacherRepo, _roomRepo, _instrumentRepo, _studentRepo).Render(); 
                    break;
                case "4": 
                    // Note: Ensure PaymentScreen constructor is updated to take IRepository dependencies
                    new PaymentScreen(_paymentsRepo).Render(); 
                    break;
                case "9": 
                    running = false; 
                    break;
                default: 
                    Console.WriteLine("Invalid selection. Press any key to retry.");
                    Console.ReadKey();
                    break;
            }
        }
    }
}