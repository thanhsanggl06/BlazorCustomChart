using BlazorReportTest.Models.Dashboard;
using BlazorReportTest.Services;
using BlazorReportTest.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorReportTest.Components.DashboardChartJs;

public partial class MonthlyBarChartJs : ComponentBase, IAsyncDisposable
{
    private const string ModulePath = "/js/dashboard-chartjs/dashboard-charts.js";

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    public IEnumerable<DashboardData> Data { get; set; } = [];

    [Parameter]
    public ChartSelection? SelectedPeriod { get; set; }

    [Parameter]
    public EventCallback<ChartSelection> OnSelected { get; set; }

    private readonly string _canvasId = $"monthly-bar-chart-{Guid.NewGuid():N}";
    private IJSObjectReference? _module;
    private DotNetObjectReference<MonthlyBarChartJs>? _dotNetRef;
    private BarChartMode _mode = BarChartMode.YearComparison;
    private List<MonthlyComparisonPoint> _points = [];
    private List<MonthlyTypeBreakdownPoint> _typePoints = [];
    private List<string> _typeLegend = [];
    private ChartSelection? _renderedSelection;
    private bool _pendingRebuild;

    protected override void OnParametersSet()
    {
        _points = DashboardAggregator.BuildMonthlyComparisonPoints(Data);
        _typePoints = DashboardAggregator.BuildMonthlyTypeBreakdown(Data);
        _typeLegend = _typePoints.Count > 0 ? _typePoints[0].TypeCounts.Select(t => t.Label).ToList() : [];
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
            await RenderChartAsync();
        }
        else if (_module is not null && _pendingRebuild)
        {
            _pendingRebuild = false;
            await RenderChartAsync();
        }
        else if (_module is not null && !Equals(_renderedSelection, SelectedPeriod))
        {
            await ApplySelectionHighlightAsync();
        }
    }

    private void SetMode(BarChartMode mode)
    {
        if (_mode == mode)
        {
            return;
        }

        _mode = mode;
        _pendingRebuild = true;
        _renderedSelection = null;
    }

    private async Task RenderChartAsync()
    {
        if (_module is null)
        {
            return;
        }

        if (_mode == BarChartMode.YearComparison)
        {
            await _module.InvokeVoidAsync(
                "createMonthlyBarChart",
                _canvasId,
                _dotNetRef,
                _points.Select(p => p.MonthLabel).ToArray(),
                _points.Select(p => p.CurrentYearValue).ToArray(),
                _points.Select(p => p.PreviousYearValue).ToArray(),
                DashboardChartColors.CurrentYear,
                DashboardChartColors.PreviousYear);
        }
        else
        {
            var typeSeries = _typeLegend
                .Select((label, index) => new
                {
                    name = label,
                    color = DashboardChartColors.FromPalette(DashboardChartColors.TypePalette, index),
                    values = _typePoints.Select(p => p.TypeCounts[index].Value).ToArray()
                })
                .ToArray();

            await _module.InvokeVoidAsync(
                "createTypeBreakdownChart",
                _canvasId,
                _dotNetRef,
                _typePoints.Select(p => p.MonthLabel).ToArray(),
                typeSeries);
        }

        await ApplySelectionHighlightAsync();
    }

    private async Task ApplySelectionHighlightAsync()
    {
        if (_module is null || SelectedPeriod is null)
        {
            return;
        }

        if (_mode == BarChartMode.YearComparison)
        {
            for (var i = 0; i < _points.Count; i++)
            {
                var point = _points[i];

                if (point.Year == SelectedPeriod.Year && point.Month == SelectedPeriod.Month)
                {
                    await _module.InvokeVoidAsync("highlightSelectedBar", _canvasId, 0, i);
                    _renderedSelection = SelectedPeriod;
                    return;
                }

                if (point.PreviousYear == SelectedPeriod.Year && point.Month == SelectedPeriod.Month)
                {
                    await _module.InvokeVoidAsync("highlightSelectedBar", _canvasId, 1, i);
                    _renderedSelection = SelectedPeriod;
                    return;
                }
            }
        }
        else
        {
            for (var i = 0; i < _typePoints.Count; i++)
            {
                var point = _typePoints[i];

                if (point.Year == SelectedPeriod.Year && point.Month == SelectedPeriod.Month)
                {
                    await _module.InvokeVoidAsync("highlightSelectedTypeBar", _canvasId, i);
                    _renderedSelection = SelectedPeriod;
                    return;
                }
            }
        }
    }

    [JSInvokable]
    public async Task OnBarClicked(int datasetIndex, int index)
    {
        if (index < 0 || index >= _points.Count)
        {
            return;
        }

        var point = _points[index];
        var selection = datasetIndex == 0
            ? new ChartSelection { Year = point.Year, Month = point.Month }
            : new ChartSelection { Year = point.PreviousYear, Month = point.Month };

        await OnSelected.InvokeAsync(selection);
    }

    [JSInvokable]
    public async Task OnTypeBarClicked(int index)
    {
        if (index < 0 || index >= _typePoints.Count)
        {
            return;
        }

        var point = _typePoints[index];
        await OnSelected.InvokeAsync(new ChartSelection { Year = point.Year, Month = point.Month });
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("destroyChart", _canvasId);
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already gone; nothing left to clean up client-side.
            }
        }

        _dotNetRef?.Dispose();
    }
}
