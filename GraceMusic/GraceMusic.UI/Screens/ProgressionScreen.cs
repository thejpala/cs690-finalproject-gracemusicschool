using System;
using GraceMusic.Core.Models;
using GraceMusic.Core.Interfaces;

namespace GraceMusic.UI.Screens;

public class ProgressionScreen
{
    private readonly IRepository<Progression> _progressionRepo;

    public ProgressionScreen(IRepository<Progression> progressionRepo){
            _progressionRepo = progressionRepo;
    }

    public void Render()
    {
        bool stayOnScreen = true;

        while (stayOnScreen)
        {
            UiRenderer.DrawHeader("PORTAL: STUDENT PROGRESSION");

            Console.Write("  Enter Student ID to lookup: ");
            string studentId = Console.ReadLine() ?? string.Empty;
            var progressions = _progressionRepo.LoadAll();
            var records = progressions.Where(p => p.StudentId == studentId).ToList();
            Console.WriteLine($"\n  Current Records for Student: {studentId}");
            Console.WriteLine("  --------------------------------------");

            if (records.Count == 0)
            {
                Console.WriteLine("  No progression records found.");
            }
            else
            {
                foreach (var record in records)
                {
                    Console.WriteLine($"  Instrument: {record.Instrument,-10} | Level: {record.Level}");
                }
            }

            Console.WriteLine("  --------------------------------------");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  1. Update Level");
            Console.WriteLine("  2. Back to Menu");

            string choice = Console.ReadLine() ?? string.Empty;

            if (choice == "1")
            {
                Console.Write("  Enter Instrument to update: ");
                string instrument = Console.ReadLine() ?? string.Empty;
                Console.Write("  Enter new Level (1-10): ");
                string levelInput = Console.ReadLine() ?? string.Empty;

                if (int.TryParse(levelInput, out int level) && level >= 1 && level <= 10)
                {
                    var record = records.FirstOrDefault(p => p.Instrument.Equals(instrument, StringComparison.OrdinalIgnoreCase));
                    if (record == null)
                    {
                        progressions.Add(new Progression(studentId, instrument, level));
                        _progressionRepo.SaveAll(progressions);
                    }
                    else
                    {
                        record.Level = level;
                        record.LastUpdated = DateTime.Today;
                    }

                    UiRenderer.PrintMessage($"{instrument} level updated to {level}.");
                    UiRenderer.WaitForInput();
                }
                else
                {
                    UiRenderer.PrintMessage("ERROR: Level must be a number from 1 to 10.");
                    UiRenderer.WaitForInput();
                }
            }
            else if (choice == "2")
            {
                stayOnScreen = false;
            }
        }
    }
}