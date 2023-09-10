using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTimetrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsPast",
                table: "Task",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleDate",
                table: "Task",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeElapsed",
                table: "Task",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeEstimate",
                table: "Task",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeTrack",
                table: "Task",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_ReceiverId",
                table: "Notification",
                column: "ReceiverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "IsPast",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "ScheduleDate",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "TimeElapsed",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "TimeEstimate",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "TimeTrack",
                table: "Task");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_ReceiverId",
                table: "Notification",
                column: "ReceiverId",
                unique: true);
        }
    }
}
