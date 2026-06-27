using System;
using Spectre.Console;

namespace GraceMusic.UI;

public class UserCancelledException : Exception { }

public static class UiRenderer
{
    public static void DrawHeader(string title)
    {
        Console.Clear(); Console.WriteLine(new string('=', 60));
        Console.WriteLine(new string(' ', (60 - title.Length) / 2) + title);
        Console.WriteLine(new string('=', 60));
    }

    public static void PrintMessage(string message) => Console.WriteLine($"> {message}");
    
    public static void WaitForInput() { Console.WriteLine("\nPress any key to return..."); Console.ReadKey(true); }

    public static string AskString(string prompt, string defaultValue = null)
    {
        var tp = new TextPrompt<string>($"{prompt} [grey](type 'cancel' to abort)[/]:").AllowEmpty();
        if (defaultValue != null) tp.DefaultValue(defaultValue);
        
        var result = AnsiConsole.Prompt(tp);
        if (result.Trim().Equals("cancel", StringComparison.OrdinalIgnoreCase)) 
            throw new UserCancelledException();
            
        return result;
    }

    public static decimal AskDecimal(string prompt)
    {
        while (true)
        {
            string result = AskString(prompt);
            if (decimal.TryParse(result, out decimal d)) return d;
            PrintMessage("Invalid number. Please enter a valid decimal.");
        }
    }

    public static int AskInt(string prompt, int defaultVal = 1)
    {
        while (true)
        {
            var tp = new TextPrompt<string>($"{prompt} [grey](type 'cancel' to abort)[/]:").DefaultValue(defaultVal.ToString());
            var result = AnsiConsole.Prompt(tp);
            
            if (result.Trim().Equals("cancel", StringComparison.OrdinalIgnoreCase)) 
                throw new UserCancelledException();
                
            if (int.TryParse(result, out int i)) return i;
            PrintMessage("Invalid integer. Please try again.");
        }
    }
}