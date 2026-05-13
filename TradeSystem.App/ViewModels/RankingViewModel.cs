using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TradeSystem.App.Models;
using TradeSystem.App.Services;

namespace TradeSystem.App.ViewModels;

public partial class RankingViewModel : ViewModel
{
    private readonly RankingService _rankingService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsToday))]
    [NotifyPropertyChangedFor(nameof(SelectedDateText))]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<RankingItem> _okxRankings = [];

    [ObservableProperty]
    private ObservableCollection<RankingItem> _binanceRankings = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    public bool IsToday => SelectedDate.Date == DateTime.Today;

    public string SelectedDateText => SelectedDate.ToString("yyyy-MM-dd");

    public RankingViewModel(RankingService rankingService)
    {
        _rankingService = rankingService;
    }

    public override async Task OnNavigatedToAsync()
    {
        await base.OnNavigatedToAsync();
        await LoadRankingsAsync();
    }

    [RelayCommand]
    private async Task GoToPreviousDay()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await LoadRankingsAsync();
    }

    [RelayCommand]
    private async Task GoToNextDay()
    {
        if (IsToday) return;
        SelectedDate = SelectedDate.AddDays(1);
        await LoadRankingsAsync();
    }

    [RelayCommand]
    private async Task GoToToday()
    {
        SelectedDate = DateTime.Today;
        await LoadRankingsAsync();
    }

    private async Task LoadRankingsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        StatusMessage = "加载中...";

        try
        {
            var okxTask = _rankingService.GetRankingsAsync(SelectedDate, 15, "OKX");
            var binanceTask = _rankingService.GetRankingsAsync(SelectedDate, 30, "Binance");
            await Task.WhenAll(okxTask, binanceTask);

            OkxRankings = new ObservableCollection<RankingItem>(okxTask.Result);
            BinanceRankings = new ObservableCollection<RankingItem>(binanceTask.Result);
            StatusMessage = $"更新于 {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
