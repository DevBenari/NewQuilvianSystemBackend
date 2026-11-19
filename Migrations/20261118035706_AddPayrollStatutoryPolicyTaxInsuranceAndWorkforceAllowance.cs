using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollStatutoryPolicyTaxInsuranceAndWorkforceAllowance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeInBpjsWageBase",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixedAllowance",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedManualAmount",
                schema: "public",
                table: "PrlTax",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculationMode",
                schema: "public",
                table: "PrlTax",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionBasis",
                schema: "public",
                table: "PrlTax",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExemptionReason",
                schema: "public",
                table: "PrlTax",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualAmountApprovalStatus",
                schema: "public",
                table: "PrlTax",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManualAmountDecidedAt",
                schema: "public",
                table: "PrlTax",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManualAmountDecidedByUserId",
                schema: "public",
                table: "PrlTax",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualAmountDecisionNote",
                schema: "public",
                table: "PrlTax",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManualAmountSubmittedAt",
                schema: "public",
                table: "PrlTax",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManualAmountSubmittedByUserId",
                schema: "public",
                table: "PrlTax",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolicyVersion",
                schema: "public",
                table: "PrlTax",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientTaxCategory",
                schema: "public",
                table: "PrlTax",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxArticle",
                schema: "public",
                table: "PrlTax",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TreatyBasis",
                schema: "public",
                table: "PrlTax",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TreatyRatePercent",
                schema: "public",
                table: "PrlTax",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BpjsEmploymentPrograms",
                schema: "public",
                table: "PrlInsurance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParticipationBasis",
                schema: "public",
                table: "PrlInsurance",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParticipationStatus",
                schema: "public",
                table: "PrlInsurance",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeInBpjsWageBase",
                schema: "public",
                table: "MstAllowanceType",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixedAllowance",
                schema: "public",
                table: "MstAllowanceType",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrlBpjsPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LegalBasis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PolicyStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    JkkRiskClass = table.Column<int>(type: "integer", nullable: true),
                    JkkRiskBasis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HealthCapAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    HealthEmployeeRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    HealthEmployerRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    JhtEmployeeRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    JhtEmployerRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    JpCapAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    JpEmployeeRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    JpEmployerRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    JkkRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    JkmRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrlBpjsPolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrlTaxPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LegalBasis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PolicyStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    PositionCostRatePercent = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    PositionCostMonthlyCap = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PositionCostAnnualCap = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NoNpwpSurchargePercent = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    PkpRoundingUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RoundTaxToWholeRupiah = table.Column<bool>(type: "boolean", nullable: false),
                    EmployerPremiumPrograms = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PensionDeductionPrograms = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NonEmployeeBasisPercent = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    NonEmployeeMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Pph26RatePercent = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    PtkpJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    TerRatesJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    BracketsJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrlTaxPolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrlWorkforceAllowance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowanceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollComponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CalculationMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "IDR"),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsProrated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MaximumAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowanceStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrlWorkforceAllowance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrlWorkforceAllowance_MstAllowanceType_AllowanceTypeId",
                        column: x => x.AllowanceTypeId,
                        principalSchema: "public",
                        principalTable: "MstAllowanceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrlWorkforceAllowance_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrlBpjsPolicy_PolicyStatus_EffectiveStartDate_EffectiveEndD~",
                schema: "public",
                table: "PrlBpjsPolicy",
                columns: new[] { "PolicyStatus", "EffectiveStartDate", "EffectiveEndDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_PrlBpjsPolicy_PolicyVersion",
                schema: "public",
                table: "PrlBpjsPolicy",
                column: "PolicyVersion",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrlTaxPolicy_PolicyStatus_EffectiveStartDate_EffectiveEndDa~",
                schema: "public",
                table: "PrlTaxPolicy",
                columns: new[] { "PolicyStatus", "EffectiveStartDate", "EffectiveEndDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_PrlTaxPolicy_PolicyVersion",
                schema: "public",
                table: "PrlTaxPolicy",
                column: "PolicyVersion",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrlWorkforceAllowance_AllowanceStatus_IsActive_EffectiveSta~",
                schema: "public",
                table: "PrlWorkforceAllowance",
                columns: new[] { "AllowanceStatus", "IsActive", "EffectiveStartDate", "EffectiveEndDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_PrlWorkforceAllowance_AllowanceTypeId",
                schema: "public",
                table: "PrlWorkforceAllowance",
                column: "AllowanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrlWorkforceAllowance_WorkforceProfileId_AllowanceTypeId_Is~",
                schema: "public",
                table: "PrlWorkforceAllowance",
                columns: new[] { "WorkforceProfileId", "AllowanceTypeId", "IsDelete" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrlBpjsPolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrlTaxPolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrlWorkforceAllowance",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "IncludeInBpjsWageBase",
                schema: "public",
                table: "PrlTransportAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "IsFixedAllowance",
                schema: "public",
                table: "PrlTransportAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "ApprovedManualAmount",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "CalculationMode",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "DecisionBasis",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "ExemptionReason",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "ManualAmountApprovalStatus",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "ManualAmountDecidedAt",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "ManualAmountDecidedByUserId",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "ManualAmountDecisionNote",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "ManualAmountSubmittedAt",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "ManualAmountSubmittedByUserId",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "PolicyVersion",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "RecipientTaxCategory",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "TaxArticle",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "TreatyBasis",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "TreatyRatePercent",
                schema: "public",
                table: "PrlTax");

            migrationBuilder.DropColumn(
                name: "BpjsEmploymentPrograms",
                schema: "public",
                table: "PrlInsurance");

            migrationBuilder.DropColumn(
                name: "ParticipationBasis",
                schema: "public",
                table: "PrlInsurance");

            migrationBuilder.DropColumn(
                name: "ParticipationStatus",
                schema: "public",
                table: "PrlInsurance");

            migrationBuilder.DropColumn(
                name: "IncludeInBpjsWageBase",
                schema: "public",
                table: "MstAllowanceType");

            migrationBuilder.DropColumn(
                name: "IsFixedAllowance",
                schema: "public",
                table: "MstAllowanceType");
        }
    }
}
