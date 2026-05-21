using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddWorkforceClinicalPrivilege : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpClinicalPrivilege",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CredentialLicenseId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrivilegeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrivilegeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrivilegeType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ClinicalScope = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SpecialtyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SubSpecialtyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProcedureGroup = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProcedureName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PracticeLocation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    PrivilegeStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsTemporary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEmergencyPrivilege = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSupervisionRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SupervisorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GrantNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SuspendedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SuspendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspensionReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LastReviewDate = table.Column<DateTime>(type: "date", nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "date", nullable: true),
                    SupportingFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SupportingFileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_WfpClinicalPrivilege", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_AspNetUsers_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_AspNetUsers_RevokedByUserId",
                        column: x => x.RevokedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_AspNetUsers_SupervisorUserId",
                        column: x => x.SupervisorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_AspNetUsers_SuspendedByUserId",
                        column: x => x.SuspendedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpClinicalPrivilege_WfpCredentialLicense_CredentialLicense~",
                        column: x => x.CredentialLicenseId,
                        principalSchema: "public",
                        principalTable: "WfpCredentialLicense",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_CredentialLicenseId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "CredentialLicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_CredentialLicenseId_PrivilegeStatus_Is~",
                schema: "public",
                table: "WfpClinicalPrivilege",
                columns: new[] { "CredentialLicenseId", "PrivilegeStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_DepartmentId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_EffectiveEndDate_IsActive_IsDelete",
                schema: "public",
                table: "WfpClinicalPrivilege",
                columns: new[] { "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_EffectiveStartDate_EffectiveEndDate",
                schema: "public",
                table: "WfpClinicalPrivilege",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_GrantedByUserId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_PositionId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_RejectedByUserId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_RevokedByUserId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_SupervisorUserId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "SupervisorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_SuspendedByUserId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "SuspendedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_WorkforceProfileId",
                schema: "public",
                table: "WfpClinicalPrivilege",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_WorkforceProfileId_DepartmentId_Positi~",
                schema: "public",
                table: "WfpClinicalPrivilege",
                columns: new[] { "WorkforceProfileId", "DepartmentId", "PositionId", "PrivilegeStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_WorkforceProfileId_PrivilegeCode",
                schema: "public",
                table: "WfpClinicalPrivilege",
                columns: new[] { "WorkforceProfileId", "PrivilegeCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_WorkforceProfileId_PrivilegeStatus_IsA~",
                schema: "public",
                table: "WfpClinicalPrivilege",
                columns: new[] { "WorkforceProfileId", "PrivilegeStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpClinicalPrivilege_WorkforceProfileId_PrivilegeType_Privi~",
                schema: "public",
                table: "WfpClinicalPrivilege",
                columns: new[] { "WorkforceProfileId", "PrivilegeType", "PrivilegeStatus" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpClinicalPrivilege",
                schema: "public");
        }
    }
}
