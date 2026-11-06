using MudBlazor;

namespace GetRDone.Web.Components.Layout;

public static class GetRDoneTheme
{
    public static MudTheme Theme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1E5AA8",
            Secondary = "#00897B",
            Tertiary = "#F9A825",
            AppbarBackground = "#1E5AA8",
            Background = "#F5F7FA",
            Surface = "#FFFFFF",
            Success = "#2E7D32",
            Warning = "#ED6C02",
            Error = "#D32F2F",
            Info = "#0288D1",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
        },
    };
}
