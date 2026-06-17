using System.IO;
using System.Collections.Generic;
using System.Linq;
using GraceMusic.Core.Interfaces;

namespace GraceMusic.Infrastructure.Repositories;

public class FileRepository<T> : IRepository<T> where T : class
{
    private readonly string _filePath;
    private readonly Func<string, T> _parser;
    private readonly Func<T, string> _serializer;

    public FileRepository(string filePath, Func<string, T> parser, Func<T, string> serializer)
    {
        _filePath = filePath;
        _parser = parser;
        _serializer = serializer;
    }

    public List<T> LoadAll()
    {
        if (!File.Exists(_filePath)) return new List<T>();
        return File.ReadAllLines(_filePath).Select(_parser).ToList();
    }

public void SaveAll(List<T> data)
    {
        // 1. Extract the folder path from the full path (e.g., gets "data" from "data/students.csv")
        var directory = Path.GetDirectoryName(_filePath);

        // 2. If there is a directory in the path, make sure it exists
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 3. Now it is completely safe to write the file!
        var lines = data.Select(_serializer);
        File.WriteAllLines(_filePath, lines);
    }
}