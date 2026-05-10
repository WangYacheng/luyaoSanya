using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradeSystem.App.ViewModels
{
    public partial class Test1ViewModel:ViewModel
    {
        private readonly IExchangeService _okxService;
        private readonly IExchangeService _binanceService;

        [ObservableProperty]
        private List<string> _okxSymbols;

        [ObservableProperty]
        private List<string> _binanceSymbols;

        public Test1ViewModel([FromKeyedServices("Binance")] IExchangeService binance,
                              [FromKeyedServices("OKX")] IExchangeService okx)
        {
            _binanceService = binance;
            _okxService = okx;
        }

        [RelayCommand]
        private async Task TestOkxSymbol()
        {
            var result = await _okxService.GetSymbolsAsync();
            OkxSymbols = result.ToList();
        }


        [RelayCommand]
        private async Task TestOkxKLine()
        {
            var result = await _binanceService.GetSymbolsAsync();
            BinanceSymbols = result.ToList();
        }

        [RelayCommand]
        private async Task TestOkxKLines()
        {

        }
        [RelayCommand]
        private async Task TestBinanceSymbol()
        {

        }
    }
}
