using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

public partial class KLineRankingPage : INavigableView<ViewModels.KLineRankingViewModel>
{
    public ViewModels.KLineRankingViewModel ViewModel { get; }

    public KLineRankingPage(ViewModels.KLineRankingViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
