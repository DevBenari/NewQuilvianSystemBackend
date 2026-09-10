using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-041 / RWI-DEC-096. Ambang tindak lanjut kekurangan uang muka dapat diubah admin:
    /// satu kolom ditambahkan pada baris pengaturan Rawat Inap, bernilai bawaan 3 hari.
    /// </summary>
    /// <remarks>
    /// Nol baris lama disentuh nilainya. Kolomnya lahir dengan defaultValue 3 di sisi database,
    /// sehingga baris pengaturan yang sudah ada langsung terbaca 3 hari - bukan 0. Tanpa nilai
    /// bawaan itu, pengingat kekurangan deposit akan menyala setiap hari tanpa ada yang memintanya.
    ///
    /// LANGKAH MUNDURNYA SIMETRIS. Down() hanya menghapus kolom yang ditambahkan Up(), dan tidak
    /// ada tabel lain, index, maupun constraint yang ikut. Satu-satunya yang hilang saat mundur
    /// adalah angka ambang yang sempat disetel admin; modul kembali memakai bawaan 3 hari lewat
    /// InpatientSettingValues.Defaults, dan nol data klinis maupun keuangan terpengaruh.
    /// </remarks>
    public partial class AddDepositFollowUpIntervalToInpatientSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepositFollowUpIntervalDays",
                schema: "public",
                table: "MstInpatientSetting",
                type: "integer",
                nullable: false,
                defaultValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositFollowUpIntervalDays",
                schema: "public",
                table: "MstInpatientSetting");
        }
    }
}
