using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingIndustry.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditVesselsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Vessels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Vessels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TypeVessel",
                table: "Vessels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "TypeVessel",
                table: "Vessels");
        }
    }
}
