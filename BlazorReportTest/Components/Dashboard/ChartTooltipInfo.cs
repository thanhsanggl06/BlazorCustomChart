namespace BlazorReportTest.Components.Dashboard;

/// <summary>
/// Position (as a percentage of the chart's container, so it tracks a responsive/scaled SVG)
/// and content for the floating <see cref="ChartTooltip"/> shared by the SVG-based charts.
/// </summary>
public sealed record ChartTooltipInfo(double XPercent, double YPercent, string Title, string Value, string Color);
