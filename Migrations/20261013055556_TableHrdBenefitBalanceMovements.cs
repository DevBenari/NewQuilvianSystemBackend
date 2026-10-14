using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class TableHrdBenefitBalanceMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxEmployeeBenefitDependent_EmployeeBenefitEnrollmentId_Dep~",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "HrdBenefitBalanceMovement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeBenefitEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeBenefitDependentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BenefitPersonKind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MovementType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SourceHandoffKey = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReversesMovementId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReasonText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HrdBenefitBalanceMovement", x => x.Id);
                    table.CheckConstraint("CK_HrdBenefitBalanceMovement_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_HrdBenefitBalanceMovement_Balance", "((\"MovementType\" = 'USE' AND \"BalanceAfter\" = \"BalanceBefore\" - \"Amount\") OR (\"MovementType\" = 'RETURN' AND \"BalanceAfter\" = \"BalanceBefore\" + \"Amount\")) AND \"BalanceAfter\" >= 0");
                    table.CheckConstraint("CK_HrdBenefitBalanceMovement_Kind", "(\"BenefitPersonKind\" = 'ENROLLMENT' AND \"EmployeeBenefitDependentId\" IS NULL) OR (\"BenefitPersonKind\" = 'DEPENDENT' AND \"EmployeeBenefitDependentId\" IS NOT NULL)");
                    table.CheckConstraint("CK_HrdBenefitBalanceMovement_Reverses", "(\"MovementType\" = 'RETURN') = (\"ReversesMovementId\" IS NOT NULL)");
                    table.CheckConstraint("CK_HrdBenefitBalanceMovement_Type", "\"MovementType\" IN ('USE','RETURN')");
                    table.ForeignKey(
                        name: "FK_HrdBenefitBalanceMovement_HrdBenefitBalanceMovement_Reverse~",
                        column: x => x.ReversesMovementId,
                        principalSchema: "public",
                        principalTable: "HrdBenefitBalanceMovement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdBenefitBalanceMovement_TrxEmployeeBenefitDependent_Emplo~",
                        column: x => x.EmployeeBenefitDependentId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeBenefitDependent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdBenefitBalanceMovement_TrxEmployeeBenefitEnrollment_Empl~",
                        column: x => x.EmployeeBenefitEnrollmentId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeBenefitEnrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_TrxEmployeeBenefitEnrollment_OpenPerPlan",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment",
                columns: new[] { "WorkforceProfileId", "BenefitPlanId" },
                unique: true,
                filter: "\"EnrollmentStatus\" IN ('Draft','Submitted','Approved','Active') AND \"IsDelete\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxEmployeeBenefitEnrollment_Balance",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment",
                sql: "\"UsedAmount\" >= 0 AND \"RemainingAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxEmployeeBenefitEnrollment_MakerChecker",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment",
                sql: "\"ApprovedByUserId\" IS NULL OR \"SubmittedByUserId\" IS NULL OR \"ApprovedByUserId\" <> \"SubmittedByUserId\"");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeBenefitDependent_EmployeeBenefitEnrollmentId_Dep~",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                columns: new[] { "EmployeeBenefitEnrollmentId", "DependentName", "RelationshipType" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"DependentStatus\" IN ('Draft','Requested','Verified')");

            migrationBuilder.CreateIndex(
                name: "UX_TrxEmployeeBenefitDependent_OpenPerFamilyMember",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                columns: new[] { "EmployeeBenefitEnrollmentId", "FamilyMemberId" },
                unique: true,
                filter: "\"DependentStatus\" IN ('Draft','Requested','Verified') AND \"FamilyMemberId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxEmployeeBenefitDependent_Balance",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                sql: "\"UsedAmount\" >= 0 AND \"RemainingAmount\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_HrdBenefitBalanceMovement_Dependent",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                column: "EmployeeBenefitDependentId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdBenefitBalanceMovement_Enrollment_OccurredAt",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                columns: new[] { "EmployeeBenefitEnrollmentId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdBenefitBalanceMovement_InvoiceId",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "UX_HrdBenefitBalanceMovement_Key",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                columns: new[] { "SourceHandoffKey", "MovementType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_HrdBenefitBalanceMovement_Reverses",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                column: "ReversesMovementId",
                unique: true,
                filter: "\"ReversesMovementId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrdBenefitBalanceMovement",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "UX_TrxEmployeeBenefitEnrollment_OpenPerPlan",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxEmployeeBenefitEnrollment_Balance",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxEmployeeBenefitEnrollment_MakerChecker",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment");

            migrationBuilder.DropIndex(
                name: "IX_TrxEmployeeBenefitDependent_EmployeeBenefitEnrollmentId_Dep~",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropIndex(
                name: "UX_TrxEmployeeBenefitDependent_OpenPerFamilyMember",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxEmployeeBenefitDependent_Balance",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeBenefitDependent_EmployeeBenefitEnrollmentId_Dep~",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                columns: new[] { "EmployeeBenefitEnrollmentId", "DependentName", "RelationshipType" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
