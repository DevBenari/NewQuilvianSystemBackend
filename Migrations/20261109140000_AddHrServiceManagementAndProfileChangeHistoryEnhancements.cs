using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddHrServiceManagementAndProfileChangeHistoryEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tambahkan kolom di MstEmployeeDocumentType
            migrationBuilder.AddColumn<string>(
                name: "NumberingPattern",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "[KODE]/RSQ-SDM/[BULAN_ROMAWI]/[TAHUN]/[NO_URUT]");

            migrationBuilder.AddColumn<int>(
                name: "LastSequenceNumber",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentYear",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "integer",
                nullable: false,
                defaultValue: 2026);

            migrationBuilder.AddColumn<string>(
                name: "TemplateBodyHtml",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportedTokensJson",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "text",
                nullable: true,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "ValidityPeriodDays",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "integer",
                nullable: false,
                defaultValue: 90);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinancialSensitive",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCbApproval",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedSignerTitle",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "Manajer Sumber Daya Manusia");

            // Tambahkan kolom di TrxHrServiceRequest
            migrationBuilder.AddColumn<string>(
                name: "PriorityTier",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Standar");

            migrationBuilder.AddColumn<int>(
                name: "SlaTargetHours",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "integer",
                nullable: false,
                defaultValue: 24);

            migrationBuilder.AddColumn<DateTime>(
                name: "SlaDeadlineUtc",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaStatus",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "WithinSla");

            migrationBuilder.AddColumn<DateTime>(
                name: "SlaPausedAtUtc",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPausedMinutes",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Tambahkan kolom di TrxEmployeeDocumentIssuance
            migrationBuilder.AddColumn<string>(
                name: "OfficialDocumentNumber",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SignerName",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerTitle",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerSignatureHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeImagePath",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinancialSensitive",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityPinHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryBrutoAmount",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryNettoAmount",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PayrollPayslipId",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuanceStatus",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "RevocationBapNumber",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EDossierDocumentId",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "uuid",
                nullable: true);

            // Buat tabel HrdProfileChangeHistory
            migrationBuilder.CreateTable(
                name: "HrdProfileChangeHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HrServiceRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentModified = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OldValuesJson = table.Column<string>(type: "jsonb", nullable: false),
                    NewValuesJson = table.Column<string>(type: "jsonb", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: Guid.Empty),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: Guid.Empty),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: Guid.Empty),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: Guid.Empty),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdProfileChangeHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdProfileChangeHistory_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProfileChangeHistory_MstWorkforceProfile_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProfileChangeHistory_TrxHrServiceRequest_HrServiceRequestId",
                        column: x => x.HrServiceRequestId,
                        principalSchema: "public",
                        principalTable: "TrxHrServiceRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HrdProfileChangeHistory_ApprovedByUserId",
                schema: "public",
                table: "HrdProfileChangeHistory",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProfileChangeHistory_EmployeeId",
                schema: "public",
                table: "HrdProfileChangeHistory",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProfileChangeHistory_EmployeeId_ComponentModified_Applied~",
                schema: "public",
                table: "HrdProfileChangeHistory",
                columns: new[] { "EmployeeId", "ComponentModified", "AppliedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdProfileChangeHistory_HrServiceRequestId",
                schema: "public",
                table: "HrdProfileChangeHistory",
                column: "HrServiceRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrdProfileChangeHistory",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "NumberingPattern",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "LastSequenceNumber",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "CurrentYear",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "TemplateBodyHtml",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "SupportedTokensJson",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "ValidityPeriodDays",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "IsFinancialSensitive",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "RequiresCbApproval",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "AuthorizedSignerTitle",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "PriorityTier",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "SlaTargetHours",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "SlaDeadlineUtc",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "SlaStatus",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "SlaPausedAtUtc",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "TotalPausedMinutes",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "OfficialDocumentNumber",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SignerName",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SignerTitle",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SignerSignatureHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "VerificationHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "QrCodeImagePath",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "IsFinancialSensitive",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SecurityPinHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SalaryBrutoAmount",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SalaryNettoAmount",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "PayrollPayslipId",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "IssuanceStatus",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "RevocationBapNumber",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "EDossierDocumentId",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");
        }
    }
}
