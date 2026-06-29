using System;

namespace GraceMusic.Core.Models;

public class Payment
{
    public string Id { get; set; }
    public string StudentId { get; set; }
    public decimal Amount { get; set; }
    public string CoverageMonth { get; set; } // e.g., "June"
    public DateTime PaymentDate { get; set; }

    public Payment() { }

    public Payment(string id, string studentId, decimal amount, string coverageMonth, DateTime paymentDate)
    {
        Id = id;
        StudentId = studentId;
        Amount = amount;
        CoverageMonth = coverageMonth;
        PaymentDate = paymentDate;
    }
}