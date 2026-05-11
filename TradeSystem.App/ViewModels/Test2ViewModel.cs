
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TradeSystem.App.ViewModels;

public partial class Test2ViewModel : ViewModel
{
    [ObservableProperty]
    private string _sampleText = "Hello, World!";

    [RelayCommand]
    private void OnSampleAction()
    {
        SampleText = "Button clicked!";
    }
}