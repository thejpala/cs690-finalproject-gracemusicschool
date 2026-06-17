using GraceMusic.Core.Interfaces;
using GraceMusic.Core.Models;
using GraceMusic.Core.Services;

namespace GraceMusic.Tests;

public class SchedulingServiceTests
{
    [Fact]
    public void ValidateBooking_ReturnsValid_WhenStudentTeacherRoomAndInstrumentMatch()
    {
        var service = CreateService();

        var result = service.ValidateBooking(
            "S-001",
            "T-001",
            "R-001",
            "Piano",
            new DateTime(2026, 6, 20),
            "4:00 PM");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateBooking_ReturnsError_WhenTeacherDoesNotTeachInstrument()
    {
        var service = CreateService();

        var result = service.ValidateBooking(
            "S-001",
            "T-002",
            "R-001",
            "Piano",
            new DateTime(2026, 6, 20),
            "4:00 PM");

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("does not teach", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateBooking_ReturnsError_WhenRoomIsAlreadyBooked()
    {
        var service = CreateService();
        service.CreateLesson(
            "S-001",
            "T-001",
            "R-001",
            "Piano",
            new DateTime(2026, 6, 20),
            "4:00 PM");

        var result = service.ValidateBooking(
            "S-001",
            "T-001",
            "R-001",
            "Piano",
            new DateTime(2026, 6, 20),
            "4:00 PM");

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("already booked", StringComparison.OrdinalIgnoreCase));
    }

    private static SchedulingService CreateService()
    {
        var studentRepo = new TestRepository<Student>(new List<Student>
        {
            new("S-001", "Alice", "Piano", 3, "555-1010")
        });

        var teacherRepo = new TestRepository<Teacher>(new List<Teacher>
        {
            new("T-001", "Mr. Lee", "Piano", 35m),
            new("T-002", "Ms. Chen", "Violin", 40m)
        });

        var roomRepo = new TestRepository<Room>(new List<Room>
        {
            new("R-001", "Studio A", 12, true)
        });

        var lessonRepo = new TestRepository<Lesson>(new List<Lesson>());

        return new SchedulingService(studentRepo, teacherRepo, roomRepo, lessonRepo);
    }

    private sealed class TestRepository<T> : IRepository<T> where T : class
    {
        private readonly List<T> _items;

        public TestRepository(List<T> items)
        {
            _items = items;
        }

        public List<T> LoadAll() => _items;

        public void SaveAll(List<T> items)
        {
            var snapshot = new List<T>(items);
            _items.Clear();
            _items.AddRange(snapshot);
        }
    }
}
