using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menutup dua kesenjangan model yang tersisa setelah rangkaian migrasi Finance
    /// (AddFinanceCollection, AddFinanceSupplierPayable, AddFinancePayment,
    /// AddFinanceMedicalServicePayable) diterapkan:
    /// 1. FinPayableAdjustment.MedicalServicePayableId belum punya index pendukung FK-nya
    ///    (FK-nya sendiri sudah dibuat di AddFinanceMedicalServicePayable).
    /// 2. LabOrder.ExaminerDoctorId sudah tidak lagi dikonfigurasi sebagai relasi ke
    ///    MstDoctor pada model saat ini, sehingga FK dan index lamanya dilepas.
    /// Catatan: migrasi ini sebelumnya bernama "ProbeSync" dan hasil scaffold-nya berisi
    /// CreateTable/CreateIndex duplikat untuk seluruh tabel Fin* yang SUDAH dibuat oleh
    /// migrasi-migrasi Finance di atas. Duplikasi itu muncul karena tiga migrasi
    /// (AddFinanceCashManagement, AddFinancePayment, AddFinanceMedicalServicePayable)
    /// mempunyai Designer.cs/snapshot yang tidak lengkap, sehingga tooling EF mengira
    /// tabel-tabel itu belum ada. Operasi duplikat tersebut sudah dihapus dari sini agar
    /// migrasi ini tidak gagal dengan error "relation already exists" saat dijalankan.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinPayableAdjustmentIndexAndDropLabOrderExaminerFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabOrder_MstDoctor_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayableAdjustment_MedicalServicePayableId",
                schema: "public",
                table: "FinPayableAdjustment",
                column: "MedicalServicePayableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinPayableAdjustment_MedicalServicePayableId",
                schema: "public",
                table: "FinPayableAdjustment");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder",
                column: "ExaminerDoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrder_MstDoctor_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder",
                column: "ExaminerDoctorId",
                principalSchema: "public",
                principalTable: "MstDoctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
