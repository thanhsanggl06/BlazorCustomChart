using BlazorReportTest.Models.Dashboard;
using BlazorReportTest.Services;
using BlazorReportTest.Theming;
using Microsoft.AspNetCore.Components;

namespace BlazorReportTest.Components.Dashboard;

public partial class MonthlyBarChart : ComponentBase
{
    private const double ViewBoxWidth = 900;
    private const double ViewBoxHeight = 320;
    private const double PlotLeft = 44;
    private const double PlotRight = 12;
    private const double PlotTop = 24;
    private const double PlotBottom = 44;
    private const int YAxisTickCount = 4;
    private const double GroupGapRatio = 0.32;
    private const double BarGapRatio = 0.14;

    private static readonly double PlotWidth = ViewBoxWidth - PlotLeft - PlotRight;
    private static readonly double PlotHeight = ViewBoxHeight - PlotTop - PlotBottom;

    [Parameter]
    public IEnumerable<DashboardData> Data { get; set; } = [];

    [Parameter]
    public ChartSelection? SelectedPeriod { get; set; }

    [Parameter]
    public EventCallback<ChartSelection> OnSelected { get; set; }

    private BarChartMode _mode = BarChartMode.YearComparison;
    private List<MonthlyComparisonPoint> _points = [];
    private List<MonthlyTypeBreakdownPoint> _typePoints = [];
    private List<BarVisual> _bars = [];
    private List<StackedGroup> _stackedGroups = [];
    private List<GridLine> _gridLines = [];
    private List<string> _typeLegend = [];

    protected override void OnParametersSet()
    {
        _points = DashboardAggregator.BuildMonthlyComparisonPoints(Data);
        _typePoints = DashboardAggregator.BuildMonthlyTypeBreakdown(Data);
        _typeLegend = _typePoints.Count > 0 ? _typePoints[0].TypeCounts.Select(t => t.Label).ToList() : [];

        RebuildVisuals();
    }

    private void SetMode(BarChartMode mode)
    {
        if (_mode == mode)
        {
            return;
        }

        _mode = mode;
        RebuildVisuals();
    }

    private void RebuildVisuals()
    {
        if (_mode == BarChartMode.YearComparison)
        {
            var niceMax = CalculateNiceMax(_points.Select(p => Math.Max(p.CurrentYearValue, p.PreviousYearValue)));
            _bars = BuildBarVisuals(_points, niceMax);
            _gridLines = BuildGridLines(niceMax);
        }
        else
        {
            var niceMax = CalculateNiceMax(_typePoints.Select(p => (int)p.Total));
            _stackedGroups = BuildStackedGroups(_typePoints, niceMax);
            _gridLines = BuildGridLines(niceMax);
        }
    }

    private static int CalculateNiceMax(IEnumerable<int> values)
    {
        var rawMax = values.DefaultIfEmpty(0).Max();

        if (rawMax <= 0)
        {
            return YAxisTickCount;
        }

        var stepSize = (int)Math.Ceiling(rawMax / (double)YAxisTickCount);
        return stepSize * YAxisTickCount;
    }

    private List<BarVisual> BuildBarVisuals(List<MonthlyComparisonPoint> points, int niceMax)
    {
        if (points.Count == 0)
        {
            return [];
        }

        var groupWidth = PlotWidth / points.Count;
        var groupGap = groupWidth * GroupGapRatio;
        var barPairWidth = groupWidth - groupGap;
        var barGap = barPairWidth * BarGapRatio;
        var barWidth = (barPairWidth - barGap) / 2;

        var bars = new List<BarVisual>(points.Count * 2);

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var groupStartX = PlotLeft + (i * groupWidth) + (groupGap / 2);

            bars.Add(CreateBarVisual(
                x: groupStartX,
                width: barWidth,
                value: point.CurrentYearValue,
                niceMax: niceMax,
                color: DashboardChartColors.CurrentYear,
                selection: new ChartSelection { Year = point.Year, Month = point.Month },
                tooltip: $"{point.MonthLabel} (Current Year): {point.CurrentYearValue}"));

            bars.Add(CreateBarVisual(
                x: groupStartX + barWidth + barGap,
                width: barWidth,
                value: point.PreviousYearValue,
                niceMax: niceMax,
                color: DashboardChartColors.PreviousYear,
                selection: new ChartSelection { Year = point.PreviousYear, Month = point.Month },
                tooltip: $"{new DateTime(point.PreviousYear, point.Month, 1):MMM yyyy} (Previous Year): {point.PreviousYearValue}"));
        }

        return bars;
    }

    private BarVisual CreateBarVisual(double x, double width, int value, int niceMax, string color, ChartSelection selection, string tooltip)
    {
        var barHeight = niceMax == 0 ? 0 : PlotHeight * value / niceMax;
        var y = PlotTop + PlotHeight - barHeight;
        var isSelected = SelectedPeriod is not null && SelectedPeriod.Year == selection.Year && SelectedPeriod.Month == selection.Month;

        return new BarVisual(x, y, width, barHeight, PlotTop, PlotHeight, color, selection, tooltip, value, isSelected);
    }

    private List<StackedGroup> BuildStackedGroups(List<MonthlyTypeBreakdownPoint> points, int niceMax)
    {
        if (points.Count == 0)
        {
            return [];
        }

        var groupWidth = PlotWidth / points.Count;
        var groupGap = groupWidth * GroupGapRatio;
        var barWidth = groupWidth - groupGap;

        var groups = new List<StackedGroup>(points.Count);

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var x = PlotLeft + (i * groupWidth) + (groupGap / 2);
            var selection = new ChartSelection { Year = point.Year, Month = point.Month };
            var isSelected = SelectedPeriod is not null && SelectedPeriod.Year == point.Year && SelectedPeriod.Month == point.Month;

            var segments = new List<StackedSegment>(point.TypeCounts.Count);
            double cumulativeHeight = 0;

            for (var typeIndex = 0; typeIndex < point.TypeCounts.Count; typeIndex++)
            {
                var typeItem = point.TypeCounts[typeIndex];
                var segmentHeight = niceMax == 0 ? 0 : PlotHeight * typeItem.Value / niceMax;
                var y = PlotTop + PlotHeight - cumulativeHeight - segmentHeight;

                segments.Add(new StackedSegment(
                    y,
                    segmentHeight,
                    DashboardChartColors.FromPalette(DashboardChartColors.TypePalette, typeIndex),
                    $"{point.MonthLabel} - {typeItem.Label}: {typeItem.Value}"));

                cumulativeHeight += segmentHeight;
            }

            groups.Add(new StackedGroup(x, barWidth, PlotTop, PlotHeight, selection, isSelected, $"{point.MonthLabel}: {point.Total}", segments));
        }

        return groups;
    }

    private static List<GridLine> BuildGridLines(int niceMax)
    {
        var lines = new List<GridLine>(YAxisTickCount + 1);
        var step = niceMax / (double)YAxisTickCount;

        for (var i = 0; i <= YAxisTickCount; i++)
        {
            var value = step * i;
            var y = PlotTop + PlotHeight - (PlotHeight * value / niceMax);
            lines.Add(new GridLine(y, ((int)Math.Round(value)).ToString()));
        }

        return lines;
    }

    private Task HandleBarClickAsync(ChartSelection selection) => OnSelected.InvokeAsync(selection);

    private double GetGroupLabelX(int index) => PlotLeft + (index * (PlotWidth / _points.Count)) + ((PlotWidth / _points.Count) / 2);

    private static string F(double value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    // The SVG <text> element cannot be written directly in Razor markup here: Razor reserves the
    // literal "<text>" tag name for its own whitespace-control syntax and rejects it once it carries
    // attributes (RZ1023). Building it via RenderTreeBuilder — the same approach MudBlazor's own
    // chart components use internally — sidesteps that parser restriction.
    private RenderFragment RenderAxisLabels() => builder =>
    {
        var seq = 0;

        foreach (var line in _gridLines)
        {
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "monthly-bar-chart-axis-label");
            builder.AddAttribute(seq++, "x", "38");
            builder.AddAttribute(seq++, "y", F(line.Y + 4));
            builder.AddAttribute(seq++, "text-anchor", "end");
            builder.AddContent(seq++, line.Label);
            builder.CloseElement();
        }

        for (var i = 0; i < _points.Count; i++)
        {
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "class", "monthly-bar-chart-axis-label");
            builder.AddAttribute(seq++, "x", F(GetGroupLabelX(i)));
            builder.AddAttribute(seq++, "y", "292");
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddContent(seq++, _points[i].MonthLabel);
            builder.CloseElement();
        }
    };

    private sealed record BarVisual(
        double X,
        double Y,
        double Width,
        double Height,
        double HitAreaY,
        double HitAreaHeight,
        string Color,
        ChartSelection Selection,
        string Tooltip,
        int Value,
        bool IsSelected);

    private sealed record StackedSegment(double Y, double Height, string Color, string Tooltip);

    private sealed record StackedGroup(
        double X,
        double Width,
        double HitAreaY,
        double HitAreaHeight,
        ChartSelection Selection,
        bool IsSelected,
        string HitTooltip,
        List<StackedSegment> Segments);

    private sealed record GridLine(double Y, string Label);
}
