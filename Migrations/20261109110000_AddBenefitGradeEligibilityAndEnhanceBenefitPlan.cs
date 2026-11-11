using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBenefitGradeEligibilityAndEnhanceBenefitPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tambah kolom spesifikasi paket 3-lapisan pada MstBenefitPlan (HRD-DEC-134)
            migrationBuilder.AddColumn<string>(
                name: "BenefitCategory",
                schema: "public",
                table: "MstBenefitPlan",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "BPJS_Mandatory");

            migrationBuilder.AddColumn<string>(
                name: "CoverageTier",
                schema: "public",
                table: "MstBenefitPlan",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Basic");

            migrationBuilder.AddColumn<decimal>(
                name: "HospitalDiscountPercentage",
                schema: "public",
                table: "MstBenefitPlan",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresBPJSPrimaryCOB",
                schema: "public",
                table: "MstBenefitPlan",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumServiceMonths",
                schema: "public",
                table: "MstBenefitPlan",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            // 2. Buat tabel matriks kelayakan benefit per grade kepegawaian (HrdBenefitGradeEligibility)
            migrationBuilder.CreateTable(
                name: "HrdBenefitGradeEligibility",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeLevelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmploymentTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Permanent"),
                    SpecificAnnualLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    InpatientRoomClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Kelas 1"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdBenefitGradeEligibility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdBenefitGradeEligibility_MstBenefitPlan_BenefitPlanId",
                        column: x => x.BenefitPlanId,
                        principalSchema: "public",
                        principalTable: "MstBenefitPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "uq_benefit_grade_plan",
                schema: "public",
                table: "HrdBenefitGradeEligibility",
                columns: new[] { "BenefitPlanId", "GradeLevelCode", "EmploymentTypeCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrdBenefitGradeEligibility",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "BenefitCategory",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "CoverageTier",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "HospitalDiscountPercentage",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "RequiresBPJSPrimaryCOB",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "MinimumServiceMonths",
                schema: "public",
                table: "MstBenefitPlan");
        }
    }
}
