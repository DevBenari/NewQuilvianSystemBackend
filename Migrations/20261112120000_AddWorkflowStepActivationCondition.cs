using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-PBJ-001 (PBJ-DEC-019, PBJ-DEC-030): satu kolom nullable pada MstWorkflowStep
    /// milik mesin workflow bersama (Wfl). Langkah yang sudah ada bernilai NULL sehingga
    /// tetap selalu dibuat; hanya langkah baru yang diberi kode yang menjadi bersyarat.
    /// Tidak ada pengisian data, tidak ada perubahan index. Aman dijalankan tanpa downtime.
    /// Urutan rilis: database dulu, lalu aplikasi — model yang memuat kolom ini tidak boleh
    /// aktif sebelum kolomnya ada, karena seluruh query langkah workflow (termasuk HR) ikut
    /// memilih kolom tersebut.
    /// </summary>
    public partial class AddWorkflowStepActivationCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivationConditionCode",
                schema: "public",
                table: "MstWorkflowStep",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivationConditionCode",
                schema: "public",
                table: "MstWorkflowStep");
        }
    }
}
