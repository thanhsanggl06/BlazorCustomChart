namespace BlazorReportTest.Models.Dashboard;

public class DashboardData
{
    public DateTime Date { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Channel the record came through, e.g. "Mail" or "SSM". Drives the bar chart's
    /// by-type breakdown mode.
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
