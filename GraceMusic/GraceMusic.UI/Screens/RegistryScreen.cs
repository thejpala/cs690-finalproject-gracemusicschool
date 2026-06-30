using System;
using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Models;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Services;
using GraceMusic.Infrastructure;
using Spectre.Console;

namespace GraceMusic.UI.Screens;

public class RegistryScreen
{
    private readonly IRepository<Teacher> _teachers;
    private readonly IRepository<Room> _rooms;
    private readonly IRepository<string> _instruments;
    private readonly IRepository<Student> _students;
    private readonly IRepository<Enrollment> _enrollments;

    private readonly PaymentService _paymentService;

    public RegistryScreen(IRepository<Teacher> t, IRepository<Room> r, IRepository<string> i, IRepository<Student> s, IRepository<Enrollment> e, PaymentService ps)
    {
        _teachers = t; _rooms = r; _instruments = i; _students = s; _enrollments = e; _paymentService = ps;
    }

    public void Render()
    {
        bool stay = true;
        while (stay)
        {
            UiRenderer.DrawHeader("PORTAL: SCHOOL REGISTRY");
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a registry to manage:")
                    .AddChoices(
                        "1. Manage Teachers",
                        "2. Manage Rooms",
                        "3. Manage Instruments",
                        "4. Manage Students & Enrollments",
                        "9. Back to Main Menu"
                    ));

            switch (choice[0].ToString())
            {
                case "1": ManageTeachers(); break; 
                case "2": ManageRooms(); break;
                case "3": ManageInstruments(); break; 
                case "4": ManageStudents(); break;
                case "9": stay = false; break;
            }
        }
    }

    private string PromptForInstrument()
    {
        var instruments = _instruments.LoadAll();
        var choices = new List<string>(instruments) { "+ Add New Instrument", "< Cancel / Go Back >" };

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select an [fuchsia]Instrument[/]:")
                .AddChoices(choices));

        if (selection == "[ Cancel / Go Back ]") throw new UserCancelledException();

        if (selection == "+ Add New Instrument")
        {
            string newInst = UiRenderer.AskString("Enter new instrument name");
            if (!instruments.Contains(newInst, StringComparer.OrdinalIgnoreCase))
            {
                instruments.Add(newInst);
                _instruments.SaveAll(instruments);
                UiRenderer.PrintMessage($"'{newInst}' added to school registry.");
            }
            return newInst;
        }
        return selection;
    }

    private void ManageTeachers()
    {
        try
        {
            var data = _teachers.LoadAll();
            UiRenderer.DrawHeader("MANAGE TEACHERS");

            var action = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Action:").AddChoices("View All Teachers","Add New Teacher", "Edit Weekly Hours", "Back"));
            if (action == "View All Teachers")
            {
                var table = new Table().Border(TableBorder.Rounded).Title("[blue]Registered Teachers[/]");
                table.AddColumns("ID", "Name", "Specialty", "Rate", "Base Hours");
                foreach (var t in data) table.AddRow(t.Id, t.Name, t.Specialty, $"${t.HourlyRate:F2}", t.BaseHours);
                AnsiConsole.Write(table);
                UiRenderer.WaitForInput();
            }
            else if (action == "Add New Teacher")
            {
                string n = UiRenderer.AskString("Teacher Name");
                string s = PromptForInstrument();
                decimal r = UiRenderer.AskDecimal("Hourly Rate ($)");
                string h = UiRenderer.AskString("Base Hours", "09:00-17:00");
                
                data.Add(new Teacher(IdGenerator.Generate("TCH", data.Count), n, s, r, h));
                _teachers.SaveAll(data);
                UiRenderer.PrintMessage("Teacher saved.");
                UiRenderer.WaitForInput();
            }
            else if (action == "Edit Weekly Hours" && data.Any())
            {
                var choices = data.ToList();
                choices.Add(new Teacher { Id = "CANCEL", Name = "< Cancel / Go Back >" });

                var target = AnsiConsole.Prompt(new SelectionPrompt<Teacher>()
                    .Title("Select Teacher to edit:")
                    .UseConverter(t => t.Id == "CANCEL" ? t.Name : $"{t.Name} (Current: {t.BaseHours})")
                    .AddChoices(choices));
                if (target.Id == "CANCEL") throw new UserCancelledException();

                string newHrs = UiRenderer.AskString($"New Base Hours for {target.Name}", target.BaseHours);
                target.BaseHours = newHrs;
                _teachers.SaveAll(data);
                UiRenderer.PrintMessage("Hours updated successfully.");
                UiRenderer.WaitForInput();
            }
        }
        catch (UserCancelledException)
        {
            UiRenderer.PrintMessage("Operation cancelled. No changes saved.");
            UiRenderer.WaitForInput();
        }
    }

    private void ManageRooms()
    {
        try
        {
            var data = _rooms.LoadAll();
            UiRenderer.DrawHeader("MANAGE ROOMS");
        
            var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Action:")
                .AddChoices("View All Rooms", "Add New Room", "Back"));
            if (action == "View All Rooms")
            {
                var table = new Table().Border(TableBorder.Rounded).Title("[yellow]Facility Rooms[/]");
                table.AddColumns("ID", "Room Name", "Capacity");
                foreach (var r in data) table.AddRow(r.Id, r.Name, r.Capacity.ToString());
                AnsiConsole.Write(table);
                UiRenderer.WaitForInput();
            }
            else if (action == "Add New Room")
            {
                string n = UiRenderer.AskString("Room Name");
                int c = UiRenderer.AskInt("Capacity");
                data.Add(new Room(IdGenerator.Generate("RM", data.Count), n, c));
                _rooms.SaveAll(data);
                UiRenderer.PrintMessage("Room saved.");
            }
        }
        catch (UserCancelledException) { UiRenderer.PrintMessage("Operation cancelled."); }
    }

    private void ManageInstruments()
    {
        try 
        {
            var data = _instruments.LoadAll();
            UiRenderer.DrawHeader("MANAGE INSTRUMENTS");

            var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Action:")
                .AddChoices("View All Instruments", "Add New Instrument", "Back"));

            if (action == "View All Instruments")
            {
                var table = new Table().Border(TableBorder.Rounded).Title("[fuchsia]School Catalog[/]");
                table.AddColumn("Instrument Name");
                foreach (var i in data) table.AddRow(i);
                AnsiConsole.Write(table);
                UiRenderer.WaitForInput();
            }
            else if (action == "Add New Instrument")
            {
                string n = UiRenderer.AskString("Instrument Name");
                if (!string.IsNullOrWhiteSpace(n) && !data.Contains(n, StringComparer.OrdinalIgnoreCase)) 
                { 
                    data.Add(n); _instruments.SaveAll(data); UiRenderer.PrintMessage("Instrument saved.");
                }
            }
        } 
        catch (UserCancelledException) { UiRenderer.PrintMessage("Operation cancelled."); }
    }

    private void ManageStudents()
    {
    try
    {
        var sData = _students.LoadAll();
        var eData = _enrollments.LoadAll();
        
        UiRenderer.DrawHeader("STUDENTS & ENROLLMENTS");
        
        var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Action:")
            .AddChoices("View Students & Status", "Add New Student", "Enroll Existing Student", "Back"));

        if (action == "View Students & Status")
        {
            string currentMonth = DateTime.Now.ToString("MMMM");
            var table = new Table().Border(TableBorder.Rounded).Title($"[green]Student Roster & Billing ({currentMonth})[/]");
            
            table.AddColumns("ID", "Name", "Active Enrollments", "Billing Status", "Balance Due");

            foreach (var s in sData)
            {
                string status = _paymentService.GetStudentPaymentStatus(s.Id, currentMonth);
                string color = status == "CURRENT" ? "green" : status == "OVERDUE" ? "red" : "grey";
                
                var studentEnr = eData.Where(e => e.StudentId == s.Id).ToList();
                string enrollmentsStr = studentEnr.Any() 
                    ? string.Join(", ", studentEnr.Select(e => $"{e.Instrument} (Lvl {e.Level})"))
                    : "[grey]None[/]";

                // Fetch and display the exact dollar amount owed
                decimal balanceDue = _paymentService.GetAmountDue(s.Id, currentMonth);
                string balanceColor = balanceDue > 0 ? "red" : "green";

                table.AddRow(s.Id, s.Name, enrollmentsStr, $"[{color}]{status}[/]", $"[{balanceColor}]${balanceDue:F2}[/]");
            }
            AnsiConsole.Write(table);
            UiRenderer.WaitForInput();
        }
        else if (action == "Add New Student")
        {
            string n = UiRenderer.AskString("Student Name");
            string p = UiRenderer.AskString("Phone");
            var newStudentId = IdGenerator.Generate("STU", sData.Count);
            sData.Add(new Student(newStudentId, n, p));
            _students.SaveAll(sData);

            if (AnsiConsole.Confirm("Add initial Instrument Enrollment now?"))
            {
                string inst = PromptForInstrument();
                int lvl = UiRenderer.AskInt("Starting Level", 1);
                eData.Add(new Enrollment(IdGenerator.Generate("ENR", eData.Count), newStudentId, inst, lvl));
                _enrollments.SaveAll(eData);
            }
            UiRenderer.PrintMessage("Student workflow complete.");
            UiRenderer.WaitForInput();
        }
        else if (action == "Enroll Existing Student" && sData.Any())
        {
            var choices = sData.ToList();
            choices.Add(new Student { Id = "CANCEL", Name = "< Cancel / Go Back >" });

            var target = AnsiConsole.Prompt(new SelectionPrompt<Student>()
                .Title("Select Student:")
                .UseConverter(s => s.Id == "CANCEL" ? s.Name : $"{s.Id} - {s.Name}")
                .AddChoices(choices));
            if (target.Id == "CANCEL") throw new UserCancelledException();

            string inst = PromptForInstrument();
            
            if (eData.Any(e => e.StudentId == target.Id && e.Instrument.Equals(inst, StringComparison.OrdinalIgnoreCase)))
            {
                UiRenderer.PrintMessage($"ERROR: {target.Name} is already enrolled in {inst}.");
            }
            else
            {
                int lvl = UiRenderer.AskInt("Starting Level", 1);
                eData.Add(new Enrollment(IdGenerator.Generate("ENR", eData.Count), target.Id, inst, lvl));
                _enrollments.SaveAll(eData);
                UiRenderer.PrintMessage("Enrollment added successfully.");
            }
            UiRenderer.WaitForInput();
        }
    }
    catch (UserCancelledException)
    {
        UiRenderer.PrintMessage("Operation cancelled. No changes saved.");
        UiRenderer.WaitForInput();
    }
    }
}