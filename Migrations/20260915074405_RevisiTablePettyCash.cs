using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class RevisiTablePettyCash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashVoucherCommand_CommandType",
                schema: "public",
                table: "BilPettyCashVoucherCommand");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashVoucherCommand_Reason",
                schema: "public",
                table: "BilPettyCashVoucherCommand");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashVoucher_Status",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropIndex(
                name: "IX_BilPettyCashBudgetMovement_Voucher_Disbursement",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_MovementType",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_Reason",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_VoucherId",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropIndex(
                name: "IX_BilPettyCashBudget_ActiveSingleton",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropIndex(
                name: "IX_BilPettyCashBudget_PoolCode",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudget_Status",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnedAmount",
                schema: "public",
                table: "BilPettyCashVoucher",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReversalReason",
                schema: "public",
                table: "BilPettyCashVoucher",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReversedAt",
                schema: "public",
                table: "BilPettyCashVoucher",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversedBy",
                schema: "public",
                table: "BilPettyCashVoucher",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetAmount",
                schema: "public",
                table: "BilPettyCashBudget",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodEnd",
                schema: "public",
                table: "BilPettyCashBudget",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodStart",
                schema: "public",
                table: "BilPettyCashBudget",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<Guid>(
                name: "SupersededByBudgetId",
                schema: "public",
                table: "BilPettyCashBudget",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "BilPettyCashBudget",
                keyColumn: "Id",
                keyValue: new Guid("b7c1f5a2-9d34-4e88-9a10-000000000001"),
                columns: new[] { "PeriodEnd", "PeriodStart", "SupersededByBudgetId" },
                values: new object[] { null, new DateOnly(2026, 9, 7), null });

            // Pemetaan data (PC-DES-024) — tidak terdeteksi otomatis oleh scaffolding, karena
            // dotnet ef migrations add hanya membandingkan bentuk skema, tidak pernah tahu
            // aturan pemetaan nilai bisnis. Ketiga langkah ini WAJIB ada di migration yang sama
            // dengan penambahan kolom, supaya tidak ada jeda saat baris lama memuat nilai yang
            // tidak dikenal kode baru.

            // 1) BudgetAmount belum ada sebelum revisi ini — kolom baru selalu berdefault 0
            //    (AddColumn di atas). Baris lama diisi dari TotalTopUpAmount-nya sendiri, supaya
            //    "Sisa Anggaran" (BudgetAmount - TotalDisbursedAmount) tidak langsung negatif
            //    begitu migration ini selesai berjalan.
            migrationBuilder.Sql(
                "UPDATE \"BilPettyCashBudget\" SET \"BudgetAmount\" = \"TotalTopUpAmount\" WHERE \"BudgetAmount\" = 0;");

            // 2) INACTIVE -> CLOSED (PC-DES-017). CK_BilPettyCashBudget_Status yang baru sudah
            //    permisif (union nilai lama dan baru) sehingga migration ini TIDAK akan gagal
            //    tanpa baris ini, tetapi baris INACTIVE yang dibiarkan akan tidak terbaca sebagai
            //    periode tertutup oleh PettyCashBudgetService.GetPeriodsAsync/ClosePeriodAsync.
            migrationBuilder.Sql(
                "UPDATE \"BilPettyCashBudget\" SET \"Status\" = 'CLOSED' WHERE \"Status\" = 'INACTIVE';");

            // 3) WAITING_APPROVAL / APPROVED -> REQUESTED (PC-DES-015, PC-DEC-016). Sama seperti
            //    di atas: PettyCashVoucherService sudah mentoleransi kosakata lama di seluruh
            //    gerbangnya (IsAwaitingDisbursement), sehingga migration ini TIDAK gagal tanpa
            //    baris ini — tetapi tanpa pemetaan ini, voucher lama tetap tersimpan dengan
            //    kosakata gerbang persetujuan yang sudah dicabut selamanya.
            migrationBuilder.Sql(
                "UPDATE \"BilPettyCashVoucher\" SET \"Status\" = 'REQUESTED' WHERE \"Status\" IN ('WAITING_APPROVAL','APPROVED');");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashVoucherCommand_CommandType",
                schema: "public",
                table: "BilPettyCashVoucherCommand",
                sql: "\"CommandType\" IN ('SUBMIT','APPROVE','REJECT','CANCEL','DISBURSE','ATTACH_PROOF','PROOF_CORRECTED','RETURN','REVERSAL')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashVoucherCommand_Reason",
                schema: "public",
                table: "BilPettyCashVoucherCommand",
                sql: "\"CommandType\" NOT IN ('REJECT','CANCEL','RETURN','REVERSAL') OR \"Reason\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashVoucher_ReturnedAmount",
                schema: "public",
                table: "BilPettyCashVoucher",
                sql: "\"ReturnedAmount\" >= 0 AND \"ReturnedAmount\" <= \"Amount\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashVoucher_ReversalReason",
                schema: "public",
                table: "BilPettyCashVoucher",
                sql: "(\"Status\" = 'REVERSED' AND \"ReversalReason\" IS NOT NULL) OR (\"Status\" <> 'REVERSED' AND \"ReversalReason\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashVoucher_Status",
                schema: "public",
                table: "BilPettyCashVoucher",
                sql: "\"Status\" IN ('WAITING_APPROVAL','APPROVED','CASH_RECEIVED','COMPLETED','REJECTED','REQUESTED','REVERSED')");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudgetMovement_Voucher_Reversal",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                column: "VoucherId",
                unique: true,
                filter: "\"MovementType\" = 'REVERSAL' AND \"IsDelete\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_MovementType",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                sql: "\"MovementType\" IN ('TOP_UP','DISBURSEMENT','ADJUSTMENT','RETURN','REVERSAL','CARRY_FORWARD_OUT','CARRY_FORWARD_IN')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_Reason",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                sql: "\"MovementType\" = 'DISBURSEMENT' OR \"Reason\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_VoucherId",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                sql: "(\"MovementType\" IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NOT NULL) OR (\"MovementType\" NOT IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudget_ActivePerPool",
                schema: "public",
                table: "BilPettyCashBudget",
                column: "PoolCode",
                unique: true,
                filter: "\"Status\" = 'ACTIVE' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudget_PoolCode_PeriodStart",
                schema: "public",
                table: "BilPettyCashBudget",
                columns: new[] { "PoolCode", "PeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "BilPettyCashBudget",
                column: "SupersededByBudgetId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudget_BudgetAmount",
                schema: "public",
                table: "BilPettyCashBudget",
                sql: "\"BudgetAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudget_Status",
                schema: "public",
                table: "BilPettyCashBudget",
                sql: "\"Status\" IN ('ACTIVE','INACTIVE','DRAFT','CLOSED')");

            migrationBuilder.AddForeignKey(
                name: "FK_BilPettyCashBudget_BilPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "BilPettyCashBudget",
                column: "SupersededByBudgetId",
                principalSchema: "public",
                principalTable: "BilPettyCashBudget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        /// <remarks>
        /// AKAN GAGAL bila sudah pernah ada perintah RETURN atau REVERSAL tercatat di
        /// BilPettyCashVoucherCommand (BE-BKC-057). Constraint CK_BilPettyCashVoucherCommand_CommandType
        /// lama di bawah tidak mengenal kedua nilai itu, dan baris tabel tersebut SENGAJA tidak
        /// dipetakan ulang di sini — ia append-only/tidak boleh disunting (PC-DEC-003), jadi
        /// kegagalan Down() yang keras di sini lebih benar daripada korupsi jejak audit yang diam-diam.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BilPettyCashBudget_BilPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashVoucherCommand_CommandType",
                schema: "public",
                table: "BilPettyCashVoucherCommand");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashVoucherCommand_Reason",
                schema: "public",
                table: "BilPettyCashVoucherCommand");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashVoucher_ReturnedAmount",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashVoucher_ReversalReason",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashVoucher_Status",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropIndex(
                name: "IX_BilPettyCashBudgetMovement_Voucher_Reversal",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_MovementType",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_Reason",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_VoucherId",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropIndex(
                name: "IX_BilPettyCashBudget_ActivePerPool",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropIndex(
                name: "IX_BilPettyCashBudget_PoolCode_PeriodStart",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropIndex(
                name: "IX_BilPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudget_BudgetAmount",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudget_Status",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropColumn(
                name: "ReturnedAmount",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropColumn(
                name: "ReversalReason",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropColumn(
                name: "ReversedAt",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropColumn(
                name: "ReversedBy",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropColumn(
                name: "BudgetAmount",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                schema: "public",
                table: "BilPettyCashBudget");

            migrationBuilder.DropColumn(
                name: "SupersededByBudgetId",
                schema: "public",
                table: "BilPettyCashBudget");

            // Penjaga sebelum memasang ulang constraint LAMA yang lebih ketat di bawah:
            // rollback ini TIDAK DAPAT sempurna (dicatat sejak desain, 02-backend-architecture.md
            // § Rencana migration langkah 6). REQUESTED tidak bisa dibedakan lagi antara asalnya
            // WAITING_APPROVAL atau APPROVED, dan REVERSED/DRAFT/CLOSED sama sekali tidak dikenal
            // constraint lama — dipetakan ke nilai lama yang paling dekat maknanya supaya
            // Down() tidak GAGAL karena melanggar constraint yang baru saja dipasang ulang.
            // Jejak yang sebenarnya tetap utuh di BilPettyCashVoucherCommand/BilPettyCashBudgetMovement
            // (append-only, tidak disentuh Down() ini).
            migrationBuilder.Sql(
                "UPDATE \"BilPettyCashVoucher\" SET \"Status\" = 'WAITING_APPROVAL' WHERE \"Status\" IN ('REQUESTED','REVERSED');");
            migrationBuilder.Sql(
                "UPDATE \"BilPettyCashBudget\" SET \"Status\" = 'INACTIVE' WHERE \"Status\" IN ('CLOSED','DRAFT');");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashVoucherCommand_CommandType",
                schema: "public",
                table: "BilPettyCashVoucherCommand",
                sql: "\"CommandType\" IN ('SUBMIT','APPROVE','REJECT','CANCEL','DISBURSE','ATTACH_PROOF','PROOF_CORRECTED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashVoucherCommand_Reason",
                schema: "public",
                table: "BilPettyCashVoucherCommand",
                sql: "\"CommandType\" NOT IN ('REJECT','CANCEL') OR \"Reason\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashVoucher_Status",
                schema: "public",
                table: "BilPettyCashVoucher",
                sql: "\"Status\" IN ('WAITING_APPROVAL','APPROVED','CASH_RECEIVED','COMPLETED','REJECTED')");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudgetMovement_Voucher_Disbursement",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                column: "VoucherId",
                unique: true,
                filter: "\"MovementType\" = 'DISBURSEMENT' AND \"IsDelete\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_MovementType",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                sql: "\"MovementType\" IN ('TOP_UP','DISBURSEMENT','ADJUSTMENT')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_Reason",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                sql: "\"MovementType\" NOT IN ('TOP_UP','ADJUSTMENT') OR \"Reason\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_VoucherId",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                sql: "(\"MovementType\" = 'DISBURSEMENT' AND \"VoucherId\" IS NOT NULL) OR (\"MovementType\" <> 'DISBURSEMENT' AND \"VoucherId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudget_ActiveSingleton",
                schema: "public",
                table: "BilPettyCashBudget",
                column: "Status",
                unique: true,
                filter: "\"Status\" = 'ACTIVE' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudget_PoolCode",
                schema: "public",
                table: "BilPettyCashBudget",
                column: "PoolCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudget_Status",
                schema: "public",
                table: "BilPettyCashBudget",
                sql: "\"Status\" IN ('ACTIVE','INACTIVE')");
        }
    }
}
