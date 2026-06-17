namespace GraceMusic.UI;
using System;


public static class UiRenderer
{
    private const int Width = 60;

    public static void DrawHeader(string title)
    {
        Console.Clear();
        Console.WriteLine(new string('=', Width));

        // Center the title
        int padding = (Width - title.Length) / 2;
        Console.WriteLine(new string(' ', padding) + title);

        Console.WriteLine(new string('=', Width));
    }

    public static void DrawFooter(string options)
    {
        Console.WriteLine(new string('-', Width));
        Console.WriteLine(options);
        Console.Write("Enter Selection: ");
    }

    public static void PrintMessage(string message)
    {
        Console.WriteLine($"> {message}");
    }

    public static void WaitForInput()
    {
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
    }
}
