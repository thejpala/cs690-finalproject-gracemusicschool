using System;
using System.Collections.Generic;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using Moq;
using Xunit;

namespace GraceMusic.Tests.Core;

public class PaymentServiceTests
{
    private readonly Mock<IRepository<Payment>> _mockPaymentRepo;
    private readonly Mock<IRepository<Enrollment>> _mockEnrollmentRepo;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _mockPaymentRepo = new Mock<IRepository<Payment>>();
        _mockEnrollmentRepo = new Mock<IRepository<Enrollment>>();
        _service = new PaymentService(_mockPaymentRepo.Object, _mockEnrollmentRepo.Object);
    }

    [Fact]
    public void CalculateExpectedMonthlyTuition_ShouldReturnCorrectAmount_BasedOnEnrollments()
    {
        // Arrange (Student 1 has 2 enrollments, Student 2 has 0)
        var enrollments = new List<Enrollment>
        {
            new Enrollment { StudentId = "STU-1" },
            new Enrollment { StudentId = "STU-1" }
        };
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(enrollments);

        // Act
        var tuition1 = _service.CalculateExpectedMonthlyTuition("STU-1");
        var tuition2 = _service.CalculateExpectedMonthlyTuition("STU-2");

        // Assert (Assuming $100 base tuition per enrollment)
        Assert.Equal(200.00m, tuition1);
        Assert.Equal(0.00m, tuition2);
    }

    [Fact]
    public void GetStudentPaymentStatus_ShouldReturnCURRENT_WhenPaidInFull()
    {
        // Arrange (1 Enrollment = $100 expected. Paid $100 for June)
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(new List<Enrollment> { new Enrollment { StudentId = "STU-1" } });
        _mockPaymentRepo.Setup(r => r.LoadAll()).Returns(new List<Payment> 
        { 
            new Payment { StudentId = "STU-1", Amount = 100.00m, CoverageMonth = "June" } 
        });

        // Act
        var status = _service.GetStudentPaymentStatus("STU-1", "June");

        // Assert
        Assert.Equal("CURRENT", status);
    }

    [Fact]
    public void GetStudentPaymentStatus_ShouldReturnOVERDUE_WhenUnderpaid()
    {
        // Arrange (2 Enrollments = $200 expected. Only paid $100 for June)
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(new List<Enrollment> 
        { 
            new Enrollment { StudentId = "STU-1" }, new Enrollment { StudentId = "STU-1" } 
        });
        
        _mockPaymentRepo.Setup(r => r.LoadAll()).Returns(new List<Payment> 
        { 
            new Payment { StudentId = "STU-1", Amount = 100.00m, CoverageMonth = "June" } 
        });

        // Act
        var status = _service.GetStudentPaymentStatus("STU-1", "June");

        // Assert
        Assert.Equal("OVERDUE", status);
    }

    [Fact]
    public void RecordPayment_ShouldAddPaymentAndTriggerSave()
    {
        // Arrange
        var payments = new List<Payment>();
        _mockPaymentRepo.Setup(r => r.LoadAll()).Returns(payments);

        // Act
        _service.RecordPayment("PAY-1", "STU-1", 100.00m, "June", DateTime.Today);

        // Assert
        Assert.Single(payments);
        _mockPaymentRepo.Verify(r => r.SaveAll(payments), Times.Once);
    }
}