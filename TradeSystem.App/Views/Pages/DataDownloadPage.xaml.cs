using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

/// <summary>
/// Interaction logic for DataDownloadPage.xaml
/// </summary>
public partial class DataDownloadPage : INavigableView<ViewModels.DataDownloadViewModel>
{
    public ViewModels.DataDownloadViewModel ViewModel { get; }

    public DataDownloadPage(ViewModels.DataDownloadViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
