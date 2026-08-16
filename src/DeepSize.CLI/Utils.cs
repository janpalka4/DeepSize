namespace DeepSize.CLI;

public static class Utils
{
    public static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
        int counter = 0;
        double number = bytes;

        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:F2} {suffixes[counter]}";
    }
    
    public static string TimeSpanToReadableString(TimeSpan ts)
    {
        var parts = new List<string>();
        int totalHours = (int)ts.TotalHours;

        if (totalHours > 0) parts.Add($"{totalHours}h");
        if (ts.Minutes > 0)  parts.Add($"{ts.Minutes}m");
        if (ts.Seconds > 0)  parts.Add($"{ts.Seconds}s");

        return parts.Count > 0 ? string.Join(" ", parts) : "0s";
    }
}