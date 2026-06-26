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
}