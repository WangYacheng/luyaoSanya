using Binance.Net.Clients;
using TradeSystem.Core.Models;

/// <summary>
/// Binance implementation
/// </summary>
public class BinanceService : IExchangeService
{
    private readonly BinanceRestClient _client;
    private MarketType _currentMarketType = MarketType.Swap; // Private field to store market context

    public string ExchangeName => "Binance";

    public BinanceService()
    {
        _client = new BinanceRestClient();
    }

    public async Task<IEnumerable<string>> GetSymbolsAsync(MarketType marketType = MarketType.Swap, string quoteAsset = "USDT")
    {
        // Store the market type into the private field
        _currentMarketType = marketType;

        if (_currentMarketType == MarketType.Spot)
        {
            var result = await _client.SpotApi.ExchangeData.GetExchangeInfoAsync();
            if (!result.Success) return Enumerable.Empty<string>();

            return result.Data.Symbols
                .Where(s => s.Status == Binance.Net.Enums.SymbolStatus.Trading &&
                            (string.IsNullOrEmpty(quoteAsset) || s.QuoteAsset.Equals(quoteAsset, StringComparison.OrdinalIgnoreCase)))
                .Select(s => s.Name);
        }
        else
        {
            // Binance uses UsdtMarginFuturesApi for Swap
            var result = await _client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();
            if (!result.Success) return Enumerable.Empty<string>();

            return result.Data.Symbols
                .Where(s => s.Status == Binance.Net.Enums.SymbolStatus.Trading &&
                            (string.IsNullOrEmpty(quoteAsset) || s.QuoteAsset.Equals(quoteAsset, StringComparison.OrdinalIgnoreCase)))
                .Select(s => s.Name);
        }
    }

    public async Task<IEnumerable<KLineData>> GetKlinesAsync(string symbol, TimeSpan interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100)
    {
        var binanceInterval = MapInterval(interval);

        // Switch API based on the stored private field
        var result = _currentMarketType == MarketType.Spot
            ? await _client.SpotApi.ExchangeData.GetKlinesAsync(symbol, binanceInterval, startTime, endTime, limit)
            : await _client.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol, binanceInterval, startTime, endTime, limit);

        if (!result.Success) return Enumerable.Empty<KLineData>();

        return result.Data.Where(x => x.CloseTime < DateTime.UtcNow).Select(k => new KLineData
        
        {
            Symbol = symbol,
            OpenTime = k.OpenTime,
            Open = k.OpenPrice,
            High = k.HighPrice,
            Low = k.LowPrice,
            Close = k.ClosePrice,
            Volume = k.Volume,
            QuoteVolume = k.QuoteVolume
            QuoteVolume = k.QuoteVolume
            //CreateTime = DateTime.Now 
        });
    }

    public async Task<Dictionary<string, IEnumerable<KLineData>>> GetMultipleKlinesAsync(IEnumerable<string> symbols, TimeSpan interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100)
    {
        var results = new Dictionary<string, IEnumerable<KLineData>>();
        using var semaphore = new SemaphoreSlim(5);

        var tasks = symbols.Select(async symbol =>
        {
            await semaphore.WaitAsync();
            try
            {
                var klines = await GetKlinesAsync(symbol, interval, startTime, endTime, limit);
                lock (results)
                {
                    results[symbol] = klines;
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    public async Task<IEnumerable<KLineData>> GetAll24HSummariesAsync()
    {
        // 1. 统一获取结果。注意：现货和合约返回的 Ticker 类型其实是不一样的
        // 现货返回的是 IBinanceTick，合约返回的是 IBinanceFuturesTick
        if (_currentMarketType == MarketType.Spot)
        {
            var result = await _client.SpotApi.ExchangeData.GetTickersAsync();
            if (!result.Success) return Enumerable.Empty<KLineData>();

            return result.Data.Select(t => new KLineData
            {
                Symbol = t.Symbol,
                High = t.HighPrice,
                Low = t.LowPrice,
                Close = t.LastPrice,
                Volume = t.Volume, // 现货中 Volume 通常指总成交额，TotalTradingVolume 指成交量
                OpenTime = t.OpenTime,         // 使用接口返回的实际开启时间
                //CloseTime = t.CloseTime        // 使用接口返回的实际结束时间
                Open=t.OpenPrice
                Open=t.OpenPrice
            });
        }
        else
        {
            // 这里根据你使用的 SDK 版本，可能是 UsdFuturesApi 或 UsdtFuturesApi
            var result = await _client.UsdFuturesApi.ExchangeData.GetTickersAsync();
            if (!result.Success) return Enumerable.Empty<KLineData>();

            return result.Data.Select(t => new KLineData
            {
                Symbol = t.Symbol,
                High = t.HighPrice,
                Low = t.LowPrice,
                Close = t.LastPrice,
                Volume = t.Volume,             // 合约接口属性名为 Volume
                OpenTime = t.OpenTime, // 合约 Ticker 接口有时不返回 OpenTime，需手动计算
                //CloseTime = DateTime.Now
                Open=t.OpenPrice
                
            });
        }
    }

    public async Task<Dictionary<string, decimal>> Get24HChangePercentAsync()
    {
        var result = await _client.UsdFuturesApi.ExchangeData.GetTickersAsync();
        if (!result.Success) return [];

        return result.Data.ToDictionary(
            t => t.Symbol,
            t => t.PriceChangePercent,
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task<Dictionary<string, decimal>> Get24HChangePercentAsync()
    {
        var result = await _client.UsdFuturesApi.ExchangeData.GetTickersAsync();
        if (!result.Success) return [];

        return result.Data.ToDictionary(
            t => t.Symbol,
            t => t.PriceChangePercent,
            StringComparer.OrdinalIgnoreCase);
    }

    private Binance.Net.Enums.KlineInterval MapInterval(TimeSpan interval)
    {
        if (interval.TotalMinutes == 1) return Binance.Net.Enums.KlineInterval.OneMinute;
        if (interval.TotalMinutes == 5) return Binance.Net.Enums.KlineInterval.FiveMinutes;
        if (interval.TotalHours == 1) return Binance.Net.Enums.KlineInterval.OneHour;
        if (interval.TotalHours == 4) return Binance.Net.Enums.KlineInterval.FourHour;
        if (interval.TotalHours == 4) return Binance.Net.Enums.KlineInterval.FourHour;
        return Binance.Net.Enums.KlineInterval.OneDay;
    }
}