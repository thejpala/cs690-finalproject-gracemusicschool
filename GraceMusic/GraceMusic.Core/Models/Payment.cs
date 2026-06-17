namespace GraceMusic.Core.Models;

public class Payment
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidDate { get; set; } = DateTime.Today;

    public Payment()
    {
    }

    public Payment(string id, string studentId, string month, decimal amount, DateTime? paidDate = null)
    {
        Id = id;
        StudentId = studentId;
        Month = month;
        Amount = amount;
        PaidDate = paidDate ?? DateTime.Today;
    }
}
