using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Migration aditif BE-MDF-008: Menyiapkan 5 tabel kalkulasi transaksi Medical Fee Management
    /// (MdfFeePeriod, MdfServiceFee, MdfServiceFeeDetail, MdfServiceFeeAdjustment, MdfUnresolvedService)
    /// beserta indeks, foreign key, dan check constraint sesuai blueprint MDF-DES-001, MDF-DES-010..014.
    /// </summary>
    /// <inheritdoc />
    public partial class AddMedicalFeeCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // 1. Tabel MdfFeePeriod (Aggregate root periode kalkulasi)
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "MdfFeePeriod",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CalculatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdfFeePeriod", x => x.Id);
                    table.CheckConstraint("CK_MdfFeePeriod_Maker", "\"VerifiedBy\" IS NULL OR \"ApprovedBy\" IS NULL OR \"VerifiedBy\" <> \"ApprovedBy\"");
                    table.CheckConstraint("CK_MdfFeePeriod_Range", "\"PeriodEnd\" >= \"PeriodStart\"");
                    table.CheckConstraint("CK_MdfFeePeriod_Status", "\"Status\" IN ('Open','Calculated','Verified','Approved','Closed')");
                });

            // =========================================================================
            // 2. Tabel MdfServiceFee (Aggregate root hasil jasa medis per penerima)
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "MdfServiceFee",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayeeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Doctor"),
                    PayeeReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AdjustmentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    FinalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Calculated"),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdfServiceFee", x => x.Id);
                    table.CheckConstraint("CK_MdfServiceFee_Gross", "\"GrossAmount\" >= 0");
                    table.CheckConstraint("CK_MdfServiceFee_Maker", "\"VerifiedBy\" IS NULL OR \"ApprovedBy\" IS NULL OR \"VerifiedBy\" <> \"ApprovedBy\"");
                    table.CheckConstraint("CK_MdfServiceFee_Status", "\"Status\" IN ('Calculated','Verified','Approved','HandedOff')");
                    table.CheckConstraint("CK_MdfServiceFee_Total", "\"FinalAmount\" = \"GrossAmount\" + \"AdjustmentAmount\"");
                    table.ForeignKey(
                        name: "FK_MdfServiceFee_MdfFeePeriod_PeriodId",
                        column: x => x.PeriodId,
                        principalSchema: "public",
                        principalTable: "MdfFeePeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // =========================================================================
            // 3. Tabel MdfServiceFeeDetail (Rincian per tindakan layanan medis)
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "MdfServiceFeeDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceFeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDomain = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceDetailId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SharingPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CalculatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SharingRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdfServiceFeeDetail", x => x.Id);
                    table.CheckConstraint("CK_MdfServiceFeeDetail_Base", "\"BaseAmount\" >= 0");
                    table.CheckConstraint("CK_MdfServiceFeeDetail_Percentage", "\"SharingPercentage\" >= 0 AND \"SharingPercentage\" <= 100");
                    table.CheckConstraint("CK_MdfServiceFeeDetail_Source", "\"SourceDomain\" IN ('OPERATING_ROOM','PROCEDURE','LABORATORY','ADHOC','ADHOC_CATALOG')");
                    table.ForeignKey(
                        name: "FK_MdfServiceFeeDetail_MdfServiceFee_ServiceFeeId",
                        column: x => x.ServiceFeeId,
                        principalSchema: "public",
                        principalTable: "MdfServiceFee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MdfServiceFeeDetail_MdfSharingRule_SharingRuleId",
                        column: x => x.SharingRuleId,
                        principalSchema: "public",
                        principalTable: "MdfSharingRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MdfServiceFeeDetail_MstMedicalFeeRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "public",
                        principalTable: "MstMedicalFeeRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MdfServiceFeeDetail_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // =========================================================================
            // 4. Tabel MdfServiceFeeAdjustment (Koreksi penyesuaian nominal jasa)
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "MdfServiceFeeAdjustment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ServiceFeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Addition"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdfServiceFeeAdjustment", x => x.Id);
                    table.CheckConstraint("CK_MdfServiceFeeAdjustment_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_MdfServiceFeeAdjustment_Direction", "\"Direction\" IN ('Addition','Deduction')");
                    table.CheckConstraint("CK_MdfServiceFeeAdjustment_Maker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
                    table.CheckConstraint("CK_MdfServiceFeeAdjustment_Status", "\"Status\" IN ('Pending','Approved','Rejected')");
                    table.ForeignKey(
                        name: "FK_MdfServiceFeeAdjustment_MdfServiceFee_ServiceFeeId",
                        column: x => x.ServiceFeeId,
                        principalSchema: "public",
                        principalTable: "MdfServiceFee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // =========================================================================
            // 5. Tabel MdfUnresolvedService (Daftar layanan belum dapat dihitung)
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "MdfUnresolvedService",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDomain = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceDetailId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReasonDetail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdfUnresolvedService", x => x.Id);
                    table.CheckConstraint("CK_MdfUnresolvedService_Reason", "\"Reason\" IN ('PERFORMER_MISSING','AGREEMENT_MISSING','RULE_MISSING','CONTRACT_EXPIRED','ROLE_UNMAPPED','SOURCE_UNSUPPORTED')");
                    table.CheckConstraint("CK_MdfUnresolvedService_Status", "\"Status\" IN ('Open','Resolved','Waived')");
                    table.ForeignKey(
                        name: "FK_MdfUnresolvedService_MdfFeePeriod_PeriodId",
                        column: x => x.PeriodId,
                        principalSchema: "public",
                        principalTable: "MdfFeePeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MdfUnresolvedService_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // =========================================================================
            // Indeks Tabel MdfFeePeriod
            // =========================================================================
            migrationBuilder.CreateIndex(
                name: "IX_MdfFeePeriod_PeriodCode",
                schema: "public",
                table: "MdfFeePeriod",
                column: "PeriodCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfFeePeriod_Status_PeriodStart",
                schema: "public",
                table: "MdfFeePeriod",
                columns: new[] { "Status", "PeriodStart" });

            // =========================================================================
            // Indeks Tabel MdfServiceFee
            // =========================================================================
            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFee_FeeNumber",
                schema: "public",
                table: "MdfServiceFee",
                column: "FeeNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFee_PeriodId_PayeeType_PayeeReferenceId",
                schema: "public",
                table: "MdfServiceFee",
                columns: new[] { "PeriodId", "PayeeType", "PayeeReferenceId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFee_PayeeType_PayeeReferenceId_Status",
                schema: "public",
                table: "MdfServiceFee",
                columns: new[] { "PayeeType", "PayeeReferenceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFee_PeriodId_Status",
                schema: "public",
                table: "MdfServiceFee",
                columns: new[] { "PeriodId", "Status" });

            // =========================================================================
            // Indeks Tabel MdfServiceFeeDetail
            // =========================================================================
            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFeeDetail_InvoiceItemId",
                schema: "public",
                table: "MdfServiceFeeDetail",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFeeDetail_RoleId",
                schema: "public",
                table: "MdfServiceFeeDetail",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFeeDetail_ServiceFeeId",
                schema: "public",
                table: "MdfServiceFeeDetail",
                column: "ServiceFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFeeDetail_SharingRuleId",
                schema: "public",
                table: "MdfServiceFeeDetail",
                column: "SharingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFeeDetail_SourceDomain_SourceDetailId",
                schema: "public",
                table: "MdfServiceFeeDetail",
                columns: new[] { "SourceDomain", "SourceDetailId" });

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFeeDetail_TariffId",
                schema: "public",
                table: "MdfServiceFeeDetail",
                column: "TariffId");

            // =========================================================================
            // Indeks Tabel MdfServiceFeeAdjustment
            // =========================================================================
            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFeeAdjustment_AdjustmentNumber",
                schema: "public",
                table: "MdfServiceFeeAdjustment",
                column: "AdjustmentNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfServiceFeeAdjustment_ServiceFeeId_Status",
                schema: "public",
                table: "MdfServiceFeeAdjustment",
                columns: new[] { "ServiceFeeId", "Status" });

            // =========================================================================
            // Indeks Tabel MdfUnresolvedService
            // =========================================================================
            migrationBuilder.CreateIndex(
                name: "IX_MdfUnresolvedService_PeriodId_SourceDomain_SourceDetailId",
                schema: "public",
                table: "MdfUnresolvedService",
                columns: new[] { "PeriodId", "SourceDomain", "SourceDetailId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfUnresolvedService_PeriodId_Status",
                schema: "public",
                table: "MdfUnresolvedService",
                columns: new[] { "PeriodId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MdfUnresolvedService_Reason",
                schema: "public",
                table: "MdfUnresolvedService",
                column: "Reason");

            migrationBuilder.CreateIndex(
                name: "IX_MdfUnresolvedService_TariffId",
                schema: "public",
                table: "MdfUnresolvedService",
                column: "TariffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MdfServiceFeeDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MdfServiceFeeAdjustment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MdfServiceFee",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MdfUnresolvedService",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MdfFeePeriod",
                schema: "public");
        }
    }
}
