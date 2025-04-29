using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingIndustry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToFish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "FishTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "FishTypes");
        }
    }
}
