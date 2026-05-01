using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeSystem.Core.Models;

namespace TradeSystem.Infrastructure.Data.Configurations
{
    /// <summary>
    /// K线数据的实体配置类：定义了 KLineData 实体与数据库表之间的映射关系
    /// </summary>
    public class KLineConfiguration : IEntityTypeConfiguration<KLineData>
    {
        public void Configure(EntityTypeBuilder<KLineData> builder)
        {
            // 设置主键
            builder.HasKey(e => e.Id);

            // 配置复合索引
            builder.HasIndex(e => new { e.Exchange, e.Symbol, e.OpenTime, e.TimeFrame })
                  .HasDatabaseName("IX_KLine_Query");

            // 字段约束
            builder.Property(e => e.Exchange).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Symbol).IsRequired().HasMaxLength(20);

            // 精度配置
            builder.Property(e => e.Open).HasPrecision(18, 8);
            builder.Property(e => e.High).HasPrecision(18, 8);
            builder.Property(e => e.Low).HasPrecision(18, 8);
            builder.Property(e => e.Close).HasPrecision(18, 8);
            builder.Property(e => e.Volume).HasPrecision(18, 8);
            builder.Property(e => e.QuoteVolume).HasPrecision(18, 8);
        }
    }
}