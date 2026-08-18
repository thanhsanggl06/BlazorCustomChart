namespace BlazorReportTest.Models.Dashboard;

public static class DashboardDataExtensions
{
    private const string UnknownLabel = "Unknown";

    public static string NormalizeOrUnknown(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? UnknownLabel : value;
}
