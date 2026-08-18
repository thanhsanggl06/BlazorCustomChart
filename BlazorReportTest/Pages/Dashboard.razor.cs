using BlazorReportTest.Models.Dashboard;
using BlazorReportTest.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorReportTest.Pages;

public partial class Dashboard : ComponentBase
{
    [Inject]
    private IDashboardService DashboardService { get; set; } = null!;

    private List<DashboardData> _allData = [];
    private ChartSelection? _selected;
    private NestedDonutData _donutData = new();
    private bool _isLoading = true;
    private bool _hasError;

    private string SelectedPeriodLabel => _selected is null
        ? string.Empty
        : new DateTime(_selected.Year, _selected.Month, 1).ToString("MMMM yyyy");

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _allData = await DashboardService.GetDashboardDataAsync();

            var today = DateTime.Today;
            _selected = new ChartSelection { Year = today.Year, Month = today.Month };
            _donutData = DashboardAggregator.BuildDonutData(FilterBySelection(_selected));
        }
        catch
        {
            _hasError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnChartSelected(ChartSelection selection)
    {
        _selected = selection;
        _donutData = DashboardAggregator.BuildDonutData(FilterBySelection(selection));
    }

    private List<DashboardData> FilterBySelection(ChartSelection selection) =>
        _allData
            .Where(x => x.Date.Year == selection.Year && x.Date.Month == selection.Month)
            .ToList();
}
