using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoTraceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddInteractiveFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "EventRegistrations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCheckedIn",
                table: "EventRegistrations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "IsCheckedIn",
                table: "EventRegistrations");
        }
    }
}
