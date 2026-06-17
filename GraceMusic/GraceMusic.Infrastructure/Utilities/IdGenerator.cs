namespace GraceMusic.Infrastructure;

public static class IdGenerator
{
    private static int _counter;

    public static string Generate(string prefix)
    {
        _counter++;
        return $"{prefix}-{DateTime.Now:yyyyMMddHHmmss}-{_counter:D3}";
    }
}
