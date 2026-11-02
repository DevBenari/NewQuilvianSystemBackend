
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddInpatientDoctorMedicalAssessmentOperationalColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: seluruh 47 kolom sudah ditambahkan oleh
            // 20260930082632_UpdatePendingModelChanges.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: kolom dimiliki oleh migration sebelumnya.
            // Jangan menghapus kolom atau data klinis pada rollback.
        }
    }
}
