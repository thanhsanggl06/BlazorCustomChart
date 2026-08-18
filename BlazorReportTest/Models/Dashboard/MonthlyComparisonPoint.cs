namespace BlazorReportTest.Models.Dashboard;

/// <summary>
/// One column group in the monthly bar chart: a month paired with its current-year
/// and previous-year record counts.
/// </summary>
public class MonthlyComparisonPoint
{
    public required int Year { get; init; }

    public required int Month { get; init; }

    public required int CurrentYearValue { get; init; }

    public required int PreviousYear { get; init; }

    public required int PreviousYearValue { get; init; }

    public string MonthLabel => new DateTime(Year, Month, 1).ToString("MMM yyyy");
}
