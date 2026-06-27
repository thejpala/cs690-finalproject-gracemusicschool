using System;

namespace GraceMusic.Infrastructure;

public static class IdGenerator
{
    // Takes the prefix and the number of existing items to create the next sequence
    public static string Generate(string prefix, int currentCount)
    {
        return $"{prefix}-{(currentCount + 1):D3}";
    }
}