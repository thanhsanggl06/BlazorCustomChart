using BlazorReportTest.Models.Dashboard;

namespace BlazorReportTest.Services;

/// <summary>
/// Pure aggregation logic shared by every dashboard rendering (MudBlazor charts, Chart.js
/// charts, or any future one): turning raw <see cref="DashboardData"/> records into the
/// 12-month year-over-year comparison and the per-period donut breakdown. Keeping this in one
/// place means every chart implementation counts records the exact same way.
/// </summary>
public static class DashboardAggregator
{
    public const int TrailingMonthsCount = 12;

    public static List<MonthlyComparisonPoint> BuildMonthlyComparisonPoints(IEnumerable<DashboardData> data)
    {
        var dataList = data as ICollection<DashboardData> ?? data.ToList();
        var anchorMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var points = new List<MonthlyComparisonPoint>(TrailingMonthsCount);

        for (var monthsAgo = TrailingMonthsCount - 1; monthsAgo >= 0; monthsAgo--)
        {
            var month = anchorMonth.AddMonths(-monthsAgo);
            var previousYearMonth = month.AddYears(-1);

            points.Add(new MonthlyComparisonPoint
            {
                Year = month.Year,
                Month = month.Month,
                CurrentYearValue = CountRecordsInMonth(dataList, month),
                PreviousYear = previousYearMonth.Year,
                PreviousYearValue = CountRecordsInMonth(dataList, previousYearMonth)
            });
        }

        return points;
    }

    public static NestedDonutData BuildDonutData(IEnumerable<DashboardData> selectedData)
    {
        var dataList = selectedData as ICollection<DashboardData> ?? selectedData.ToList();

        return new NestedDonutData
        {
            Status = AggregateByKey(dataList, x => x.Status),
            Departments = AggregateByKey(dataList, x => x.Department),
            Systems = AggregateByKey(dataList, x => x.SystemName)
        };
    }

    /// <summary>
    /// Builds the current-year 12-month series used by the bar chart's "by Type" mode. The set of
    /// Type labels is fixed (alphabetical) across every month, so a given Type always lands in the
    /// same stack position and gets the same color, whether or not it has records in every month.
    /// </summary>
    public static List<MonthlyTypeBreakdownPoint> BuildMonthlyTypeBreakdown(IEnumerable<DashboardData> data)
    {
        var dataList = data as ICollection<DashboardData> ?? data.ToList();
        var orderedTypes = dataList
            .Select(x => x.Type.NormalizeOrUnknown())
            .Distinct()
            .OrderBy(type => type, StringComparer.Ordinal)
            .ToList();

        var anchorMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var points = new List<MonthlyTypeBreakdownPoint>(TrailingMonthsCount);

        for (var monthsAgo = TrailingMonthsCount - 1; monthsAgo >= 0; monthsAgo--)
        {
            var month = anchorMonth.AddMonths(-monthsAgo);
            var recordsInMonth = dataList
                .Where(x => x.Date.Year == month.Year && x.Date.Month == month.Month)
                .ToList();

            points.Add(new MonthlyTypeBreakdownPoint
            {
                Year = month.Year,
                Month = month.Month,
                TypeCounts = orderedTypes
                    .Select(type => new DonutItem
                    {
                        Label = type,
                        Value = recordsInMonth.Count(x => x.Type.NormalizeOrUnknown() == type)
                    })
                    .ToList()
            });
        }

        return points;
    }

    private static int CountRecordsInMonth(ICollection<DashboardData> data, DateTime month) =>
        data.Count(x => x.Date.Year == month.Year && x.Date.Month == month.Month);

    private static List<DonutItem> AggregateByKey(ICollection<DashboardData> data, Func<DashboardData, string> keySelector) =>
        data
            .GroupBy(x => keySelector(x).NormalizeOrUnknown())
            .Select(group => new DonutItem { Label = group.Key, Value = group.Count() })
            .OrderByDescending(item => item.Value)
            .ToList();
}
