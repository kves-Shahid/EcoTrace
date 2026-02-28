using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoTraceApp.Migrations
{
    /// <inheritdoc />
    public partial class Phase2ImpactUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImpactSummary",
                table: "Events",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Events",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImpactSummary",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Events");
        }
    }
}
