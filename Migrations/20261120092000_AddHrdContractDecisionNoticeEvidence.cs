using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// <c>BE-HRD-419</c> (20 November 2026): artefak bukti pemberitahuan tertulis H-7 pada keputusan kontrak
    /// (<c>HrdContractDecision.NoticeEvidence*</c>). Status <c>ON_TIME</c> kini hanya dihitung bila berkas bukti tersimpan.
    /// Ditulis manual dan idempoten (pola <c>20261118100000_AddHrdOvertimeSetting</c>).
    /// </summary>
    /// <remarks>Ditulis atas izin pemilik 20 November 2026 ("migration boleh ditulis"); BELUM dijalankan ke database mana pun.</remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261120092000_AddHrdContractDecisionNoticeEvidence")]
    public partial class AddHrdContractDecisionNoticeEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE IF EXISTS public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""NoticeEvidencePath"" character varying(500) NULL;
ALTER TABLE IF EXISTS public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""NoticeEvidenceFileName"" character varying(255) NULL;
ALTER TABLE IF EXISTS public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""NoticeEvidenceUploadedAt"" timestamp with time zone NULL;
ALTER TABLE IF EXISTS public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""NoticeEvidenceUploadedByUserId"" uuid NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE IF EXISTS public.""HrdContractDecision"" DROP COLUMN IF EXISTS ""NoticeEvidenceUploadedByUserId"";
ALTER TABLE IF EXISTS public.""HrdContractDecision"" DROP COLUMN IF EXISTS ""NoticeEvidenceUploadedAt"";
ALTER TABLE IF EXISTS public.""HrdContractDecision"" DROP COLUMN IF EXISTS ""NoticeEvidenceFileName"";
ALTER TABLE IF EXISTS public.""HrdContractDecision"" DROP COLUMN IF EXISTS ""NoticeEvidencePath"";");
        }
    }
}
