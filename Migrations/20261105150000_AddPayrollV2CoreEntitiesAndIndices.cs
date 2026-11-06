using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollV2CoreEntitiesAndIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tabel HrdPayrollSetting
            migrationBuilder.CreateTable(
                name: "HrdPayrollSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    SettingName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BpjsKesehatanCapAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 12000000m),
                    BpjsKesehatanEmployeeRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.00m),
                    BpjsKesehatanEmployerRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 4.00m),
                    BpjsKetenagakerjaanJpCapAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 10042300m),
                    BpjsKetenagakerjaanJpEmployeeRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.00m),
                    BpjsKetenagakerjaanJpEmployerRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 2.00m),
                    BpjsKetenagakerjaanJhtEmployeeRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 2.00m),
                    BpjsKetenagakerjaanJhtEmployerRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 3.70m),
                    BpjsKetenagakerjaanJkkRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0.89m),
                    BpjsKetenagakerjaanJkmRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0.30m),
                    VarianceThresholdPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 20.00m),
                    VarianceThresholdAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 2000000m),
                    MonthlyCutoffDay = table.Column<int>(type: "integer", nullable: false, defaultValue: 20),
                    DefaultPaymentDay = table.Column<int>(type: "integer", nullable: false, defaultValue: 25),
                    DefaultNightShiftAllowanceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 50000m),
                    MaxLoanDeductionPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 50.00m),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdPayrollSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdPayrollSetting_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 2. Tabel TrxPayrollVarianceAlert
            migrationBuilder.CreateTable(
                name: "TrxPayrollVarianceAlert",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PreviousNetPay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentNetPay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VarianceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VariancePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AlertSeverity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Warning"),
                    AlertReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ResolutionStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PendingReview"),
                    JustificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPayrollVarianceAlert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollVarianceAlert_TrxPayrollRun_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrxPayrollVarianceAlert_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollVarianceAlert_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3. Tabel TrxPayrollHandoffBatch
            migrationBuilder.CreateTable(
                name: "TrxPayrollHandoffBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    TotalEmployees = table.Column<int>(type: "integer", nullable: false),
                    TotalDisbursementAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChecksumSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HandoffStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "PendingTransfer"),
                    TransferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransferredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinanceReferenceNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FinanceNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PayloadSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPayrollHandoffBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollHandoffBatch_TrxPayrollRun_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollHandoffBatch_AspNetUsers_TransferredByUserId",
                        column: x => x.TransferredByUserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indeks Performa
            migrationBuilder.CreateIndex(
                name: "idx_hrd_payroll_setting_site",
                schema: "public",
                table: "HrdPayrollSetting",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "idx_payroll_variance_run_emp",
                schema: "public",
                table: "TrxPayrollVarianceAlert",
                columns: new[] { "PayrollRunId", "WorkforceProfileId" });

            migrationBuilder.CreateIndex(
                name: "idx_payroll_variance_status",
                schema: "public",
                table: "TrxPayrollVarianceAlert",
                column: "ResolutionStatus");

            migrationBuilder.CreateIndex(
                name: "idx_payroll_handoff_batch_number",
                schema: "public",
                table: "TrxPayrollHandoffBatch",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_payroll_handoff_period",
                schema: "public",
                table: "TrxPayrollHandoffBatch",
                columns: new[] { "PeriodYear", "PeriodMonth" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxPayrollHandoffBatch",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollVarianceAlert",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPayrollSetting",
                schema: "public");
        }
    }
}
