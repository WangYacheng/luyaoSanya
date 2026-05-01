using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
// 引用你的项目空间
using TradeSystem.Core.Abstractions;
using TradeSystem.Infrastructure.Data;

namespace TradeSystem.App.Configuration
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// 创建并配置主机 (Host)
        /// </summary>
        public static IHost CreateHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // 1. 配置 appsettings.json 读取路径
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // 获取配置对象
                    var configuration = hostContext.Configuration;
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    // 2. 注册数据库上下文 (EF Core)
                    // 建议使用 MySqlConnector 驱动
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseMySql(
                            connectionString, 
                            ServerVersion.AutoDetect(connectionString),
                            // 这里可以指定迁移文件所在的程序集，如果你把 Migrations 放在 Infrastructure 里
                            b => b.MigrationsAssembly("TradeSystem.Infrastructure")
                        );
                    });

                    // 3. 注册业务逻辑服务 (Core 接口 -> Infrastructure 实现)
                    //services.AddScoped<IKLineService, KLineService>();

                    // 4. 注册 WPF 窗口与 ViewModels
                    // 只有注册了，窗口才能通过构造函数注入服务
                    services.AddSingleton<MainWindow>();
                    // services.AddSingleton<MainWindowViewModel>(); 
                })
                .Build();
        }
    }
}