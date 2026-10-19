using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRadOrderPhotoNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoNumber",
                schema: "public",
                table: "RadOrder",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_PhotoNumber",
                schema: "public",
                table: "RadOrder",
                column: "PhotoNumber",
                unique: true,
                filter: "\"PhotoNumber\" IS NOT NULL AND \"IsDelete\" = false");

            // No. Foto v1 diberikan saat Konfirmasi. Pesanan yang SUDAH dikonfirmasi (punya jejak
            // Order.Accept) diberi nomor kategori alatnya, urut waktu konfirmasi per kategori —
            // pemetaannya sama dengan RadExamCategory. Pesanan yang belum dikonfirmasi tetap kosong.
            migrationBuilder.Sql(@"
WITH konfirmasi AS (
    SELECT h.""RadOrderId"", MIN(h.""OccurredAt"") AS waktu
    FROM public.""RadTransitionHistory"" h
    WHERE h.""Action"" = 'Order.Accept'
    GROUP BY h.""RadOrderId""
),
kategori AS (
    SELECT o.""Id"", k.waktu,
           CASE UPPER(m.""ModalityCode"")
               WHEN 'US' THEN 'RAD-02'
               WHEN 'CT' THEN 'RAD-03'
               WHEN 'MR' THEN 'RAD-04'
               WHEN 'MG' THEN 'RAD-05'
               WHEN 'RF' THEN 'RAD-06'
               WHEN 'XA' THEN 'RAD-07'
               WHEN 'NM' THEN 'RAD-08'
               WHEN 'PT' THEN 'RAD-08'
               WHEN 'BMD' THEN 'RAD-09'
               WHEN 'DXA' THEN 'RAD-09'
               ELSE 'RAD-01'
           END AS kode
    FROM public.""RadOrder"" o
    JOIN konfirmasi k ON k.""RadOrderId"" = o.""Id""
    JOIN public.""MstRadModality"" m ON m.""Id"" = o.""ModalityId""
    WHERE o.""PhotoNumber"" IS NULL AND o.""IsDelete"" = false
),
urut AS (
    SELECT ""Id"", kode, ROW_NUMBER() OVER (PARTITION BY kode ORDER BY waktu, ""Id"") AS r
    FROM kategori
)
UPDATE public.""RadOrder"" o
SET ""PhotoNumber"" = u.kode || '-' || LPAD(u.r::text, 2, '0')
FROM urut u
WHERE o.""Id"" = u.""Id"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RadOrder_PhotoNumber",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "PhotoNumber",
                schema: "public",
                table: "RadOrder");
        }
    }
}
