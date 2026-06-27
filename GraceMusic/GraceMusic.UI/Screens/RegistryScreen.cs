using System;
using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Models;
using GraceMusic.Core.Interfaces;
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

    public RegistryScreen(IRepository<Teacher> t, IRepository<Room> r, IRepository<string> i, IRepository<Student> s, IRepository<Enrollment> e)
    {
        _teachers = t; _rooms = r; _instruments = i; _students = s; _enrollments = e;
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
            foreach (var t in data) Console.WriteLine($"[{t.Id}] {t.Name,-15} | {t.Specialty,-10} | Hours: {t.BaseHours}");
            Console.WriteLine();

            var action = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Action:").AddChoices("Add New Teacher", "Edit Weekly Hours", "Back"));

            if (action == "Add New Teacher")
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
            foreach (var r in data) Console.WriteLine($"[{r.Id}] {r.Name,-15} | Cap: {r.Capacity}");
            Console.WriteLine();

            if (AnsiConsole.Confirm("Add new Room?"))
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
        var data = _instruments.LoadAll();
        UiRenderer.DrawHeader("MANAGE INSTRUMENTS");
        foreach (var i in data) Console.WriteLine($"- {i}");
        Console.WriteLine();

        try 
        {
            if (AnsiConsole.Confirm("Add new Instrument?"))
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
            foreach (var s in sData)
            {
                Console.WriteLine($"[{s.Id}] {s.Name}");
                var studentEnr = eData.Where(e => e.StudentId == s.Id).ToList();
                if (!studentEnr.Any()) Console.WriteLine("   -> (No active enrollments)");
                foreach (var e in studentEnr) Console.WriteLine($"   -> Enrolled: {e.Instrument} (Level {e.Level})");
            }
            Console.WriteLine();

            var action = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Action:").AddChoices("Add New Student", "Enroll Existing Student", "Back"));

            if (action == "Add New Student")
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