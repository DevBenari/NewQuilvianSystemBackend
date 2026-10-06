using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-074 (FR-FIN-154..?; FIN-DEC-126, 134, 139; FIN-DES-086, 087, 092; DDL R14.7, R14.8).
    /// Membuat dua tabel baru:
    /// 1. FinTransactionProof: metadata bukti pembayaran langsung (berkasnya sendiri di luar database)
    /// 2. MstDirectPaymentThreshold: ambang nilai pembayaran langsung, master berjejak satu baris aktif
    /// Murni tabel baru, belum dieksekusi ke database (otorisasi pembuatan via FIN-DEC-138).
    ///
    /// CATATAN CAKUPAN: kolom ProofId pada FinReceivableMovement/FinSupplierPayableMovement (sudah ada
    /// sejak BE-FIN-058, AddFinanceSubledgerMovementLedgers) SENGAJA TIDAK diberi foreign key ke tabel
    /// FinTransactionProof pada migration ini — menyesuaikan kedua tabel tersebut akan melanggar
    /// acceptance criteria BE-FIN-074 sendiri ("nol tabel berjalan disentuh"). Lihat laporan task
    /// BE-FIN-074 bagian 7 untuk rincian dan dampaknya.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceTransactionProofAndDirectPaymentThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinTransactionProof",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProofType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_FinTransactionProof", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstDirectPaymentThreshold",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_MstDirectPaymentThreshold", x => x.Id);
                    table.CheckConstraint("CK_MstDirectPaymentThreshold_Amount", "\"Amount\" > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinTransactionProof_ProofType",
                schema: "public",
                table: "FinTransactionProof",
                column: "ProofType");

            migrationBuilder.CreateIndex(
                name: "IX_FinTransactionProof_StoredFileName",
                schema: "public",
                table: "FinTransactionProof",
                column: "StoredFileName",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinTransactionProof_UploadedBy",
                schema: "public",
                table: "FinTransactionProof",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MstDirectPaymentThreshold_Active",
                schema: "public",
                table: "MstDirectPaymentThreshold",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinTransactionProof",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDirectPaymentThreshold",
                schema: "public");
        }
    }
}
