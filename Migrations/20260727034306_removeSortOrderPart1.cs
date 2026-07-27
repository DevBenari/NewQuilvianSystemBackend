using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class removeSortOrderPart1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstTrainingCategory_SortOrder_TrainingCategoryName",
                schema: "public",
                table: "MstTrainingCategory");

            migrationBuilder.DropIndex(
                name: "IX_MstJobFamily_SortOrder_IsActive_IsDelete",
                schema: "public",
                table: "MstJobFamily");

            migrationBuilder.DropIndex(
                name: "IX_MstCompetency_SortOrder_CompetencyName",
                schema: "public",
                table: "MstCompetency");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstTrainingCategory");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstTrainingCatalog");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstSpecialization");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstShiftPattern");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstShiftGroup");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstShift");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstProfession");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstPositionCompetencyRequirement");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstOvertimePolicy");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstOrganizationUnit");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstOnCallType");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstMinimumRestPolicy");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstLicenseType");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstLeaveType");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstJobFamily");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstGracePeriodPolicy");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstCredentialingRequirement");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstCompetency");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstClinicalPrivilegeCatalog");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstCertificationType");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstAttendancePolicy");

            migrationBuilder.CreateIndex(
                name: "IX_MstTrainingCategory_CreateDateTime_TrainingCategoryName",
                schema: "public",
                table: "MstTrainingCategory",
                columns: new[] { "CreateDateTime", "TrainingCategoryName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstTrainingCategory_CreateDateTime_TrainingCategoryName",
                schema: "public",
                table: "MstTrainingCategory");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstTrainingCategory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstTrainingCatalog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstSpecialization",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstShiftPattern",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstShiftGroup",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstShift",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstProfession",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstPositionCompetencyRequirement",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstOvertimePolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstOrganizationUnit",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstOnCallType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstMinimumRestPolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstLicenseType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstLeaveType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstLeavePolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstJobFamily",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstGracePeriodPolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstCredentialingRequirement",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstCompetency",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstClinicalPrivilegeCatalog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstCertificationType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstAttendancePolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MstTrainingCategory_SortOrder_TrainingCategoryName",
                schema: "public",
                table: "MstTrainingCategory",
                columns: new[] { "SortOrder", "TrainingCategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstJobFamily_SortOrder_IsActive_IsDelete",
                schema: "public",
                table: "MstJobFamily",
                columns: new[] { "SortOrder", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCompetency_SortOrder_CompetencyName",
                schema: "public",
                table: "MstCompetency",
                columns: new[] { "SortOrder", "CompetencyName" });
        }
    }
}
