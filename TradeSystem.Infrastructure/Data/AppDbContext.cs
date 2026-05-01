using Microsoft.EntityFrameworkCore;
using TradeSystem.Core.Models;

namespace TradeSystem.Infrastructure.Data
{
    /// <summary>
    /// 数据库上下文：负责与数据库的实际通信
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 对应数据库中的 KLineDatas 表
        public DbSet<KLineData> KLines { get; set; }

        // 对应数据库中的 NavigationMenus 表
        public DbSet<NavigationMenuItem> NavigationMenus { get; set; }

        /// <summary>
        /// 优雅地配置映射关系
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 核心改进：自动扫描并应用当前程序集中所有实现了 IEntityTypeConfiguration 的类
            // 这样你以后增加 100 张表，这里也只需要这一行代码，非常整洁
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}