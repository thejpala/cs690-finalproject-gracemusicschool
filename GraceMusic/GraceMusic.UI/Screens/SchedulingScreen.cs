using System;
using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using Spectre.Console;

namespace GraceMusic.UI.Screens;

public class SchedulingScreen
{
    private readonly SchedulingService _service;
    private readonly IRepository<Student> _students;
    private readonly IRepository<Teacher> _teachers;
    private readonly IRepository<Room> _rooms;
    private readonly IRepository<Enrollment> _enrollments;
    private readonly IRepository<Lesson> _lessons;

    public SchedulingScreen(
        IRepository<Student> s, IRepository<Teacher> t, IRepository<Room> r, 
        IRepository<Enrollment> e, IRepository<Lesson> l)
    {
        _students = s; _teachers = t; _rooms = r; _enrollments = e; _lessons = l;
        _service = new SchedulingService(l, e);
    }

    public void Render()
    {
        bool stay = true;
        while (stay)
        {
            UiRenderer.DrawHeader("LESSON SCHEDULING & MANAGEMENT");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an action:")
                    .AddChoices(
                        "1. Book a Lesson (Smart Schedule)",
                        "2. Check Teacher Availability Slots",
                        "3. Cancel/Remove Scheduled Lesson",
                        "9. Back to Main Menu"
                    ));

            switch (choice[0].ToString())
            {
                case "1": BookLesson(); break;
                case "2": CheckTeacherAvailability(); break;
                case "3": CancelScheduledLesson(); break;
                case "9": stay = false; break;
            }
        }
    }

    private void BookLesson()
    {
        try
        {
            var allStudents = _students.LoadAll();
            var allTeachers = _teachers.LoadAll();
            var allRooms = _rooms.LoadAll();

            if (!allStudents.Any() || !allTeachers.Any() || !allRooms.Any())
            {
                UiRenderer.PrintMessage("System requires at least 1 Student, Teacher, and Room registered.");
                UiRenderer.WaitForInput(); return;
            }

            var studentChoices = allStudents.ToList();
            studentChoices.Add(new Student { Id = "CANCEL", Name = "< Cancel / Go Back >" });

            var student = AnsiConsole.Prompt(new SelectionPrompt<Student>()
                .Title("Select a [green]Student[/]:")
                .UseConverter(s => s.Id == "CANCEL" ? s.Name : $"{s.Id} - {s.Name}")
                .AddChoices(studentChoices));
            if (student.Id == "CANCEL") throw new UserCancelledException();

            var activeEnrollments = _enrollments.LoadAll().Where(e => e.StudentId == student.Id).ToList();
            if (!activeEnrollments.Any())
            {
                UiRenderer.PrintMessage("ERROR: This student is not enrolled in any instruments yet.");
                UiRenderer.WaitForInput(); return;
            }
            activeEnrollments.Add(new Enrollment { Id = "CANCEL", Instrument = "< Cancel / Go Back >" });

            var enrollment = AnsiConsole.Prompt(new SelectionPrompt<Enrollment>()
                .Title($"Select an [fuchsia]Instrument[/] for {student.Name}:")
                .UseConverter(e => e.Id == "CANCEL" ? e.Instrument : $"{e.Instrument} (Level {e.Level})")
                .AddChoices(activeEnrollments));
            if (enrollment.Id == "CANCEL") throw new UserCancelledException();

            var matchingTeachers = allTeachers.Where(t => t.Specialty.Equals(enrollment.Instrument, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!matchingTeachers.Any())
            {
                UiRenderer.PrintMessage($"ERROR: There are currently no teachers registered who specialize in {enrollment.Instrument}.");
                UiRenderer.WaitForInput(); return;
            }
            matchingTeachers.Add(new Teacher { Id = "CANCEL", Name = "< Cancel / Go Back >" });

            var teacher = AnsiConsole.Prompt(new SelectionPrompt<Teacher>()
                .Title($"Select a [blue]Teacher[/] for {enrollment.Instrument}:")
                .UseConverter(t => t.Id == "CANCEL" ? t.Name : $"{t.Name} (Hours: {t.BaseHours})")
                .AddChoices(matchingTeachers));
            if (teacher.Id == "CANCEL") throw new UserCancelledException();

            var roomChoices = allRooms.ToList();
            roomChoices.Add(new Room { Id = "CANCEL", Name = "< Cancel / Go Back >" });
            
            var room = AnsiConsole.Prompt(new SelectionPrompt<Room>()
                .Title("Select a [yellow]Room[/]:")
                .UseConverter(r => r.Id == "CANCEL" ? r.Name : $"{r.Name} (Capacity: {r.Capacity})")
                .AddChoices(roomChoices));
            if (room.Id == "CANCEL") throw new UserCancelledException();

            string dateInput = UiRenderer.AskString("Enter Lesson Date (MM/DD/YYYY)");
            if (!DateTime.TryParse(dateInput, out DateTime lessonDate)) lessonDate = DateTime.Today;

            var availableSlots = _service.GetAvailableTimeSlots(teacher, room, student.Id, lessonDate);
            if (!availableSlots.Any())
            {
                UiRenderer.PrintMessage($"CRITICAL CONFLICT: No common time slots available for {teacher.Name}, {room.Name}, and {student.Name} on this date.");
                UiRenderer.WaitForInput(); return;
            }
            availableSlots.Add("< Cancel / Go Back >");

            var timeSlot = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Select an [cyan]Available Time Slot[/]:").AddChoices(availableSlots));
            if (timeSlot == "< Cancel / Go Back >") throw new UserCancelledException();
            
            _service.BookLesson(enrollment.Id, teacher.Id, room.Id, lessonDate, timeSlot);
            UiRenderer.PrintMessage($"SUCCESS: Lesson booked safely for {timeSlot}!");
            UiRenderer.WaitForInput();
        }
        catch (UserCancelledException)
        {
            UiRenderer.PrintMessage("Operation cancelled. Returning to menu...");
            UiRenderer.WaitForInput();
        }
    }

    private void CheckTeacherAvailability()
    {
        try 
        {
            var allTeachers = _teachers.LoadAll();
            if (!allTeachers.Any()) { UiRenderer.PrintMessage("No teachers registered."); UiRenderer.WaitForInput(); return; }

            var teacherChoices = allTeachers.ToList();
            teacherChoices.Add(new Teacher { Id = "CANCEL", Name = "< Cancel / Go Back >" });

            var teacher = AnsiConsole.Prompt(new SelectionPrompt<Teacher>()
                .Title("Select a [blue]Teacher[/]:")
                .UseConverter(t => t.Id == "CANCEL" ? t.Name : $"{t.Name} ({t.Specialty}) (Hours: {t.BaseHours})")
                .AddChoices(teacherChoices));
            if (teacher.Id == "CANCEL") throw new UserCancelledException();

            string dateInput = UiRenderer.AskString("Enter Date to Check (MM/DD/YYYY)");
            if (!DateTime.TryParse(dateInput, out DateTime checkDate)) checkDate = DateTime.Today;

            var freeSlots = _service.GetTeacherOnlyAvailableSlots(teacher, checkDate);
            
            Console.WriteLine($"\n--- Availability for {teacher.Name} on {checkDate:d} ---");
            if (!freeSlots.Any()) Console.WriteLine("  [Fully Booked or Unavailable]");
            else freeSlots.ForEach(slot => Console.WriteLine($"  [OPEN] - {slot}"));
            Console.WriteLine("---------------------------------------------------");
            UiRenderer.WaitForInput();
        }
        catch (UserCancelledException)
        {
            UiRenderer.PrintMessage("Operation cancelled. Returning to menu...");
            UiRenderer.WaitForInput();
        }
    }

    private void CancelScheduledLesson()
    {
        try 
        {
            var lessons = _lessons.LoadAll();
            if (!lessons.Any())
            {
                UiRenderer.PrintMessage("There are currently no scheduled lessons to cancel.");
                UiRenderer.WaitForInput(); return;
            }

            var teacherData = _teachers.LoadAll();
            var choices = new List<Lesson>(lessons.OrderBy(l => l.LessonDate).ThenBy(l => l.TimeSlot));
            choices.Add(new Lesson { Id = "CANCEL" });
            
            var lessonToCancel = AnsiConsole.Prompt(
                new SelectionPrompt<Lesson>()
                    .Title("Select a scheduled lesson to [red]Cancel/Remove[/]:")
                    .UseConverter(l => 
                    {
                        if (l.Id == "CANCEL") return "< Cancel / Go Back >";
                        var tName = teacherData.FirstOrDefault(t => t.Id == l.TeacherId)?.Name ?? "Unknown";
                        return $"{l.LessonDate:MM/dd/yyyy} @ {l.TimeSlot} | Teacher: {tName} | ID: {l.Id}";
                    })
                    .AddChoices(choices));
            
            if (lessonToCancel.Id == "CANCEL") throw new UserCancelledException();

            if (AnsiConsole.Confirm($"Are you sure you want to cancel Lesson {lessonToCancel.Id}?"))
            {
                _service.CancelLesson(lessonToCancel.Id);
                UiRenderer.PrintMessage("Lesson successfully cancelled and removed from the system.");
            }
            UiRenderer.WaitForInput();
        }
        catch (UserCancelledException)
        {
            UiRenderer.PrintMessage("Operation cancelled. Returning to menu...");
            UiRenderer.WaitForInput();
        }
    }
}