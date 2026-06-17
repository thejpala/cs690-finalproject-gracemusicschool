using System;
using GraceMusic.Core.Models;
using GraceMusic.Core.Interfaces;
namespace GraceMusic.UI.Screens;

public class PaymentScreen
{
   private readonly IRepository<Payment> _paymentsRepo;

    public PaymentScreen(IRepository<Payment> paymentsRepo){
            _paymentsRepo = paymentsRepo;
    }

    public void Render()
    {
        bool stayOnScreen = true;

        while (stayOnScreen)
        {
            UiRenderer.DrawHeader("PORTAL: PAYMENT PROCESSING");

            Console.Write("  Enter Student ID     : ");
            string studentId = Console.ReadLine() ?? string.Empty;

            Console.Write("  Enter Payment Month  : ");
            string month = Console.ReadLine() ?? string.Empty;

            Console.Write("  Enter Amount Paid ($): ");
            string amountStr = Console.ReadLine() ?? string.Empty;

            if (decimal.TryParse(amountStr, out decimal amount))
            {
                var payment = new Payment(
                    id: $"PAY-{Guid.NewGuid().ToString("N")[..8]}",
                    studentId: studentId,
                    month: month,
                    amount: amount);
                var payments = _paymentsRepo.LoadAll();
                payments.Add(payment);
                _paymentsRepo.SaveAll(payments);
                
                Console.WriteLine();
                UiRenderer.PrintMessage($"Payment of ${amount:F2} for {month} successfully logged for Student {studentId}.");
                UiRenderer.PrintMessage($"Receipt ID: {payment.Id}");
            }
            else
            {
                UiRenderer.PrintMessage("ERROR: Invalid amount format. Please enter numbers only.");
            }

            Console.WriteLine();
            UiRenderer.DrawFooter("[A] Log Another Payment  [B] Back to Menu");

            string choice = Console.ReadLine()?.ToUpper() ?? string.Empty;
            if (choice == "B")
            {
                stayOnScreen = false;
            }
        }
    }
}