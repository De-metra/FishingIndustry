using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingIndustry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFishHabitatAndFishingZoneRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Family",
                table: "FishTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HabitatType",
                table: "FishTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "FishingZones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "FishingZones",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "FishingZones",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "FishTypeFishingZone",
                columns: table => new
                {
                    FishTypesId = table.Column<int>(type: "int", nullable: false),
                    FishingZonesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishTypeFishingZone", x => new { x.FishTypesId, x.FishingZonesId });
                    table.ForeignKey(
                        name: "FK_FishTypeFishingZone_FishTypes_FishTypesId",
                        column: x => x.FishTypesId,
                        principalTable: "FishTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FishTypeFishingZone_FishingZones_FishingZonesId",
                        column: x => x.FishingZonesId,
                        principalTable: "FishingZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FishTypeFishingZone_FishingZonesId",
                table: "FishTypeFishingZone",
                column: "FishingZonesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FishTypeFishingZone");

            migrationBuilder.DropColumn(
                name: "Family",
                table: "FishTypes");

            migrationBuilder.DropColumn(
                name: "HabitatType",
                table: "FishTypes");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "FishingZones");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "FishingZones");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "FishingZones");
        }
    }
}
