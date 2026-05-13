namespace TradeSystem.App.Models;

public class RankingItem
{
    public int Rank { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public decimal Open { get; set; }
    public decimal LastPrice { get; set; }
    public decimal ChangePct { get; set; }
    public decimal Volume { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
}
