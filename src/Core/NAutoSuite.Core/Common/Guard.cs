namespace NAutoSuite.Core.Common;

/// <summary>
/// Guard clauses for parameter validation
/// </summary>
public static class Guard
{
    public static void AgainstNull<T>(T value, string parameterName) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(parameterName);
    }

    public static void AgainstNullOrEmpty(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty", parameterName);
    }

    public static void AgainstNegative(double value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentException("Value cannot be negative", parameterName);
    }

    public static void AgainstOutOfRange(double value, double min, double max, string parameterName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {min} and {max}");
    }
}
