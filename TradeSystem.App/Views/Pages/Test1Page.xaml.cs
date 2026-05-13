using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

/// <summary>
/// Interaction logic for Test1Page.xaml
/// </summary>
public partial class Test1Page : INavigableView<ViewModels.Test1ViewModel>
{
    public ViewModels.Test1ViewModel ViewModel { get; }

    public Test1Page(ViewModels.Test1ViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
