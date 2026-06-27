using System;
using System.Collections.Generic;
namespace GraceMusic.Infrastructure;
public static class CsvParser
{
    public static string Escape(string value) => string.IsNullOrEmpty(value) ? string.Empty : (value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value);
    public static string[] Split(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return Array.Empty<string>();
        var values = new List<string>(); string current = ""; bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"') { if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { current += '"'; i++; } else inQuotes = !inQuotes; }
            else if (line[i] == ',' && !inQuotes) { values.Add(current); current = ""; }
            else current += line[i];
        }
        values.Add(current); return values.ToArray();
    }
}