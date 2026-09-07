using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralPointerToPatientEncounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReferralDoctorId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferralInstitutionId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_ReferralDoctorId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "ReferralDoctorId",
                filter: "\"ReferralDoctorId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_ReferralInstitutionId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "ReferralInstitutionId",
                filter: "\"ReferralInstitutionId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstReferralDoctor_ReferralDoctorId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "ReferralDoctorId",
                principalSchema: "public",
                principalTable: "MstReferralDoctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstReferralInstitution_ReferralInstitut~",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "ReferralInstitutionId",
                principalSchema: "public",
                principalTable: "MstReferralInstitution",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstReferralDoctor_ReferralDoctorId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstReferralInstitution_ReferralInstitut~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_ReferralDoctorId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_ReferralInstitutionId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "ReferralDoctorId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "ReferralInstitutionId",
                schema: "public",
                table: "TrxPatientEncounter");
        }
    }
}
