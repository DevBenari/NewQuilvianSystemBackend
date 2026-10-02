using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-079 (FR-FIN-173, FR-FIN-183; FIN-DEC-129, 136, 138; FIN-DES-089, 093; DDL R14.6,
    /// R14.9, R14.10). RISIKO TERTINGGI REVISI 14 — satu-satunya migration yang menyentuh tabel
    /// berjalan (FinReceivable, FinSupplierPayable). Belum dieksekusi ke database — wewenang
    /// eksekusi milik Yasmin (02-backend-architecture.md L.7).
    ///
    /// Urutan DDL yang dipakai (MUST diikuti bila migration ini dijalankan):
    /// 1. CreateTable FinOpeningItemBatch (tabel baru, nol risiko).
    /// 2. ALTER COLUMN DROP NOT NULL pada tiga kolom asal Billing FinReceivable — perubahan katalog
    ///    saja, tanpa penulisan ulang tabel.
    /// 3. ADD COLUMN OpeningItemBatchId pada FinReceivable dan FinSupplierPayable, beserta FK-nya.
    /// 4. ADD CONSTRAINT CK_FinReceivable_OpeningItem ... NOT VALID, lalu VALIDATE CONSTRAINT
    ///    terpisah — menghindari pemindaian seluruh tabel di dalam kunci tulis.
    /// 5. DROP INDEX lalu CREATE UNIQUE INDEX CONCURRENTLY untuk IX_FinReceivable_SourceHandoffKey
    ///    dengan filter baru — CONCURRENTLY wajib supaya tidak memblokir tulisan ke FinReceivable
    ///    selama pembuatan index unik. Jendela antara DROP dan selesainya CREATE CONCURRENTLY
    ///    membuka celah idempotensi intake Billing (dua handoff key sama dapat lolos bersamaan)
    ///    yang MUST disebut eksplisit pada laporan task — lihat laporan BE-FIN-079 bagian 7.
    ///
    /// Baris FinReceivable/FinSupplierPayable yang sudah ada tetap sah tanpa backfill: ketiga
    /// kolom Billing-nya terisi dan OpeningItemBatchId kosong — persis cabang pertama check
    /// constraint di atas.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceOpeningItemMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinOpeningItemBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemKind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    CutoverDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalItemCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalOutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DeclaredAccountingOpeningAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AccountingReferenceDocument = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UploadedFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    SourceFormat = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ValidationSummaryJson = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinOpeningItemBatch", x => x.Id);
                    table.CheckConstraint("CK_FinOpeningItemBatch_ItemKind", "\"ItemKind\" IN ('RECEIVABLE','SUPPLIER_PAYABLE')");
                    table.CheckConstraint("CK_FinOpeningItemBatch_Status", "\"Status\" IN ('DRAFT','VALIDATED','APPROVED','LOCKED','REJECTED')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinOpeningItemBatch_BatchNumber",
                schema: "public",
                table: "FinOpeningItemBatch",
                column: "BatchNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinOpeningItemBatch_ItemKind",
                schema: "public",
                table: "FinOpeningItemBatch",
                column: "ItemKind");

            migrationBuilder.CreateIndex(
                name: "IX_FinOpeningItemBatch_Status",
                schema: "public",
                table: "FinOpeningItemBatch",
                column: "Status");

            // --- FinReceivable: kolom asal Billing menjadi nullable bersyarat (DROP NOT NULL saja — tanpa penulisan ulang tabel) ---
            migrationBuilder.AlterColumn<Guid>(
                name: "SourceHandoffKey",
                schema: "public",
                table: "FinReceivable",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "SourceHandoffId",
                schema: "public",
                table: "FinReceivable",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvoiceId",
                schema: "public",
                table: "FinReceivable",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "OpeningItemBatchId",
                schema: "public",
                table: "FinReceivable",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinReceivable_FinOpeningItemBatch_OpeningItemBatchId",
                schema: "public",
                table: "FinReceivable",
                column: "OpeningItemBatchId",
                principalSchema: "public",
                principalTable: "FinOpeningItemBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivable_OpeningItemBatchId",
                schema: "public",
                table: "FinReceivable",
                column: "OpeningItemBatchId");

            // Dua langkah sengaja dipisah (NOT VALID lalu VALIDATE) supaya tidak memindai seluruh
            // tabel di dalam kunci tulis — tidak ada overload MigrationBuilder.AddCheckConstraint
            // yang memulangkan NOT VALID, sehingga ditulis SQL mentah.
            migrationBuilder.Sql(
                "ALTER TABLE public.\"FinReceivable\" ADD CONSTRAINT \"CK_FinReceivable_OpeningItem\" CHECK (" +
                "(\"SourceHandoffKey\" IS NOT NULL AND \"SourceHandoffId\" IS NOT NULL AND \"InvoiceId\" IS NOT NULL AND \"OpeningItemBatchId\" IS NULL) " +
                "OR (\"SourceHandoffKey\" IS NULL AND \"SourceHandoffId\" IS NULL AND \"InvoiceId\" IS NULL AND \"OpeningItemBatchId\" IS NOT NULL)" +
                ") NOT VALID;");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"FinReceivable\" VALIDATE CONSTRAINT \"CK_FinReceivable_OpeningItem\";");

            // Index unik diganti filternya. DROP murni metadata (cepat); CREATE wajib CONCURRENTLY
            // supaya tidak memblokir tulisan ke FinReceivable selama pembuatan index unik baru.
            // Anotasi Npgsql:CreatedConcurrently menjadikan perintah CREATE ini TransactionSuppressed
            // secara otomatis (wajib di PostgreSQL — CREATE INDEX CONCURRENTLY tidak dapat berjalan
            // di dalam transaksi).
            migrationBuilder.DropIndex(
                name: "IX_FinReceivable_SourceHandoffKey",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.CreateIndex(
                    name: "IX_FinReceivable_SourceHandoffKey",
                    schema: "public",
                    table: "FinReceivable",
                    column: "SourceHandoffKey",
                    unique: true,
                    filter: "\"IsDelete\" = false AND \"SourceHandoffKey\" IS NOT NULL")
                .Annotation("Npgsql:CreatedConcurrently", true);

            // --- FinSupplierPayable: satu kolom, nol kolom lain berubah (R14.10) ---
            migrationBuilder.AddColumn<Guid>(
                name: "OpeningItemBatchId",
                schema: "public",
                table: "FinSupplierPayable",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinSupplierPayable_FinOpeningItemBatch_OpeningItemBatchId",
                schema: "public",
                table: "FinSupplierPayable",
                column: "OpeningItemBatchId",
                principalSchema: "public",
                principalTable: "FinOpeningItemBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayable_OpeningItemBatchId",
                schema: "public",
                table: "FinSupplierPayable",
                column: "OpeningItemBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Tidak sepenuhnya aman (02-backend-architecture.md L.7): bila ada baris migrasi
            // (OpeningItemBatchId terisi, tiga kolom Billing kosong) saat Down() dijalankan, AlterColumn
            // kembali ke NOT NULL di bawah akan GAGAL. Down() ini hanya aman selama FinOpeningItemBatch
            // belum pernah dipakai — sama seperti peringatan Up()-nya, migration ini sendiri belum dieksekusi.
            migrationBuilder.DropForeignKey(
                name: "FK_FinSupplierPayable_FinOpeningItemBatch_OpeningItemBatchId",
                schema: "public",
                table: "FinSupplierPayable");

            migrationBuilder.DropIndex(
                name: "IX_FinSupplierPayable_OpeningItemBatchId",
                schema: "public",
                table: "FinSupplierPayable");

            migrationBuilder.DropColumn(
                name: "OpeningItemBatchId",
                schema: "public",
                table: "FinSupplierPayable");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivable_SourceHandoffKey",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivable_SourceHandoffKey",
                schema: "public",
                table: "FinReceivable",
                column: "SourceHandoffKey",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"FinReceivable\" DROP CONSTRAINT \"CK_FinReceivable_OpeningItem\";");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivable_OpeningItemBatchId",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropForeignKey(
                name: "FK_FinReceivable_FinOpeningItemBatch_OpeningItemBatchId",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropColumn(
                name: "OpeningItemBatchId",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvoiceId",
                schema: "public",
                table: "FinReceivable",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SourceHandoffId",
                schema: "public",
                table: "FinReceivable",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SourceHandoffKey",
                schema: "public",
                table: "FinReceivable",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "FinOpeningItemBatch",
                schema: "public");
        }
    }
}
