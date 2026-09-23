using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Mengembalikan FK_LabOrder_MstDoctor_ExaminerDoctorId dan index pendukungnya yang
    /// terlanjur dilepas oleh migrasi AddFinPayableAdjustmentIndexAndDropLabOrderExaminerFk.
    /// Migrasi itu keliru mengira LabOrder.ExaminerDoctorId sudah tidak dikonfigurasi
    /// sebagai relasi; nyatanya LabOrderConfiguration (lihat komentar di sana) masih
    /// mendefinisikannya sebagai FK Restrict ke MstDoctor. Migrasi ini menyamakan kembali
    /// database dengan model saat ini.
    /// </summary>
    /// <inheritdoc />
    public partial class RestoreLabOrderExaminerDoctorFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabOrder_MstDoctor_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder");
        }
    }
}
