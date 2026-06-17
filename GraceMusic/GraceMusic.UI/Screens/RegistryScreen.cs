using System;
using System.Collections.Generic;
using GraceMusic.Core.Models;
using GraceMusic.Core.Interfaces;

namespace GraceMusic.UI.Screens;

public class RegistryScreen
{
    private readonly IRepository<Teacher> _teacherRepo;
    private readonly IRepository<Room> _roomRepo;
    private readonly IRepository<string> _instrumentRepo;
    private readonly IRepository<Student> _studentRepo;

    // Constructor Injection
    public RegistryScreen(
        IRepository<Teacher> teacherRepo,
        IRepository<Room> roomRepo,
        IRepository<string> instrumentRepo,
        IRepository<Student> studentRepo)
    {
        _teacherRepo = teacherRepo;
        _roomRepo = roomRepo;
        _instrumentRepo = instrumentRepo;
        _studentRepo = studentRepo;
    }

    public void Render()
    {
        bool stayOnScreen = true;

        while (stayOnScreen)
        {
            UiRenderer.DrawHeader("PORTAL: SCHOOL REGISTRY");

            Console.WriteLine("  1. Manage Teachers");
            Console.WriteLine("  2. Manage Rooms");
            Console.WriteLine("  3. Manage Instruments");
            Console.WriteLine("  4. Manage Students");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("  9. Back to Main Menu");
            Console.WriteLine("============================================================");
            Console.Write("Enter Selection [1-4, 9]: ");

            string choice = Console.ReadLine() ?? string.Empty;

            switch (choice)
            {
                case "1":
                    ManageTeachers();
                    break;
                case "2":
                    ManageRooms();
                    break;
                case "3":
                    ManageInstruments();
                    break;
                case "4":
                    ManageStudents();
                    break;
                case "9":
                    stayOnScreen = false;
                    break;
                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }
        }
    }

    private void ManageTeachers()
    {
        UiRenderer.DrawHeader("PORTAL: MANAGE TEACHERS");
        var teachers = _teacherRepo.LoadAll();
        foreach (var teacher in teachers)
        {
            Console.WriteLine($"  {teacher.Id} | {teacher.Name} | {teacher.Specialty} | ${teacher.HourlyRate:F2}/hr");
        }

        Console.Write("\nAdd a new teacher? (Y/N): ");
        if ((Console.ReadLine() ?? string.Empty).Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            Console.Write("  Teacher ID: ");
            string id = Console.ReadLine() ?? string.Empty;
            Console.Write("  Teacher Name: ");
            string name = Console.ReadLine() ?? string.Empty;
            Console.Write("  Specialty: ");
            string specialty = Console.ReadLine() ?? string.Empty;
            Console.Write("  Hourly Rate: ");
            string rate = Console.ReadLine() ?? string.Empty;

            if (decimal.TryParse(rate, out decimal hourlyRate))
            {
                teachers.Add(new Teacher(id, name, specialty, hourlyRate));
                _teacherRepo.SaveAll(teachers);
                UiRenderer.PrintMessage("Teacher added successfully.");
            }
            else
            {
                UiRenderer.PrintMessage("ERROR: Invalid hourly rate.");
            }
        }

        UiRenderer.WaitForInput();
    }

    private void ManageRooms()
    {
        UiRenderer.DrawHeader("PORTAL: MANAGE ROOMS");
        var rooms = _roomRepo.LoadAll();
        foreach (var room in rooms)
        {
            Console.WriteLine($"  {room.Id} | {room.Name} | Capacity: {room.Capacity} | Available: {(room.IsAvailable ? "Yes" : "No")}");
        }

        Console.Write("\nAdd a new room? (Y/N): ");
        if ((Console.ReadLine() ?? string.Empty).Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            Console.Write("  Room ID: ");
            string id = Console.ReadLine() ?? string.Empty;
            Console.Write("  Room Name: ");
            string name = Console.ReadLine() ?? string.Empty;
            Console.Write("  Capacity: ");
            string capacity = Console.ReadLine() ?? string.Empty;

            if (int.TryParse(capacity, out int roomCapacity))
            {
                rooms.Add(new Room(id, name, roomCapacity));
                _roomRepo.SaveAll(rooms);
                UiRenderer.PrintMessage("Room added successfully.");
            }
            else
            {
                UiRenderer.PrintMessage("ERROR: Invalid capacity.");
            }
        }

        UiRenderer.WaitForInput();
    }

    private void ManageInstruments()
    {
        UiRenderer.DrawHeader("PORTAL: MANAGE INSTRUMENTS");
        var instruments = _instrumentRepo.LoadAll();
        foreach (var instrument in instruments)
        {
            Console.WriteLine($"  - {instrument}");
        }

        Console.Write("\nAdd a new instrument? (Y/N): ");
        if ((Console.ReadLine() ?? string.Empty).Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            Console.Write("  Instrument Name: ");
            string instrument = Console.ReadLine() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(instrument) && !instruments.Contains(instrument))
            {
                instruments.Add(instrument);
                _instrumentRepo.SaveAll(instruments);
                UiRenderer.PrintMessage("Instrument added successfully.");
            }
        }

        UiRenderer.WaitForInput();
    }

    private void ManageStudents()
    {
        UiRenderer.DrawHeader("PORTAL: MANAGE STUDENTS");
         var students = _studentRepo.LoadAll();
        foreach (var student in students)
        {
            Console.WriteLine($"  {student.Id} | {student.Name} | {student.Instrument} | Level {student.Level}");
        }

        Console.Write("\nAdd a new student? (Y/N): ");
        if ((Console.ReadLine() ?? string.Empty).Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            Console.Write("  Student ID: ");
            string id = Console.ReadLine() ?? string.Empty;
            Console.Write("  Student Name: ");
            string name = Console.ReadLine() ?? string.Empty;
            Console.Write("  Instrument: ");
            string instrument = Console.ReadLine() ?? string.Empty;
            Console.Write("  Level: ");
            string level = Console.ReadLine() ?? string.Empty;
            Console.Write("  Phone: ");
            string phone = Console.ReadLine() ?? string.Empty;

            if (int.TryParse(level, out int studentLevel))
            {
                students.Add(new Student(id, name, instrument, studentLevel, phone));
                _studentRepo.SaveAll(students);
                UiRenderer.PrintMessage("Student added successfully.");
            }
            else
            {
                UiRenderer.PrintMessage("ERROR: Invalid level.");
            }
        }

        UiRenderer.WaitForInput();
    }
}