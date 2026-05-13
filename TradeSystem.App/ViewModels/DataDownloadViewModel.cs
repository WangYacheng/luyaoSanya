using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using TradeSystem.App.Services;

namespace TradeSystem.App.ViewModels;

public partial class DataDownloadViewModel : ViewModel
{
    private readonly DataDownloadService _downloadService;
    private readonly StringBuilder _statusLog = new();

    [ObservableProperty]
    private bool _isOkxDownloading;

    [ObservableProperty]
    private bool _isBinanceDownloading;

    [ObservableProperty]
    private int _okxCurrent;

    [ObservableProperty]
    private int _okxTotal;

    [ObservableProperty]
    private int _binanceCurrent;

    [ObservableProperty]
    private int _binanceTotal;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private string _statusLogText = string.Empty;

    public DataDownloadViewModel(DataDownloadService downloadService)
    {
        _downloadService = downloadService;
    }

    [RelayCommand]
    private async Task DownloadOkx()
    {
        if (IsOkxDownloading) return;
        IsOkxDownloading = true;
        OkxCurrent = 0;
        OkxTotal = 0;

        await DownloadExchange("OKX", _downloadService.DownloadOkxAsync,
            () => IsOkxDownloading = false);
    }

    [RelayCommand]
    private async Task DownloadBinance()
    {
        if (IsBinanceDownloading) return;
        IsBinanceDownloading = true;
        BinanceCurrent = 0;
        BinanceTotal = 0;

        await DownloadExchange("Binance", _downloadService.DownloadBinanceAsync,
            () => IsBinanceDownloading = false);
    }

    private async Task DownloadExchange(
        string exchangeName,
        Func<IProgress<DownloadProgress>, Task> downloadFunc,
        Action onComplete)
    {
        _statusLog.AppendLine($"[{DateTime.Now:HH:mm:ss}] 开始下载 {exchangeName} K线数据...");
        StatusLogText = _statusLog.ToString();
        StatusMessage = $"正在获取 {exchangeName} 交易对...";

        try
        {
            var progress = new Progress<DownloadProgress>(p =>
            {
                if (p.Exchange == "OKX")
                {
                    OkxCurrent = p.Current;
                    if (p.Total > 0) OkxTotal = p.Total;
                }
                else if (p.Exchange == "Binance")
                {
                    BinanceCurrent = p.Current;
                    if (p.Total > 0) BinanceTotal = p.Total;
                }

                if (!string.IsNullOrEmpty(p.StatusMessage))
                {
                    StatusMessage = p.StatusMessage;
                    _statusLog.AppendLine($"[{DateTime.Now:HH:mm:ss}] {p.StatusMessage}");
                    StatusLogText = _statusLog.ToString();
                }
            });

            await downloadFunc(progress);
            _statusLog.AppendLine($"[{DateTime.Now:HH:mm:ss}] {exchangeName} 下载完成");
            StatusLogText = _statusLog.ToString();
        }
        catch (Exception ex)
        {
            var detail = $"{exchangeName} 错误: {ex.Message}";
            var inner = ex.InnerException;
            while (inner != null)
            {
                detail += $"\n  内部: {inner.Message}";
                inner = inner.InnerException;
            }
            StatusMessage = detail;
            _statusLog.AppendLine($"[{DateTime.Now:HH:mm:ss}] {detail}");
            StatusLogText = _statusLog.ToString();
        }
        finally
        {
            onComplete();
        }
    }
}
