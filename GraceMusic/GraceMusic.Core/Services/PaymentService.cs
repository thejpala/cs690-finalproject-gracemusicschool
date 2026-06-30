using System;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;

namespace GraceMusic.Core.Services;

public class PaymentService
{
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<Enrollment> _enrollmentRepo;
    
    // Baseline tuition cost per active enrollment
    private const decimal BaseTuitionPerEnrollment = 100.00m; 

    public PaymentService(IRepository<Payment> paymentRepo, IRepository<Enrollment> enrollmentRepo)
    {
        _paymentRepo = paymentRepo;
        _enrollmentRepo = enrollmentRepo;
    }

    public decimal CalculateExpectedMonthlyTuition(string studentId)
    {
        var activeEnrollments = _enrollmentRepo.LoadAll().Count(e => e.StudentId == studentId);
        return activeEnrollments * BaseTuitionPerEnrollment;
    }

    // NEW: Calculates the exact remaining balance for a specific month
    public decimal GetAmountDue(string studentId, string targetMonth)
    {
        var expectedAmount = CalculateExpectedMonthlyTuition(studentId);
        
        var amountPaid = _paymentRepo.LoadAll()
            .Where(p => p.StudentId == studentId && p.CoverageMonth.Equals(targetMonth, StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Amount);

        // Prevents negative balances if they overpay
        return expectedAmount - amountPaid > 0 ? expectedAmount - amountPaid : 0; 
    }

    // Resolves FR-2.2: Dynamic Status resolution
    public string GetStudentPaymentStatus(string studentId, string targetMonth)
    {
        var expectedAmount = CalculateExpectedMonthlyTuition(studentId);
        if (expectedAmount == 0) return "NO ACTIVE ENROLLMENTS";

        var payments = _paymentRepo.LoadAll()
            .Where(p => p.StudentId == studentId && p.CoverageMonth.Equals(targetMonth, StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Amount);

        return payments >= expectedAmount ? "CURRENT" : "OVERDUE";
    }

    // Resolves FR-2.1: Recording payment details
    public void RecordPayment(string id, string studentId, decimal amount, string coverageMonth, DateTime date)
    {
        var payments = _paymentRepo.LoadAll();
        payments.Add(new Payment(id, studentId, amount, coverageMonth, date));
        _paymentRepo.SaveAll(payments);
    }
}