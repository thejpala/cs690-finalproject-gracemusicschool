using System;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using Spectre.Console;

namespace GraceMusic.UI.Screens;

public class SchedulingScreen
{
    private readonly SchedulingService _schedulingService;


    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Teacher> _teacherRepo;
    private readonly IRepository<Room> _roomRepo;
    private readonly IRepository<string> _instrumentRepo;

    // Constructor Injection
    public SchedulingScreen(
        IRepository<Student> studentRepo,
        IRepository<Teacher> teacherRepo,
        IRepository<Room> roomRepo,
        IRepository<string> instrumentRepo,
        IRepository<Lesson> lessonRepo)
    {
        _studentRepo = studentRepo;
        _teacherRepo = teacherRepo;
        _roomRepo = roomRepo;
        _instrumentRepo = instrumentRepo;
        
        _schedulingService = new SchedulingService(studentRepo, teacherRepo, roomRepo, lessonRepo);
    }

    public void Render()
    {
        bool stayOnScreen = true;

        while (stayOnScreen)
        {
            UiRenderer.DrawHeader("PORTAL: LESSON SCHEDULING");

            // 1. Fetch current data
            var students = _studentRepo.LoadAll();
            var teachers = _teacherRepo.LoadAll();
            var rooms = _roomRepo.LoadAll();
            var instruments = _instrumentRepo.LoadAll();

            if (!students.Any() || !teachers.Any() || !rooms.Any())
            {
                UiRenderer.PrintMessage("ERROR: You must add at least one Student, Teacher, and Room in the Registry first.");
                UiRenderer.WaitForInput();
                return; // Exit back to main menu
            }

            // 2. Interactive Dropdowns using Spectre.Console
            var selectedStudent = AnsiConsole.Prompt(
                new SelectionPrompt<Student>()
                    .Title("Select a [green]Student[/]:")
                    .UseConverter(s => $"{s.Id} - {s.Name}") // Formats how the object displays in the list
                    .AddChoices(students));

            var selectedTeacher = AnsiConsole.Prompt(
                new SelectionPrompt<Teacher>()
                    .Title("Select a [blue]Teacher[/]:")
                    .UseConverter(t => $"{t.Id} - {t.Name} ({t.Specialty})")
                    .AddChoices(teachers));

            var selectedRoom = AnsiConsole.Prompt(
                new SelectionPrompt<Room>()
                    .Title("Select a [yellow]Room[/]:")
                    .UseConverter(r => $"{r.Id} - {r.Name}")
                    .AddChoices(rooms));

            var selectedInstrument = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an [fuchsia]Instrument[/]:")
                    .AddChoices(instruments));

            // 3. Keep standard text input for Date and Time
            string dateInput = AnsiConsole.Ask<string>("Enter Lesson Date (MM/DD/YYYY): ");
            string timeSlot = AnsiConsole.Ask<string>("Enter Time Slot (e.g. 14:00): ");

            Console.WriteLine("\nRunning Defensive Guardrails...");

            // 4. Pass the selected IDs to your service
            var validation = _schedulingService.ValidateBooking(
                selectedStudent.Id,
                selectedTeacher.Id,
                selectedRoom.Id,
                selectedInstrument,
                DateTime.TryParse(dateInput, out var lessonDate) ? lessonDate : DateTime.Today,
                timeSlot);

            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    UiRenderer.PrintMessage(error);
                }
            }
            else
            {
                UiRenderer.PrintMessage("Student Enrolled? [PASS]");
                UiRenderer.PrintMessage("Teacher Available? [PASS]");
                UiRenderer.PrintMessage("Room Available? [PASS]");
            }

            Console.WriteLine();
            UiRenderer.DrawFooter("[S] Save Booking  [B] Back to Menu");

            string choice = Console.ReadLine()?.ToUpper() ?? string.Empty;
            if (choice == "B")
            {
                stayOnScreen = false;
            }
            else if (choice == "S")
            {
                if (!validation.IsValid)
                {
                    UiRenderer.PrintMessage("Booking was not saved because validation failed.");
                    UiRenderer.WaitForInput();
                    continue;
                }

                try
                {
                    var lesson = _schedulingService.CreateLesson(
                        selectedStudent.Id,
                        selectedTeacher.Id,
                        selectedRoom.Id,
                        selectedInstrument,
                        DateTime.TryParse(dateInput, out var savedDate) ? savedDate : DateTime.Today,
                        timeSlot);

                    UiRenderer.PrintMessage($"Booking Saved Successfully! Lesson ID: {lesson.Id}");
                    UiRenderer.WaitForInput();
                    stayOnScreen = false;
                }
                catch (Exception ex)
                {
                    UiRenderer.PrintMessage($"ERROR: {ex.Message}");
                    UiRenderer.WaitForInput();
                }
            }
        }
    }
}