namespace Partivex.Application.Constants;

public static class SalesRecognitionRules
{
    public static readonly string[] RecognizedStatusValues = ["paid", "completed", "complete"];

    public static bool IsRecognized(string? status)
    {
        return !string.IsNullOrWhiteSpace(status)
            && RecognizedStatusValues.Contains(status.Trim().ToLowerInvariant());
    }
}
