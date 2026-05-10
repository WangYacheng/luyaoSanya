using TradeSystem.Core.Models;

/// <summary>
/// 市场类型枚举：现货、永续合约
/// </summary>
public enum MarketType
    {
        Spot,
        Swap // 永续合约
    }

    /// <summary>
    /// Unified exchange market data service interface
    /// </summary>
    public interface IExchangeService
    {
        string ExchangeName { get; }
        
        /// <summary>
        /// 获取交易对名称
        /// </summary>
        /// <param name="marketType">现货或合约</param>
        /// <param name="quoteAsset">结算货币，如 USDT</param>
        Task<IEnumerable<string>> GetSymbolsAsync(MarketType marketType = MarketType.Swap, string quoteAsset = "USDT");
        
        // Get history Klines for a single symbol
        Task<IEnumerable<KLineData>> GetKlinesAsync(string symbol, TimeSpan interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100);
        
        // Get history Klines for multiple symbols
        Task<Dictionary<string, IEnumerable<KLineData>>> GetMultipleKlinesAsync(IEnumerable<string> symbols, TimeSpan interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 100);
        
        // Get 24h ticker summaries for all symbols
        Task<IEnumerable<KLineData>> GetAll24HSummariesAsync();
    }