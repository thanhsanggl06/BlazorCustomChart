using System.Globalization;
using BlazorReportTest.Models.Dashboard;
using BlazorReportTest.Theming;
using Microsoft.AspNetCore.Components;

namespace BlazorReportTest.Components.Dashboard;

public partial class NestedDonutChart : ComponentBase
{
    private const double CenterX = 150;
    private const double CenterY = 150;
    private const double OuterRadius = 128;
    private const double StrokeWidth = 26;
    private const double RingGap = 4;

    [Parameter]
    public NestedDonutData Data { get; set; } = new();

    private List<RingSegment> _statusSegments = [];
    private List<RingSegment> _departmentSegments = [];
    private List<RingSegment> _systemSegments = [];
    private bool _hasData;

    protected override void OnParametersSet()
    {
        _hasData = Data.HasData;

        if (!_hasData)
        {
            _statusSegments = [];
            _departmentSegments = [];
            _systemSegments = [];
            return;
        }

        var middleRadius = OuterRadius - StrokeWidth - RingGap;
        var innerRadius = middleRadius - StrokeWidth - RingGap;

        _statusSegments = BuildSegments(Data.Status, OuterRadius, DashboardChartColors.StatusPalette);
        _departmentSegments = BuildSegments(Data.Departments, middleRadius, DashboardChartColors.DepartmentPalette);
        _systemSegments = BuildSegments(Data.Systems, innerRadius, DashboardChartColors.SystemPalette);
    }

    private static List<RingSegment> BuildSegments(List<DonutItem> items, double radius, IReadOnlyList<string> palette)
    {
        var total = items.Sum(x => x.Value);
        if (total <= 0)
        {
            return [];
        }

        var circumference = 2 * Math.PI * radius;
        var segments = new List<RingSegment>(items.Count);
        var cumulativeLength = 0.0;

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var fraction = item.Value / total;
            var dashLength = fraction * circumference;

            segments.Add(new RingSegment(
                Radius: F(radius),
                DashArray: $"{F(dashLength)} {F(circumference - dashLength)}",
                DashOffset: F(-cumulativeLength),
                Color: DashboardChartColors.FromPalette(palette, i),
                Label: item.Label,
                Value: item.Value,
                PercentageLabel: Math.Round(fraction * 100, 1).ToString("0.#", CultureInfo.InvariantCulture)));

            cumulativeLength += dashLength;
        }

        return segments;
    }

    private static string F(double value) => value.ToString(CultureInfo.InvariantCulture);

    private sealed record RingSegment(
        string Radius,
        string DashArray,
        string DashOffset,
        string Color,
        string Label,
        double Value,
        string PercentageLabel);
}
