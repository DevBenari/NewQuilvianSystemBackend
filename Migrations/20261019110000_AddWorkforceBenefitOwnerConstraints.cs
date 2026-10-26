using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Pelengkap AddWorkforceBenefitOwnerIdentity (yang sudah diterapkan dan TIDAK diubah): FK
    /// MstPatientCompanyGuarantor.BenefitOwnerWorkforceProfileId -> MstWorkforceProfile (Restrict, mengikuti FK EmployeeId),
    /// index terfilter pada kolom itu, dan CHECK BenefitOwnerType IN ('EMPLOYEE','DOCTOR') yang hanya sah bersama
    /// WorkforceProfileId. Snapshot kunjungan, handoff AR, dan piutang sengaja tanpa FK (rekam historis, sama seperti
    /// EmployeeIdSnapshot/BenefitOwnerId). Kolom sumbernya baru dan masih kosong untuk baris lama, jadi tidak ada data lama
    /// yang dapat ditolak. Down dijaga agar rollback tidak menghilangkan identitas benefit dokter tetap.
    /// </summary>
    /// <inheritdoc />
    public partial class AddWorkforceBenefitOwnerConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "BenefitOwnerWorkforceProfileId",
                filter: "\"BenefitOwnerWorkforceProfileId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstPatientCompanyGuarantor_BenefitOwnerType",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                sql: "(\"BenefitOwnerWorkforceProfileId\" IS NULL AND \"BenefitOwnerType\" IS NULL) OR (\"BenefitOwnerWorkforceProfileId\" IS NOT NULL AND \"BenefitOwnerType\" IN ('EMPLOYEE','DOCTOR'))");

            migrationBuilder.AddForeignKey(
                name: "FK_MstPatientCompanyGuarantor_BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "BenefitOwnerWorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Baris dokter hanya membawa kolom Workforce (tanpa EmployeeId/BenefitOwnerId). Rollback AddWorkforceBenefitOwnerIdentity
            // sesudah ini akan menghapus kolom tersebut dan menghilangkan identitas benefit dokter, sedangkan constraint lama
            // menolak baris itu. Karena Down migration ini selalu berjalan lebih dulu, penjaga di sini menghentikan rollback bila
            // masih ada baris yang identitasnya HANYA Workforce. Baris karyawan hasil backfill aman: kolom legacy tidak pernah diubah.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public."MstPatientCompanyGuarantor" WHERE "BenefitOwnerWorkforceProfileId" IS NOT NULL AND "EmployeeId" IS NULL)
                       OR EXISTS (SELECT 1 FROM public."RegPatientEncounterGuarantor" WHERE "WorkforceProfileIdSnapshot" IS NOT NULL AND "EmployeeIdSnapshot" IS NULL)
                       OR EXISTS (SELECT 1 FROM public."BilArHandoff" WHERE "BenefitOwnerWorkforceProfileId" IS NOT NULL AND "BenefitOwnerId" IS NULL)
                       OR EXISTS (SELECT 1 FROM public."FinReceivable" WHERE "BenefitOwnerWorkforceProfileId" IS NOT NULL AND "BenefitOwnerId" IS NULL)
                    THEN
                        RAISE EXCEPTION 'Rollback dibatalkan: masih ada kartu/snapshot/handoff/piutang benefit yang hanya beridentitas WorkforceProfileId (dokter tetap). Selesaikan atau pindahkan data tersebut sebelum rollback.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_MstPatientCompanyGuarantor_BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstPatientCompanyGuarantor_BenefitOwnerType",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientCompanyGuarantor_BenefitOwnerWorkforceProfileId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");
        }
    }
}
