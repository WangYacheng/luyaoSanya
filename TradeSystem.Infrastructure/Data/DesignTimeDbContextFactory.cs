using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TradeSystem.Infrastructure.Data
{
    /// <summary>
    /// 专门为 EF Core 命令行工具提供的工厂类
    /// 解决 "Unable to create an instance of 'AppDbContext'" 报错
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {

            // 0. 获取当前执行路径
            string basePath = Directory.GetCurrentDirectory();
            if (!basePath.EndsWith("TradeSystem.App") && Directory.Exists(Path.Combine(basePath, "TradeSystem.App")))
            {
                basePath = Path.Combine(basePath, "TradeSystem.App");
            }

            // 1. 手动读取 appsettings.json (注意路径：它在 App 项目里)
            // 如果你在命令行根目录运行，需要确保能找到这个文件
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            

            Console.WriteLine($"[EF DesignTime] Loading configuration from: {basePath}");

            // 2. 配置与 ServiceRegistration 中一致的参数
            builder.UseMySql(
                connectionString, 
                ServerVersion.AutoDetect(connectionString),
                b => b.MigrationsAssembly("TradeSystem.Infrastructure")
            );

            return new AppDbContext(builder.Options);
        }
    }
}