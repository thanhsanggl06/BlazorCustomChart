using BlazorReportTest.Models.Dashboard;
using BlazorReportTest.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorReportTest.Components.DashboardChartJs;

public partial class NestedDonutChartJs : ComponentBase, IAsyncDisposable
{
    private const string ModulePath = "/js/dashboard-chartjs/dashboard-charts.js";

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    public NestedDonutData Data { get; set; } = new();

    private readonly string _canvasId = $"nested-donut-chart-{Guid.NewGuid():N}";
    private IJSObjectReference? _module;
    private bool _hasData;
    private bool _pendingRender;

    protected override void OnParametersSet()
    {
        _hasData = Data.HasData;
        _pendingRender = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
        }

        if (_module is not null && _pendingRender)
        {
            _pendingRender = false;

            if (_hasData)
            {
                var rings = new object[]
                {
                    BuildRing("Status", Data.Status, DashboardChartColors.StatusPalette),
                    BuildRing("Department", Data.Departments, DashboardChartColors.DepartmentPalette),
                    BuildRing("System", Data.Systems, DashboardChartColors.SystemPalette)
                };

                await _module.InvokeVoidAsync("createNestedDonutChart", _canvasId, rings);
            }
            else
            {
                await _module.InvokeVoidAsync("destroyChart", _canvasId);
            }
        }
    }

    private static object BuildRing(string name, List<DonutItem> items, IReadOnlyList<string> palette) => new
    {
        name,
        labels = items.Select(i => i.Label).ToArray(),
        values = items.Select(i => i.Value).ToArray(),
        colors = items.Select((_, index) => DashboardChartColors.FromPalette(palette, index)).ToArray()
    };

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
    }
}
