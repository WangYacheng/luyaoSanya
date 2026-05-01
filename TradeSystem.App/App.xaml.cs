using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeSystem.App.Configuration;
using TradeSystem.App.Views.Pages;
using Wpf.Ui;

namespace TradeSystem.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = ServiceRegistration.CreateHost(System.Array.Empty<string>());
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await _host.StartAsync();

        var navigationWindow = _host.Services.GetRequiredService<INavigationWindow>();
        navigationWindow.ShowWindow();
        navigationWindow.Navigate(typeof(DashboardPage));
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}
