using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menambahkan nomor pesanan radiologi yang terbaca manusia — <c>RAD-CONF-001</c> bagian 8
    /// butir 2.
    /// </summary>
    /// <remarks>
    /// Urutan tiga langkah di bawah tidak boleh ditukar. Kolomnya wajib diisi dan index-nya
    /// unik, sehingga membuat index sebelum pengisian mundur akan menolak setiap database yang
    /// sudah memuat lebih dari satu pesanan — seluruh barisnya bernomor string kosong yang sama.
    /// </remarks>
    public partial class AddRadOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                schema: "public",
                table: "RadOrder",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Pengisian mundur bagi pesanan yang lahir sebelum kolom ini ada.
            //
            // Nomornya dibentuk dari waktu baris itu dibuat, lalu dibedakan dengan nomor urut di
            // dalam detik yang sama. Cara ini TERJAMIN tidak menghasilkan nomor kembar: dalam
            // satu detik nomor urutnya berbeda, dan antar detik cap waktunya berbeda — tanpa
            // bergantung pada keberuntungan potongan Guid.
            //
            // Bentuknya sengaja sama persis dengan nomor yang diterbitkan
            // RadOrderNumberService, supaya petugas tidak perlu mengenali dua bentuk nomor.
            //
            // Baris yang sudah dihapus lunak ikut diisi. Ia memang tidak masuk index karena
            // penyaring IsDelete, tetapi membiarkannya bernomor kosong berarti riwayat lama
            // tidak dapat ditunjuk sama sekali.
            migrationBuilder.Sql("""
                WITH bernomor AS (
                    SELECT "Id",
                           'RAD-ORD-'
                             || to_char("CreateDateTime" AT TIME ZONE 'UTC', 'YYMMDDHH24MISS')
                             || '-'
                             || lpad(
                                  (ROW_NUMBER() OVER (
                                       PARTITION BY date_trunc('second', "CreateDateTime")
                                       ORDER BY "Id"))::text,
                                  6, '0') AS nomor
                    FROM public."RadOrder"
                    WHERE "OrderNumber" = ''
                )
                UPDATE public."RadOrder" AS o
                SET "OrderNumber" = b.nomor
                FROM bernomor AS b
                WHERE o."Id" = b."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_OrderNumber",
                schema: "public",
                table: "RadOrder",
                column: "OrderNumber",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RadOrder_OrderNumber",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                schema: "public",
                table: "RadOrder");
        }
    }
}
