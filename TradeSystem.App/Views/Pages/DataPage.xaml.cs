using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

/// <summary>
/// Interaction logic for DataPage.xaml
/// </summary>
public partial class DataPage : INavigableView<ViewModels.DataViewModel>
{
    public ViewModels.DataViewModel ViewModel { get; }

    public DataPage(ViewModels.DataViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
