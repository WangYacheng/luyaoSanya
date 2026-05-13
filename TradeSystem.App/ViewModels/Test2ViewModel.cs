using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Binance.Net.Clients;
using OKX.Net.Clients;
using OKX.Net.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TradeSystem.App.ViewModels;

public partial class Test2ViewModel : ViewModel
{
    private readonly IExchangeService _okxService;
    private readonly IExchangeService _binanceService;

    [ObservableProperty]
    private string _logOutput = string.Empty;

    [ObservableProperty]
    private string _param1 = string.Empty;

    [ObservableProperty]
    private string _param2 = string.Empty;

    [ObservableProperty]
    private string _param3 = string.Empty;

    [ObservableProperty]
    private string _param4 = string.Empty;

    public Test2ViewModel(
        [FromKeyedServices("OKX")] IExchangeService okxService,
        [FromKeyedServices("Binance")] IExchangeService binanceService)
    {
        _okxService = okxService;
        _binanceService = binanceService;
    }

    private void AppendLog(string message)
        => LogOutput += $"[{DateTime.Now:HH:mm:ss}] {message}\n";

    [RelayCommand]
    private async Task TestGetSymbols()
    {
        AppendLog("获取 OKX 交易对...");
        try
        {
            var marketType = Param1?.ToLower() switch
            {
                "spot" => MarketType.Spot,
                _ => MarketType.Swap
            };
            var quoteAsset = string.IsNullOrWhiteSpace(Param2) ? "USDT" : Param2.Trim();
            AppendLog($"参数: MarketType={marketType}, QuoteAsset={quoteAsset}");

            var result = await _okxService.GetSymbolsAsync(marketType, quoteAsset);
            var list = result.ToList();
            AppendLog($"获取成功，共 {list.Count} 个交易对");

            // 输出第一个元素的所有属性
            if (list.Count > 0)
            {
                var first = list[0];
                var type = first?.GetType();
                AppendLog($"第一个元素类型: {type?.Name}");

                if (type == typeof(string))
                {
                    AppendLog($"  Value: {first}");
                }
                else
                {
                    foreach (var prop in type?.GetProperties() ?? [])
                    {
                        try
                        {
                            var val = prop.GetValue(first);
                            AppendLog($"  {prop.Name}: {val}");
                        }
                        catch (Exception ex)
                        {
                            AppendLog($"  {prop.Name}: <{ex.Message}>");
                        }
                    }
                }
                if (list.Count > 1)
                    AppendLog($"  ... 还有 {list.Count - 1} 个未展开");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"错误: {ex.Message}");
        }
        AppendLog("完成");
    }

    [RelayCommand]
    private async Task TestGetBinanceSymbols()
    {
        AppendLog("获取 Binance USDT 永续合约...");
        try
        {
            var marketType = Param1?.ToLower() == "spot" ? MarketType.Spot : MarketType.Swap;
            var quoteAsset = string.IsNullOrWhiteSpace(Param2) ? "USDT" : Param2.Trim();
            AppendLog($"参数: MarketType={marketType}, QuoteAsset={quoteAsset}");

            var result = await _binanceService.GetSymbolsAsync(marketType, quoteAsset);
            var list = result.ToList();
            AppendLog($"获取成功，共 {list.Count} 个交易对");

            if (list.Count > 0)
            {
                var first = list[0];
                var type = first?.GetType();
                AppendLog($"第一个元素类型: {type?.Name}");

                if (type == typeof(string))
                {
                    AppendLog($"  Value: {first}");
                }
                else
                {
                    foreach (var prop in type?.GetProperties() ?? [])
                    {
                        try
                        {
                            var val = prop.GetValue(first);
                            AppendLog($"  {prop.Name}: {val}");
                        }
                        catch (Exception ex)
                        {
                            AppendLog($"  {prop.Name}: <{ex.Message}>");
                        }
                    }
                }
                if (list.Count > 1)
                    AppendLog($"  ... 还有 {list.Count - 1} 个未展开");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"错误: {ex.Message}");
        }
        AppendLog("完成");
    }

    [RelayCommand]
    private async Task TestGetBinanceSymbolsRaw()
    {
        AppendLog("Binance 原生 API 调用 USDT 永续合约...");
        try
        {
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12;
            var client = new BinanceRestClient();
            var result = await client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();

            AppendLog($"调用成功: {result.Success}");
            if (result.Error != null)
            {
                AppendLog($"错误信息: {result.Error.Message}");
                return;
            }

            var symbols = result.Data?.Symbols?.ToList();
            if (symbols == null || symbols.Count == 0)
            {
                AppendLog("无数据");
                return;
            }
            AppendLog($"返回数据量: {symbols.Count}");

            var first = symbols[0];
            var type = first?.GetType();
            AppendLog($"第一个元素类型: {type?.Name}");
            foreach (var prop in type?.GetProperties() ?? [])
            {
                try
                {
                    var val = prop.GetValue(first);
                    AppendLog($"  {prop.Name}: {val}");
                }
                catch (Exception ex)
                {
                    AppendLog($"  {prop.Name}: <{ex.Message}>");
                }
            }

            // 循环输出所有元素的 Name, Pair, BaseAsset, MarginAsset, QuoteAsset
            AppendLog($"--- 所有元素关键属性 (共 {symbols.Count} 个) ---");
            foreach (var item in symbols)
            {
                var t = item?.GetType();
                var name = t?.GetProperty("Name")?.GetValue(item);
                var pair = t?.GetProperty("Pair")?.GetValue(item);
                var baseAsset = t?.GetProperty("BaseAsset")?.GetValue(item);
                var marginAsset = t?.GetProperty("MarginAsset")?.GetValue(item);
                var quoteAsset = t?.GetProperty("QuoteAsset")?.GetValue(item);
                AppendLog($"  Name: {name}, Pair: {pair}, BaseAsset: {baseAsset}, MarginAsset: {marginAsset}, QuoteAsset: {quoteAsset}");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"异常: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                AppendLog($"内部异常: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                if (ex.InnerException.InnerException != null)
                    AppendLog($"内部异常2: {ex.InnerException.InnerException.Message}");
            }
        }
        AppendLog("完成");
    }

    [RelayCommand]
    private async Task TestGetSymbolsRaw()
    {
        AppendLog("诊断：直接调用 OKX 原始 API...");
        try
        {
            var instType = Param1?.ToLower() == "spot" ? InstrumentType.Spot : InstrumentType.Swap;
            AppendLog($"参数: instType={instType}");

            var client = new OKXRestClient();
            var result = await client.UnifiedApi.ExchangeData.GetSymbolsAsync(instType);

            AppendLog($"调用成功: {result.Success}");
            if (result.Error != null)
            {
                var code = result.Error.GetType().GetProperty("Code")?.GetValue(result.Error);
                AppendLog($"错误代码: {code}");
                AppendLog($"错误信息: {result.Error.Message}");
            }

            if (result.Data != null)
            {
                var dataList = result.Data.ToList();
                AppendLog($"返回数据量: {dataList.Count}");

                // 输出第一个元素的所有属性
                if (dataList.Count > 0)
                {
                    var first = dataList[0];
                    var type = first?.GetType();
                    AppendLog($"第一个元素类型: {type?.Name}");
                    foreach (var prop in type?.GetProperties() ?? [])
                    {
                        try
                        {
                            var val = prop.GetValue(first);
                            AppendLog($"  {prop.Name}: {val}");
                        }
                        catch (Exception ex)
                        {
                            AppendLog($"  {prop.Name}: <{ex.Message}>");
                        }
                    }
                }

                // 循环输出所有元素的 Symbol, SettlementAsset, BaseAsset, QuoteAsset
                AppendLog($"--- 所有元素关键属性 (共 {dataList.Count} 个) ---");
                foreach (var item in dataList)
                {
                    var t = item?.GetType();
                    var symbol = t?.GetProperty("Symbol")?.GetValue(item);
                    var settlementAsset = t?.GetProperty("SettlementAsset")?.GetValue(item);
                    var baseAsset = t?.GetProperty("BaseAsset")?.GetValue(item);
                    var quoteAsset = t?.GetProperty("QuoteAsset")?.GetValue(item);
                    AppendLog($"  Symbol: {symbol}, SettlementAsset: {settlementAsset}, BaseAsset: {baseAsset}, QuoteAsset: {quoteAsset}");
                }
            }
        }
        catch (Exception ex)
        {
            AppendLog($"异常: {ex.GetType().Name}: {ex.Message}");
        }
        AppendLog("完成");
    }

    [RelayCommand]
    private async Task TestOkx24HKLine()
    {
        AppendLog("=== OKX 24H K线数据 ===");
        try
        {
            var summaries = (await _okxService.GetAll24HSummariesAsync()).ToList();
            AppendLog($"共 {summaries.Count} 个交易对");
            AppendLog("");

            var sorted = summaries.OrderByDescending(s => s.ChangePct).ToList();
            var rank = 0;
            foreach (var s in sorted)
            {
                rank++;
                var sign = s.ChangePct >= 0 ? "+" : "";
                AppendLog($"  #{rank,-4} {s.Symbol,-24} 涨幅: {sign}{s.ChangePct:F2}%");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"错误: {ex.Message}");
        }
        AppendLog("完成");
    }

    [RelayCommand]
    private async Task TestBinance24HKLine()
    {
        AppendLog("=== Binance 24H K线数据 ===");
        try
        {
            var summaries = (await _binanceService.GetAll24HSummariesAsync()).ToList();
            AppendLog($"共 {summaries.Count} 个交易对");
            AppendLog("");

            var sorted = summaries.OrderByDescending(s => s.ChangePct).ToList();
            var rank = 0;
            foreach (var s in sorted)
            {
                rank++;
                var sign = s.ChangePct >= 0 ? "+" : "";
                AppendLog($"  #{rank,-4} {s.Symbol,-24} 涨幅: {sign}{s.ChangePct:F2}% {s.Open} {s.Close} {s.High} {s.Low}");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"错误: {ex.Message}");
        }
        AppendLog("完成");
    }

    [RelayCommand]
    private void ClearLog() => LogOutput = string.Empty;

    [RelayCommand]
    private async Task TestSymbolUnion()
    {
        AppendLog("获取 OKX 交易对...");
        try
        {
            var marketType = Param1?.ToLower() switch
            {
                "spot" => MarketType.Spot,
                _ => MarketType.Swap
            };
            var quoteAsset = string.IsNullOrWhiteSpace(Param2) ? "USDT" : Param2.Trim();
            AppendLog($"参数: MarketType={marketType}, QuoteAsset={quoteAsset}");

            var result = await _okxService.GetSymbolsAsync(marketType, quoteAsset);
            var list = result.ToList();
            list = list.Select(x => x.Substring(0, x.Length - 10)).ToList();
            AppendLog($"获取成功，共 {list.Count} 个交易对");

            if (list.Count > 1)
                AppendLog($"  ... 还有 {list.Count - 1} 个未展开");

            AppendLog("获取 Binance USDT 永续合约...");
            var marketType2 = Param1?.ToLower() == "spot" ? MarketType.Spot : MarketType.Swap;
            var quoteAsset2 = string.IsNullOrWhiteSpace(Param2) ? "USDT" : Param2.Trim();
            AppendLog($"参数: MarketType={marketType}, QuoteAsset={quoteAsset}");

            var result2 = await _binanceService.GetSymbolsAsync(marketType, quoteAsset);
            var list2 = result2.ToList();
            list2 = list2.Select(x => x.Substring(0, x.Length - 4)).ToList();
            AppendLog($"获取成功，共 {list2.Count} 个交易对");
            AppendLog($"okx: {list[0]} binance:{list2[0]}");

            var intersect = list.Intersect(list2).ToList();
            AppendLog($"交集总数：{intersect.Count}");
            intersect.ForEach(x => AppendLog($"{x}"));

            AppendLog($"binance并集前 {list2.Count} 个交易对");
            list2.RemoveAll(x => list.Contains(x));
            AppendLog($"binance并集后 {list2.Count} 个交易对");
        }
        catch (Exception ex)
        {
            AppendLog($"错误: {ex.Message}");
        }
        AppendLog("完成");
    }
}
