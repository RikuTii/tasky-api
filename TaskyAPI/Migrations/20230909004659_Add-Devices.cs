using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "TimeEstimate",
                table: "Task",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TimeElapsed",
                table: "Task",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);



            migrationBuilder.CreateTable(
                name: "UserDevice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    LastActive = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AuthToken = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FcmToken = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevice_UserAccount_AccountId",
                        column: x => x.AccountId,
                        principalTable: "UserAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDevice_UserAccount_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_AccountId",
                table: "UserDevice",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_UserAccountId",
                table: "UserDevice",
                column: "UserAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDevice");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "File");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeEstimate",
                table: "Task",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeElapsed",
                table: "Task",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
