namespace BlazorReportTest.Models.Dashboard;

/// <summary>
/// One column in the bar chart's "by Type" mode: a single current-year month broken
/// down into its Type segments (e.g. Mail, SSM), stacked in a fixed, chart-wide order.
/// </summary>
public class MonthlyTypeBreakdownPoint
{
    public required int Year { get; init; }

    public required int Month { get; init; }

    public required List<DonutItem> TypeCounts { get; init; }

    public string MonthLabel => new DateTime(Year, Month, 1).ToString("MMM yyyy");

    public double Total => TypeCounts.Sum(t => t.Value);
}
