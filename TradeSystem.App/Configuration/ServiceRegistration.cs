using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using TradeSystem.Core.Abstractions;
using TradeSystem.App.ViewModels;
using TradeSystem.App.Views;
using TradeSystem.App.Views.Pages;
using TradeSystem.Infrastructure.Data;
using TradeSystem.Infrastructure.Services;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace TradeSystem.App.Configuration
{
    public static class ServiceRegistration
    {
        public static IHost CreateHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseMySql(
                            connectionString,
                            ServerVersion.AutoDetect(connectionString),
                            b => b.MigrationsAssembly("TradeSystem.Infrastructure")
                        );
                    });
                    services.AddDbContextFactory<AppDbContext>(options =>
                    {
                        options.UseMySql(
                            connectionString,
                            ServerVersion.AutoDetect(connectionString),
                            b => b.MigrationsAssembly("TradeSystem.Infrastructure")
                        );
                    });

                    services.AddNavigationViewPageProvider();

                    services.AddSingleton<INavigationMenuItemService, NavigationMenuItemService>();

                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<INavigationWindow, MainWindow>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainWindowViewModel>();

                    services.AddSingleton<DashboardPage>();
                    services.AddSingleton<DashboardViewModel>();
                    services.AddSingleton<DataPage>();
                    services.AddSingleton<DataViewModel>();
                    services.AddSingleton<SettingsPage>();
                    services.AddSingleton<SettingsViewModel>();
                })
                .Build();
        }
    }
}
