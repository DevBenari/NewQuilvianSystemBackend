using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// <c>BE-HRD-404</c> (20 November 2026), gerbang <c>{MIG-M05-JD}</c>: tabel Uraian Tugas berversi per jabatan
    /// (<c>MstJobDescription</c>) dan butirnya (<c>MstJobDescriptionItem</c>). Model, konfigurasi EF, dan <c>DbSet</c>
    /// sudah ada sejak laporan Menu 05, tetapi tabelnya belum pernah dibuat sehingga endpoint Uraian Tugas Menu 05 dan
    /// Uraian Tugas Pegawai Menu 07 gagal <c>42P01</c>. Ditulis manual dan idempoten (pola
    /// <c>20261118100000_AddHrdOvertimeSetting</c>) karena snapshot model tidak sinkron dengan migration SDM manual.
    /// </summary>
    /// <remarks>
    /// Selain index dari konfigurasi EF, dua index unik parsial ditambahkan di database:
    /// satu versi <c>Active</c> per jabatan dan satu <c>Draft</c> per jabatan. Keduanya menjadi penjaga terakhir bila
    /// dua petugas mengaktifkan atau membuat draft bersamaan (pelanggaran → SQLSTATE 23505 → 409 di middleware).
    /// Ditulis atas izin pemilik 20 November 2026 ("migration boleh ditulis"); BELUM dijalankan ke database mana pun.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261120091000_AddMstJobDescription")]
    public partial class AddMstJobDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstJobDescription') THEN
        CREATE TABLE public.""MstJobDescription"" (
            ""Id"" uuid NOT NULL,
            ""PositionId"" uuid NOT NULL,
            ""JobDescriptionCode"" character varying(50) NOT NULL,
            ""VersionNumber"" integer NOT NULL,
            ""EffectiveStartDate"" timestamp with time zone NOT NULL,
            ""EffectiveEndDate"" timestamp with time zone NULL,
            ""Status"" character varying(30) NOT NULL DEFAULT 'Draft',
            ""Purpose"" character varying(2000) NOT NULL,
            ""IsActive"" boolean NOT NULL DEFAULT true,
            ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ""CreateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""UpdateDateTime"" timestamp with time zone NULL,
            ""UpdateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""DeleteDateTime"" timestamp with time zone NULL,
            ""DeleteBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""CancelDateTime"" timestamp with time zone NULL,
            ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""IsCancel"" boolean NOT NULL DEFAULT false,
            ""IsDelete"" boolean NOT NULL DEFAULT false,
            CONSTRAINT ""PK_MstJobDescription"" PRIMARY KEY (""Id""),
            CONSTRAINT ""FK_MstJobDescription_MstPosition_PositionId"" FOREIGN KEY (""PositionId"")
                REFERENCES public.""MstPosition"" (""Id"") ON DELETE RESTRICT,
            CONSTRAINT ""CK_MstJobDescription_Status"" CHECK (""Status"" IN ('Draft', 'Active', 'Archived')),
            CONSTRAINT ""CK_MstJobDescription_EffectiveRange"" CHECK (""EffectiveEndDate"" IS NULL OR ""EffectiveEndDate"" >= ""EffectiveStartDate"")
        );

        CREATE UNIQUE INDEX ""IX_MstJobDescription_PositionId_VersionNumber""
            ON public.""MstJobDescription"" (""PositionId"", ""VersionNumber"");
        CREATE UNIQUE INDEX ""IX_MstJobDescription_JobDescriptionCode""
            ON public.""MstJobDescription"" (""JobDescriptionCode"");
        CREATE INDEX ""IX_MstJobDescription_PositionId_Status""
            ON public.""MstJobDescription"" (""PositionId"", ""Status"");
        CREATE UNIQUE INDEX ""UX_MstJobDescription_OneActivePerPosition""
            ON public.""MstJobDescription"" (""PositionId"")
            WHERE ""Status"" = 'Active' AND ""IsDelete"" = false;
        CREATE UNIQUE INDEX ""UX_MstJobDescription_OneDraftPerPosition""
            ON public.""MstJobDescription"" (""PositionId"")
            WHERE ""Status"" = 'Draft' AND ""IsDelete"" = false;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstJobDescriptionItem') THEN
        CREATE TABLE public.""MstJobDescriptionItem"" (
            ""Id"" uuid NOT NULL,
            ""JobDescriptionId"" uuid NOT NULL,
            ""SectionType"" character varying(50) NOT NULL,
            ""Sequence"" integer NOT NULL,
            ""Content"" character varying(2000) NOT NULL,
            ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ""CreateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""UpdateDateTime"" timestamp with time zone NULL,
            ""UpdateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""DeleteDateTime"" timestamp with time zone NULL,
            ""DeleteBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""CancelDateTime"" timestamp with time zone NULL,
            ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""IsCancel"" boolean NOT NULL DEFAULT false,
            ""IsDelete"" boolean NOT NULL DEFAULT false,
            CONSTRAINT ""PK_MstJobDescriptionItem"" PRIMARY KEY (""Id""),
            CONSTRAINT ""FK_MstJobDescriptionItem_MstJobDescription_JobDescriptionId"" FOREIGN KEY (""JobDescriptionId"")
                REFERENCES public.""MstJobDescription"" (""Id"") ON DELETE CASCADE
        );

        CREATE INDEX ""IX_MstJobDescriptionItem_JobDescriptionId_SectionType_Sequence""
            ON public.""MstJobDescriptionItem"" (""JobDescriptionId"", ""SectionType"", ""Sequence"");
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS public.""MstJobDescriptionItem"";
DROP TABLE IF EXISTS public.""MstJobDescription"";");
        }
    }
}
