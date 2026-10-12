using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdmissionTransferOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AdmissionTransferId",
                schema: "public",
                table: "RegPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionTransferSourceType",
                schema: "public",
                table: "RegPatientEncounter",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegPatientEncounter_AdmissionTransferSourceType_AdmissionTr~",
                schema: "public",
                table: "RegPatientEncounter",
                columns: new[] { "AdmissionTransferSourceType", "AdmissionTransferId" },
                unique: true,
                filter: "\"AdmissionTransferId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegPatientEncounter_SourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter",
                column: "SourceEncounterId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegPatientEncounter_RegPatientEncounter_SourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter",
                column: "SourceEncounterId",
                principalSchema: "public",
                principalTable: "RegPatientEncounter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegPatientEncounter_RegPatientEncounter_SourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_RegPatientEncounter_AdmissionTransferSourceType_AdmissionTr~",
                schema: "public",
                table: "RegPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_RegPatientEncounter_SourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AdmissionTransferId",
                schema: "public",
                table: "RegPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AdmissionTransferSourceType",
                schema: "public",
                table: "RegPatientEncounter");

            migrationBuilder.DropColumn(
                name: "SourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter");
        }
    }
}
