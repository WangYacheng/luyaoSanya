using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeSystem.Core.Models;

namespace TradeSystem.Infrastructure.Data.Configurations
{
    /// <summary>
    /// 导航菜单的实体配置类：定义了 NavigationMenuItem 实体与数据库表之间的映射关系
    /// </summary>
    public class NavigationMenuConfiguration : IEntityTypeConfiguration<NavigationMenuItem>
    {
        public void Configure(EntityTypeBuilder<NavigationMenuItem> builder)
        {
            // 1. 基本约束
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Title).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Icon).HasMaxLength(50);
            builder.Property(e => e.TargetPageTag).HasMaxLength(100);

            // 2. 配置多层级自关联逻辑 (核心点)
            // 显式指定 ParentId 作为外键，关联到 Id 属性
            builder.HasOne<NavigationMenuItem>() 
                   .WithMany() 
                   .HasForeignKey(e => e.ParentId)
                   .OnDelete(DeleteBehavior.Restrict); // 防止删除父级时导致数据库级联冲突

            // 3. 排序与可见性索引
            builder.HasIndex(e => new { e.ParentId, e.Order });
        }
    }
}