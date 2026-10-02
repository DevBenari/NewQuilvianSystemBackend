using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalChargeInvoiceSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextDispatchAttemptAt",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciliationRequiredAt",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReconciliationResolutionNote",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciliationResolvedAt",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReconciliationResolvedByUserId",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceAdjustmentId",
                schema: "public",
                table: "BilProcessingEffect",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                schema: "public",
                table: "BilProcessingEffect",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceItemId",
                schema: "public",
                table: "BilProcessingEffect",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSourceDetailId",
                schema: "public",
                table: "BilProcessingEffect",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSourceDomain",
                schema: "public",
                table: "BilProcessingEffect",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceSyncAttemptCount",
                schema: "public",
                table: "BilProcessingEffect",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSyncErrorCode",
                schema: "public",
                table: "BilProcessingEffect",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSyncErrorMessage",
                schema: "public",
                table: "BilProcessingEffect",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceSyncNextAttemptAt",
                schema: "public",
                table: "BilProcessingEffect",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceSyncStatus",
                schema: "public",
                table: "BilProcessingEffect",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceSyncVersion",
                schema: "public",
                table: "BilProcessingEffect",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceSyncedAt",
                schema: "public",
                table: "BilProcessingEffect",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClinicalCancellation",
                schema: "public",
                table: "BilProcessingEffect",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReconciliationResolutionNote",
                schema: "public",
                table: "BilProcessingEffect",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciliationResolvedAt",
                schema: "public",
                table: "BilProcessingEffect",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReconciliationResolvedByUserId",
                schema: "public",
                table: "BilProcessingEffect",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstBillingSyncPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MaxAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    BaseDelaySeconds = table.Column<int>(type: "integer", nullable: false),
                    MaxDelaySeconds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstBillingSyncPolicy", x => x.Id);
                    table.CheckConstraint("CK_MstBillingSyncPolicy_BaseDelaySeconds", "\"BaseDelaySeconds\" BETWEEN 10 AND 3600");
                    table.CheckConstraint("CK_MstBillingSyncPolicy_MaxAttemptCount", "\"MaxAttemptCount\" BETWEEN 0 AND 20");
                    table.CheckConstraint("CK_MstBillingSyncPolicy_MaxDelaySeconds", "\"MaxDelaySeconds\" >= \"BaseDelaySeconds\" AND \"MaxDelaySeconds\" <= 86400");
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "MstBillingSyncPolicy",
                columns: new[] { "Id", "BaseDelaySeconds", "CancelBy", "CancelDateTime", "CreateBy", "CreateDateTime", "DeleteBy", "DeleteDateTime", "Description", "IsActive", "MaxAttemptCount", "MaxDelaySeconds", "PolicyCode", "PolicyName", "RowVersion", "UpdateBy", "UpdateDateTime" },
                values: new object[,]
                {
                    { new Guid("5d0c2b61-7f3a-4e8e-9b1c-2a6f0e3d9a01"), 60, new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, "Penyerahan fakta klinis yang hasilnya belum pasti dikirim ulang dengan identitas dan kunci yang sama.", true, 5, 3600, "FACT_DISPATCH", "Kirim ulang fakta klinis ke folio", new Guid("9b3e7c42-1d58-4f06-a2b7-6c1e8d4f0a11"), new Guid("00000000-0000-0000-0000-000000000000"), null },
                    { new Guid("5d0c2b61-7f3a-4e8e-9b1c-2a6f0e3d9a02"), 60, new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, "Efek folio yang gagal diteruskan ke invoice canonical dikirim ulang dengan kunci idempotency yang sama.", true, 5, 3600, "INVOICE_SYNC", "Kirim ulang efek folio ke invoice", new Guid("9b3e7c42-1d58-4f06-a2b7-6c1e8d4f0a12"), new Guid("00000000-0000-0000-0000-000000000000"), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CliClinicalMilestoneFact_DispatchStatus_NextDispatchAttempt~",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                columns: new[] { "DispatchStatus", "NextDispatchAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilProcessingEffect_InvoiceAdjustmentId",
                schema: "public",
                table: "BilProcessingEffect",
                column: "InvoiceAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BilProcessingEffect_InvoiceId",
                schema: "public",
                table: "BilProcessingEffect",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BilProcessingEffect_InvoiceItemId",
                schema: "public",
                table: "BilProcessingEffect",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BilProcessingEffect_InvoiceSyncStatus_InvoiceSyncNextAttemp~",
                schema: "public",
                table: "BilProcessingEffect",
                columns: new[] { "InvoiceSyncStatus", "InvoiceSyncNextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingSyncPolicy_PolicyCode",
                schema: "public",
                table: "MstBillingSyncPolicy",
                column: "PolicyCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_BilProcessingEffect_BilAdjustment_InvoiceAdjustmentId",
                schema: "public",
                table: "BilProcessingEffect",
                column: "InvoiceAdjustmentId",
                principalSchema: "public",
                principalTable: "BilAdjustment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BilProcessingEffect_BilInvoiceItem_InvoiceItemId",
                schema: "public",
                table: "BilProcessingEffect",
                column: "InvoiceItemId",
                principalSchema: "public",
                principalTable: "BilInvoiceItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BilProcessingEffect_BilInvoice_InvoiceId",
                schema: "public",
                table: "BilProcessingEffect",
                column: "InvoiceId",
                principalSchema: "public",
                principalTable: "BilInvoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // RJ-E2E-DEC-014. Efek folio yang terbentuk sebelum jembatan folio → invoice ada
            // ditandai perlu rekonsiliasi, bukan diteruskan otomatis: kasir mungkin sudah
            // menagihnya manual, dan meneruskannya akan menagih dua kali. Hanya efek yang benar-
            // benar menghasilkan folio (Outcome Succeeded = 3, PartialOutcome = 5) pada kunjungan
            // Rawat Jalan (EncounterType Outpatient = 1) dari lima konteks cakupan V2.
            // Efek lain tetap NotApplicable (0) dari nilai bawaan kolom.
            migrationBuilder.Sql(@"
UPDATE public.""BilProcessingEffect"" AS e
SET ""InvoiceSyncStatus"" = 4,
    ""InvoiceSyncErrorCode"" = 'LEGACY_PRE_BRIDGE',
    ""InvoiceSyncErrorMessage"" = 'Pelayanan ini tercatat sebelum penagihan otomatis aktif. Periksa apakah sudah ditagih manual.'
FROM public.""BilFolio"" AS f
JOIN public.""RegPatientEncounter"" AS r ON r.""Id"" = f.""EncounterId""
WHERE e.""FolioId"" = f.""Id""
  AND e.""IsDelete"" = false
  AND e.""Outcome"" IN (3, 5)
  AND r.""EncounterType"" = 1
  AND e.""SourceContext"" IN ('Procedure', 'Laboratory', 'Radiology', 'Prescription', 'Consultation');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BilProcessingEffect_BilAdjustment_InvoiceAdjustmentId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropForeignKey(
                name: "FK_BilProcessingEffect_BilInvoiceItem_InvoiceItemId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropForeignKey(
                name: "FK_BilProcessingEffect_BilInvoice_InvoiceId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropTable(
                name: "MstBillingSyncPolicy",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_CliClinicalMilestoneFact_DispatchStatus_NextDispatchAttempt~",
                schema: "public",
                table: "CliClinicalMilestoneFact");

            migrationBuilder.DropIndex(
                name: "IX_BilProcessingEffect_InvoiceAdjustmentId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropIndex(
                name: "IX_BilProcessingEffect_InvoiceId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropIndex(
                name: "IX_BilProcessingEffect_InvoiceItemId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropIndex(
                name: "IX_BilProcessingEffect_InvoiceSyncStatus_InvoiceSyncNextAttemp~",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "NextDispatchAttemptAt",
                schema: "public",
                table: "CliClinicalMilestoneFact");

            migrationBuilder.DropColumn(
                name: "ReconciliationRequiredAt",
                schema: "public",
                table: "CliClinicalMilestoneFact");

            migrationBuilder.DropColumn(
                name: "ReconciliationResolutionNote",
                schema: "public",
                table: "CliClinicalMilestoneFact");

            migrationBuilder.DropColumn(
                name: "ReconciliationResolvedAt",
                schema: "public",
                table: "CliClinicalMilestoneFact");

            migrationBuilder.DropColumn(
                name: "ReconciliationResolvedByUserId",
                schema: "public",
                table: "CliClinicalMilestoneFact");

            migrationBuilder.DropColumn(
                name: "InvoiceAdjustmentId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceItemId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSourceDetailId",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSourceDomain",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSyncAttemptCount",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSyncErrorCode",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSyncErrorMessage",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSyncNextAttemptAt",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSyncStatus",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSyncVersion",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "InvoiceSyncedAt",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "IsClinicalCancellation",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "ReconciliationResolutionNote",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "ReconciliationResolvedAt",
                schema: "public",
                table: "BilProcessingEffect");

            migrationBuilder.DropColumn(
                name: "ReconciliationResolvedByUserId",
                schema: "public",
                table: "BilProcessingEffect");
        }
    }
}
