using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalAssessmentPolicyMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstClinicalAssessmentPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AssessmentType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ServiceUnitType = table.Column<int>(type: "integer", nullable: true),
                    DueWithinMinutes = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_MstClinicalAssessmentPolicy", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_PolicyId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_MstClinicalAssessmentPolicy_AssessmentType_ServiceUnitType_~",
                schema: "public",
                table: "MstClinicalAssessmentPolicy",
                columns: new[] { "AssessmentType", "ServiceUnitType", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_MstClinicalAssessmentPolicy_IsActive_IsDelete",
                schema: "public",
                table: "MstClinicalAssessmentPolicy",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstClinicalAssessmentPolicy_PolicyCode",
                schema: "public",
                table: "MstClinicalAssessmentPolicy",
                column: "PolicyCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientAssessment_MstClinicalAssessmentPolicy_PolicyId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "PolicyId",
                principalSchema: "public",
                principalTable: "MstClinicalAssessmentPolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientAssessment_MstClinicalAssessmentPolicy_PolicyId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropTable(
                name: "MstClinicalAssessmentPolicy",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_PolicyId",
                schema: "public",
                table: "TrxPatientAssessment");
        }
    }
}
