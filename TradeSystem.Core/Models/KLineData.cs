using System;
using System.Collections.Generic;
using System.Text;

namespace TradeSystem.Core.Models
{
    public class KLineData
    {
        // 数据库主键 ID
        public long Id { get; set; }

        // 交易所名称 (如: Binance, OKX)
        public string Exchange { get; set; } = string.Empty;

        // 交易对名称 (如: BTCUSDT)
        public string Symbol { get; set; } = string.Empty;

        // K线周期 (如: 1m, 1h, 1d)
        public string TimeFrame { get; set; } = string.Empty;

        // 开盘时间
        public DateTime OpenTime { get; set; }

        // 价格信息
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }

        // 成交量 (标的资产数量，如多少个 BTC)
        public decimal Volume { get; set; }

        // 交易金额 (计价资产数量，如多少个 USDT)
        public decimal QuoteVolume { get; set; }

        public int Type { get; set; }

        // 业务属性：日涨幅百分比 (不存数据库，用于 UI 计算)
        public decimal ChangePct => Open != 0 ? (Close - Open) / Open * 100 : 0;
    }
}
