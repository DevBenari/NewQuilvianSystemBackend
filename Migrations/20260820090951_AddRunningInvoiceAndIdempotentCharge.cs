using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRunningInvoiceAndIdempotentCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilInvoice",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CurrentCalculationVersion = table.Column<int>(type: "integer", nullable: false),
                    InvoiceDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_BilInvoice", x => x.Id);
                    table.CheckConstraint("CK_BilInvoice_Status", "\"Status\" IN ('OPEN','FINAL','CLOSED','SETTLED_BY_WRITE_OFF')");
                });

            migrationBuilder.CreateTable(
                name: "BilNumberSeries",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResetPolicy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentValue = table.Column<long>(type: "bigint", nullable: false),
                    LastAllocatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_BilNumberSeries", x => x.Id);
                    table.CheckConstraint("CK_BilNumberSeries_CurrentValue", "\"CurrentValue\" > 0");
                    table.CheckConstraint("CK_BilNumberSeries_ResetPolicy", "\"ResetPolicy\" IN ('NEVER','YEARLY','MONTHLY','DAILY')");
                });

            migrationBuilder.CreateTable(
                name: "BilInvoiceItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDomain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceDetailId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceVersion = table.Column<long>(type: "bigint", nullable: false),
                    SourceContractVersion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceOccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DescriptionSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DoctorShare = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VoidReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastIdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
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
                    table.PrimaryKey("PK_BilInvoiceItem", x => x.Id);
                    table.CheckConstraint("CK_BilInvoiceItem_Amounts", "\"UnitPrice\" >= 0 AND \"DoctorShare\" >= 0 AND \"DoctorShare\" <= (\"Quantity\" * \"UnitPrice\")");
                    table.CheckConstraint("CK_BilInvoiceItem_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_BilInvoiceItem_SourceVersion", "\"SourceVersion\" > 0");
                    table.CheckConstraint("CK_BilInvoiceItem_Status", "\"Status\" IN ('ACTIVE','VOIDED')");
                    table.ForeignKey(
                        name: "FK_BilInvoiceItem_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilInvoiceItem_MstBillingItemCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "public",
                        principalTable: "MstBillingItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilChargeReceipt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDomain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceDetailId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_BilChargeReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilChargeReceipt_BilInvoiceItem_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalSchema: "public",
                        principalTable: "BilInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilChargeReceipt_IdempotencyKey",
                schema: "public",
                table: "BilChargeReceipt",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilChargeReceipt_InvoiceItemId",
                schema: "public",
                table: "BilChargeReceipt",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BilChargeReceipt_SourceDomain_SourceDetailId_ReceivedAt",
                schema: "public",
                table: "BilChargeReceipt",
                columns: new[] { "SourceDomain", "SourceDetailId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoice_EncounterId",
                schema: "public",
                table: "BilInvoice",
                column: "EncounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoice_InvoiceNumber",
                schema: "public",
                table: "BilInvoice",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItem_CategoryId",
                schema: "public",
                table: "BilInvoiceItem",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItem_InvoiceId_Status",
                schema: "public",
                table: "BilInvoiceItem",
                columns: new[] { "InvoiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItem_SourceDomain_SourceDetailId",
                schema: "public",
                table: "BilInvoiceItem",
                columns: new[] { "SourceDomain", "SourceDetailId" },
                unique: true,
                filter: "\"Status\" <> 'VOIDED' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilNumberSeries_SequenceKey_ScopeKey",
                schema: "public",
                table: "BilNumberSeries",
                columns: new[] { "SequenceKey", "ScopeKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilChargeReceipt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilNumberSeries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilInvoiceItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilInvoice",
                schema: "public");
        }
    }
}
