using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionInpatientContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxPrescription",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPrescription",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescriptionOrderType",
                schema: "public",
                table: "TrxPrescription",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_IdempotencyKey",
                schema: "public",
                table: "TrxPrescription",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_InpEpisodeId",
                schema: "public",
                table: "TrxPrescription",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PrescriptionOrderType",
                schema: "public",
                table: "TrxPrescription",
                column: "PrescriptionOrderType");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPrescription_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPrescription",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPrescription_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescription_IdempotencyKey",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescription_InpEpisodeId",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescription_PrescriptionOrderType",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropColumn(
                name: "PrescriptionOrderType",
                schema: "public",
                table: "TrxPrescription");
        }
    }
}
