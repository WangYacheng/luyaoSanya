using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TradeSystem.App.ViewModels;

public partial class DashboardViewModel : ViewModel
{
    [ObservableProperty]
    private int _counter;

    [RelayCommand]
    private void OnCounterIncrement()
    {
        Counter++;
    }
}
