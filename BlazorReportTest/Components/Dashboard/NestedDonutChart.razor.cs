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
    private const double MinPercentageForOnChartLabel = 7.0;
    private const double ViewBoxSize = 300;

    [Parameter]
    public NestedDonutData Data { get; set; } = new();

    private List<RingSegment> _statusSegments = [];
    private List<RingSegment> _departmentSegments = [];
    private List<RingSegment> _systemSegments = [];
    private bool _hasData;
    private ChartTooltipInfo? _hoverInfo;

    protected override void OnParametersSet()
    {
        _hasData = Data.HasData;
        _hoverInfo = null;

        if (!_hasData)
        {
            _statusSegments = [];
            _departmentSegments = [];
            _systemSegments = [];
            return;
        }

        var middleRadius = OuterRadius - StrokeWidth - RingGap;
        var innerRadius = middleRadius - StrokeWidth - RingGap;

        _statusSegments = BuildSegments("Status", Data.Status, OuterRadius, DashboardChartColors.StatusPalette);
        _departmentSegments = BuildSegments("Department", Data.Departments, middleRadius, DashboardChartColors.DepartmentPalette);
        _systemSegments = BuildSegments("System", Data.Systems, innerRadius, DashboardChartColors.SystemPalette);
    }

    private static List<RingSegment> BuildSegments(string ringName, List<DonutItem> items, double radius, IReadOnlyList<string> palette)
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
            var percentage = Math.Round(fraction * 100, 1);

            var midFraction = (cumulativeLength + (dashLength / 2)) / circumference;
            var midAngleRad = (-90 + (midFraction * 360)) * Math.PI / 180;
            var labelX = CenterX + (radius * Math.Cos(midAngleRad));
            var labelY = CenterY + (radius * Math.Sin(midAngleRad));

            segments.Add(new RingSegment(
                RingName: ringName,
                Radius: F(radius),
                DashArray: $"{F(dashLength)} {F(circumference - dashLength)}",
                DashOffset: F(-cumulativeLength),
                Color: DashboardChartColors.FromPalette(palette, i),
                Label: item.Label,
                Value: item.Value,
                PercentageLabel: percentage.ToString("0.#", CultureInfo.InvariantCulture),
                LabelX: labelX,
                LabelY: labelY,
                ShowOnChartLabel: percentage >= MinPercentageForOnChartLabel));

            cumulativeLength += dashLength;
        }

        return segments;
    }

    private void ShowTooltip(RingSegment segment) =>
        _hoverInfo = new ChartTooltipInfo(
            segment.LabelX / ViewBoxSize * 100,
            segment.LabelY / ViewBoxSize * 100,
            $"{segment.RingName} - {segment.Label}",
            $"{segment.Value} ({segment.PercentageLabel}%)",
            segment.Color);

    private void HideTooltip() => _hoverInfo = null;

    private static string F(double value) => value.ToString(CultureInfo.InvariantCulture);

    // Rendered via RenderTreeBuilder rather than a literal <text> tag — see the identical note in
    // MonthlyBarChart.razor.cs: Razor reserves "<text>" for its own syntax once attributes are added.
    private RenderFragment RenderOnChartLabels() => builder =>
    {
        var seq = 0;

        foreach (var segment in _statusSegments.Concat(_departmentSegments).Concat(_systemSegments))
        {
            if (!segment.ShowOnChartLabel)
            {
                continue;
            }

            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "nested-donut-label");
            builder.AddAttribute(seq++, "x", F(segment.LabelX));
            builder.AddAttribute(seq++, "y", F(segment.LabelY));
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddAttribute(seq++, "dominant-baseline", "central");
            builder.AddContent(seq++, $"{segment.PercentageLabel}%");
            builder.CloseElement();
        }
    };

    private sealed record RingSegment(
        string RingName,
        string Radius,
        string DashArray,
        string DashOffset,
        string Color,
        string Label,
        double Value,
        string PercentageLabel,
        double LabelX,
        double LabelY,
        bool ShowOnChartLabel);
}
