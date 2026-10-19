using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddReferredAdminCreditAndSupersededItemReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ReferredOutpatientAdminCreditAmount",
                schema: "public",
                table: "BilCalculationVersion",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferredOutpatientAdminUnappliedCreditAmount",
                schema: "public",
                table: "BilCalculationVersion",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
ALTER TABLE public.""BilCalculationVersion"" DROP CONSTRAINT IF EXISTS ""CK_BilCalculationVersion_Amounts"";
ALTER TABLE public.""BilCalculationVersion"" ADD CONSTRAINT ""CK_BilCalculationVersion_Amounts"" CHECK (""GrossAmount"" >= 0 AND ""AdministrationFeeAmount"" >= 0 AND ""RoomChargeAmount"" >= 0 AND ""ItemDiscount"" >= 0 AND ""TotalDiscount"" >= 0 AND ""TaxAmount"" >= 0 AND ""PatientAmount"" >= 0 AND ""PrimaryAmount"" >= 0 AND ""ExcessAmount"" >= 0 AND ""UnresolvedCoverageAmount"" >= 0 AND ""ReferredOutpatientAdminCreditAmount"" >= 0 AND ""ReferredOutpatientAdminUnappliedCreditAmount"" >= 0) NOT VALID;
ALTER TABLE public.""BilCalculationVersion"" VALIDATE CONSTRAINT ""CK_BilCalculationVersion_Amounts"";
");

            migrationBuilder.Sql(@"
ALTER TABLE public.""BilRefundableCredit"" DROP CONSTRAINT IF EXISTS ""CK_BilRefundableCredit_SourceType"";
ALTER TABLE public.""BilRefundableCredit"" ADD CONSTRAINT ""CK_BilRefundableCredit_SourceType"" CHECK (""SourceType"" IN ('ALLOCATION_EXCESS','SETTLEMENT','REFERRED_OUTPATIENT_ADMIN')) NOT VALID;
ALTER TABLE public.""BilRefundableCredit"" VALIDATE CONSTRAINT ""CK_BilRefundableCredit_SourceType"";
");

            migrationBuilder.CreateTable(
                name: "BilSupersededItemReview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    InvoiceStatusAtDecision = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReinstatedInvoiceItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
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
                    table.PrimaryKey("PK_BilSupersededItemReview", x => x.Id);
                    table.CheckConstraint("CK_BilSupersededItemReview_Decision", "\"Decision\" IN ('REINSTATE','KEEP','NEEDS_POST_CLOSE_CORRECTION')");
                    table.CheckConstraint("CK_BilSupersededItemReview_Reason", "length(trim(\"Reason\")) > 0");
                    table.CheckConstraint("CK_BilSupersededItemReview_ReinstatedItem", "(\"Decision\" = 'REINSTATE' AND \"ReinstatedInvoiceItemId\" IS NOT NULL) OR (\"Decision\" <> 'REINSTATE' AND \"ReinstatedInvoiceItemId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_BilSupersededItemReview_BilInvoiceItem_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalSchema: "public",
                        principalTable: "BilInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilSupersededItemReview_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilSupersededItemReview_BilInvoiceItem_ReinstatedInvoiceItemId",
                        column: x => x.ReinstatedInvoiceItemId,
                        principalSchema: "public",
                        principalTable: "BilInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilSupersededItemReview_Decision",
                schema: "public",
                table: "BilSupersededItemReview",
                column: "Decision");

            migrationBuilder.CreateIndex(
                name: "IX_BilSupersededItemReview_InvoiceId",
                schema: "public",
                table: "BilSupersededItemReview",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "UX_BilSupersededItemReview_IdempotencyKey",
                schema: "public",
                table: "BilSupersededItemReview",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BilSupersededItemReview_InvoiceItem",
                schema: "public",
                table: "BilSupersededItemReview",
                column: "InvoiceItemId",
                unique: true,
                filter: "NOT \"IsDelete\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilSupersededItemReview",
                schema: "public");

            // Catatan: Memulihkan constraint SourceType tanpa 'REFERRED_OUTPATIENT_ADMIN' akan gagal jika
            // sudah ada baris BilRefundableCredit bertipe REFERRED_OUTPATIENT_ADMIN di basis data.
            // Kegagalan ini sengaja untuk melindungi integritas audit finansial agar tidak terhapus diam-diam.
            migrationBuilder.Sql(@"
ALTER TABLE public.""BilRefundableCredit"" DROP CONSTRAINT IF EXISTS ""CK_BilRefundableCredit_SourceType"";
ALTER TABLE public.""BilRefundableCredit"" ADD CONSTRAINT ""CK_BilRefundableCredit_SourceType"" CHECK (""SourceType"" IN ('ALLOCATION_EXCESS','SETTLEMENT')) NOT VALID;
ALTER TABLE public.""BilRefundableCredit"" VALIDATE CONSTRAINT ""CK_BilRefundableCredit_SourceType"";
");

            migrationBuilder.Sql(@"
ALTER TABLE public.""BilCalculationVersion"" DROP CONSTRAINT IF EXISTS ""CK_BilCalculationVersion_Amounts"";
ALTER TABLE public.""BilCalculationVersion"" ADD CONSTRAINT ""CK_BilCalculationVersion_Amounts"" CHECK (""GrossAmount"" >= 0 AND ""AdministrationFeeAmount"" >= 0 AND ""RoomChargeAmount"" >= 0 AND ""ItemDiscount"" >= 0 AND ""TotalDiscount"" >= 0 AND ""TaxAmount"" >= 0 AND ""PatientAmount"" >= 0 AND ""PrimaryAmount"" >= 0 AND ""ExcessAmount"" >= 0 AND ""UnresolvedCoverageAmount"" >= 0) NOT VALID;
ALTER TABLE public.""BilCalculationVersion"" VALIDATE CONSTRAINT ""CK_BilCalculationVersion_Amounts"";
");

            migrationBuilder.DropColumn(
                name: "ReferredOutpatientAdminCreditAmount",
                schema: "public",
                table: "BilCalculationVersion");

            migrationBuilder.DropColumn(
                name: "ReferredOutpatientAdminUnappliedCreditAmount",
                schema: "public",
                table: "BilCalculationVersion");
        }
    }
}

