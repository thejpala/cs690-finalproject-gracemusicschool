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

            decimal expectedTuition = _paymentService.CalculateExpectedMonthlyTuition(student.Id);
            UiRenderer.PrintMessage($"Expected Monthly Tuition for {student.Name}: ${expectedTuition:F2}");

            var monthChoices = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", "< Cancel / Go Back >" };
            string defaultMonth = DateTime.Now.ToString("MMMM");

            var coverageMonth = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Select Coverage Month:")
                .AddChoices(monthChoices));
                
            if (coverageMonth == "< Cancel / Go Back >") throw new UserCancelledException();

            decimal amount = UiRenderer.AskDecimal($"Enter Payment Amount for {coverageMonth}");
            
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