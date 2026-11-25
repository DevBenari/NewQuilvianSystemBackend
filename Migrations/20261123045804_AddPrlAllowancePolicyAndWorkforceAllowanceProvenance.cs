using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPrlAllowancePolicyAndWorkforceAllowanceProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AllowancePolicyGradeRateId",
                schema: "public",
                table: "PrlWorkforceAllowance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AllowancePolicyId",
                schema: "public",
                table: "PrlWorkforceAllowance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentSource",
                schema: "public",
                table: "PrlWorkforceAllowance",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Direct");

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryGradeId",
                schema: "public",
                table: "PrlWorkforceAllowance",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrlAllowancePolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowanceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CalculationMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "IDR"),
                    PayrollComponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevisionNo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SupersedesPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PrlAllowancePolicy", x => x.Id);
                    table.CheckConstraint("CK_PrlAllowancePolicy_Dates", "\"EffectiveEndDate\" IS NULL OR \"EffectiveEndDate\" >= \"EffectiveStartDate\"");
                    table.CheckConstraint("CK_PrlAllowancePolicy_Method", "\"CalculationMethod\" IN ('FixedAmount','PercentageOfBaseSalary')");
                    table.CheckConstraint("CK_PrlAllowancePolicy_Revision", "\"RevisionNo\" >= 1");
                    table.ForeignKey(
                        name: "FK_PrlAllowancePolicy_AllowanceType",
                        column: x => x.AllowanceTypeId,
                        principalSchema: "public",
                        principalTable: "MstAllowanceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrlAllowancePolicy_HospitalSite",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrlAllowancePolicy_LegalEntity",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrlAllowancePolicy_PayrollComponent",
                        column: x => x.PayrollComponentId,
                        principalSchema: "public",
                        principalTable: "MstPayrollComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrlAllowancePolicy_Supersedes",
                        column: x => x.SupersedesPolicyId,
                        principalSchema: "public",
                        principalTable: "PrlAllowancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrlAllowancePolicyGradeRate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowancePolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Percentage = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false, defaultValue: 0m),
                    MaximumAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PrlAllowancePolicyGradeRate", x => x.Id);
                    table.CheckConstraint("CK_PrlAllowancePolicyGradeRate_Amounts", "\"Amount\" >= 0 AND \"Percentage\" >= 0 AND \"Percentage\" <= 100 AND (\"MaximumAmount\" IS NULL OR \"MaximumAmount\" > 0)");
                    table.CheckConstraint("CK_PrlAllowancePolicyGradeRate_Dates", "\"EffectiveEndDate\" IS NULL OR \"EffectiveEndDate\" >= \"EffectiveStartDate\"");
                    table.CheckConstraint("CK_PrlAllowancePolicyGradeRate_Value", "\"Amount\" > 0 OR \"Percentage\" > 0");
                    table.ForeignKey(
                        name: "FK_PrlAllowancePolicyGradeRate_Policy",
                        column: x => x.AllowancePolicyId,
                        principalSchema: "public",
                        principalTable: "PrlAllowancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrlAllowancePolicyGradeRate_SalaryGrade",
                        column: x => x.SalaryGradeId,
                        principalSchema: "public",
                        principalTable: "MstSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrlWorkforceAllowance_AllowancePolicyGradeRateId",
                schema: "public",
                table: "PrlWorkforceAllowance",
                column: "AllowancePolicyGradeRateId");

            migrationBuilder.CreateIndex(
                name: "IX_PrlWorkforceAllowance_AllowancePolicyId",
                schema: "public",
                table: "PrlWorkforceAllowance",
                column: "AllowancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PrlWorkforceAllowance_SalaryGradeId",
                schema: "public",
                table: "PrlWorkforceAllowance",
                column: "SalaryGradeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrlWorkforceAllowance_AssignmentSource",
                schema: "public",
                table: "PrlWorkforceAllowance",
                sql: "\"AssignmentSource\" IN ('Direct','Policy')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrlWorkforceAllowance_PolicySourceHasLinks",
                schema: "public",
                table: "PrlWorkforceAllowance",
                sql: "\"AssignmentSource\" <> 'Policy' OR (\"AllowancePolicyId\" IS NOT NULL AND \"AllowancePolicyGradeRateId\" IS NOT NULL AND \"SalaryGradeId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_PrlAllowancePolicy_AllowanceType_Active",
                schema: "public",
                table: "PrlAllowancePolicy",
                columns: new[] { "AllowanceTypeId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_PrlAllowancePolicy_HospitalSiteId",
                schema: "public",
                table: "PrlAllowancePolicy",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PrlAllowancePolicy_LegalEntityId",
                schema: "public",
                table: "PrlAllowancePolicy",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PrlAllowancePolicy_PayrollComponentId",
                schema: "public",
                table: "PrlAllowancePolicy",
                column: "PayrollComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrlAllowancePolicy_SupersedesPolicyId",
                schema: "public",
                table: "PrlAllowancePolicy",
                column: "SupersedesPolicyId");

            migrationBuilder.CreateIndex(
                name: "UX_PrlAllowancePolicy_PolicyCode",
                schema: "public",
                table: "PrlAllowancePolicy",
                column: "PolicyCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrlAllowancePolicyGradeRate_SalaryGrade_Active",
                schema: "public",
                table: "PrlAllowancePolicyGradeRate",
                columns: new[] { "SalaryGradeId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "UX_PrlAllowancePolicyGradeRate_Policy_Grade_Start",
                schema: "public",
                table: "PrlAllowancePolicyGradeRate",
                columns: new[] { "AllowancePolicyId", "SalaryGradeId", "EffectiveStartDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_PrlWorkforceAllowance_AllowancePolicy",
                schema: "public",
                table: "PrlWorkforceAllowance",
                column: "AllowancePolicyId",
                principalSchema: "public",
                principalTable: "PrlAllowancePolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrlWorkforceAllowance_AllowancePolicyGradeRate",
                schema: "public",
                table: "PrlWorkforceAllowance",
                column: "AllowancePolicyGradeRateId",
                principalSchema: "public",
                principalTable: "PrlAllowancePolicyGradeRate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrlWorkforceAllowance_SalaryGrade",
                schema: "public",
                table: "PrlWorkforceAllowance",
                column: "SalaryGradeId",
                principalSchema: "public",
                principalTable: "MstSalaryGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrlWorkforceAllowance_AllowancePolicy",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropForeignKey(
                name: "FK_PrlWorkforceAllowance_AllowancePolicyGradeRate",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropForeignKey(
                name: "FK_PrlWorkforceAllowance_SalaryGrade",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropTable(
                name: "PrlAllowancePolicyGradeRate",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrlAllowancePolicy",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_PrlWorkforceAllowance_AllowancePolicyGradeRateId",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropIndex(
                name: "IX_PrlWorkforceAllowance_AllowancePolicyId",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropIndex(
                name: "IX_PrlWorkforceAllowance_SalaryGradeId",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrlWorkforceAllowance_AssignmentSource",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrlWorkforceAllowance_PolicySourceHasLinks",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropColumn(
                name: "AllowancePolicyGradeRateId",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropColumn(
                name: "AllowancePolicyId",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropColumn(
                name: "AssignmentSource",
                schema: "public",
                table: "PrlWorkforceAllowance");

            migrationBuilder.DropColumn(
                name: "SalaryGradeId",
                schema: "public",
                table: "PrlWorkforceAllowance");
        }
    }
}
