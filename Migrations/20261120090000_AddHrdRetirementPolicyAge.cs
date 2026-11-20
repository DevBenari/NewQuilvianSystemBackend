using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// <c>BE-HRD-368</c> (20 November 2026): usia pensiun normal pindah dari angka tertanam di
    /// <c>RetirementService</c> (55/58) ke pengaturan siklus kepegawaian per rumah sakit
    /// (<c>HrdEmploymentLifecycleSetting.NormalRetirementAge</c>). Kolom nullable: kosong berarti kebijakan
    /// belum ditetapkan dan kandidat pensiun otomatis diblokir (<c>POLICY_NOT_CONFIGURED</c>). Ditulis manual dan
    /// idempoten (pola <c>20261118100000_AddHrdOvertimeSetting</c>) karena snapshot model tidak sinkron dengan
    /// migration SDM manual.
    /// </summary>
    /// <remarks>Ditulis atas izin pemilik 20 November 2026 ("migration boleh ditulis"); BELUM dijalankan ke database mana pun.</remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261120090000_AddHrdRetirementPolicyAge")]
    public partial class AddHrdRetirementPolicyAge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdEmploymentLifecycleSetting') THEN
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = 'HrdEmploymentLifecycleSetting' AND column_name = 'NormalRetirementAge') THEN
            ALTER TABLE public.""HrdEmploymentLifecycleSetting"" ADD COLUMN ""NormalRetirementAge"" integer NULL;
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM pg_constraint
            WHERE conname = 'CK_HrdEmploymentLifecycleSetting_NormalRetirementAge') THEN
            ALTER TABLE public.""HrdEmploymentLifecycleSetting""
                ADD CONSTRAINT ""CK_HrdEmploymentLifecycleSetting_NormalRetirementAge""
                CHECK (""NormalRetirementAge"" IS NULL OR ""NormalRetirementAge"" BETWEEN 45 AND 70);
        END IF;
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE IF EXISTS public.""HrdEmploymentLifecycleSetting"" DROP CONSTRAINT IF EXISTS ""CK_HrdEmploymentLifecycleSetting_NormalRetirementAge"";
ALTER TABLE IF EXISTS public.""HrdEmploymentLifecycleSetting"" DROP COLUMN IF EXISTS ""NormalRetirementAge"";");
        }
    }
}
