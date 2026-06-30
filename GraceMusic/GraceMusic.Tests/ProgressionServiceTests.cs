using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using Moq;
using Xunit;

namespace GraceMusic.Tests.Core;

public class ProgressionServiceTests
{
    private readonly Mock<IRepository<Enrollment>> _mockEnrollmentRepo;
    private readonly ProgressionService _service;

    public ProgressionServiceTests()
    {
        _mockEnrollmentRepo = new Mock<IRepository<Enrollment>>();
        _service = new ProgressionService(_mockEnrollmentRepo.Object);
    }

    [Fact]
    public void UpdateApprovalStatus_ShouldChangeFlagAndSave()
    {
        // Arrange
        var enrollments = new List<Enrollment> 
        { 
            new Enrollment { Id = "ENR-1", IsInstructorApproved = false } 
        };
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(enrollments);

        // Act
        _service.UpdateApprovalStatus("ENR-1", true);

        // Assert
        Assert.True(enrollments.First().IsInstructorApproved);
        _mockEnrollmentRepo.Verify(r => r.SaveAll(enrollments), Times.Once);
    }

    [Fact]
    public void TryPromoteStudent_ShouldDeny_IfInstructorNotApproved()
    {
        // Arrange
        var enrollments = new List<Enrollment> 
        { 
            new Enrollment { Id = "ENR-1", Level = 1, IsInstructorApproved = false } 
        };
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(enrollments);

        // Act
        bool success = _service.TryPromoteStudent("ENR-1", out string message);

        // Assert
        Assert.False(success);
        Assert.Equal("Promotion Denied. Level advancement depends explicitly on instructor progress sign-off.", message);
        Assert.Equal(1, enrollments.First().Level); // Verifies level did not change
        _mockEnrollmentRepo.Verify(r => r.SaveAll(It.IsAny<List<Enrollment>>()), Times.Never); // Verifies no save occurred
    }

    [Fact]
    public void TryPromoteStudent_ShouldPromoteAndResetFlag_IfApproved()
    {
        // Arrange
        var enrollments = new List<Enrollment> 
        { 
            new Enrollment { Id = "ENR-1", Level = 1, IsInstructorApproved = true } 
        };
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(enrollments);

        // Act
        bool success = _service.TryPromoteStudent("ENR-1", out string message);

        // Assert
        Assert.True(success);
        Assert.Contains("Success", message);
        Assert.Equal(2, enrollments.First().Level); // Verifies promotion happened!
        Assert.False(enrollments.First().IsInstructorApproved); // Verifies flag was reset!
        _mockEnrollmentRepo.Verify(r => r.SaveAll(enrollments), Times.Once); // Verifies save occurred
    }

    [Fact]
    public void TryPromoteStudent_ShouldFailGracefully_IfEnrollmentDoesNotExist()
    {
        // Arrange
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(new List<Enrollment>());

        // Act
        bool success = _service.TryPromoteStudent("NON-EXISTENT-ID", out string message);

        // Assert
        Assert.False(success);
        Assert.Equal("Error: Enrollment record not found.", message);
        _mockEnrollmentRepo.Verify(r => r.SaveAll(It.IsAny<List<Enrollment>>()), Times.Never);
    }
}