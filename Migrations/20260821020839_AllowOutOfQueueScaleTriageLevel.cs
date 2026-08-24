using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Melonggarkan batas Level pada master level triase dari 1-5 menjadi 0-5, supaya
    /// kategori Hitam yang berada "di luar skala antrean" punya nilai yang sah. Lima nilai
    /// skala antrean tidak bergeser, sehingga data lama tidak berubah arti.
    ///
    /// Catatan rollback: <c>Down</c> mengembalikan batas menjadi 1-5 dan karena itu akan
    /// gagal bila baris Level 0 sudah ada. Hapus baris kategori Hitam lebih dulu sebelum
    /// memundurkan migrasi ini.
    /// </summary>
    public partial class AllowOutOfQueueScaleTriageLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MstEmergencyTriageLevel_Level",
                schema: "public",
                table: "MstEmergencyTriageLevel");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstEmergencyTriageLevel_Level",
                schema: "public",
                table: "MstEmergencyTriageLevel",
                sql: "\"Level\" >= 0 AND \"Level\" <= 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MstEmergencyTriageLevel_Level",
                schema: "public",
                table: "MstEmergencyTriageLevel");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstEmergencyTriageLevel_Level",
                schema: "public",
                table: "MstEmergencyTriageLevel",
                sql: "\"Level\" >= 1 AND \"Level\" <= 5");
        }
    }
}
