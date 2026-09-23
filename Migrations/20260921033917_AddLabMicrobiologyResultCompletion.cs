using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabMicrobiologyResultCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsultedAt",
                schema: "public",
                table: "LabExamination",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConsultedByUserId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsultedToName",
                schema: "public",
                table: "LabExamination",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CultureType",
                schema: "public",
                table: "LabExamination",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAt",
                schema: "public",
                table: "LabExamination",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinalizedByUserId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MicrobiologyFinding",
                schema: "public",
                table: "LabExamination",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReopenCount",
                schema: "public",
                table: "LabExamination",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResultQualifier",
                schema: "public",
                table: "LabExamination",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SusceptibilityMethod",
                schema: "public",
                table: "LabExamination",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_FinalizedAt",
                schema: "public",
                table: "LabExamination",
                column: "FinalizedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabExamination_FinalizedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ConsultedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ConsultedByUserId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ConsultedToName",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "CultureType",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "FinalizedByUserId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "MicrobiologyFinding",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ReopenCount",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ResultQualifier",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "SusceptibilityMethod",
                schema: "public",
                table: "LabExamination");
        }
    }
}
