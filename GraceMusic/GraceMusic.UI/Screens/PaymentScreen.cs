using System;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using GraceMusic.Infrastructure;
using Spectre.Console;

namespace GraceMusic.UI.Screens;

public class PaymentScreen
{
    private readonly PaymentService _paymentService;
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Payment> _paymentRepo;

    public PaymentScreen(PaymentService paymentService, IRepository<Student> studentRepo, IRepository<Payment> paymentRepo)
    {
        _paymentService = paymentService;
        _studentRepo = studentRepo;
        _paymentRepo = paymentRepo;
    }

    public void Render()
    {
        try
        {
            UiRenderer.DrawHeader("STUDENT PAYMENTS PORTAL");
            var allStudents = _studentRepo.LoadAll();
            
            if (!allStudents.Any())
            {
                UiRenderer.PrintMessage("No students registered in the system.");
                UiRenderer.WaitForInput(); return;
            }

            var studentChoices = allStudents.ToList();
            studentChoices.Add(new Student { Id = "CANCEL", Name = "< Cancel / Go Back >" });

            var student = AnsiConsole.Prompt(new SelectionPrompt<Student>()
                .Title("Select Student to log payment for:")
                .UseConverter(s => s.Id == "CANCEL" ? s.Name : $"{s.Id} - {s.Name}")
                .AddChoices(studentChoices));
                
            if (student.Id == "CANCEL") throw new UserCancelledException();

            // Ask for the month FIRST
            var monthChoices = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", "< Cancel / Go Back >" };
            var coverageMonth = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Select Coverage Month:")
                .AddChoices(monthChoices));
                
            if (coverageMonth == "< Cancel / Go Back >") throw new UserCancelledException();

            // NEW: Calculate dynamic balances based on the selected month
            decimal expectedTuition = _paymentService.CalculateExpectedMonthlyTuition(student.Id);
            decimal amountDue = _paymentService.GetAmountDue(student.Id, coverageMonth);
            decimal amountPaid = expectedTuition > 0 ? (expectedTuition - amountDue) : 0;

            // Render a clean Account Summary Panel
            AnsiConsole.WriteLine();
            var panel = new Panel(
                $"Expected Tuition: ${expectedTuition:F2}\n" +
                $"Amount Paid:      ${amountPaid:F2}\n" +
                $"[bold {(amountDue > 0 ? "red" : "green")}]Remaining Due:    ${amountDue:F2}[/]")
                .Header($"[bold blue]{student.Name} - {coverageMonth} Account Summary[/]")
                .Padding(1, 1, 1, 1)
                .RoundedBorder();
            AnsiConsole.Write(panel);

            // Prevent accidental double-charges
            if (amountDue == 0)
            {
                if (!AnsiConsole.Confirm("\n[green]This student is fully paid for this month.[/] Do you still want to log an overpayment/credit?"))
                {
                    return;
                }
            }

            decimal amount = UiRenderer.AskDecimal($"\nEnter Payment Amount for {coverageMonth}");
            
            var payments = _paymentRepo.LoadAll();
            string newId = IdGenerator.Generate("PAY", payments.Count);
            
            _paymentService.RecordPayment(newId, student.Id, amount, coverageMonth, DateTime.Now);
            
            UiRenderer.PrintMessage($"SUCCESS: Payment of ${amount:F2} for {coverageMonth} logged successfully!");
            UiRenderer.WaitForInput();
        }
        catch (UserCancelledException)
        {
            UiRenderer.PrintMessage("Operation cancelled. Returning to menu...");
            UiRenderer.WaitForInput();
        }
    }
}