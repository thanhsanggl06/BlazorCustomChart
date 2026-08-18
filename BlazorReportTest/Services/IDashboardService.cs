using BlazorReportTest.Models.Dashboard;

namespace BlazorReportTest.Services;

public interface IDashboardService
{
    Task<List<DashboardData>> GetDashboardDataAsync();
}
