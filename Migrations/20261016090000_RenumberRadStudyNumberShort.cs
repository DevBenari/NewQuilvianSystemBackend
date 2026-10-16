using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261016090000_RenumberRadStudyNumberShort")]
    public partial class RenumberRadStudyNumberShort : Migration
    {
        // No. Foto radiologi pindah dari bentuk panjang `RAD-{Guid pesanan}-{urut}` (40 karakter)
        // ke bentuk pendek `RAD-000042` yang dialokasikan RadStudyNumberService — keputusan
        // pemilik modul 2026-10-16, mengikuti konsep No. Foto v1 dan keluarga nomor Lab.
        //
        // Migration data saja: skema, kolom, dan index unik `StudyNumber` tidak berubah. Study
        // berbentuk lama dinomori ulang menurut urutan dibuat, melanjutkan nomor pendek tertinggi
        // yang mungkin sudah ada, sehingga hasilnya tidak bertabrakan dengan index unik.
        //
        // Yang SENGAJA tidak disentuh: `RuleSnapshot` fakta kelayakan tagih yang sudah terkirim ke
        // Billing. Itu potret saat kejadian, bukan rujukan hidup.
        //
        // Arah turun memulihkan bentuk lama secara deterministik dari `RadOrderId` dan
        // `StudySequence` — persis rumus pembentuk lamanya.

        private const string PolaPendek = "'^RAD-[0-9]+$'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
WITH dasar AS (
    SELECT COALESCE(MAX(CAST(SUBSTRING(""StudyNumber"" FROM 5) AS BIGINT)), 0) AS n
    FROM public.""RadStudy""
    WHERE ""StudyNumber"" ~ {PolaPendek}
),
urut AS (
    SELECT s.""Id"",
           ROW_NUMBER() OVER (ORDER BY s.""CreateDateTime"", s.""StudySequence"", s.""Id"") AS r
    FROM public.""RadStudy"" s
    WHERE s.""StudyNumber"" !~ {PolaPendek}
)
UPDATE public.""RadStudy"" s
SET ""StudyNumber"" = 'RAD-' || LPAD((dasar.n + urut.r)::text, 6, '0')
FROM urut, dasar
WHERE s.""Id"" = urut.""Id"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE public.""RadStudy""
SET ""StudyNumber"" = 'RAD-' || UPPER(REPLACE(""RadOrderId""::text, '-', '')) || '-' || LPAD(""StudySequence""::text, 3, '0')
WHERE ""StudyNumber"" ~ {PolaPendek};");
        }
    }
}
