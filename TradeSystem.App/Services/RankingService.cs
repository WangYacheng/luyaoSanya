using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeSystem.App.Models;
using TradeSystem.Core.Models;
using TradeSystem.Infrastructure.Data;

namespace TradeSystem.App.Services;

public class RankingService
{
    private readonly IExchangeService _binanceService;
    private readonly IExchangeService _okxService;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public RankingService(
        [FromKeyedServices("Binance")] IExchangeService binanceService,
        [FromKeyedServices("OKX")] IExchangeService okxService,
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _binanceService = binanceService;
        _okxService = okxService;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<RankingItem>> GetRankingsAsync(DateTime date, int topN, string exchange)
    {
        if (date.Date == DateTime.Today)
            return exchange switch
            {
                "OKX" => await GetOkxLiveRankingsAsync(topN),
                "Binance" => await GetBinanceLiveRankingsAsync(topN),
                _ => throw new ArgumentException($"Unknown exchange: {exchange}")
            };

        return await GetHistoricalRankingsAsync(date, topN, exchange);
    }

    private async Task<List<RankingItem>> GetOkxLiveRankingsAsync(int topN)
    {
        var summaries = await _okxService.GetAll24HSummariesAsync();

        return summaries
            .Where(k => k.Open > 0)
            .OrderByDescending(k => k.ChangePct)
            .Take(topN)
            .Select((k, i) => new RankingItem
            {
                Rank = i + 1,
                Symbol = k.Symbol.Replace("-USDT-SWAP", "", StringComparison.OrdinalIgnoreCase),
                Exchange = "OKX",
                LastPrice = k.Close,
                ChangePct = k.ChangePct,
                Volume = k.Volume,
                High = k.High,
                Low = k.Low
            })
            .ToList();
    }

    private async Task<List<RankingItem>> GetBinanceLiveRankingsAsync(int topN)
    {
        var summariesTask = _binanceService.GetAll24HSummariesAsync();
        var changePctsTask = _binanceService.Get24HChangePercentAsync();
        await Task.WhenAll(summariesTask, changePctsTask);

        var summaries = summariesTask.Result;
        var changePcts = changePctsTask.Result;

        return summaries
            .Where(k => changePcts.ContainsKey(k.Symbol))
            .Select(k => new RankingItem
            {
                Symbol = k.Symbol,
                Exchange = "Binance",
                LastPrice = k.Close,
                ChangePct = changePcts[k.Symbol],
                Volume = k.Volume,
                High = k.High,
                Low = k.Low
            })
            .OrderByDescending(r => r.ChangePct)
            .Take(topN)
            .Select((r, i) =>
            {
                r.Rank = i + 1;
                return r;
            })
            .ToList();
    }

    private async Task<List<RankingItem>> GetHistoricalRankingsAsync(DateTime date, int topN, string exchange)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var utcStart = date.Date.AddHours(-8);
        var utcEnd = utcStart.AddDays(1);

        var klines = await db.KLines
            .Where(k => k.Exchange == exchange
                && k.TimeFrame == "1d"
                && k.OpenTime >= utcStart
                && k.OpenTime < utcEnd)
            .ToListAsync();

        return klines
            .Where(k => k.Open > 0)
            .OrderByDescending(k => k.ChangePct)
            .Take(topN)
            .Select((k, i) => new RankingItem
            {
                Rank = i + 1,
                Symbol = exchange == "OKX"
                    ? k.Symbol.Replace("-USDT-SWAP", "", StringComparison.OrdinalIgnoreCase)
                    : k.Symbol,
                Exchange = exchange,
                LastPrice = k.Close,
                ChangePct = k.ChangePct,
                Volume = k.Volume,
                High = k.High,
                Low = k.Low
            })
            .ToList();
    }
}
