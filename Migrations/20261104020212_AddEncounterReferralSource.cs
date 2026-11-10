using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEncounterReferralSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReferralSourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegPatientEncounter_ReferralSourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter",
                column: "ReferralSourceEncounterId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegPatientEncounter_RegPatientEncounter_ReferralSourceEncou~",
                schema: "public",
                table: "RegPatientEncounter",
                column: "ReferralSourceEncounterId",
                principalSchema: "public",
                principalTable: "RegPatientEncounter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegPatientEncounter_RegPatientEncounter_ReferralSourceEncou~",
                schema: "public",
                table: "RegPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_RegPatientEncounter_ReferralSourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter");

            migrationBuilder.DropColumn(
                name: "ReferralSourceEncounterId",
                schema: "public",
                table: "RegPatientEncounter");
        }
    }
}
