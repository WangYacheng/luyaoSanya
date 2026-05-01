using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeSystem.App.Configuration;

namespace TradeSystem.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

        public App()
        {
            // 调用我们刚才写的静态方法创建 Host
            _host = ServiceRegistration.CreateHost(System.Array.Empty<string>());
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // 启动后台服务
            await _host.StartAsync();

            // 从容器中获取 MainWindow 的实例
            // 此时 EF Core 和 KLineService 会自动被注入到 MainWindow 中
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // 优雅退出，确保数据库连接等资源被正常释放
            using (_host)
            {
                await _host.StopAsync();
            }
            base.OnExit(e);
        }
}

