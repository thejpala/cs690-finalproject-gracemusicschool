using System;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using GraceMusic.Infrastructure;
using Spectre.Console;

namespace GraceMusic.UI.Screens;

public class ProgressionScreen
{
    private readonly ProgressionService _progressionService;
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Enrollment> _enrollmentRepo;

    public ProgressionScreen(ProgressionService progressionService, IRepository<Student> studentRepo, IRepository<Enrollment> enrollmentRepo)
    {
        _progressionService = progressionService;
        _studentRepo = studentRepo;
        _enrollmentRepo = enrollmentRepo;
    }

    public void Render()
    {
        bool stay = true;
        while (stay)
        {
            UiRenderer.DrawHeader("STUDENT LEVELS & PROGRESSION");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an action:")
                    .AddChoices(
                        "1. View Student Progress Attributes",
                        "2. Update Instructor Progress Approval Flag",
                        "3. Promote Student Skill Level",
                        "9. Back to Main Menu"
                    ));

            switch (choice[0].ToString())
            {
                case "1": ViewProgressAttributes(); break;
                case "2": UpdateApprovalFlag(); break;
                case "3": PromoteStudent(); break;
                case "9": stay = false; break;
            }
        }
    }

    // Resolves FR-4.1: Viewing baseline student progress attributes
    private void ViewProgressAttributes()
    {
        var students = _studentRepo.LoadAll();
        var enrollments = _enrollmentRepo.LoadAll();

        var table = new Table().Border(TableBorder.Rounded).Title("[blue]Student Progress Evaluation[/]");
        table.AddColumns("Student Name", "Current Tier Level", "Instructor Progress Approval");

        foreach (var student in students)
        {
            var studentEnrs = enrollments.Where(e => e.StudentId == student.Id).ToList();
            if (!studentEnrs.Any()) continue;

            foreach (var enr in studentEnrs)
            {
                string tierLevel = $"Level {enr.Level} {enr.Instrument}";
                string approvalStatus = enr.IsInstructorApproved ? "[green]APPROVED[/]" : "[red]NOT APPROVED[/]";
                
                table.AddRow(student.Name, tierLevel, approvalStatus);
            }
        }

        AnsiConsole.Write(table);
        UiRenderer.WaitForInput();
    }

    // Resolves FR-4.3: Update Instructor Approval for Progression
    private void UpdateApprovalFlag()
    {
        try
        {
            var enrollment = PromptForEnrollment("Select Enrollment to Update Approval Flag:");
            
            var newStatus = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Select new evaluation state:")
                .AddChoices("APPROVED", "NOT APPROVED"));

            bool isApproved = newStatus == "APPROVED";
            
            _progressionService.UpdateApprovalStatus(enrollment.Id, isApproved);
            
            UiRenderer.PrintMessage($"[green]Success: Evaluation flag updated to {newStatus}.[/]");
            UiRenderer.WaitForInput();
        }
        catch (UserCancelledException) { }
    }

    // Resolves FR-4.2: Enforcing instructor approval for tier promotion
    private void PromoteStudent()
    {
        try
        {
            var enrollment = PromptForEnrollment("Select Student to Promote to Next Level:");
            
            bool success = _progressionService.TryPromoteStudent(enrollment.Id, out string message);

            if (success)
            {
                AnsiConsole.MarkupLine($"\n[green]{Markup.Escape(message)}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"\n[red]{Markup.Escape(message)}[/]");
            }
            UiRenderer.WaitForInput();
        }
        catch (UserCancelledException) { }
    }

    // Helper to generate a clean UI dropdown for enrollments
    private Enrollment PromptForEnrollment(string title)
    {
        var students = _studentRepo.LoadAll();
        var enrollments = _enrollmentRepo.LoadAll().ToList();

        if (!enrollments.Any())
        {
            UiRenderer.PrintMessage("No active enrollments found.");
            throw new UserCancelledException();
        }

        enrollments.Add(new Enrollment { Id = "CANCEL", Instrument = "< Cancel / Go Back >" });

        var selected = AnsiConsole.Prompt(new SelectionPrompt<Enrollment>()
            .Title(title)
            .UseConverter(e => {
                if (e.Id == "CANCEL") return e.Instrument;
                var s = students.FirstOrDefault(st => st.Id == e.StudentId);
                string name = s != null ? s.Name : "Unknown";
                string status = e.IsInstructorApproved ? "APPROVED" : "NOT APPROVED";
                return Markup.Escape($"{name} - {e.Instrument} Lvl {e.Level} [{status}]");
            })
            .AddChoices(enrollments));

        if (selected.Id == "CANCEL") throw new UserCancelledException();
        
        return selected;
    }
}