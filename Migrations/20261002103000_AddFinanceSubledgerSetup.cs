using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-064 (FR-FIN-141, FR-FIN-146; FIN-DEC-113, 128; FIN-DES-080, 088; DDL R14.4, R14.5).
    /// Membuat dua skema tabel pengaturan subledger cutover:
    /// 1. FinSubledgerControlAccountMap: pemetaan kelompok saldo dan segmen ke kode akun control Accounting
    /// 2. FinOpeningBalance: pencatatan, persetujuan, dan penguncian saldo awal cutover per kelompok saldo
    /// Murni tabel baru, belum dieksekusi ke database (otorisasi pembuatan via FIN-DEC-138).
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceSubledgerSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinSubledgerControlAccountMap",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceGroup = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SegmentKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ControlAccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
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
                    table.PrimaryKey("PK_FinSubledgerControlAccountMap", x => x.Id);
                    table.CheckConstraint("CK_FinSubledgerControlAccountMap_BalanceGroup", "\"BalanceGroup\" IN ('KAS-KASIR','KAS-KECIL','PIUTANG','UTANG-SUPPLIER','UTANG-JASA-MEDIS')");
                });

            migrationBuilder.CreateTable(
                name: "FinOpeningBalance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceGroup = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CutoverDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AccountingReferenceDocument = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_FinOpeningBalance", x => x.Id);
                    table.CheckConstraint("CK_FinOpeningBalance_ItemGroupZero", "\"BalanceGroup\" NOT IN ('PIUTANG','UTANG-SUPPLIER','UTANG-JASA-MEDIS') OR \"Amount\" = 0");
                    table.CheckConstraint("CK_FinOpeningBalance_Status", "\"Status\" IN ('DRAFT','APPROVED','LOCKED')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinOpeningBalance_BalanceGroup",
                schema: "public",
                table: "FinOpeningBalance",
                column: "BalanceGroup",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinOpeningBalance_CutoverDate",
                schema: "public",
                table: "FinOpeningBalance",
                column: "CutoverDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinOpeningBalance_Status",
                schema: "public",
                table: "FinOpeningBalance",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinSubledgerControlAccountMap_ControlAccountCode",
                schema: "public",
                table: "FinSubledgerControlAccountMap",
                column: "ControlAccountCode",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinSubledgerControlAccountMap_Group_Segment",
                schema: "public",
                table: "FinSubledgerControlAccountMap",
                columns: new[] { "BalanceGroup", "SegmentKey" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinSubledgerControlAccountMap_IsActive",
                schema: "public",
                table: "FinSubledgerControlAccountMap",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinOpeningBalance",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinSubledgerControlAccountMap",
                schema: "public");
        }
    }
}
