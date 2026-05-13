using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKLineDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "KLines",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "KLines");
        }
    }
}
