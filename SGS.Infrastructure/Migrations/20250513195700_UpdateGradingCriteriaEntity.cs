using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGradingCriteriaEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradingCriteria_Users_UserId",
                table: "GradingCriteria");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GradingCriteria");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "GradingCriteria",
                newName: "AssignmentId");

            migrationBuilder.RenameColumn(
                name: "CriteriaDescription",
                table: "GradingCriteria",
                newName: "Title");

            migrationBuilder.RenameIndex(
                name: "IX_GradingCriteria_UserId",
                table: "GradingCriteria",
                newName: "IX_GradingCriteria_AssignmentId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GradingCriteria",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "GradingCriteria",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Documents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_GradingCriteria_Assignments_AssignmentId",
                table: "GradingCriteria",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradingCriteria_Assignments_AssignmentId",
                table: "GradingCriteria");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "GradingCriteria");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "GradingCriteria");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "GradingCriteria",
                newName: "CriteriaDescription");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "GradingCriteria",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GradingCriteria_AssignmentId",
                table: "GradingCriteria",
                newName: "IX_GradingCriteria_UserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GradingCriteria",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_GradingCriteria_Users_UserId",
                table: "GradingCriteria",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
