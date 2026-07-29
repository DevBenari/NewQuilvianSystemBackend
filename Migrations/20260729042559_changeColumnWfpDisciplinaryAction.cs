using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnWfpDisciplinaryAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ActionStatus",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "AccessClassification",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "HighlyRestricted",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<Guid>(
                name: "DisciplinaryActionTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeRelationCaseTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestReasonId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SanctionTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ViolationTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowDefinitionId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstDisciplinaryActionType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActionTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultActionLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DefaultEffectiveDays = table.Column<int>(type: "integer", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsAppeal = table.Column<bool>(type: "boolean", nullable: false),
                    IsConfidential = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_MstDisciplinaryActionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstEmployeeRelationCaseType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CaseTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CaseCategory = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequiresInvestigation = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresHearing = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultConfidential = table.Column<bool>(type: "boolean", nullable: false),
                    TargetResolutionDays = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_MstEmployeeRelationCaseType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstSanctionType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SanctionTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SanctionTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SanctionLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DefaultDurationDays = table.Column<int>(type: "integer", nullable: true),
                    IsFinalSanction = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsAppeal = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_MstSanctionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstViolationType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViolationTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ViolationTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ViolationCategory = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SeverityLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RequiresInvestigation = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_MstViolationType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_DisciplinaryActionTypeId_ViolationTyp~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                columns: new[] { "DisciplinaryActionTypeId", "ViolationTypeId", "SanctionTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_EmployeeRelationCaseTypeId_RequestRea~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                columns: new[] { "EmployeeRelationCaseTypeId", "RequestReasonId", "WorkflowDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_RequestReasonId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "RequestReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_SanctionTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "SanctionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_ViolationTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "ViolationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_WorkflowDefinitionId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDisciplinaryActionType_ActionTypeCode",
                schema: "public",
                table: "MstDisciplinaryActionType",
                column: "ActionTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDisciplinaryActionType_ActionTypeName",
                schema: "public",
                table: "MstDisciplinaryActionType",
                column: "ActionTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDisciplinaryActionType_IsActive_IsDelete_SortOrder",
                schema: "public",
                table: "MstDisciplinaryActionType",
                columns: new[] { "IsActive", "IsDelete", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployeeRelationCaseType_CaseTypeCode",
                schema: "public",
                table: "MstEmployeeRelationCaseType",
                column: "CaseTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployeeRelationCaseType_CaseTypeName",
                schema: "public",
                table: "MstEmployeeRelationCaseType",
                column: "CaseTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployeeRelationCaseType_IsActive_IsDelete_SortOrder",
                schema: "public",
                table: "MstEmployeeRelationCaseType",
                columns: new[] { "IsActive", "IsDelete", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstSanctionType_IsActive_IsDelete_SortOrder",
                schema: "public",
                table: "MstSanctionType",
                columns: new[] { "IsActive", "IsDelete", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstSanctionType_SanctionTypeCode",
                schema: "public",
                table: "MstSanctionType",
                column: "SanctionTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstSanctionType_SanctionTypeName",
                schema: "public",
                table: "MstSanctionType",
                column: "SanctionTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_MstViolationType_IsActive_IsDelete_SortOrder",
                schema: "public",
                table: "MstViolationType",
                columns: new[] { "IsActive", "IsDelete", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstViolationType_ViolationTypeCode",
                schema: "public",
                table: "MstViolationType",
                column: "ViolationTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstViolationType_ViolationTypeName",
                schema: "public",
                table: "MstViolationType",
                column: "ViolationTypeName");

            migrationBuilder.AddForeignKey(
                name: "FK_WfpDisciplinaryAction_MstDisciplinaryActionType_Disciplinar~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "DisciplinaryActionTypeId",
                principalSchema: "public",
                principalTable: "MstDisciplinaryActionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpDisciplinaryAction_MstEmployeeRelationCaseType_EmployeeR~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "EmployeeRelationCaseTypeId",
                principalSchema: "public",
                principalTable: "MstEmployeeRelationCaseType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpDisciplinaryAction_MstRequestReason_RequestReasonId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "RequestReasonId",
                principalSchema: "public",
                principalTable: "MstRequestReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpDisciplinaryAction_MstSanctionType_SanctionTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "SanctionTypeId",
                principalSchema: "public",
                principalTable: "MstSanctionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpDisciplinaryAction_MstViolationType_ViolationTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "ViolationTypeId",
                principalSchema: "public",
                principalTable: "MstViolationType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpDisciplinaryAction_MstWorkflowDefinition_WorkflowDefinit~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "WorkflowDefinitionId",
                principalSchema: "public",
                principalTable: "MstWorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WfpDisciplinaryAction_MstDisciplinaryActionType_Disciplinar~",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpDisciplinaryAction_MstEmployeeRelationCaseType_EmployeeR~",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpDisciplinaryAction_MstRequestReason_RequestReasonId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpDisciplinaryAction_MstSanctionType_SanctionTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpDisciplinaryAction_MstViolationType_ViolationTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpDisciplinaryAction_MstWorkflowDefinition_WorkflowDefinit~",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropTable(
                name: "MstDisciplinaryActionType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstEmployeeRelationCaseType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSanctionType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstViolationType",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_WfpDisciplinaryAction_DisciplinaryActionTypeId_ViolationTyp~",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropIndex(
                name: "IX_WfpDisciplinaryAction_EmployeeRelationCaseTypeId_RequestRea~",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropIndex(
                name: "IX_WfpDisciplinaryAction_RequestReasonId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropIndex(
                name: "IX_WfpDisciplinaryAction_SanctionTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropIndex(
                name: "IX_WfpDisciplinaryAction_ViolationTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropIndex(
                name: "IX_WfpDisciplinaryAction_WorkflowDefinitionId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropColumn(
                name: "DisciplinaryActionTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropColumn(
                name: "EmployeeRelationCaseTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropColumn(
                name: "RequestReasonId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropColumn(
                name: "SanctionTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropColumn(
                name: "ViolationTypeId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionId",
                schema: "public",
                table: "WfpDisciplinaryAction");

            migrationBuilder.AlterColumn<string>(
                name: "ActionStatus",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldDefaultValue: "Draft");

            migrationBuilder.AlterColumn<string>(
                name: "AccessClassification",
                schema: "public",
                table: "WfpDisciplinaryAction",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "HighlyRestricted");
        }
    }
}
