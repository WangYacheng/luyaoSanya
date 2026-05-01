using OKX.Net.Clients;
using OKX.Net.Enums;
using OKX.Net.Objects.Public;

namespace TestConsoleApp.JKExchange
{
    internal class DeepseekJK
    {
        /// <summary>
        /// 获取OKX交易所所有现货交易对（例如 BTC-USDT, ETH-USDT 等）
        /// </summary>
        public static async Task<List<string>> GetAllSpotTradingPairsAsync()
        {
            var restClient = new OKXRestClient();

            var result = await restClient.UnifiedApi.ExchangeData.GetSymbolsAsync(
                InstrumentType.Spot,
                null,   // underlying
                null,   // instFamily
                null,   // instId
                default
            );

            if (!result.Success)
            {
                Console.WriteLine($"获取交易对失败: {result.Error?.Message}");
                return new List<string>();
            }

            var pairs = result.Data
                .Where(i => i.State == InstrumentState.Live) // 只返回活跃的交易对
                .Select(i => i.Symbol)
                .OrderBy(s => s)
                .ToList();

            Console.WriteLine($"OKX 现货交易对共 {pairs.Count} 个");
            return pairs;
        }

        /// <summary>
        /// 获取OKX交易所指定类型的所有交易对（SPOT, SWAP, FUTURES, OPTION）
        /// </summary>
        public static async Task<List<string>> GetAllTradingPairsAsync(InstrumentType instrumentType)
        {
            var restClient = new OKXRestClient();

            var result = await restClient.UnifiedApi.ExchangeData.GetSymbolsAsync(
                instrumentType,
                null,
                null,
                null,
                default
            );

            if (!result.Success)
            {
                Console.WriteLine($"获取交易对失败: {result.Error?.Message}");
                return new List<string>();
            }

            var pairs = result.Data
                .Where(i => i.State == InstrumentState.Live)
                .Select(i => i.Symbol)
                .OrderBy(s => s)
                .ToList();

            Console.WriteLine($"OKX {instrumentType} 交易对共 {pairs.Count} 个");
            return pairs;
        }

        /// <summary>
                /// 获取OKX所有类型的所有活跃交易对（支持按 InstrumentType 过滤）
                /// </summary>
                public static async Task<Dictionary<InstrumentType, List<string>>> GetAllTradingPairsByTypeAsync()
                {
                    var restClient = new OKXRestClient();
                    var result = new Dictionary<InstrumentType, List<string>>();

                    var types = new[] { InstrumentType.Spot, InstrumentType.Swap, InstrumentType.Futures, InstrumentType.Option };

                    foreach (var type in types)
                    {
                        var response = await restClient.UnifiedApi.ExchangeData.GetSymbolsAsync(
                            type,
                            null,
                            null,
                            null,
                            default
                        );

                        if (response.Success)
                        {
                            var pairs = response.Data
                                .Where(i => i.State == InstrumentState.Live)
                                .Select(i => i.Symbol)
                                .OrderBy(s => s)
                                .ToList();

                            result[type] = pairs;
                            Console.WriteLine($"OKX {type}: {pairs.Count} 个交易对");
                        }
                        else
                        {
                            Console.WriteLine($"获取 {type} 交易对失败: {response.Error?.Message}");
                            result[type] = new List<string>();
                        }
                    }

                    return result;
                }

                /// <summary>
                /// 从 API 获取指定类型的交易对信息，筛选出结算单位（SettlementAsset）为 USDT 的交易对
                /// </summary>
                /// <param name="instrumentType">交易类型：SPOT（现货）、SWAP（永续合约）、FUTURES（交割合约）等</param>
                /// <param name="instIds">可选参数：传入要筛选的交易对符号集合（如 ["BTC-USDT", "ETH-USDT"]），
                /// 如果为 null 则不过滤交易对符号，返回该类型所有结算单位为 USDT 的交易对</param>
                /// <returns>结算单位为 USDT 的交易对符号列表，按符号排序</returns>
                public static async Task<List<string>> FilterUsdtSettlementPairsAsync(
                    InstrumentType instrumentType,
                    List<string>? instIds = null)
                {
                    var restClient = new OKXRestClient();

                    var result = await restClient.UnifiedApi.ExchangeData.GetSymbolsAsync(
                        instrumentType,
                        null,
                        null,
                        null,
                        default
                    );

                    if (!result.Success)
                    {
                        Console.WriteLine($"获取 {instrumentType} 交易对信息失败: {result.Error?.Message}");
                        return new List<string>();
                    }

                    // 筛选条件：仅活跃的交易对，且结算单位为 USDT（不区分大小写）
                    var query = result.Data
                        .Where(i => i.State == InstrumentState.Live)
                        .Where(i => string.Equals(i.SettlementAsset, "USDT", StringComparison.OrdinalIgnoreCase));

                    // 如果传入了指定的交易对符号集合，则进一步过滤
                    if (instIds != null && instIds.Count > 0)
                    {
                        var instIdSet = new HashSet<string>(instIds, StringComparer.OrdinalIgnoreCase);
                        query = query.Where(i => instIdSet.Contains(i.Symbol));
                    }

                    var pairs = query
                        .Select(i => i.Symbol)
                        .OrderBy(s => s)
                        .ToList();

                    Console.WriteLine($"结算单位为 USDT 的 {instrumentType} 交易对共 {pairs.Count} 个");
                    return pairs;
                }

                /// <summary>
                /// 从已获取的交易对明细列表中，筛选出结算单位为 USDT 的交易对
                /// </summary>
                /// <param name="instruments">OKXInstrument 明细列表，例如已从 GetSymbolsAsync 获取到的数据</param>
                /// <param name="instIds">可选参数：指定只关心哪些交易对符号，null 表示全部</param>
                /// <returns>结算单位为 USDT 的交易对符号列表</returns>
                public static List<string> FilterUsdtSettlementPairsFromInstruments(
                    IEnumerable<OKXInstrument> instruments,
                    List<string>? instIds = null)
                {
                    var query = instruments
                        .Where(i => i.State == InstrumentState.Live)
                        .Where(i => string.Equals(i.SettlementAsset, "USDT", StringComparison.OrdinalIgnoreCase));

                    if (instIds != null && instIds.Count > 0)
                    {
                        var instIdSet = new HashSet<string>(instIds, StringComparer.OrdinalIgnoreCase);
                        query = query.Where(i => instIdSet.Contains(i.Symbol));
                    }

                    return query
                        .Select(i => i.Symbol)
                        .OrderBy(s => s)
                        .ToList();
                }

                /// <summary>
                /// 从 API 直接获取所有结算单位为 USDT 的现货交易对
                /// </summary>
                public static async Task<List<string>> GetSpotUsdtPairsAsync()
                {
                    return await FilterUsdtSettlementPairsAsync(InstrumentType.Spot);
                }

                /// <summary>
                /// 从 API 直接获取所有结算单位为 USDT 的永续合约交易对
                /// </summary>
                public static async Task<List<string>> GetSwapUsdtPairsAsync()
                {
                    return await FilterUsdtSettlementPairsAsync(InstrumentType.Swap);
                }
            }
        }
