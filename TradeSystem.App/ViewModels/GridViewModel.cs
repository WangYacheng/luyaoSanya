using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TradeSystem.App.ViewModels;

public partial class GridViewModel : ViewModel
{
    [ObservableProperty]
    private int _selectedDemo;

    public string[] DemoNames { get; } =
    [
        "基础行列定义",
        "Auto与Star混合",
        "跨行跨列(Span)",
        "嵌套Grid",
        "对齐方式"
    ];

    [RelayCommand]
    private void SelectDemo(string name)
    {
        var index = Array.IndexOf(DemoNames, name);
        if (index >= 0)
            SelectedDemo = index;
    }
}
