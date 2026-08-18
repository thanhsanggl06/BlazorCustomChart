using BlazorReportTest.Models.Dashboard;

namespace BlazorReportTest.Services;

/// <summary>
/// Generates sample dashboard data. Swap the body of <see cref="GetDashboardDataAsync"/>
/// for a real data source (database, API, etc.) without touching any consumer of
/// <see cref="IDashboardService"/>.
/// </summary>
public class DashboardService : IDashboardService
{
    private static readonly string[] Statuses = ["Success", "Error", "Warning"];
    private static readonly string[] Departments = ["IT", "Sales", "HR", "Finance"];
    private static readonly string[] SystemNames = ["System A", "System B", "System C", "System D"];
    private static readonly string[] Types = ["Mail", "SSM"];

    private const int MonthsOfHistory = 23;
    private const int SkipMonthsAgoForEmptyStateDemo = 3;
    private const double UnknownValueChance = 0.05;
    private const int MinRecordsPerMonth = 15;
    private const int MaxRecordsPerMonth = 60;

    public Task<List<DashboardData>> GetDashboardDataAsync()
    {
        var random = new Random(20260818);
        var data = new List<DashboardData>();

        var earliestMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-MonthsOfHistory);
        var emptyMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-SkipMonthsAgoForEmptyStateDemo);

        for (var offset = 0; offset <= MonthsOfHistory; offset++)
        {
            var month = earliestMonth.AddMonths(offset);
            if (month == emptyMonth)
            {
                continue;
            }

            AddRecordsForMonth(data, random, month);
        }

        return Task.FromResult(data);
    }

    private static void AddRecordsForMonth(List<DashboardData> data, Random random, DateTime month)
    {
        var recordCount = random.Next(MinRecordsPerMonth, MaxRecordsPerMonth);
        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

        for (var i = 0; i < recordCount; i++)
        {
            data.Add(new DashboardData
            {
                Date = month.AddDays(random.Next(0, daysInMonth)),
                Status = PickValueOrEmpty(random, Statuses),
                Department = PickValueOrEmpty(random, Departments),
                SystemName = PickValueOrEmpty(random, SystemNames),
                Type = PickValueOrEmpty(random, Types)
            });
        }
    }

    private static string PickValueOrEmpty(Random random, string[] values) =>
        random.NextDouble() < UnknownValueChance ? string.Empty : values[random.Next(values.Length)];
}
