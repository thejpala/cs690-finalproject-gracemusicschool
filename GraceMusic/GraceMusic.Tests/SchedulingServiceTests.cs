using System;
using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using Moq;
using Xunit;

namespace GraceMusic.Tests.Core;

public class SchedulingServiceTests
{
    private readonly Mock<IRepository<Lesson>> _mockLessonRepo;
    private readonly Mock<IRepository<Enrollment>> _mockEnrollmentRepo;
    private readonly SchedulingService _service;

    public SchedulingServiceTests()
    {
        _mockLessonRepo = new Mock<IRepository<Lesson>>();
        _mockEnrollmentRepo = new Mock<IRepository<Enrollment>>();
        _service = new SchedulingService(_mockLessonRepo.Object, _mockEnrollmentRepo.Object);
    }

    [Fact]
    public void GetAvailableTimeSlots_ShouldReturnAllSlots_WhenNoConflictsExist()
    {
        // Arrange
        var teacher = new Teacher { Id = "TCH-1", BaseHours = "10:00-11:30" }; // 3 slots: 10:00, 10:30, 11:00
        var room = new Room { Id = "RM-1" };
        var studentId = "STU-1";
        var date = DateTime.Today;

        _mockLessonRepo.Setup(r => r.LoadAll()).Returns(new List<Lesson>());
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(new List<Enrollment>());

        // Act
        var slots = _service.GetAvailableTimeSlots(teacher, room, studentId, date);

        // Assert
        Assert.Equal(3, slots.Count);
        Assert.Contains("10:00", slots);
        Assert.Contains("10:30", slots);
        Assert.Contains("11:00", slots);
    }

    [Fact]
    public void GetAvailableTimeSlots_ShouldFilterOutTeacherConflicts()
    {
        // Arrange
        var teacher = new Teacher { Id = "TCH-1", BaseHours = "10:00-11:30" };
        var room = new Room { Id = "RM-1" };
        var studentId = "STU-1";
        var date = DateTime.Today;

        var existingLessons = new List<Lesson>
        {
            new Lesson { TeacherId = "TCH-1", LessonDate = date, TimeSlot = "10:30" }
        };

        _mockLessonRepo.Setup(r => r.LoadAll()).Returns(existingLessons);
        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(new List<Enrollment>());

        // Act
        var slots = _service.GetAvailableTimeSlots(teacher, room, studentId, date);

        // Assert
        Assert.Equal(2, slots.Count);
        Assert.DoesNotContain("10:30", slots); // Successfully filtered out
    }

    [Fact]
    public void GetAvailableTimeSlots_ShouldFilterOutStudentConflicts()
    {
        // Arrange
        var teacher = new Teacher { Id = "TCH-1", BaseHours = "10:00-11:30" };
        var room = new Room { Id = "RM-1" };
        var studentId = "STU-1";
        var date = DateTime.Today;

        // Student is already taking a Guitar lesson at 11:00
        var enrollments = new List<Enrollment> { new Enrollment { Id = "ENR-1", StudentId = studentId } };
        var existingLessons = new List<Lesson> { new Lesson { EnrollmentId = "ENR-1", LessonDate = date, TimeSlot = "11:00" } };

        _mockEnrollmentRepo.Setup(r => r.LoadAll()).Returns(enrollments);
        _mockLessonRepo.Setup(r => r.LoadAll()).Returns(existingLessons);

        // Act
        var slots = _service.GetAvailableTimeSlots(teacher, room, studentId, date);

        // Assert
        Assert.Equal(2, slots.Count);
        Assert.DoesNotContain("11:00", slots); // Prevented student double-booking
    }

    [Fact]
    public void BookLesson_ShouldAddLessonAndSave()
    {
        // Arrange
        var lessons = new List<Lesson>();
        _mockLessonRepo.Setup(r => r.LoadAll()).Returns(lessons);

        // Act
        _service.BookLesson("ENR-1", "TCH-1", "RM-1", DateTime.Today, "14:00");

        // Assert
        Assert.Single(lessons);
        Assert.Equal("ENR-1", lessons.First().EnrollmentId);
        _mockLessonRepo.Verify(r => r.SaveAll(lessons), Times.Once); // Verifies the file write was triggered
    }

    [Fact]
    public void CancelLesson_ShouldRemoveLessonAndSave()
    {
        // Arrange
        var lesson = new Lesson { Id = "LSN-1" };
        var lessons = new List<Lesson> { lesson };
        _mockLessonRepo.Setup(r => r.LoadAll()).Returns(lessons);

        // Act
        _service.CancelLesson("LSN-1");

        // Assert
        Assert.Empty(lessons);
        _mockLessonRepo.Verify(r => r.SaveAll(lessons), Times.Once);
    }
}