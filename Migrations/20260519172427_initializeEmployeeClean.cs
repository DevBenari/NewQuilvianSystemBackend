using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeEmployeeClean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmpAttendance_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropTable(
                name: "DctFeeProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DctLicense",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DctPracticeProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpBankAccount",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpInsuranceProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpOrganizationAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpPayrollProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTaxProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTransportAllowanceTransaction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExtUserContract",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExtUserDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTransportAllowancePolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTransportAllowanceProfile",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_IsActi~",
                schema: "public",
                table: "MstWorkforceRequirement");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_Requir~",
                schema: "public",
                table: "MstWorkforceRequirement");

            migrationBuilder.DropIndex(
                name: "IX_MstExternalUser_ExternalUserType",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstDoctor_DoctorType",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.DropColumn(
                name: "ExternalUserType",
                schema: "public",
                table: "MstExternalUser");

            migrationBuilder.DropColumn(
                name: "DoctorType",
                schema: "public",
                table: "MstDoctor");

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileRequired",
                schema: "public",
                table: "MstWorkforceRequirement",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TargetEntityName",
                schema: "public",
                table: "MstWorkforceRequirement",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstEmployee",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "WfpInsurance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsBpjsKesehatanEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BpjsKesehatanNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsBpjsKetenagakerjaanEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BpjsKetenagakerjaanNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPrivateInsuranceEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PrivateInsuranceProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrivateInsuranceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_WfpInsurance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpInsurance_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpPayroll",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Default"),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "BankTransfer"),
                    PrimaryBankAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    FixedAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    FixedDeduction = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsOvertimeEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPayrollActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_WfpPayroll", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpPayroll_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpPayroll_WfpBankAccount_PrimaryBankAccountId",
                        column: x => x.PrimaryBankAccountId,
                        principalSchema: "public",
                        principalTable: "WfpBankAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpTax",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    NpwpNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TaxStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "TK0"),
                    IsTaxed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TaxCalculationMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Gross"),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_WfpTax", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpTax_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpTransportAllowancePolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AllowanceMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultMonthlyAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DefaultDailyAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DefaultNightAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    NightStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    NightEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    RequireAttendance = table.Column<bool>(type: "boolean", nullable: false),
                    ExcludeIfAbsent = table.Column<bool>(type: "boolean", nullable: false),
                    ExcludeIfLeave = table.Column<bool>(type: "boolean", nullable: false),
                    ExcludeIfHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false),
                    IsPayrollComponent = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_WfpTransportAllowancePolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WfpTransportAllowance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportAllowancePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRegularTransportEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNightTransportEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllowanceMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "None"),
                    MonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DailyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    NightAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsProrated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPayrollComponent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_WfpTransportAllowance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpTransportAllowance_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpTransportAllowance_WfpTransportAllowancePolicy_Transport~",
                        column: x => x.TransportAllowancePolicyId,
                        principalSchema: "public",
                        principalTable: "WfpTransportAllowancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpTransportAllowanceTransaction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportAllowanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransportAllowancePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodYearMonth = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AllowanceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Regular"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsGeneratedFromAttendance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNightShift = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TransactionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    Notes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_WfpTransportAllowanceTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpTransportAllowanceTransaction_EmpAttendance_AttendanceId",
                        column: x => x.AttendanceId,
                        principalSchema: "public",
                        principalTable: "EmpAttendance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpTransportAllowanceTransaction_MstWorkforceProfile_Workfo~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpTransportAllowanceTransaction_WfpTransportAllowance_Tran~",
                        column: x => x.TransportAllowanceId,
                        principalSchema: "public",
                        principalTable: "WfpTransportAllowance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpTransportAllowanceTransaction_WfpTransportAllowancePolic~",
                        column: x => x.TransportAllowancePolicyId,
                        principalSchema: "public",
                        principalTable: "WfpTransportAllowancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_Requir~",
                schema: "public",
                table: "MstWorkforceRequirement",
                columns: new[] { "UserType", "RequirementCategory", "RequirementCode", "TargetEntityName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_Target~",
                schema: "public",
                table: "MstWorkforceRequirement",
                columns: new[] { "UserType", "RequirementCategory", "TargetEntityName", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee",
                column: "WorkforceProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_BpjsKesehatanNumber",
                schema: "public",
                table: "WfpInsurance",
                column: "BpjsKesehatanNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_BpjsKetenagakerjaanNumber",
                schema: "public",
                table: "WfpInsurance",
                column: "BpjsKetenagakerjaanNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_IsBpjsKesehatanEnabled_IsBpjsKetenagakerjaanEn~",
                schema: "public",
                table: "WfpInsurance",
                columns: new[] { "IsBpjsKesehatanEnabled", "IsBpjsKetenagakerjaanEnabled", "IsPrivateInsuranceEnabled", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpInsurance_WorkforceProfileId",
                schema: "public",
                table: "WfpInsurance",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPayroll_PayrollGroup_IsPayrollActive_IsActive_IsDelete",
                schema: "public",
                table: "WfpPayroll",
                columns: new[] { "PayrollGroup", "IsPayrollActive", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpPayroll_PrimaryBankAccountId",
                schema: "public",
                table: "WfpPayroll",
                column: "PrimaryBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPayroll_WorkforceProfileId",
                schema: "public",
                table: "WfpPayroll",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTax_NpwpNumber",
                schema: "public",
                table: "WfpTax",
                column: "NpwpNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTax_TaxStatus_IsTaxed_IsActive_IsDelete",
                schema: "public",
                table: "WfpTax",
                columns: new[] { "TaxStatus", "IsTaxed", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpTax_WorkforceProfileId",
                schema: "public",
                table: "WfpTax",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowance_AllowanceMode_IsEligible_IsActive_IsD~",
                schema: "public",
                table: "WfpTransportAllowance",
                columns: new[] { "AllowanceMode", "IsEligible", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowance_IsNightTransportEligible",
                schema: "public",
                table: "WfpTransportAllowance",
                column: "IsNightTransportEligible");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowance_TransportAllowancePolicyId",
                schema: "public",
                table: "WfpTransportAllowance",
                column: "TransportAllowancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowance_WorkforceProfileId",
                schema: "public",
                table: "WfpTransportAllowance",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowanceTransaction_AttendanceId",
                schema: "public",
                table: "WfpTransportAllowanceTransaction",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowanceTransaction_IsNightShift",
                schema: "public",
                table: "WfpTransportAllowanceTransaction",
                column: "IsNightShift");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowanceTransaction_TransactionDate_Transactio~",
                schema: "public",
                table: "WfpTransportAllowanceTransaction",
                columns: new[] { "TransactionDate", "TransactionStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowanceTransaction_TransportAllowanceId",
                schema: "public",
                table: "WfpTransportAllowanceTransaction",
                column: "TransportAllowanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowanceTransaction_TransportAllowancePolicyId",
                schema: "public",
                table: "WfpTransportAllowanceTransaction",
                column: "TransportAllowancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowanceTransaction_WorkforceProfileId",
                schema: "public",
                table: "WfpTransportAllowanceTransaction",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpTransportAllowanceTransaction_WorkforceProfileId_PeriodY~",
                schema: "public",
                table: "WfpTransportAllowanceTransaction",
                columns: new[] { "WorkforceProfileId", "PeriodYearMonth", "AllowanceType", "TransactionStatus", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmpAttendance_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmpAttendance_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropTable(
                name: "WfpInsurance",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpPayroll",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpTax",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpTransportAllowanceTransaction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpTransportAllowance",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpTransportAllowancePolicy",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_Requir~",
                schema: "public",
                table: "MstWorkforceRequirement");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_Target~",
                schema: "public",
                table: "MstWorkforceRequirement");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "IsProfileRequired",
                schema: "public",
                table: "MstWorkforceRequirement");

            migrationBuilder.DropColumn(
                name: "TargetEntityName",
                schema: "public",
                table: "MstWorkforceRequirement");

            migrationBuilder.AddColumn<int>(
                name: "ExternalUserType",
                schema: "public",
                table: "MstExternalUser",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "MstEmployee",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "DoctorType",
                schema: "public",
                table: "MstDoctor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DctFeeProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsultationFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DoctorSharePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    FeeCalculationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FollowUpFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFeeActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TelemedicineFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DctFeeProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DctFeeProfile_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DctLicense",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IssuedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LicenseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DctLicense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DctLicense_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DctPracticeProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowOnlineAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AllowTelemedicine = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllowWalkInAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DefaultConsultationDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MaxPatientPerSession = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PolyclinicName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PracticeNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DctPracticeProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DctPracticeProfile_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpBankAccount",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BankBranch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpBankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpBankAccount_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpDocument_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpInsuranceProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BpjsEmploymentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BpjsEmploymentRegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BpjsHealthNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BpjsHealthRegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsBpjsEmploymentActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBpjsHealthActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PrivateInsuranceEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrivateInsuranceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrivateInsuranceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrivateInsuranceStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpInsuranceProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpInsuranceProfile_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpOrganizationAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpOrganizationAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpOrganizationAssignment_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmpPayrollProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FixedAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FixedDeduction = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOvertimeEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPayrollActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MealAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OtherAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PayrollNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PositionAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SalaryType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TransportAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpPayrollProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpPayrollProfile_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpTaxProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPph21Active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTaxPaidByCompany = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PtkpStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaxNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaxRegisteredAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TaxRegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaxRegisteredName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpTaxProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpTaxProfile_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpTransportAllowancePolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowanceMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "DailyAttendance"),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DefaultDailyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DefaultMonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DefaultNightAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ExcludeIfAbsent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExcludeIfHoliday = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExcludeIfLeave = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    NightEndTime = table.Column<TimeSpan>(type: "time without time zone", nullable: true),
                    NightStartTime = table.Column<TimeSpan>(type: "time without time zone", nullable: true),
                    PolicyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RequireAttendance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpTransportAllowancePolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpTransportAllowanceProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowanceMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "None"),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DailyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNightTransportEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPayrollComponent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsProrated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    NightAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpTransportAllowanceProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpTransportAllowanceProfile_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtUserContract",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContractStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PaymentTerm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ScopeOfWork = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtUserContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtUserContract_MstExternalUser_ExternalUserId",
                        column: x => x.ExternalUserId,
                        principalSchema: "public",
                        principalTable: "MstExternalUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtUserDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtUserDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtUserDocument_MstExternalUser_ExternalUserId",
                        column: x => x.ExternalUserId,
                        principalSchema: "public",
                        principalTable: "MstExternalUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpTransportAllowanceTransaction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportAllowancePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransportAllowanceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllowanceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Daily"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    AttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsGeneratedFromAttendance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNightShift = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PeriodYearMonth = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    TransactionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpTransportAllowanceTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpTransportAllowanceTransaction_EmpTransportAllowancePolic~",
                        column: x => x.TransportAllowancePolicyId,
                        principalSchema: "public",
                        principalTable: "EmpTransportAllowancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpTransportAllowanceTransaction_EmpTransportAllowanceProfi~",
                        column: x => x.TransportAllowanceProfileId,
                        principalSchema: "public",
                        principalTable: "EmpTransportAllowanceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpTransportAllowanceTransaction_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_IsActi~",
                schema: "public",
                table: "MstWorkforceRequirement",
                columns: new[] { "UserType", "RequirementCategory", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkforceRequirement_UserType_RequirementCategory_Requir~",
                schema: "public",
                table: "MstWorkforceRequirement",
                columns: new[] { "UserType", "RequirementCategory", "RequirementCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstExternalUser_ExternalUserType",
                schema: "public",
                table: "MstExternalUser",
                column: "ExternalUserType");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_WorkforceProfileId",
                schema: "public",
                table: "MstEmployee",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctor_DoctorType",
                schema: "public",
                table: "MstDoctor",
                column: "DoctorType");

            migrationBuilder.CreateIndex(
                name: "IX_DctFeeProfile_DoctorId",
                schema: "public",
                table: "DctFeeProfile",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DctLicense_DoctorId",
                schema: "public",
                table: "DctLicense",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DctLicense_LicenseNumber",
                schema: "public",
                table: "DctLicense",
                column: "LicenseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DctLicense_LicenseType",
                schema: "public",
                table: "DctLicense",
                column: "LicenseType");

            migrationBuilder.CreateIndex(
                name: "IX_DctPracticeProfile_DoctorId",
                schema: "public",
                table: "DctPracticeProfile",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpBankAccount_AccountNumber",
                schema: "public",
                table: "EmpBankAccount",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpBankAccount_EmployeeId",
                schema: "public",
                table: "EmpBankAccount",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpDocument_DocumentNumber",
                schema: "public",
                table: "EmpDocument",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpDocument_DocumentType",
                schema: "public",
                table: "EmpDocument",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_EmpDocument_EmployeeId",
                schema: "public",
                table: "EmpDocument",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpInsuranceProfile_BpjsEmploymentNumber",
                schema: "public",
                table: "EmpInsuranceProfile",
                column: "BpjsEmploymentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpInsuranceProfile_BpjsHealthNumber",
                schema: "public",
                table: "EmpInsuranceProfile",
                column: "BpjsHealthNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpInsuranceProfile_EmployeeId",
                schema: "public",
                table: "EmpInsuranceProfile",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpOrganizationAssignment_DepartmentId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpOrganizationAssignment_EmployeeId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpOrganizationAssignment_PositionId",
                schema: "public",
                table: "EmpOrganizationAssignment",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpPayrollProfile_EmployeeId",
                schema: "public",
                table: "EmpPayrollProfile",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpPayrollProfile_PayrollNumber",
                schema: "public",
                table: "EmpPayrollProfile",
                column: "PayrollNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTaxProfile_EmployeeId",
                schema: "public",
                table: "EmpTaxProfile",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpTaxProfile_TaxNumber",
                schema: "public",
                table: "EmpTaxProfile",
                column: "TaxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowancePolicy_AllowanceMode",
                schema: "public",
                table: "EmpTransportAllowancePolicy",
                column: "AllowanceMode");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowancePolicy_IsActive_IsDelete",
                schema: "public",
                table: "EmpTransportAllowancePolicy",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowancePolicy_PolicyCode",
                schema: "public",
                table: "EmpTransportAllowancePolicy",
                column: "PolicyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowancePolicy_PolicyName",
                schema: "public",
                table: "EmpTransportAllowancePolicy",
                column: "PolicyName");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceProfile_AllowanceMode",
                schema: "public",
                table: "EmpTransportAllowanceProfile",
                column: "AllowanceMode");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceProfile_EffectiveStartDate_EffectiveEn~",
                schema: "public",
                table: "EmpTransportAllowanceProfile",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceProfile_EmployeeId",
                schema: "public",
                table: "EmpTransportAllowanceProfile",
                column: "EmployeeId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceProfile_IsEligible_IsNightTransportEli~",
                schema: "public",
                table: "EmpTransportAllowanceProfile",
                columns: new[] { "IsEligible", "IsNightTransportEligible", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_AttendanceId",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_EmployeeId_PeriodYearMonth~",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                columns: new[] { "EmployeeId", "PeriodYearMonth", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_EmployeeId_TransactionDate~",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                columns: new[] { "EmployeeId", "TransactionDate", "AllowanceType", "IsNightShift", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_PeriodYearMonth_Transactio~",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                columns: new[] { "PeriodYearMonth", "TransactionStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_ShiftId",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_TransactionDate_AllowanceT~",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                columns: new[] { "TransactionDate", "AllowanceType", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_TransportAllowancePolicyId",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                column: "TransportAllowancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpTransportAllowanceTransaction_TransportAllowanceProfileId",
                schema: "public",
                table: "EmpTransportAllowanceTransaction",
                column: "TransportAllowanceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserContract_ContractNumber",
                schema: "public",
                table: "ExtUserContract",
                column: "ContractNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserContract_ContractType",
                schema: "public",
                table: "ExtUserContract",
                column: "ContractType");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserContract_ExternalUserId",
                schema: "public",
                table: "ExtUserContract",
                column: "ExternalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserDocument_DocumentNumber",
                schema: "public",
                table: "ExtUserDocument",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserDocument_DocumentType",
                schema: "public",
                table: "ExtUserDocument",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_ExtUserDocument_ExternalUserId",
                schema: "public",
                table: "ExtUserDocument",
                column: "ExternalUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmpAttendance_MstWorkforceProfile_WorkforceProfileId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id");
        }
    }
}
