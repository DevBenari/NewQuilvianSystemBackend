using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261106153000_AddRestitutionAndPayrollDeductionSliceB6Core")]
    public partial class AddRestitutionAndPayrollDeductionSliceB6Core : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tabel HrdRestitutionSetting
            migrationBuilder.CreateTable(
                name: "HrdRestitutionSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeLevelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BenefitCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnnualLimitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    MinimumServiceMonthsRequired = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxDeductionPercentageOfGross = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 50.00m),
                    AllowEmergencyCitoCashier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_HrdRestitutionSetting", x => x.Id);
                });

            // 2. Tabel TrxEmployeeRestitutionClaim
            migrationBuilder.CreateTable(
                name: "TrxEmployeeRestitutionClaim",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClaimAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    ReceiptAttachmentUrl = table.Column<string>(type: "text", nullable: false),
                    MedicalResumeAttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    ClaimStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    ReasonNotes = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    SupervisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupervisorApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HrReviewerId = table.Column<Guid>(type: "uuid", nullable: true),
                    HrReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinalApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCitoEmergency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_TrxEmployeeRestitutionClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeRestitutionClaim_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeRestitutionClaim_MstEmployee_SupervisorId",
                        column: x => x.SupervisorId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeRestitutionClaim_AspNetUsers_HrReviewerId",
                        column: x => x.HrReviewerId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeRestitutionClaim_AspNetUsers_FinalApproverId",
                        column: x => x.FinalApproverId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3. Tabel TrxPayrollDeductionSchedule
            migrationBuilder.CreateTable(
                name: "TrxPayrollDeductionSchedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false),
                    ExternalReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalLoanPrincipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyInstallmentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalInstallmentMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PaidInstallmentCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RemainingPrincipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduleStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    AutoCarryOverOnCapExceeded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_TrxPayrollDeductionSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollDeductionSchedule_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 4. Tabel TrxPayrollDeductionItem
            migrationBuilder.CreateTable(
                name: "TrxPayrollDeductionItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualDeductedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    DeferredAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    ExecutionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    DeferralReason = table.Column<string>(type: "text", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TrxPayrollDeductionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollDeductionItem_TrxPayrollRun_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollDeductionItem_TrxPayrollDeductionSchedule_DeductionScheduleId",
                        column: x => x.DeductionScheduleId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollDeductionSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollDeductionItem_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 5. Tabel TrxPayrollRestitution
            migrationBuilder.CreateTable(
                name: "TrxPayrollRestitution",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestitutionNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestitutionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AffectedPayrollRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    AffectedPayslipId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetPayrollRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    Justification = table.Column<string>(type: "text", nullable: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_TrxPayrollRestitution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_TrxEmployeeRestitutionClaim_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeRestitutionClaim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_TrxPayrollRun_AffectedPayrollRunId",
                        column: x => x.AffectedPayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_TrxPayrollPayslip_AffectedPayslipId",
                        column: x => x.AffectedPayslipId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollPayslip",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_TrxPayrollRun_TargetPayrollRunId",
                        column: x => x.TargetPayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 6. Tabel TrxCashierDisbursementVoucher
            migrationBuilder.CreateTable(
                name: "TrxCashierDisbursementVoucher",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SecurityTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VoucherStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Issued"),
                    CashedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CashedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CashierReceiptNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TrxCashierDisbursementVoucher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCashierDisbursementVoucher_TrxEmployeeRestitutionClaim_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeRestitutionClaim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCashierDisbursementVoucher_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCashierDisbursementVoucher_AspNetUsers_CashedByUserId",
                        column: x => x.CashedByUserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indeks-indeks
            migrationBuilder.CreateIndex(
                name: "uq_restitution_setting_grade_category",
                schema: "public",
                table: "HrdRestitutionSetting",
                columns: new[] { "GradeLevelCode", "BenefitCategory" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_employee_restitution_claim_number",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_restitution_claim_employee_date",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                columns: new[] { "EmployeeId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "idx_restitution_claim_status",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                column: "ClaimStatus");

            migrationBuilder.CreateIndex(
                name: "uq_payroll_deduction_schedule_number",
                schema: "public",
                table: "TrxPayrollDeductionSchedule",
                column: "ScheduleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_deduction_schedule_emp_priority",
                schema: "public",
                table: "TrxPayrollDeductionSchedule",
                columns: new[] { "EmployeeId", "PriorityLevel" });

            migrationBuilder.CreateIndex(
                name: "idx_deduction_schedule_status",
                schema: "public",
                table: "TrxPayrollDeductionSchedule",
                column: "ScheduleStatus");

            migrationBuilder.CreateIndex(
                name: "uq_deduction_item_run_schedule",
                schema: "public",
                table: "TrxPayrollDeductionItem",
                columns: new[] { "PayrollRunId", "DeductionScheduleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_deduction_item_emp_priority",
                schema: "public",
                table: "TrxPayrollDeductionItem",
                columns: new[] { "EmployeeId", "PriorityLevel" });

            migrationBuilder.CreateIndex(
                name: "idx_deduction_item_status",
                schema: "public",
                table: "TrxPayrollDeductionItem",
                column: "ExecutionStatus");

            migrationBuilder.CreateIndex(
                name: "uq_payroll_restitution_number",
                schema: "public",
                table: "TrxPayrollRestitution",
                column: "RestitutionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_payroll_restitution_emp_target",
                schema: "public",
                table: "TrxPayrollRestitution",
                columns: new[] { "EmployeeId", "TargetPayrollRunId" });

            migrationBuilder.CreateIndex(
                name: "idx_payroll_restitution_status",
                schema: "public",
                table: "TrxPayrollRestitution",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "uq_cashier_disbursement_voucher_number",
                schema: "public",
                table: "TrxCashierDisbursementVoucher",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_cashier_voucher_status_exp",
                schema: "public",
                table: "TrxCashierDisbursementVoucher",
                columns: new[] { "VoucherStatus", "ExpiredAt" });

            migrationBuilder.CreateIndex(
                name: "idx_cashier_voucher_claim",
                schema: "public",
                table: "TrxCashierDisbursementVoucher",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxCashierDisbursementVoucher",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollRestitution",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollDeductionItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollDeductionSchedule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmployeeRestitutionClaim",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdRestitutionSetting",
                schema: "public");
        }
    }
}
