using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraceMusic.Infrastructure;
using GraceMusic.Infrastructure.Repositories;
using Xunit;

namespace GraceMusic.Tests.Infrastructure;

public class DummyEntity
{
    public string Id { get; set; }
    public string Name { get; set; }

    public DummyEntity(string id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class FileRepositoryTests : IDisposable
{
    private readonly string _tempFilePath;
    private readonly FileRepository<DummyEntity> _repository;

    public FileRepositoryTests()
    {
        // 1. Create a safe, temporary file path for testing
        _tempFilePath = Path.GetTempFileName();
        
        // 2. Initialize the repository with simple CSV parsing rules
        _repository = new FileRepository<DummyEntity>(
            _tempFilePath,
            line => 
            {
                var parts = line.Split(',');
                return new DummyEntity(parts[0], parts[1]);
            },
            entity => $"{entity.Id},{entity.Name}"
        );
    }

    [Fact]
    public void LoadAll_ShouldReturnEmptyList_WhenFileDoesNotExist()
    {
        // Arrange - Delete the temp file to simulate a missing database file
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }

        // Act
        var result = _repository.LoadAll();

        // Assert - Should not crash, just returns an empty list
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void SaveAll_ShouldWriteDataToCsvFile()
    {
        // Arrange
        var dataToSave = new List<DummyEntity>
        {
            new DummyEntity("1", "Alice"),
            new DummyEntity("2", "Bob")
        };

        // Act
        _repository.SaveAll(dataToSave);

        // Assert
        Assert.True(File.Exists(_tempFilePath));
        var lines = File.ReadAllLines(_tempFilePath);
        Assert.Equal(2, lines.Length);
        Assert.Equal("1,Alice", lines[0]);
        Assert.Equal("2,Bob", lines[1]);
    }

    [Fact]
    public void LoadAll_ShouldReadDataFromCsvFile()
    {
        // Arrange - Manually write data to the file like a database would
        File.WriteAllLines(_tempFilePath, new[] { "1,Charlie", "2,Diana" });

        // Act
        var result = _repository.LoadAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].Id);
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("2", result[1].Id);
        Assert.Equal("Diana", result[1].Name);
    }

    [Fact]
    public void SaveAll_ThenLoadAll_ShouldPersistAndRetrieveCorrectly()
    {
        // Arrange
        var originalData = new List<DummyEntity>
        {
            new DummyEntity("99", "IntegrationTest")
        };

        // Act
        _repository.SaveAll(originalData);
        var loadedData = _repository.LoadAll();

        // Assert
        Assert.Single(loadedData);
        Assert.Equal("99", loadedData.First().Id);
        Assert.Equal("IntegrationTest", loadedData.First().Name);
    }

    // Cleanup method runs automatically after every test
    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}