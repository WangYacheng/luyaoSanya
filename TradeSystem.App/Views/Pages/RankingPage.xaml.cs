using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

public partial class RankingPage : INavigableView<ViewModels.RankingViewModel>
{
    public ViewModels.RankingViewModel ViewModel { get; }

    public RankingPage(ViewModels.RankingViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
