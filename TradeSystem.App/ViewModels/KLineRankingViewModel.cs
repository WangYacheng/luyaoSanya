using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TradeSystem.App.Models;
using TradeSystem.App.Services;
using TradeSystem.Core.Abstractions;
using TradeSystem.Core.Models;

namespace TradeSystem.App.ViewModels;

public partial class KLineRankingViewModel : ViewModel
{
    private readonly IExchangeService _okxService;
    private readonly IExchangeService _binanceService;
    private readonly DataDownloadService _dataDownloadService;

    private List<string> _okxSymbols = [];
    private List<string> _binanceSymbols = [];

    private List<Dictionary<string, KLineData>> _okxSlots = [];
    private List<Dictionary<string, KLineData>> _binanceSlots = [];

    [ObservableProperty]
    private TimeSpan _selectedPeriod = TimeSpan.FromHours(1);

    [ObservableProperty]
    private int _kLineCount = 20;

    [ObservableProperty]
    private int _okxCurrentIndex;

    [ObservableProperty]
    private int _binanceCurrentIndex;

    [ObservableProperty]
    private string _okxPeriodText = "-";

    [ObservableProperty]
    private string _binancePeriodText = "-";

    [ObservableProperty]
    private bool _okxCanGoPrev;

    [ObservableProperty]
    private bool _okxCanGoNext;

    [ObservableProperty]
    private bool _binanceCanGoPrev;

    [ObservableProperty]
    private bool _binanceCanGoNext;

    [ObservableProperty]
    private string _prevButtonText = "上一小时";

    [ObservableProperty]
    private string _nextButtonText = "下一小时";

    [ObservableProperty]
    private ObservableCollection<RankingItem> _okxRankings = [];

    [ObservableProperty]
    private ObservableCollection<RankingItem> _binanceRankings = [];

    [ObservableProperty]
    private bool _isOkxLoading;

    [ObservableProperty]
    private bool _isBinanceLoading;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    public List<KeyValuePair<string, TimeSpan>> Periods { get; } =
    [
        new("15分钟", TimeSpan.FromMinutes(15)),
        new("1小时", TimeSpan.FromHours(1)),
        new("4小时", TimeSpan.FromHours(4)),
        new("1日", TimeSpan.FromDays(1)),
    ];

    public KLineRankingViewModel(
        [FromKeyedServices("OKX")] IExchangeService okxService,
        [FromKeyedServices("Binance")] IExchangeService binanceService,
        DataDownloadService dataDownloadService)
    {
        _okxService = okxService;
        _binanceService = binanceService;
        _dataDownloadService = dataDownloadService;
    }

    public override async Task OnNavigatedToAsync()
    {
        await base.OnNavigatedToAsync();
        if (_okxSymbols.Count > 0) return;
        await LoadSymbolsAsync();
    }

    partial void OnSelectedPeriodChanged(TimeSpan value)
    {
        UpdateNavButtonTexts();
    }

    private void UpdateNavButtonTexts()
    {
        if (SelectedPeriod.TotalMinutes == 15)
        {
            PrevButtonText = "◀ 上一刻钟";
            NextButtonText = "下一刻钟 ▶";
        }
        else if (SelectedPeriod.TotalHours == 1)
        {
            PrevButtonText = "◀ 上一小时";
            NextButtonText = "下一小时 ▶";
        }
        else if (SelectedPeriod.TotalHours == 4)
        {
            PrevButtonText = "◀ 上四小时";
            NextButtonText = "下四小时 ▶";
        }
        else
        {
            PrevButtonText = "◀ 上一天";
            NextButtonText = "下一天 ▶";
        }
    }

    private async Task LoadSymbolsAsync()
    {
        StatusMessage = "正在加载交易对...";
        UpdateNavButtonTexts();
        var (okx, binance) = await _dataDownloadService.GetSymbolUnionAsync();
        _okxSymbols = okx;
        _binanceSymbols = binance;
        StatusMessage = $"OKX {okx.Count} 交易对, Binance {binance.Count} 交易对 | 就绪";
    }

    [RelayCommand]
    private async Task LoadOkxRanking()
    {
        if (IsOkxLoading) return;
        IsOkxLoading = true;
        try
        {
            _okxSlots = await LoadSlotsAsync(_okxService, "OKX", _okxSymbols);
            if (_okxSlots.Count > 0)
            {
                OkxCurrentIndex = _okxSlots.Count - 1;
                RefreshOkxDisplay();
            }
            StatusMessage = $"OKX 排行榜已更新 ({_okxSlots.Count} 个时段)  {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"OKX 加载失败: {ex.Message}";
        }
        finally
        {
            IsOkxLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadBinanceRanking()
    {
        if (IsBinanceLoading) return;
        IsBinanceLoading = true;
        try
        {
            _binanceSlots = await LoadSlotsAsync(_binanceService, "Binance", _binanceSymbols);
            if (_binanceSlots.Count > 0)
            {
                BinanceCurrentIndex = _binanceSlots.Count - 1;
                RefreshBinanceDisplay();
            }
            StatusMessage = $"Binance 排行榜已更新 ({_binanceSlots.Count} 个时段)  {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Binance 加载失败: {ex.Message}";
        }
        finally
        {
            IsBinanceLoading = false;
        }
    }

    [RelayCommand]
    private void GoToOkxPrev()
    {
        if (!OkxCanGoPrev) return;
        OkxCurrentIndex--;
        RefreshOkxDisplay();
    }

    [RelayCommand]
    private void GoToOkxNext()
    {
        if (!OkxCanGoNext) return;
        OkxCurrentIndex++;
        RefreshOkxDisplay();
    }

    [RelayCommand]
    private void GoToOkxLatest()
    {
        if (_okxSlots.Count == 0) return;
        OkxCurrentIndex = _okxSlots.Count - 1;
        RefreshOkxDisplay();
    }

    [RelayCommand]
    private void GoToBinancePrev()
    {
        if (!BinanceCanGoPrev) return;
        BinanceCurrentIndex--;
        RefreshBinanceDisplay();
    }

    [RelayCommand]
    private void GoToBinanceNext()
    {
        if (!BinanceCanGoNext) return;
        BinanceCurrentIndex++;
        RefreshBinanceDisplay();
    }

    [RelayCommand]
    private void GoToBinanceLatest()
    {
        if (_binanceSlots.Count == 0) return;
        BinanceCurrentIndex = _binanceSlots.Count - 1;
        RefreshBinanceDisplay();
    }

    private async Task<List<Dictionary<string, KLineData>>> LoadSlotsAsync(
        IExchangeService exchange,
        string exchangeName,
        List<string> symbols)
    {
        var symbolKlines = new Dictionary<string, List<KLineData>>();
        var batchSize = 5;

        for (var i = 0; i < symbols.Count; i += batchSize)
        {
            var batch = symbols.Skip(i).Take(batchSize).ToList();
            var klinesDict = await exchange.GetMultipleKlinesAsync(batch, SelectedPeriod, null, null, KLineCount,false);

            foreach (var kvp in klinesDict)
            {
                var sorted = kvp.Value.OrderBy(k => k.OpenTime).ToList();
                if (sorted.Count > 0)
                    symbolKlines[kvp.Key] = sorted;
            }

            var processed = Math.Min(i + batchSize, symbols.Count);
            StatusMessage = $"{exchangeName}: {processed}/{symbols.Count}";
        }

        if (symbolKlines.Count == 0) return [];

        var maxSlots = symbolKlines.Values.Max(x => x.Count);
        var slots = new List<Dictionary<string, KLineData>>();

        for (var slot = 0; slot < maxSlots; slot++)
        {
            var slotDict = new Dictionary<string, KLineData>();
            foreach (var (symbol, klines) in symbolKlines)
            {
                if (slot < klines.Count)
                    slotDict[symbol] = klines[slot];
            }
            slots.Add(slotDict);
        }

        return slots;
    }

    private void RefreshOkxDisplay()
    {
        if (_okxSlots.Count == 0 || OkxCurrentIndex < 0 || OkxCurrentIndex >= _okxSlots.Count)
        {
            OkxRankings.Clear();
            return;
        }
        var items = BuildRankingFromSlot(_okxSlots[OkxCurrentIndex], "OKX");
        OkxRankings = new ObservableCollection<RankingItem>(items);
        OkxPeriodText = GetSlotTime(_okxSlots, OkxCurrentIndex);
        OkxCanGoPrev = OkxCurrentIndex > 0;
        OkxCanGoNext = OkxCurrentIndex < _okxSlots.Count - 1;
    }

    private void RefreshBinanceDisplay()
    {
        if (_binanceSlots.Count == 0 || BinanceCurrentIndex < 0 || BinanceCurrentIndex >= _binanceSlots.Count)
        {
            BinanceRankings.Clear();
            return;
        }
        var items = BuildRankingFromSlot(_binanceSlots[BinanceCurrentIndex], "Binance");
        BinanceRankings = new ObservableCollection<RankingItem>(items);
        BinancePeriodText = GetSlotTime(_binanceSlots, BinanceCurrentIndex);
        BinanceCanGoPrev = BinanceCurrentIndex > 0;
        BinanceCanGoNext = BinanceCurrentIndex < _binanceSlots.Count - 1;
    }

    private static string GetSlotTime(List<Dictionary<string, KLineData>> slots, int index)
    {
        if (slots.Count > index && slots[index].Count > 0)
        {
            var beijingTime = slots[index].Values.First().OpenTime.AddHours(8);
            return beijingTime.ToString("yyyy-MM-dd HH:mm");
        }
        return "-";
    }

    private static List<RankingItem> BuildRankingFromSlot(Dictionary<string, KLineData> slot, string exchange)
    {
        var items = slot
            .Select(kvp =>
            {
                var k = kvp.Value;
                var changePct = k.Open != 0 ? (k.Close - k.Open) / k.Open * 100m : 0m;
                return new RankingItem
                {
                    Symbol = kvp.Key,
                    Exchange = exchange,
                    ChangePct = changePct,
                    Open = k.Open,
                    LastPrice = k.Close,
                    Volume = k.Volume,
                    High = k.High,
                    Low = k.Low,
                };
            })
            .OrderByDescending(x => x.ChangePct)
            .Take(15)
            .ToList();

        for (var i = 0; i < items.Count; i++)
            items[i].Rank = i + 1;

        return items;
    }
}
