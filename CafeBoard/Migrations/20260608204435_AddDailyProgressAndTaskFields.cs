using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBoard.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyProgressAndTaskFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                table: "CafeTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "CafeTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DailyProgresses",
                columns: table => new
                {
                    ProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    DeveloperId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoursWorked = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyProgresses", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK_DailyProgresses_CafeTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "CafeTasks",
                        principalColumn: "TaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyProgresses_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "DeveloperId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyProgresses_DeveloperId",
                table: "DailyProgresses",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyProgresses_TaskId",
                table: "DailyProgresses",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyProgresses");

            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                table: "CafeTasks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "CafeTasks");
        }
    }
}
