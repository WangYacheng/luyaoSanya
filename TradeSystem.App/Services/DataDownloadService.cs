using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeSystem.Core.Abstractions;
using TradeSystem.Core.Models;
using TradeSystem.Infrastructure.Data;

namespace TradeSystem.App.Services;

public class DownloadProgress
{
    public string Exchange { get; set; } = string.Empty;
    public int Current { get; set; }
    public int Total { get; set; }
    public string CurrentSymbol { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
}

public class DataDownloadService
{
    private readonly IExchangeService _binanceService;
    private readonly IExchangeService _okxService;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    private List<string> _okxSymbols = null!;
    private List<string> _binanceSymbols = null!;
    private bool _symbolsInitialized;

    public DataDownloadService(
        [FromKeyedServices("Binance")] IExchangeService binanceService,
        [FromKeyedServices("OKX")] IExchangeService okxService,
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _binanceService = binanceService;
        _okxService = okxService;
        _dbContextFactory = dbContextFactory;
    }

    private async ValueTask EnsureSymbolsAsync()
    {
        if (_symbolsInitialized) return;
        (_okxSymbols, _binanceSymbols) = await TestSymbolUnion();
        _symbolsInitialized = true;
    }

    public async Task<List<string>> GetOkxSymbolsAsync()
    {
        return (await _okxService.GetSymbolsAsync(MarketType.Swap, "USDT")).ToList();
    }

    public async Task<List<string>> GetBinanceSymbolsAsync()
    {
        var okxRaw = await _okxService.GetSymbolsAsync(MarketType.Swap, "USDT");
        var binanceRaw = await _binanceService.GetSymbolsAsync(MarketType.Swap, "USDT");

        var okxNormalized = okxRaw.Select(ToBinanceFormat).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return binanceRaw.Where(s => !okxNormalized.Contains(s)).ToList();
    }

    public async Task DownloadOkxAsync(IProgress<DownloadProgress> progress)
    {
        await EnsureSymbolsAsync();
        await DownloadExchangeDailyAsync(_okxService, "OKX", _okxSymbols, progress);
    }

    public async Task DownloadBinanceAsync(IProgress<DownloadProgress> progress)
    {
        await EnsureSymbolsAsync();
        await DownloadExchangeDailyAsync(_binanceService, "Binance", _binanceSymbols, progress);
    }

    public async Task DownloadExchangeDailyAsync(
        IExchangeService exchangeService,
        string exchangeName,
        List<string> symbols,
        IProgress<DownloadProgress> progress)
    {
        if (symbols == null || symbols.Count == 0)
        {
            progress.Report(new DownloadProgress
            {
                Exchange = exchangeName,
                StatusMessage = $"{exchangeName} 未获取到交易对"
            });
            return;
        }

        progress.Report(new DownloadProgress
        {
            Exchange = exchangeName,
            Total = symbols.Count,
            StatusMessage = $"{exchangeName} 共 {symbols.Count} 个交易对"
        });

        var interval = TimeSpan.FromDays(1);
        var limit = 5;
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);

        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var batchSize = 20;
        for (var i = 0; i < symbols.Count; i += batchSize)
        {
            var batch = symbols.Skip(i).Take(batchSize).ToList();
            var allKlines = new List<KLineData>();

            foreach (var symbol in batch)
            {
                progress.Report(new DownloadProgress
                {
                    Exchange = exchangeName,
                    Current = Math.Min(i + batch.IndexOf(symbol) + 1, symbols.Count),
                    Total = symbols.Count,
                    CurrentSymbol = symbol,
                    StatusMessage = $"正在下载 {exchangeName} {symbol} 日线数据..."
                });

                var klines = (await exchangeService.GetKlinesAsync(symbol, interval, null, null, limit)).ToList();

                foreach (var k in klines)
                {
                    k.Exchange = exchangeName;
                    k.TimeFrame = "1d";
                    k.Type = 0;
                }

                allKlines.AddRange(klines.Where(k => k.OpenTime >= oneDayAgo.AddDays(-10)));
            }

            if (allKlines.Count == 0) continue;

            var minTime = allKlines.Min(x => x.OpenTime);
            var maxTime = allKlines.Max(x => x.OpenTime);
            var existingKeys = await db.KLines
                .Where(k => k.Exchange == exchangeName
                         && k.TimeFrame == "1d"
                         && batch.Contains(k.Symbol)
                         && k.OpenTime >= minTime
                         && k.OpenTime <= maxTime)
                .Select(k => new { k.Exchange, k.Symbol, k.OpenTime, k.TimeFrame })
                .ToListAsync();

            var existingSet = existingKeys
                .Select(k => $"{k.Exchange}|{k.Symbol}|{k.OpenTime.Ticks}|{k.TimeFrame}")
                .ToHashSet();

            var newKlines = allKlines
                .Where(k => !existingSet.Contains($"{k.Exchange}|{k.Symbol}|{k.OpenTime.Ticks}|{k.TimeFrame}"))
                .ToList();

            if (newKlines.Count > 0)
            {
                db.KLines.AddRange(newKlines);
                await db.SaveChangesAsync();
            }
        }

        progress.Report(new DownloadProgress
        {
            Exchange = exchangeName,
            Current = symbols.Count,
            Total = symbols.Count,
            StatusMessage = $"{exchangeName} 下载完成"
        });
    }

    private async Task<(List<string> Okx, List<string> Binance)> TestSymbolUnion()
    {
        try
        {
            var list = (await _okxService.GetSymbolsAsync(MarketType.Swap, "USDT"))
                .Select(x => x[..^10])
                .ToList();

            var list2 = (await _binanceService.GetSymbolsAsync(MarketType.Swap, "USDT"))
                .Select(x => x[..^4])
                .ToList();

            var okxSet = new HashSet<string>(list);
            list2.RemoveAll(okxSet.Contains);

            return (
                list.Select(x => x + "-USDT-SWAP").ToList(),
                list2.Select(x => x + "USDT").ToList()
            );
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.ToString());
            return (new List<string>(), new List<string>());
        }
    }
    private static string ToBinanceFormat(string okxSymbol)
    {
        var s = okxSymbol.ToUpperInvariant();
        if (s.EndsWith("-SWAP"))
            s = s[..^5];
        return s.Replace("-", "");
    }
}
