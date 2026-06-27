using System;
using GraceMusic.Core.Models;
using Xunit;

namespace GraceMusic.Tests.Core;

public class DomainModelTests
{
    [Fact]
    public void Teacher_Constructor_ShouldSetDefaultBaseHours_WhenNoneProvided()
    {
        // Act
        var teacher = new Teacher("TCH-1", "John Doe", "Piano", 50m);

        // Assert
        Assert.Equal("09:00-17:00", teacher.BaseHours);
    }

    [Fact]
    public void Enrollment_Constructor_ShouldSetDefaultLevelToOne()
    {
        // Act
        var enrollment = new Enrollment("ENR-1", "STU-1", "Guitar");

        // Assert
        Assert.Equal(1, enrollment.Level);
    }

    [Fact]
    public void Room_Constructor_ShouldAssignPropertiesCorrectly()
    {
        // Act
        var room = new Room("RM-1", "Studio A", 2);

        // Assert
        Assert.Equal("RM-1", room.Id);
        Assert.Equal("Studio A", room.Name);
        Assert.Equal(2, room.Capacity);
    }
    [Fact]
    public void Payment_Constructor_ShouldAssignPropertiesCorrectly()
    {
        var date = DateTime.Today;
        var payment = new Payment("PAY-1", "STU-1", 150.00m, "July", date);

        Assert.Equal("PAY-1", payment.Id);
        Assert.Equal(150.00m, payment.Amount);
        Assert.Equal("July", payment.CoverageMonth);
    }

    [Fact]
    public void TeacherLeave_Constructor_ShouldSetDefaultTimeSlotToALL()
    {
        var leave = new TeacherLeave("LV-1", "TCH-1", DateTime.Today);
        Assert.Equal("ALL", leave.TimeSlot);
    }

    [Fact]
    public void MakeupRequest_Constructor_ShouldAssignPropertiesCorrectly()
    {
        var date = DateTime.Today;
        var makeup = new MakeupRequest("MKP-1", "STU-1", "ENR-1", date);

        Assert.Equal("MKP-1", makeup.Id);
        Assert.Equal("ENR-1", makeup.EnrollmentId);
    }

    [Fact]
    public void ConflictAlert_Constructor_ShouldAssignPropertiesCorrectly()
    {
        // Arrange & Act
        var alert = new ConflictAlert("TEST_TYPE", "Test Message", "REF-123");

        // Assert
        Assert.Equal("TEST_TYPE", alert.Type);
        Assert.Equal("Test Message", alert.Message);
        Assert.Equal("REF-123", alert.ReferenceId);
    }
}