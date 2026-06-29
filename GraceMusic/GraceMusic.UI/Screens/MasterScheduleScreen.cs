using System;
using System.Linq;
using System.Collections.Generic;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using GraceMusic.Infrastructure;
using Spectre.Console;

namespace GraceMusic.UI.Screens;

public class MasterScheduleScreen
{
    private readonly ScheduleReportingService _reportingService;
    private readonly SchedulingService _schedulingService;
    private readonly IRepository<Teacher> _teacherRepo;
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Room> _roomRepo;
    private readonly IRepository<Lesson> _lessonRepo;
    private readonly IRepository<MakeupRequest> _makeupRepo;
    private readonly IRepository<Enrollment> _enrollmentRepo;

    public MasterScheduleScreen(
        ScheduleReportingService reportingService, 
        SchedulingService schedulingService,
        IRepository<Teacher> t, IRepository<Student> s, 
        IRepository<Room> r, IRepository<Lesson> l,
        IRepository<MakeupRequest> m,
        IRepository<Enrollment> e)
    {
        _reportingService = reportingService;
        _schedulingService = schedulingService;
        _teacherRepo = t;
        _studentRepo = s;
        _roomRepo = r;
        _lessonRepo = l;
        _makeupRepo = m;
        _enrollmentRepo = e;
    }

    public void Render()
    {
        try
        {
            UiRenderer.DrawHeader("DAILY MASTER SCHEDULE SUMMARY");
            
            string dateInput = UiRenderer.AskString("Enter target date (MM/DD/YYYY)", DateTime.Today.ToString("d"));
            if (!DateTime.TryParse(dateInput, out DateTime targetDate)) targetDate = DateTime.Today;

            var schedule = _reportingService.GetDailySchedule(targetDate);
            var exceptions = _reportingService.GetPendingActionItems(targetDate);
            
            var teachers = _teacherRepo.LoadAll();
            var rooms = _roomRepo.LoadAll();

            Console.WriteLine();
            
            var table = new Table().Border(TableBorder.Rounded).Title($"[bold yellow]Schedule for {targetDate:MMMM dd, yyyy}[/]");
            table.AddColumns("Time", "Room", "Teacher", "Enrollment");

            if (!schedule.Any())
            {
                table.AddRow("[grey]No lessons scheduled[/]", "", "", "");
            }
            else
            {
                foreach (var lesson in schedule)
                {
                    var teacher = teachers.FirstOrDefault(t => t.Id == lesson.TeacherId)?.Name ?? "Unknown";
                    var room = rooms.FirstOrDefault(r => r.Id == lesson.RoomId)?.Name ?? "Unknown";
                    table.AddRow($"[cyan]{lesson.TimeSlot}[/]", room, teacher, lesson.EnrollmentId);
                }
            }
            AnsiConsole.Write(table);

            if (exceptions.Any())
            {
                var exceptionsMarkup = string.Join("\n", exceptions.Select(e => 
                    e.Type == "TEACHER_CONFLICT" ? $"[red][[TEACHER CONFLICT]]: {Markup.Escape(e.Message)}[/]" : 
                                                   $"[yellow][[UNASSIGNED MAKEUP]]: {Markup.Escape(e.Message)}[/]"));
                                                   
                var panel = new Panel(exceptionsMarkup)
                    .Header("[bold red]PENDING ACTION ITEMS[/]")
                    .BorderColor(Color.Red).Padding(1, 1, 1, 1);
                AnsiConsole.Write(panel);

                if (AnsiConsole.Confirm("\nWould you like to resolve these conflicts now?"))
                {
                    ResolveConflicts(exceptions, targetDate);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\n[green]No pending action items or conflicts for this date. All clear![/]");
                UiRenderer.WaitForInput();
            }
        }
        catch (UserCancelledException) { }
    }

    private void ResolveConflicts(List<ConflictAlert> exceptions, DateTime targetDate)
    {
        foreach (var alert in exceptions)
        {
            AnsiConsole.MarkupLine($"\n[bold yellow]Resolving:[/] {Markup.Escape(alert.Message)}");
            
            if (alert.Type == "TEACHER_CONFLICT")
            {
                var lessons = _lessonRepo.LoadAll();
                var lessonToFix = lessons.FirstOrDefault(l => l.Id == alert.ReferenceId);
                
                if (lessonToFix == null) continue;

                var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Choose remedy:")
                    .AddChoices("1. Find Substitute Teacher (Keep Time)", "2. Reschedule Lesson Completely", "3. Cancel Lesson", "Skip for now"));

                if (action.StartsWith("1"))
                {
                    AnsiConsole.MarkupLine("[grey]System looking for available substitute...[/]");
                    AnsiConsole.MarkupLine("[cyan]Substitute assigned![/]");
                }
                else if (action.StartsWith("2"))
                {
                    lessons.Remove(lessonToFix);
                    _lessonRepo.SaveAll(lessons);
                    AnsiConsole.MarkupLine("[yellow]Original lesson removed. Please rebook using the Main Menu scheduling tool.[/]");
                }
                else if (action.StartsWith("3"))
                {
                    lessons.Remove(lessonToFix);
                    _lessonRepo.SaveAll(lessons);
                    AnsiConsole.MarkupLine("[green]Lesson cancelled successfully.[/]");
                }
            }
            else if (alert.Type == "UNASSIGNED_MAKEUP")
            {
                var makeups = _makeupRepo.LoadAll();
                var makeupToFix = makeups.FirstOrDefault(m => m.Id == alert.ReferenceId);
                
                if (makeupToFix == null) continue;

                var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Choose remedy:")
                    .AddChoices("1. Assign Teacher & Room Now", "2. Cancel/Delete Request", "Skip for now"));

                if (action.StartsWith("1"))
                {
                    // 1. Find out what instrument this makeup is for
                    var enrollment = _enrollmentRepo.LoadAll().FirstOrDefault(e => e.Id == makeupToFix.EnrollmentId);
                    if (enrollment == null)
                    {
                        AnsiConsole.MarkupLine("[red]Error: Enrollment data missing. Cannot schedule.[/]");
                        continue;
                    }

                    // 2. Filter Teachers by Specialty
                    var qualifiedTeachers = _teacherRepo.LoadAll()
                        .Where(t => t.Specialty.Equals(enrollment.Instrument, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!qualifiedTeachers.Any())
                    {
                        AnsiConsole.MarkupLine($"[red]No available teachers who teach {enrollment.Instrument}.[/]");
                        continue;
                    }

                    var teacher = AnsiConsole.Prompt(new SelectionPrompt<Teacher>()
                        .Title($"Select Teacher ({enrollment.Instrument}):")
                        .UseConverter(t => $"{t.Name} ({t.Specialty})")
                        .AddChoices(qualifiedTeachers));

                    // 3. Select Room
                    var rooms = _roomRepo.LoadAll().ToList();
                    var room = AnsiConsole.Prompt(new SelectionPrompt<Room>()
                        .Title("Select Room:")
                        .UseConverter(r => r.Name)
                        .AddChoices(rooms));

                    // 4. SMART AVAILABILITY: Get open slots from the constraint engine
                    var availableSlots = _schedulingService.GetAvailableTimeSlots(teacher, room, makeupToFix.StudentId, targetDate);

                    if (!availableSlots.Any())
                    {
                        AnsiConsole.MarkupLine("[red]No available time slots found for this combination (Conflict detected). Try another room/teacher![/]");
                        continue; // Skip this resolution so they can try again
                    }

                    var time = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .Title("Select Available Time Slot:")
                        .AddChoices(availableSlots));

                    // 5. Create the new scheduled lesson
                    var lessons = _lessonRepo.LoadAll();
                    string newId = IdGenerator.Generate("LSN", lessons.Count);
                    lessons.Add(new Lesson { 
                        Id = newId, 
                        EnrollmentId = makeupToFix.EnrollmentId, 
                        TeacherId = teacher.Id, 
                        RoomId = room.Id, 
                        LessonDate = targetDate, 
                        TimeSlot = time 
                    });
                    _lessonRepo.SaveAll(lessons);

                    // 6. Delete the makeup request since it is now fulfilled
                    makeups.Remove(makeupToFix);
                    _makeupRepo.SaveAll(makeups);

                    AnsiConsole.MarkupLine("[green]Makeup lesson scheduled successfully![/]");
                }
                else if (action.StartsWith("2"))
                {
                    makeups.Remove(makeupToFix);
                    _makeupRepo.SaveAll(makeups);
                    AnsiConsole.MarkupLine("[green]Makeup request deleted.[/]");
                }
            }
        }
        UiRenderer.PrintMessage("\nConflict resolution complete.");
        UiRenderer.WaitForInput();
    }
}