using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Migrasi Skema DB Ekstensi Model BusinessTravelManagement & ExpenseManagement V2 serta Index Komposit (BE-HRD-318, HRD-DEC-188 s.d. 193)
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261109210000_AddBusinessTravelAndExpenseManagementV2")]
    public partial class AddBusinessTravelAndExpenseManagementV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- BE-HRD-318: Ekstensi Kolom TrxTravelSettlement V2
                ALTER TABLE ""TrxTravelSettlement"" ADD COLUMN IF NOT EXISTS ""IsOverdue"" boolean DEFAULT false;
                ALTER TABLE ""TrxTravelSettlement"" ADD COLUMN IF NOT EXISTS ""SettledAt"" timestamp with time zone;
                ALTER TABLE ""TrxTravelSettlement"" ADD COLUMN IF NOT EXISTS ""DefaultedAt"" timestamp with time zone;
                ALTER TABLE ""TrxTravelSettlement"" ADD COLUMN IF NOT EXISTS ""RefundVoucherNumber"" character varying(100);
                ALTER TABLE ""TrxTravelSettlement"" ADD COLUMN IF NOT EXISTS ""PaymentOrRefundRef"" character varying(100);
                ALTER TABLE ""TrxTravelSettlement"" ADD COLUMN IF NOT EXISTS ""ReceiptDocUrl"" character varying(500);

                -- BE-HRD-318: Ekstensi Kolom TrxTravelExpenseItem V2 (Deklarasi Tanpa Kuitansi Darurat HRD-DEC-192)
                ALTER TABLE ""TrxTravelExpenseItem"" ADD COLUMN IF NOT EXISTS ""IsMissingReceiptDeclared"" boolean DEFAULT false;
                ALTER TABLE ""TrxTravelExpenseItem"" ADD COLUMN IF NOT EXISTS ""MissingReceiptReason"" character varying(1000);
                ALTER TABLE ""TrxTravelExpenseItem"" ADD COLUMN IF NOT EXISTS ""MissingReceiptApprovedBy"" uuid;

                -- Index Gabungan Performa Tinggi (BE-HRD-318):
                -- 1. Optimasi dasbor pemohon dan riwayat SPPD (WorkforceProfileId, TravelStatus)
                CREATE INDEX IF NOT EXISTS ix_trx_business_travel_request_workforce_status 
                    ON ""TrxBusinessTravelRequest"" (""WorkforceProfileId"", ""TravelStatus"");

                -- 2. Optimasi pelaporan pemakaian pagu anggaran dinas unit cost center (CostCenterId, StartDate)
                CREATE INDEX IF NOT EXISTS ix_trx_business_travel_request_costcenter_startdate 
                    ON ""TrxBusinessTravelRequest"" (""CostCenterId"", ""StartDate"");

                -- 3. Optimasi worker pemantauan tunggakan settlement & SLA (IsOverdue, SettledAt)
                CREATE INDEX IF NOT EXISTS ix_trx_travel_settlement_overdue_settled 
                    ON ""TrxTravelSettlement"" (""IsOverdue"", ""SettledAt"");

                -- 4. Optimasi klaim non-trip dan verifikasi finance
                CREATE INDEX IF NOT EXISTS ix_trx_expense_claim_workforce_status 
                    ON ""TrxExpenseClaim"" (""WorkforceProfileId"", ""ClaimStatus"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ix_trx_business_travel_request_workforce_status;
                DROP INDEX IF EXISTS ix_trx_business_travel_request_costcenter_startdate;
                DROP INDEX IF EXISTS ix_trx_travel_settlement_overdue_settled;
                DROP INDEX IF EXISTS ix_trx_expense_claim_workforce_status;
            ");
        }
    }
}
