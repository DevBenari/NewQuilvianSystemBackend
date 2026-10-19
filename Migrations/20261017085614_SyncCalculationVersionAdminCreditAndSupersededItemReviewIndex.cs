using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menyelaraskan kolom kredit administrasi rawat jalan rujukan (ReferredOutpatientAdminCreditAmount &amp;
    /// ReferredOutpatientAdminUnappliedCreditAmount) ke BilCalculationVersion, serta menambahkan indeks
    /// IX_BilSupersededItemReview_ReinstatedInvoiceItemId pada tabel BilSupersededItemReview (BE-BKC-088, BKC-DEC-158).
    /// </summary>
    /// <inheritdoc />
    public partial class SyncCalculationVersionAdminCreditAndSupersededItemReviewIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Catatan: Kolom ReferredOutpatientAdminCreditAmount dan ReferredOutpatientAdminUnappliedCreditAmount
            // pada tabel BilCalculationVersion sudah dibuat oleh migrasi AddReferredAdminCreditAndSupersededItemReview,
            // dan tidak pernah ada pada tabel AccJournalApproval di database nyata.
            // Klausa IF EXISTS dan IF NOT EXISTS memastikan migrasi ini 100% idempoten dan aman dieksekusi
            // tanpa memicu PostgreSQL error 42703 (column does not exist) atau 42701 (column already exists).
            migrationBuilder.Sql(@"
ALTER TABLE public.""AccJournalApproval"" DROP COLUMN IF EXISTS ""ReferredOutpatientAdminCreditAmount"";
ALTER TABLE public.""AccJournalApproval"" DROP COLUMN IF EXISTS ""ReferredOutpatientAdminUnappliedCreditAmount"";
ALTER TABLE public.""BilCalculationVersion"" ADD COLUMN IF NOT EXISTS ""ReferredOutpatientAdminCreditAmount"" numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE public.""BilCalculationVersion"" ADD COLUMN IF NOT EXISTS ""ReferredOutpatientAdminUnappliedCreditAmount"" numeric(18,2) NOT NULL DEFAULT 0;
");

            migrationBuilder.CreateIndex(
                name: "IX_BilSupersededItemReview_ReinstatedInvoiceItemId",
                schema: "public",
                table: "BilSupersededItemReview",
                column: "ReinstatedInvoiceItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BilSupersededItemReview_ReinstatedInvoiceItemId",
                schema: "public",
                table: "BilSupersededItemReview");
        }
    }
}
