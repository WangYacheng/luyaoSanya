
using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

/// <summary>
/// Interaction logic for Test3Page.xaml
/// </summary>
public partial class Test3Page : INavigableView<ViewModels.Test3ViewModel>
{
    public ViewModels.Test3ViewModel ViewModel { get; }

    public Test3Page(ViewModels.Test3ViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}