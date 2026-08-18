using MudBlazor;

namespace BlazorReportTest.Theming;

/// <summary>
/// Single source of truth for every color used across the dashboard charts.
/// Built from MudBlazor's own <see cref="Colors"/> tokens so the dashboard stays
/// visually consistent with the rest of the MudBlazor theme.
/// </summary>
public static class DashboardChartColors
{
    public static readonly string CurrentYear = Colors.Blue.Accent3;

    public static readonly string PreviousYear = Colors.Gray.Lighten1;

    public static readonly IReadOnlyList<string> StatusPalette =
    [
        Colors.Green.Accent3, Colors.Red.Accent3, Colors.Amber.Accent3, Colors.LightBlue.Accent3, Colors.DeepPurple.Accent3
    ];

    public static readonly IReadOnlyList<string> DepartmentPalette =
    [
        Colors.Teal.Accent3, Colors.Orange.Accent3, Colors.Indigo.Accent3, Colors.Pink.Accent2, Colors.Lime.Darken2
    ];

    public static readonly IReadOnlyList<string> SystemPalette =
    [
        Colors.Cyan.Darken1, Colors.DeepOrange.Accent2, Colors.Purple.Lighten1, Colors.BlueGray.Lighten1, Colors.Brown.Lighten1
    ];

    public static readonly IReadOnlyList<string> TypePalette =
    [
        Colors.Blue.Accent3, Colors.Teal.Accent3, Colors.Amber.Accent3, Colors.Pink.Accent2, Colors.Gray.Lighten1
    ];

    public static string FromPalette(IReadOnlyList<string> palette, int index) => palette[index % palette.Count];
}
