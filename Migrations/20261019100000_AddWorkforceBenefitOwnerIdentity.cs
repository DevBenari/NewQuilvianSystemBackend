using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Benefit kesehatan berbasis MstWorkforceProfile.Id (karyawan dan dokter tetap). Aditif dan nullable:
    /// MstPatientCompanyGuarantor.BenefitOwnerWorkforceProfileId/BenefitOwnerType,
    /// RegPatientEncounterGuarantor.WorkforceProfileIdSnapshot, BilArHandoff.BenefitOwnerWorkforceProfileId, dan
    /// FinReceivable.BenefitOwnerWorkforceProfileId. EmployeeId/BenefitOwnerId lama tidak diubah artinya.
    /// Tiga check constraint pemilik diperluas agar baris dokter (tanpa EmployeeId) sah; baris lama tetap memenuhinya.
    /// Backfill data lama TIDAK ada di sini; skrip terpisah menunggu persetujuan.
    /// </summary>
    /// <inheritdoc />
    public partial class AddWorkforceBenefitOwnerIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_BenefitOwner",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinReceivable_BenefitOwner",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstPatientCompanyGuarantor_BenefitLink",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.AddColumn<string>(
                name: "BenefitOwnerType",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceProfileIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "BilArHandoff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "FinReceivable",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegPatientEncounterGuarantor_WorkforceProfileIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                column: "WorkforceProfileIdSnapshot",
                filter: "\"WorkforceProfileIdSnapshot\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilArHandoff_BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "BilArHandoff",
                column: "BenefitOwnerWorkforceProfileId",
                filter: "\"BenefitOwnerWorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivable_BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "FinReceivable",
                column: "BenefitOwnerWorkforceProfileId",
                filter: "\"BenefitOwnerWorkforceProfileId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_BenefitOwner",
                schema: "public",
                table: "BilArHandoff",
                sql: "(\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND (\"BenefitOwnerId\" IS NOT NULL OR \"BenefitOwnerWorkforceProfileId\" IS NOT NULL)) OR (\"DebtorType\" <> 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NULL AND \"BenefitOwnerWorkforceProfileId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinReceivable_BenefitOwner",
                schema: "public",
                table: "FinReceivable",
                sql: "(\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND (\"BenefitOwnerId\" IS NOT NULL OR \"BenefitOwnerWorkforceProfileId\" IS NOT NULL)) OR (\"DebtorType\" <> 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NULL AND \"BenefitOwnerWorkforceProfileId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstPatientCompanyGuarantor_BenefitLink",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                sql: "(\"EmployeeId\" IS NULL AND \"FamilyMemberId\" IS NULL AND \"BenefitRelationship\" IS NULL AND \"BenefitOwnerWorkforceProfileId\" IS NULL AND \"BenefitOwnerType\" IS NULL) OR ((\"EmployeeId\" IS NOT NULL OR \"BenefitOwnerWorkforceProfileId\" IS NOT NULL) AND \"BenefitRelationship\" IS NOT NULL AND ((\"BenefitRelationship\" = 'SELF') = (\"FamilyMemberId\" IS NULL)))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down menghapus kolom identitas Workforce; data dokter (yang hanya membawa kolom ini) ikut hilang, dan
            // constraint lama menolak baris tanpa EmployeeId. Jalankan hanya bila belum ada kartu/piutang dokter.

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_BenefitOwner",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinReceivable_BenefitOwner",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstPatientCompanyGuarantor_BenefitLink",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_RegPatientEncounterGuarantor_WorkforceProfileIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_BilArHandoff_BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivable_BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropColumn(
                name: "BenefitOwnerType",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropColumn(
                name: "BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropColumn(
                name: "WorkforceProfileIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropColumn(
                name: "BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_BenefitOwner",
                schema: "public",
                table: "BilArHandoff",
                sql: "(\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NOT NULL) OR (\"DebtorType\" <> 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinReceivable_BenefitOwner",
                schema: "public",
                table: "FinReceivable",
                sql: "(\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NOT NULL) OR (\"DebtorType\" <> 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstPatientCompanyGuarantor_BenefitLink",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                sql: "(\"EmployeeId\" IS NULL AND \"FamilyMemberId\" IS NULL AND \"BenefitRelationship\" IS NULL) OR (\"EmployeeId\" IS NOT NULL AND \"BenefitRelationship\" IS NOT NULL AND ((\"BenefitRelationship\" = 'SELF') = (\"FamilyMemberId\" IS NULL)))");
        }
    }
}
