using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoTraceApp.Migrations
{
    /// <inheritdoc />
    public partial class FinalVerificationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTasks_AspNetUsers_AssignedUserId",
                table: "EventTasks");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "varchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EventTasks",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "EventTasks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "EventTasks",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPendingVerification",
                table: "EventTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "EventTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProofImagePath",
                table: "EventTasks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_EventTasks_AspNetUsers_AssignedUserId",
                table: "EventTasks",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTasks_AspNetUsers_AssignedUserId",
                table: "EventTasks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "EventTasks");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "EventTasks");

            migrationBuilder.DropColumn(
                name: "IsPendingVerification",
                table: "EventTasks");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "EventTasks");

            migrationBuilder.DropColumn(
                name: "ProofImagePath",
                table: "EventTasks");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EventTasks",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_EventTasks_AspNetUsers_AssignedUserId",
                table: "EventTasks",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
