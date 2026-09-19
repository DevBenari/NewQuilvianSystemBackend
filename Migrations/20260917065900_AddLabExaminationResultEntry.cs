using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabExaminationResultEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExaminedAt",
                schema: "public",
                table: "LabExamination",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResultEnteredAt",
                schema: "public",
                table: "LabExamination",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResultEnteredByUserId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ResultNumeric",
                schema: "public",
                table: "LabExamination",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResultOptionId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultUnitSnapshot",
                schema: "public",
                table: "LabExamination",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResultValueBoundId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ExaminedAt",
                schema: "public",
                table: "LabExamination",
                column: "ExaminedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ResultOptionId",
                schema: "public",
                table: "LabExamination",
                column: "ResultOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ResultValueBoundId",
                schema: "public",
                table: "LabExamination",
                column: "ResultValueBoundId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabExamination_LabValueBound_ResultValueBoundId",
                schema: "public",
                table: "LabExamination",
                column: "ResultValueBoundId",
                principalSchema: "public",
                principalTable: "LabValueBound",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabExamination_LabValueOption_ResultOptionId",
                schema: "public",
                table: "LabExamination",
                column: "ResultOptionId",
                principalSchema: "public",
                principalTable: "LabValueOption",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabExamination_LabValueBound_ResultValueBoundId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropForeignKey(
                name: "FK_LabExamination_LabValueOption_ResultOptionId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropIndex(
                name: "IX_LabExamination_ExaminedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropIndex(
                name: "IX_LabExamination_ResultOptionId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropIndex(
                name: "IX_LabExamination_ResultValueBoundId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ExaminedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ResultEnteredAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ResultEnteredByUserId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ResultNumeric",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ResultOptionId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ResultUnitSnapshot",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ResultValueBoundId",
                schema: "public",
                table: "LabExamination");
        }
    }
}
