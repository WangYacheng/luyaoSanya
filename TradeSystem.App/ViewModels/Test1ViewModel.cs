using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeSystem.App.ViewModels
{
    public partial class Test1ViewModel : ViewModel
    {
        private readonly IExchangeService _okxService;
        private readonly IExchangeService _binanceService;

        [ObservableProperty]
        private List<string> _okxSymbols = [];

        [ObservableProperty]
        private List<string> _binanceSymbols = [];

        [ObservableProperty]
        private string _logOutput = string.Empty;

        public Test1ViewModel([FromKeyedServices("Binance")] IExchangeService binance,
                              [FromKeyedServices("OKX")] IExchangeService okx)
        {
            _binanceService = binance;
            _okxService = okx;
        }

        [RelayCommand]
        private async Task StartTest()
        {
            LogOutput += "开始测试...\n";
            try
            {
                var okxResult = await _okxService.GetSymbolsAsync();
                OkxSymbols = okxResult.ToList();
                LogOutput += $"OKX 符号数: {OkxSymbols.Count}\n";

                var binanceResult = await _binanceService.GetSymbolsAsync();
                BinanceSymbols = binanceResult.ToList();
                LogOutput += $"Binance 符号数: {BinanceSymbols.Count}\n";
            }
            catch (Exception ex)
            {
                LogOutput += $"错误: {ex.Message}\n";
            }
            LogOutput += "测试完成\n";
        }

        [RelayCommand]
        private void ClearLog()
        {
            LogOutput = string.Empty;
        }

        [RelayCommand]
        private async Task ExportReport()
        {
            var report = $"测试报告 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
            report += $"OKX 符号数: {OkxSymbols.Count}\n";
            report += $"Binance 符号数: {BinanceSymbols.Count}\n";
            report += $"日志:\n{LogOutput}";

            var filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            );
            await File.WriteAllTextAsync(filePath, report);
            LogOutput += $"报告已导出: {filePath}\n";
        }
    }
}
