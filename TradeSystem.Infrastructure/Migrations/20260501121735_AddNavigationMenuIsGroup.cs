using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationMenuIsGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGroup",
                table: "NavigationMenus",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGroup",
                table: "NavigationMenus");
        }
    }
}
