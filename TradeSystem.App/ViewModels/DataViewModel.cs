using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using TradeSystem.App.Models;

namespace TradeSystem.App.ViewModels;

public partial class DataViewModel : ViewModel
{
    private bool _isInitialized;

    [ObservableProperty]
    private List<DataColor> _colors = [];

    public override void OnNavigatedTo()
    {
        if (!_isInitialized)
        {
            InitializeViewModel();
        }
    }

    private void InitializeViewModel()
    {
        var random = new Random();
        Colors.Clear();

        for (var i = 0; i < 8192; i++)
        {
            Colors.Add(
                new DataColor
                {
                    Color = new SolidColorBrush(
                        Color.FromArgb(
                            200,
                            (byte)random.Next(0, 250),
                            (byte)random.Next(0, 250),
                            (byte)random.Next(0, 250)
                        )
                    ),
                }
            );
        }

        _isInitialized = true;
    }
}
