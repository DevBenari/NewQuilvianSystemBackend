using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabExaminationIdToLabTransitionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LabExaminationId",
                schema: "public",
                table: "LabTransitionHistory",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabTransitionHistory_LabExaminationId",
                schema: "public",
                table: "LabTransitionHistory",
                column: "LabExaminationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabTransitionHistory_LabExamination_LabExaminationId",
                schema: "public",
                table: "LabTransitionHistory",
                column: "LabExaminationId",
                principalSchema: "public",
                principalTable: "LabExamination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabTransitionHistory_LabExamination_LabExaminationId",
                schema: "public",
                table: "LabTransitionHistory");

            migrationBuilder.DropIndex(
                name: "IX_LabTransitionHistory_LabExaminationId",
                schema: "public",
                table: "LabTransitionHistory");

            migrationBuilder.DropColumn(
                name: "LabExaminationId",
                schema: "public",
                table: "LabTransitionHistory");
        }
    }
}
