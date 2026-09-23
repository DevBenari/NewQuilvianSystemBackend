using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-019: utang supplier input manual (FIN-DES-015 bagian supplier). FinPayableAdjustment
    /// dibuat dengan kolom MedicalServicePayableId sudah ada tapi TANPA foreign key constraint —
    /// FinMedicalServicePayable belum dibangun (BE-FIN-021, BLOCKED menunggu modul Medical Fee).
    /// Menambahkan FK-nya nanti adalah ALTER TABLE ADD CONSTRAINT aditif murni, bukan migrasi yang
    /// merusak baris yang sudah ada (kolom itu akan selalu NULL sampai BE-FIN-021 ada).
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceSupplierPayable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinSupplierPayable",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayableNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierInvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SupplierInvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AdjustedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentTermDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "OUTSTANDING"),
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
                    table.PrimaryKey("PK_FinSupplierPayable", x => x.Id);
                    table.CheckConstraint("CK_FinSupplierPayable_Status", "\"Status\" IN ('OUTSTANDING','PARTIAL','PAID','CANCELLED')");
                    table.CheckConstraint("CK_FinSupplierPayable_Outstanding", "\"OutstandingAmount\" >= 0");
                    table.CheckConstraint("CK_FinSupplierPayable_Balance", "\"OriginalAmount\" = \"OutstandingAmount\" + \"PaidAmount\" + \"AdjustedAmount\"");
                    table.ForeignKey(
                        name: "FK_FinSupplierPayable_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPayableAdjustment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PayableType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SupplierPayableId = table.Column<Guid>(type: "uuid", nullable: true),
                    MedicalServicePayableId = table.Column<Guid>(type: "uuid", nullable: true),
                    Direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "REQUESTED"),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinPayableAdjustment", x => x.Id);
                    table.CheckConstraint("CK_FinPayableAdjustment_PayableType", "\"PayableType\" IN ('SUPPLIER','MEDICAL_SERVICE')");
                    table.CheckConstraint("CK_FinPayableAdjustment_Direction", "\"Direction\" IN ('DEBIT','CREDIT')");
                    table.CheckConstraint("CK_FinPayableAdjustment_Status", "\"Status\" IN ('REQUESTED','APPROVED','REJECTED')");
                    table.CheckConstraint("CK_FinPayableAdjustment_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinPayableAdjustment_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
                    table.CheckConstraint("CK_FinPayableAdjustment_ExactlyOnePayable", "num_nonnulls(\"SupplierPayableId\", \"MedicalServicePayableId\") = 1");
                    table.CheckConstraint("CK_FinPayableAdjustment_PayableTypeMatch", "(\"PayableType\" = 'SUPPLIER' AND \"SupplierPayableId\" IS NOT NULL) OR (\"PayableType\" = 'MEDICAL_SERVICE' AND \"MedicalServicePayableId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_FinPayableAdjustment_FinSupplierPayable_SupplierPayableId",
                        column: x => x.SupplierPayableId,
                        principalSchema: "public",
                        principalTable: "FinSupplierPayable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinSupplierPayableItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayableId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 1m),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_FinSupplierPayableItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinSupplierPayableItem_FinSupplierPayable_PayableId",
                        column: x => x.PayableId,
                        principalSchema: "public",
                        principalTable: "FinSupplierPayable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayable_PayableNumber",
                schema: "public",
                table: "FinSupplierPayable",
                column: "PayableNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayable_Supplier_InvoiceNumber",
                schema: "public",
                table: "FinSupplierPayable",
                columns: new[] { "SupplierId", "SupplierInvoiceNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayable_Status_DueDate",
                schema: "public",
                table: "FinSupplierPayable",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinPayableAdjustment_Number",
                schema: "public",
                table: "FinPayableAdjustment",
                column: "AdjustmentNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayableAdjustment_SupplierPayableId",
                schema: "public",
                table: "FinPayableAdjustment",
                column: "SupplierPayableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableItem_PayableId",
                schema: "public",
                table: "FinSupplierPayableItem",
                column: "PayableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinPayableAdjustment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinSupplierPayableItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinSupplierPayable",
                schema: "public");
        }
    }
}
