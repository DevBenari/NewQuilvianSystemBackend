using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPrlOpeningTaxBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrlOpeningTaxBalance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxYear = table.Column<int>(type: "integer", nullable: false),
                    CoveredMonths = table.Column<List<int>>(type: "integer[]", nullable: false),
                    EmployerSource = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TaxableGross = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EmployerPremium = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PensionDeduction = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxWithheld = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrlOpeningTaxBalance", x => x.Id);
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_Amounts", "\"TaxableGross\" >= 0 AND \"EmployerPremium\" >= 0 AND \"PensionDeduction\" >= 0 AND \"TaxWithheld\" >= 0");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_ApprovedHasDecision", "\"Status\" <> 'APPROVED' OR (\"ApprovedAt\" IS NOT NULL AND \"ApprovedByUserId\" IS NOT NULL)");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_ApprovedHasSource", "\"Status\" <> 'APPROVED' OR length(btrim(coalesce(\"SourceReference\", ''))) > 0");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_CoveredMonths", "cardinality(\"CoveredMonths\") BETWEEN 1 AND 12 AND \"CoveredMonths\" <@ ARRAY[1,2,3,4,5,6,7,8,9,10,11,12]::integer[]");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_EmployerSource", "\"EmployerSource\" IN ('SAME_EMPLOYER_PRE_CUTOVER','PREVIOUS_EMPLOYER')");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_MakerChecker", "\"ApprovedByUserId\" IS NULL OR \"SubmittedByUserId\" IS NULL OR \"ApprovedByUserId\" <> \"SubmittedByUserId\"");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_RejectedHasDecision", "\"Status\" <> 'REJECTED' OR (\"RejectedAt\" IS NOT NULL AND \"RejectedByUserId\" IS NOT NULL AND length(btrim(coalesce(\"DecisionNote\", ''))) > 0)");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_Status", "\"Status\" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','SUPERSEDED')");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_TaxYear", "\"TaxYear\" BETWEEN 2000 AND 2100");
                    table.CheckConstraint("CK_PrlOpeningTaxBalance_Version", "\"Version\" >= 1");
                    table.ForeignKey(
                        name: "FK_PrlOpeningTaxBalance_WorkforceProfile",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrlOpeningTaxBalance_WorkforceProfileId_TaxYear_Status",
                schema: "public",
                table: "PrlOpeningTaxBalance",
                columns: new[] { "WorkforceProfileId", "TaxYear", "Status", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "UX_PrlOpeningTaxBalance_OneApprovedPerSource",
                schema: "public",
                table: "PrlOpeningTaxBalance",
                columns: new[] { "WorkforceProfileId", "TaxYear", "EmployerSource" },
                unique: true,
                filter: "\"Status\" = 'APPROVED' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_PrlOpeningTaxBalance_Profile_Year_Source_Version",
                schema: "public",
                table: "PrlOpeningTaxBalance",
                columns: new[] { "WorkforceProfileId", "TaxYear", "EmployerSource", "Version" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrlOpeningTaxBalance",
                schema: "public");
        }
    }
}
