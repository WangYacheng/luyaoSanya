using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TradeSystem.Core.Models;

namespace TradeSystem.App.ViewModels;

public partial class Test3ViewModel : ViewModel
{
    private readonly IExchangeService _binanceService;
    private readonly IExchangeService _okxService;

    [ObservableProperty]
    private string _exchange = "Binance";

    [ObservableProperty]
    private List<string> _exchangeOptions = ["Binance", "OKX"];

    [ObservableProperty]
    private string _interval = "1h";

    [ObservableProperty]
    private List<string> _intervalOptions = ["1m", "5m", "15m", "30m", "1h", "4h", "1d"];

    [ObservableProperty]
    private int _limit = 100;

    [ObservableProperty]
    private int _batchSize = 200;

    [ObservableProperty]
    private string _symbol = "BTCUSDT";

    [ObservableProperty]
    private ObservableCollection<KLineData> _kLineResults = [];

    [ObservableProperty]
    private string _logOutput = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public Test3ViewModel(
        [FromKeyedServices("Binance")] IExchangeService binance,
        [FromKeyedServices("OKX")] IExchangeService okx)
    {
        _binanceService = binance;
        _okxService = okx;
    }

    [RelayCommand]
    private async Task RequestKlines()
    {
        if (IsLoading) return;

        IsLoading = true;
        KLineResults.Clear();
        LogOutput = string.Empty;

        try
        {
            var service = Exchange == "OKX" ? _okxService : _binanceService;
            var timeSpan = MapInterval(Interval);

            LogOutput += $"交易所: {Exchange}\n";
            LogOutput += $"交易对: {Symbol}\n";
            LogOutput += $"周期: {Interval}\n";
            LogOutput += $"目标根数: {Limit}\n";
            LogOutput += $"单次上限: {BatchSize}\n";
            LogOutput += "---\n";

            var results = await FetchKlinesRecursive(service, Symbol, timeSpan, Limit, BatchSize);

            LogOutput += $"---\n实际获取: {results.Count} 根 K 线\n";

            foreach (var k in results)
                KLineResults.Add(k);
        }
        catch (Exception ex)
        {
            LogOutput += $"错误: {ex.Message}\n";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<List<KLineData>> FetchKlinesRecursive(
        IExchangeService service,
        string symbol,
        TimeSpan interval,
        int totalLimit,
        int batchSize)
    {
        var allResults = new List<KLineData>();
        DateTime? endTime = null;
        var batchCount = 0;

        while (allResults.Count < totalLimit)
        {
            batchCount++;
            var remaining = totalLimit - allResults.Count;
            var currentBatchSize = Math.Min(remaining, batchSize);

            LogOutput += $"第 {batchCount} 批请求: limit={currentBatchSize}... ";

            var klines = (await service.GetKlinesAsync(symbol, interval, null, endTime, currentBatchSize)).ToList();

            LogOutput += $"返回 {klines.Count} 根\n";

            if (klines.Count == 0)
            {
                LogOutput += "API 返回空，数据已耗尽\n";
                break;
            }

            allResults.AddRange(klines);

            if (klines.Count < currentBatchSize)
            {
                LogOutput += $"API 数据不足（请求 {currentBatchSize} 根，实际返回 {klines.Count} 根）\n";
                break;
            }

            var earliestTime = klines.Min(k => k.OpenTime);
            endTime = earliestTime.AddTicks(-1);
        }

        return allResults;
    }

    private static TimeSpan MapInterval(string interval)
    {
        return interval switch
        {
            "1m" => TimeSpan.FromMinutes(1),
            "5m" => TimeSpan.FromMinutes(5),
            "15m" => TimeSpan.FromMinutes(15),
            "30m" => TimeSpan.FromMinutes(30),
            "1h" => TimeSpan.FromHours(1),
            "4h" => TimeSpan.FromHours(4),
            "1d" => TimeSpan.FromDays(1),
            _ => TimeSpan.FromHours(1)
        };
    }
}
