using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261016100000_RenumberRadOrderNumberLikeLab")]
    public partial class RenumberRadOrderNumberLikeLab : Migration
    {
        // Nomor pesanan radiologi pindah dari `RAD-ORD-{yyMMddHHmmss}-{6 acak}` ke bentuk nomor
        // Lab `RAD-RSMMC-000042` yang dialokasikan RadOrderNumberService — keputusan pemilik modul
        // 2026-10-16.
        //
        // Migration data saja: skema dan index unik `OrderNumber` tidak berubah. Pesanan berbentuk
        // lama dinomori ulang menurut urutan dibuat, melanjutkan nomor baru tertinggi yang mungkin
        // sudah ada, sehingga tidak bertabrakan dengan index unik.
        //
        // BATAS ARAH TURUN: enam karakter acak bentuk lama tidak tersimpan di mana pun, sehingga
        // tidak dapat dipulihkan persis. Down membentuk ulang bentuk lama secara deterministik dari
        // waktu dibuat (UTC, sama dengan pembentuk lamanya) dan enam karakter pertama Id pesanan.

        private const string PolaBaru = "'^RAD-RSMMC-[0-9]+$'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
WITH dasar AS (
    SELECT COALESCE(MAX(CAST(SUBSTRING(""OrderNumber"" FROM 11) AS BIGINT)), 0) AS n
    FROM public.""RadOrder""
    WHERE ""OrderNumber"" ~ {PolaBaru}
),
urut AS (
    SELECT o.""Id"",
           ROW_NUMBER() OVER (ORDER BY o.""CreateDateTime"", o.""Id"") AS r
    FROM public.""RadOrder"" o
    WHERE o.""OrderNumber"" !~ {PolaBaru}
)
UPDATE public.""RadOrder"" o
SET ""OrderNumber"" = 'RAD-RSMMC-' || LPAD((dasar.n + urut.r)::text, 6, '0')
FROM urut, dasar
WHERE o.""Id"" = urut.""Id"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE public.""RadOrder""
SET ""OrderNumber"" = 'RAD-ORD-' || TO_CHAR(""CreateDateTime"", 'YYMMDDHH24MISS') || '-'
    || UPPER(SUBSTRING(REPLACE(""Id""::text, '-', '') FROM 1 FOR 6))
WHERE ""OrderNumber"" ~ {PolaBaru};");
        }
    }
}
