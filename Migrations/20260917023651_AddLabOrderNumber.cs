using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// <c>LAB-DEC-072</c> — nomor pesanan laboratorium yang dapat disebut manusia.
    ///
    /// <b>Migration ini disunting tangan, dan alasannya pantas dibaca sebelum diubah.</b>
    /// Bentuk bawaan yang dihasilkan EF adalah satu <c>AddColumn</c> ber-<c>nullable: false</c>
    /// dengan <c>defaultValue: ""</c>, lalu satu index unik. Pada tabel yang sudah berisi,
    /// urutan itu <b>pasti gagal</b>: seluruh baris lama memperoleh nilai yang sama — string
    /// kosong — dan index unik menolaknya pada baris kedua.
    ///
    /// Urutan yang dipakai di sini: kolomnya lahir <b>boleh kosong</b>, baris lama diisi nomor
    /// berurutan, barulah kolomnya dijadikan wajib dan indexnya dipasang.
    /// </summary>
    public partial class AddLabOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Kolomnya lahir boleh kosong. Tanpa langkah ini, pengisian di bawah tidak punya
            //    tempat, dan nilai bawaan apa pun akan bertabrakan pada index unik.
            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                schema: "public",
                table: "LabOrder",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            // 2. Pesanan yang sudah ada diberi nomor menurut urutan lahirnya.
            //
            //    Baris ber-`IsDelete` IKUT diisi, dan itu disengaja: index unik tidak
            //    membedakan baris terhapus, dan kolom wajib berlaku atas seluruh tabel. Nomor
            //    yang terpakai baris terhapus tidak akan pernah dipakai ulang — sama seperti
            //    perilaku alokasi pada `LabOrderNumberService`.
            //
            //    Urutannya `CreateDateTime` lalu `Id` sebagai pemecah seri, sehingga hasilnya
            //    sama pada setiap kali dijalankan. `NULLS LAST` menjaga baris lama yang waktunya
            //    tidak terisi tetap memperoleh nomor, bukan menggagalkan seluruh migration.
            migrationBuilder.Sql(@"
                WITH terurut AS (
                    SELECT ""Id"",
                           ROW_NUMBER() OVER (ORDER BY ""CreateDateTime"" NULLS LAST, ""Id"") AS urut
                    FROM public.""LabOrder""
                )
                UPDATE public.""LabOrder"" o
                SET ""OrderNumber"" = 'LAB-RSMMC-' || LPAD(t.urut::text, 6, '0')
                FROM terurut t
                WHERE o.""Id"" = t.""Id"";
            ");

            // 3. Barulah kolomnya menjadi wajib. Nomor order tidak punya keadaan "belum": setiap
            //    pesanan memilikinya sejak lahir, dan kolom yang boleh kosong hanya akan
            //    menyembunyikan jalur tulis yang lupa mengalokasikan.
            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                schema: "public",
                table: "LabOrder",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            // 4. Jaring pengaman terakhir alokasi. Kunci advisory mengurangi tabrakan; index
            //    inilah yang membuat dua pesanan bernomor sama menjadi mustahil.
            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_OrderNumber",
                schema: "public",
                table: "LabOrder",
                column: "OrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabOrder_OrderNumber",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                schema: "public",
                table: "LabOrder");
        }
    }
}
