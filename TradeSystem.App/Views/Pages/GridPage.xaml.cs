
using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

/// <summary>
/// Interaction logic for GridPage.xaml
/// </summary>
public partial class GridPage : INavigableView<ViewModels.GridViewModel>
{
    public ViewModels.GridViewModel ViewModel { get; }

    public GridPage(ViewModels.GridViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
