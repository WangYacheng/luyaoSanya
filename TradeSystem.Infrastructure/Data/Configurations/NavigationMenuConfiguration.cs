using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeSystem.Core.Models;

namespace TradeSystem.Infrastructure.Data.Configurations
{
    public class NavigationMenuConfiguration : IEntityTypeConfiguration<NavigationMenuItem>
    {
        public void Configure(EntityTypeBuilder<NavigationMenuItem> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Title).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Icon).HasMaxLength(50);
            builder.Property(e => e.TargetPageTag).HasMaxLength(100);
            builder.Property(e => e.IsGroup).HasDefaultValue(false);

            builder.HasOne<NavigationMenuItem>()
                .WithMany()
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => new { e.ParentId, e.Order });
        }
    }
}
