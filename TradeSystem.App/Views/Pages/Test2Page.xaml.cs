
using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

/// <summary>
/// Interaction logic for Test2Page.xaml
/// </summary>
public partial class Test2Page : INavigableView<ViewModels.Test2ViewModel>
{
    public ViewModels.Test2ViewModel ViewModel { get; }

    public Test2Page(ViewModels.Test2ViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}