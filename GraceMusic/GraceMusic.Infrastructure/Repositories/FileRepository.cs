using System;
using System.Collections.Generic;
using System.IO;
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
        _filePath = filePath; _parser = parser; _serializer = serializer;
    }

    public List<T> LoadAll()
    {
        if (!File.Exists(_filePath)) return new List<T>();
        return File.ReadAllLines(_filePath).Select(_parser).ToList();
    }

    public void SaveAll(List<T> data)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
        File.WriteAllLines(_filePath, data.Select(_serializer));
    }
}