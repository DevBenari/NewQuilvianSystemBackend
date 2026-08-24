using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class renameEmployeeRecognitionToHrd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TrxEmployeeRecognition",
                schema: "public",
                newName: "HrdEmployeeRecognition",
                newSchema: "public");

            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_ApprovedByUserId", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_ApprovedByUserId");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_CostCenterId", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_CostCenterId");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_DepartmentId", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_DepartmentId");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_EmployeeId", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_EmployeeId");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_HospitalSiteId", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_HospitalSiteId");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_NominatedByUserId", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_NominatedByUserId");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_OrganizationAssignmentId", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_OrganizationAssignmentId");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_RecognitionNumber", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_RecognitionNumber");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_RecognitionStatus_RecognitionType", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_RecognitionStatus_RecognitionType");
            migrationBuilder.RenameIndex(name: "IX_TrxEmployeeRecognition_WorkforceProfileId_RecognitionDate", table: "HrdEmployeeRecognition", newName: "IX_HrdEmployeeRecognition_WorkforceProfileId_RecognitionDate");

            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"PK_TrxEmployeeRecognition\" TO \"PK_HrdEmployeeRecognition\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_TrxEmployeeRecognition_AspNetUsers_ApprovedByUserId\" TO \"FK_HrdEmployeeRecognition_AspNetUsers_ApprovedByUserId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_TrxEmployeeRecognition_AspNetUsers_NominatedByUserId\" TO \"FK_HrdEmployeeRecognition_AspNetUsers_NominatedByUserId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_TrxEmployeeRecognition_MstCostCenter_CostCenterId\" TO \"FK_HrdEmployeeRecognition_MstCostCenter_CostCenterId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_TrxEmployeeRecognition_MstDepartment_DepartmentId\" TO \"FK_HrdEmployeeRecognition_MstDepartment_DepartmentId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_TrxEmployeeRecognition_MstEmployee_EmployeeId\" TO \"FK_HrdEmployeeRecognition_MstEmployee_EmployeeId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_TrxEmployeeRecognition_MstHospitalSite_HospitalSiteId\" TO \"FK_HrdEmployeeRecognition_MstHospitalSite_HospitalSiteId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_TrxEmployeeRecognition_MstWorkforceProfile_WorkforceProfile~\" TO \"FK_HrdEmployeeRecognition_MstWorkforceProfile_WorkforceProfile~\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_TrxEmployeeRecognition_WfpOrganizationAssignment_Organizati~\" TO \"FK_HrdEmployeeRecognition_WfpOrganizationAssignment_Organizati~\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_HrdEmployeeRecognition_AspNetUsers_ApprovedByUserId\" TO \"FK_TrxEmployeeRecognition_AspNetUsers_ApprovedByUserId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_HrdEmployeeRecognition_AspNetUsers_NominatedByUserId\" TO \"FK_TrxEmployeeRecognition_AspNetUsers_NominatedByUserId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_HrdEmployeeRecognition_MstCostCenter_CostCenterId\" TO \"FK_TrxEmployeeRecognition_MstCostCenter_CostCenterId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_HrdEmployeeRecognition_MstDepartment_DepartmentId\" TO \"FK_TrxEmployeeRecognition_MstDepartment_DepartmentId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_HrdEmployeeRecognition_MstEmployee_EmployeeId\" TO \"FK_TrxEmployeeRecognition_MstEmployee_EmployeeId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_HrdEmployeeRecognition_MstHospitalSite_HospitalSiteId\" TO \"FK_TrxEmployeeRecognition_MstHospitalSite_HospitalSiteId\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_HrdEmployeeRecognition_MstWorkforceProfile_WorkforceProfile~\" TO \"FK_TrxEmployeeRecognition_MstWorkforceProfile_WorkforceProfile~\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"FK_HrdEmployeeRecognition_WfpOrganizationAssignment_Organizati~\" TO \"FK_TrxEmployeeRecognition_WfpOrganizationAssignment_Organizati~\";");
            migrationBuilder.Sql("ALTER TABLE public.\"HrdEmployeeRecognition\" RENAME CONSTRAINT \"PK_HrdEmployeeRecognition\" TO \"PK_TrxEmployeeRecognition\";");

            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_ApprovedByUserId", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_ApprovedByUserId");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_CostCenterId", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_CostCenterId");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_DepartmentId", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_DepartmentId");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_EmployeeId", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_EmployeeId");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_HospitalSiteId", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_HospitalSiteId");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_NominatedByUserId", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_NominatedByUserId");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_OrganizationAssignmentId", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_OrganizationAssignmentId");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_RecognitionNumber", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_RecognitionNumber");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_RecognitionStatus_RecognitionType", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_RecognitionStatus_RecognitionType");
            migrationBuilder.RenameIndex(name: "IX_HrdEmployeeRecognition_WorkforceProfileId_RecognitionDate", table: "HrdEmployeeRecognition", newName: "IX_TrxEmployeeRecognition_WorkforceProfileId_RecognitionDate");

            migrationBuilder.RenameTable(
                name: "HrdEmployeeRecognition",
                schema: "public",
                newName: "TrxEmployeeRecognition",
                newSchema: "public");
        }
    }
}
