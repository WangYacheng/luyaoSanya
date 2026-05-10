using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.Models;

namespace TradeSystem.App.ViewModels;
public partial class LoadKLineViewModel : ViewModel
{
    private readonly IExchangeService _exchangeService;

    public LoadKLineViewModel(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    [ObservableProperty]
    private string _symbol;

    [ObservableProperty]
    private TimeSpan _interval = TimeSpan.FromMinutes(1);

    [ObservableProperty]
    private DateTime? _startTime;

    [ObservableProperty]
    private DateTime? _endTime;

    [RelayCommand]
    private async Task LoadKLinesAsync()
    {
        var klines = await _exchangeService.GetKlinesAsync(Symbol, Interval, StartTime, EndTime);
        // 这里可以将 klines 绑定到 UI 上显示，或者保存到数据库等
    }
}