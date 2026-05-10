
using OKX.Net.Clients;
using OKX.Net.Enums;
using TradeSystem.Core.Models;

/// <summary>
/// OKX 实现类
/// </summary>
public class OkxService : IExchangeService
    {
        private readonly OKXRestClient _client;
        private MarketType _currentMarketType = MarketType.Swap;

        public string ExchangeName => "OKX";

        public OkxService()
        {
            _client = new OKXRestClient();
        }

        /// <summary>
        /// 将内部 MarketType 转换为 OKX API 所需的 InstrumentType (instType) 字符串
        /// </summary>
        private InstrumentType ToOkxInstType(MarketType marketType)
        {
            return marketType switch
            {
                MarketType.Spot => InstrumentType.Spot,
                MarketType.Swap => InstrumentType.Swap,
                _ => InstrumentType.Swap
            };
        }
        public async Task<IEnumerable<string>> GetSymbolsAsync(MarketType marketType = MarketType.Swap, string quoteAsset = "USDT")
        {
            _currentMarketType = marketType;

            // OKX 使用 UnifiedApi，instType 对应：SPOT, SWAP
            var instType = ToOkxInstType(_currentMarketType);
            
            // 修正：使用 UnifiedApi 替代不存在的 SpotApi
            var result = await _client.UnifiedApi.ExchangeData.GetSymbolsAsync(instType);
            if (!result.Success) return Enumerable.Empty<string>();

            return result.Data
                .Where(s => string.IsNullOrEmpty(quoteAsset) || s.QuoteAsset.Equals(quoteAsset, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Symbol);
        }

        public async Task<IEnumerable<KLineData>> GetKlinesAsync(string symbol, TimeSpan interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100)
        {
            var okxInterval = MapInterval(interval);
            
            // 修正：OKX V5 API K线接口在 UnifiedApi 下
            var result = await _client.UnifiedApi.ExchangeData.GetKlinesAsync(symbol, okxInterval, startTime, endTime, limit);
            
            if (!result.Success) return Enumerable.Empty<KLineData>();

            return result.Data.Select(k => new KLineData
            {
                Symbol = symbol,
                OpenTime = k.Time,
                Open = k.OpenPrice,
                High = k.HighPrice,
                Low = k.LowPrice,
                Close = k.ClosePrice,
                Volume = k.Volume,
                QuoteVolume = k.VolumeCurrency
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
            // 修正：OKX Ticker 接口在 UnifiedApi 下，通过 instType 区分现货与合约
            var instType = ToOkxInstType(_currentMarketType);
            var result = await _client.UnifiedApi.ExchangeData.GetTickersAsync(instType);
            if (!result.Success) return Enumerable.Empty<KLineData>();

            return result.Data.Select(t => new KLineData
            {
                Symbol = t.Symbol,
                High = (decimal)t.HighPrice,
                Low = (decimal)t.LowPrice,
                Close = (decimal)t.LastPrice,
                Volume = t.Volume,
                QuoteVolume = t.QuoteVolume
                //CreateTime = DateTime.Now
            });
        }

        private KlineInterval MapInterval(TimeSpan interval)
        {
            if (interval.TotalMinutes == 1) return KlineInterval.OneMinute;
            if (interval.TotalMinutes == 5) return KlineInterval.FiveMinutes;
            if (interval.TotalHours == 1) return KlineInterval.OneHour;
            return KlineInterval.OneDay;
        }
    }