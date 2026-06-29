using System;
using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;
using Moq;
using Xunit;

namespace GraceMusic.Tests.Core;

public class ScheduleReportingServiceTests
{
    private readonly Mock<IRepository<Lesson>> _mockLessonRepo;
    private readonly Mock<IRepository<TeacherLeave>> _mockLeaveRepo;
    private readonly Mock<IRepository<MakeupRequest>> _mockMakeupRepo;
    private readonly Mock<IRepository<Teacher>> _mockTeacherRepo;
    private readonly Mock<IRepository<Student>> _mockStudentRepo;
    private readonly ScheduleReportingService _service;

    public ScheduleReportingServiceTests()
    {
        _mockLessonRepo = new Mock<IRepository<Lesson>>();
        _mockLeaveRepo = new Mock<IRepository<TeacherLeave>>();
        _mockMakeupRepo = new Mock<IRepository<MakeupRequest>>();
        _mockTeacherRepo = new Mock<IRepository<Teacher>>();
        _mockStudentRepo = new Mock<IRepository<Student>>();

        _service = new ScheduleReportingService(
            _mockLessonRepo.Object, _mockLeaveRepo.Object, _mockMakeupRepo.Object,
            _mockTeacherRepo.Object, _mockStudentRepo.Object);
    }

    [Fact]
    public void GetDailySchedule_ShouldReturnLessonsChronologically_ForTargetDateOnly()
    {
        // Arrange
        var targetDate = new DateTime(2026, 6, 12);
        var lessons = new List<Lesson>
        {
            new Lesson { LessonDate = targetDate, TimeSlot = "16:00" },
            new Lesson { LessonDate = targetDate.AddDays(1), TimeSlot = "09:00" }, // Wrong Date
            new Lesson { LessonDate = targetDate, TimeSlot = "14:30" }
        };
        _mockLessonRepo.Setup(r => r.LoadAll()).Returns(lessons);

        // Act
        var result = _service.GetDailySchedule(targetDate);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("14:30", result[0].TimeSlot); // Ensure chronological sort
        Assert.Equal("16:00", result[1].TimeSlot);
    }

    [Fact]
    public void GetPendingActionItems_ShouldFlagUnassignedMakeups()
    {
        // Arrange
        var targetDate = new DateTime(2026, 6, 12);
        
        _mockMakeupRepo.Setup(r => r.LoadAll()).Returns(new List<MakeupRequest>
        {
            new MakeupRequest { Id = "MKP-1", StudentId = "STU-1", TargetDate = targetDate }
        });
        _mockStudentRepo.Setup(r => r.LoadAll()).Returns(new List<Student>
        {
            new Student { Id = "STU-1", Name = "Alice Smith" }
        });

        // Provide empty lists for the data we aren't testing here
        _mockLeaveRepo.Setup(r => r.LoadAll()).Returns(new List<TeacherLeave>());
        _mockTeacherRepo.Setup(r => r.LoadAll()).Returns(new List<Teacher>());
        _mockLessonRepo.Setup(r => r.LoadAll()).Returns(new List<Lesson>());

        // Act
        var alerts = _service.GetPendingActionItems(targetDate);

        // Assert
        Assert.Single(alerts);
        Assert.Equal("UNASSIGNED_MAKEUP", alerts[0].Type);
        Assert.Contains("Alice Smith", alerts[0].Message);
        Assert.Equal("MKP-1", alerts[0].ReferenceId); // Verifying the ID passes through
    }

    [Fact]
    public void GetPendingActionItems_ShouldFlagTeacherConflicts()
    {
        // Arrange
        var targetDate = new DateTime(2026, 6, 12);
        
        _mockLessonRepo.Setup(r => r.LoadAll()).Returns(new List<Lesson>
        {
            new Lesson { Id = "LSN-1", TeacherId = "TCH-1", LessonDate = targetDate, TimeSlot = "16:00" }
        });
        
        _mockLeaveRepo.Setup(r => r.LoadAll()).Returns(new List<TeacherLeave>
        {
            new TeacherLeave { TeacherId = "TCH-1", LeaveDate = targetDate, TimeSlot = "ALL" }
        });
        
        _mockTeacherRepo.Setup(r => r.LoadAll()).Returns(new List<Teacher>
        {
            new Teacher { Id = "TCH-1", Name = "Jane Vance" }
        });

        // Provide empty lists for the data we aren't testing here
        _mockMakeupRepo.Setup(r => r.LoadAll()).Returns(new List<MakeupRequest>());
        _mockStudentRepo.Setup(r => r.LoadAll()).Returns(new List<Student>());

        // Act
        var alerts = _service.GetPendingActionItems(targetDate);

        // Assert
        Assert.Single(alerts);
        Assert.Equal("TEACHER_CONFLICT", alerts[0].Type);
        Assert.Contains("Jane Vance", alerts[0].Message);
        Assert.Equal("LSN-1", alerts[0].ReferenceId); // Verifying the ID passes through
    }
}