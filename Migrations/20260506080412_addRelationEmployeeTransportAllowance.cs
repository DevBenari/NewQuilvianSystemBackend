using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addRelationEmployeeTransportAllowance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpTransportAllowancePolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AllowanceMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "DailyAttendance"),
                    DefaultMonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DefaultDailyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    DefaultNightAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    NightStartTime = table.Column<TimeSpan>(type: "time without time zone", nullable: true),
                    NightEndTime = table.Column<TimeSpan>(type: "time without time zone", nullable: true),
                    RequireAttendance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExcludeIfAbsent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExcludeIfLeave = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExcludeIfHoliday = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_EmpTransportAllowancePolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpTransportAllowanceProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                name: "EmpTransportAllowanceTransaction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportAllowanceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransportAllowancePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodYearMonth = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AllowanceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Daily"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsGeneratedFromAttendance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNightShift = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
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
                name: "IX_MstEmployee_PrimaryDepartmentId_PrimaryPositionId_IsActive_~",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "PrimaryDepartmentId", "PrimaryPositionId", "IsActive", "IsDelete" });

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpTransportAllowanceTransaction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTransportAllowancePolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmpTransportAllowanceProfile",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_PrimaryDepartmentId_PrimaryPositionId_IsActive_~",
                schema: "public",
                table: "MstEmployee");
        }
    }
}
