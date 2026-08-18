namespace BlazorReportTest.Models.Dashboard;

public class NestedDonutData
{
    public List<DonutItem> Status { get; set; } = [];

    public List<DonutItem> Departments { get; set; } = [];

    public List<DonutItem> Systems { get; set; } = [];

    public bool HasData => Status.Count > 0 && Departments.Count > 0 && Systems.Count > 0;
}
