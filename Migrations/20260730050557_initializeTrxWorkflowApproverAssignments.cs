using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeTrxWorkflowApproverAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxWorkflowStepInstance_AspNetUsers_AssignedApproverUserId",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxWorkflowStepInstance_MstWorkforceProfile_AssignedApprove~",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowStepInstance_ApprovalMatrixId",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowStepInstance_AssignedApproverUserId_StepStatus_D~",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowStepInstance_AssignedApproverWorkforceProfileId_~",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowStepInstance_WorkflowInstanceId_StepOrder",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowComment_CommentByUserId",
                schema: "public",
                table: "TrxWorkflowComment");

            migrationBuilder.DropIndex(
                name: "IX_TrxApprovalDelegation_ApprovalDelegationPolicyId",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropIndex(
                name: "IX_TrxApprovalDelegation_WorkflowDefinitionId_WorkflowStepId",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropColumn(
                name: "AssignedApproverRoleCode",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropColumn(
                name: "AssignedApproverUserId",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropColumn(
                name: "AssignedApproverWorkforceProfileId",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.AlterColumn<string>(
                name: "StepTypeSnapshot",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Approval",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "StepStatus",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "ApproverSourceSnapshot",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "RequesterManager",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalModeSnapshot",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Any",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "TotalAssignmentCount",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CommentType",
                schema: "public",
                table: "TrxWorkflowComment",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "General",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "DelegationStatus",
                schema: "public",
                table: "TrxApprovalDelegation",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<Guid>(
                name: "ActualActionByUserId",
                schema: "public",
                table: "TrxApprovalAction",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Approve",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "ActionSource",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Web",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "ActionReasonCodeSnapshot",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActionReasonId",
                schema: "public",
                table: "TrxApprovalAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActionReasonNameSnapshot",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActionReasonType",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DelegatedFromWorkforceProfileId",
                schema: "public",
                table: "TrxApprovalAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowApproverAssignmentId",
                schema: "public",
                table: "TrxApprovalAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrxWorkflowApproverAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowStepInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovalMatrixId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalDelegationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedApproverUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedApproverWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalApproverWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedApproverRoleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApproverSourceSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "RequesterManager"),
                    AssignmentOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AssignmentStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "Pending"),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DelegatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCurrentAssignment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelegated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ResolutionSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_TrxWorkflowApproverAssignment", x => x.Id);
                    table.CheckConstraint("CK_TrxWorkflowApproverAssignment_AssignmentOrder", "\"AssignmentOrder\" > 0");
                    table.CheckConstraint("CK_TrxWorkflowApproverAssignment_CompletedAt", "\"CompletedAt\" IS NULL OR \"CompletedAt\" >= \"AssignedAt\"");
                    table.ForeignKey(
                        name: "FK_TrxWorkflowApproverAssignment_AspNetUsers_AssignedApproverU~",
                        column: x => x.AssignedApproverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxWorkflowApproverAssignment_AspNetUsers_OriginalApproverU~",
                        column: x => x.OriginalApproverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxWorkflowApproverAssignment_MstApprovalMatrix_ApprovalMat~",
                        column: x => x.ApprovalMatrixId,
                        principalSchema: "public",
                        principalTable: "MstApprovalMatrix",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxWorkflowApproverAssignment_MstWorkforceProfile_AssignedA~",
                        column: x => x.AssignedApproverWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxWorkflowApproverAssignment_MstWorkforceProfile_OriginalA~",
                        column: x => x.OriginalApproverWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxWorkflowApproverAssignment_TrxApprovalDelegation_Approva~",
                        column: x => x.ApprovalDelegationId,
                        principalSchema: "public",
                        principalTable: "TrxApprovalDelegation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxWorkflowApproverAssignment_TrxWorkflowInstance_WorkflowI~",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "public",
                        principalTable: "TrxWorkflowInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxWorkflowApproverAssignment_TrxWorkflowStepInstance_Workf~",
                        column: x => x.WorkflowStepInstanceId,
                        principalSchema: "public",
                        principalTable: "TrxWorkflowStepInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowStepInstance_ApprovalMatrixId_StepStatus",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                columns: new[] { "ApprovalMatrixId", "StepStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowStepInstance_DueAt",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                column: "DueAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowStepInstance_WorkflowInstanceId_StepCodeSnapshot",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                columns: new[] { "WorkflowInstanceId", "StepCodeSnapshot" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowStepInstance_WorkflowInstanceId_StepOrder_IsActi~",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                columns: new[] { "WorkflowInstanceId", "StepOrder", "IsActive", "IsDelete" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxWorkflowStepInstance_ActionCounters",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                sql: "\"TotalAssignmentCount\" >= 0 AND \"ApprovedActionCount\" >= 0 AND \"RejectedActionCount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxWorkflowStepInstance_RequiredApprovalCount",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                sql: "\"RequiredApprovalCount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxWorkflowStepInstance_RequiredApprovalPercentage",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                sql: "\"RequiredApprovalPercentage\" IS NULL OR (\"RequiredApprovalPercentage\" > 0 AND \"RequiredApprovalPercentage\" <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxWorkflowStepInstance_StepOrder",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                sql: "\"StepOrder\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowComment_CommentByUserId_CommentedAt",
                schema: "public",
                table: "TrxWorkflowComment",
                columns: new[] { "CommentByUserId", "CommentedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowComment_ParentCommentId_CommentedAt",
                schema: "public",
                table: "TrxWorkflowComment",
                columns: new[] { "ParentCommentId", "CommentedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxWorkflowComment_ParentNotSelf",
                schema: "public",
                table: "TrxWorkflowComment",
                sql: "\"ParentCommentId\" IS NULL OR \"ParentCommentId\" <> \"Id\"");

            migrationBuilder.CreateIndex(
                name: "IX_TrxApprovalDelegation_ApprovalDelegationPolicyId_Delegation~",
                schema: "public",
                table: "TrxApprovalDelegation",
                columns: new[] { "ApprovalDelegationPolicyId", "DelegationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxApprovalDelegation_WorkflowDefinitionId_WorkflowStepId_D~",
                schema: "public",
                table: "TrxApprovalDelegation",
                columns: new[] { "WorkflowDefinitionId", "WorkflowStepId", "DelegationStatus" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxApprovalDelegation_DifferentUser",
                schema: "public",
                table: "TrxApprovalDelegation",
                sql: "\"DelegatorUserId\" <> \"DelegateUserId\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxApprovalDelegation_EffectivePeriod",
                schema: "public",
                table: "TrxApprovalDelegation",
                sql: "\"EffectiveEndAt\" > \"EffectiveStartAt\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxApprovalDelegation_StepRequiresDefinition",
                schema: "public",
                table: "TrxApprovalDelegation",
                sql: "\"WorkflowStepId\" IS NULL OR \"WorkflowDefinitionId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxApprovalDelegation_WorkflowScope",
                schema: "public",
                table: "TrxApprovalDelegation",
                sql: "\"AppliesToAllWorkflows\" = true OR \"WorkflowDefinitionId\" IS NOT NULL OR \"ScopeDefinitionJson\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxApprovalAction_ActionReasonId_ActionReasonType",
                schema: "public",
                table: "TrxApprovalAction",
                columns: new[] { "ActionReasonId", "ActionReasonType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxApprovalAction_DelegatedFromWorkforceProfileId",
                schema: "public",
                table: "TrxApprovalAction",
                column: "DelegatedFromWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxApprovalAction_WorkflowApproverAssignmentId_ActionType_A~",
                schema: "public",
                table: "TrxApprovalAction",
                columns: new[] { "WorkflowApproverAssignmentId", "ActionType", "ActionAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxApprovalAction_ActualActor",
                schema: "public",
                table: "TrxApprovalAction",
                sql: "\"IsSystemAction\" = true OR \"ActualActionByUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_ApprovalDelegationId_IsDelega~",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                columns: new[] { "ApprovalDelegationId", "IsDelegated" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_ApprovalMatrixId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                column: "ApprovalMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_AssignedApproverUserId_Assign~",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                columns: new[] { "AssignedApproverUserId", "AssignmentStatus", "AvailableAt", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_AssignedApproverWorkforceProf~",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                columns: new[] { "AssignedApproverWorkforceProfileId", "AssignmentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_OriginalApproverUserId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                column: "OriginalApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_OriginalApproverWorkforceProf~",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                column: "OriginalApproverWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_WorkflowInstanceId_Assignment~",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                columns: new[] { "WorkflowInstanceId", "AssignmentStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_WorkflowStepInstanceId_Assig~1",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                columns: new[] { "WorkflowStepInstanceId", "AssignmentOrder" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_WorkflowStepInstanceId_Assign~",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                columns: new[] { "WorkflowStepInstanceId", "AssignedApproverUserId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxApprovalAction_MstWorkforceProfile_DelegatedFromWorkforc~",
                schema: "public",
                table: "TrxApprovalAction",
                column: "DelegatedFromWorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxApprovalAction_TrxWorkflowApproverAssignment_WorkflowApp~",
                schema: "public",
                table: "TrxApprovalAction",
                column: "WorkflowApproverAssignmentId",
                principalSchema: "public",
                principalTable: "TrxWorkflowApproverAssignment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxWorkflowComment_TrxWorkflowComment_ParentCommentId",
                schema: "public",
                table: "TrxWorkflowComment",
                column: "ParentCommentId",
                principalSchema: "public",
                principalTable: "TrxWorkflowComment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxApprovalAction_MstWorkforceProfile_DelegatedFromWorkforc~",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxApprovalAction_TrxWorkflowApproverAssignment_WorkflowApp~",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxWorkflowComment_TrxWorkflowComment_ParentCommentId",
                schema: "public",
                table: "TrxWorkflowComment");

            migrationBuilder.DropTable(
                name: "TrxWorkflowApproverAssignment",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowStepInstance_ApprovalMatrixId_StepStatus",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowStepInstance_DueAt",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowStepInstance_WorkflowInstanceId_StepCodeSnapshot",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowStepInstance_WorkflowInstanceId_StepOrder_IsActi~",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxWorkflowStepInstance_ActionCounters",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxWorkflowStepInstance_RequiredApprovalCount",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxWorkflowStepInstance_RequiredApprovalPercentage",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxWorkflowStepInstance_StepOrder",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowComment_CommentByUserId_CommentedAt",
                schema: "public",
                table: "TrxWorkflowComment");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowComment_ParentCommentId_CommentedAt",
                schema: "public",
                table: "TrxWorkflowComment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxWorkflowComment_ParentNotSelf",
                schema: "public",
                table: "TrxWorkflowComment");

            migrationBuilder.DropIndex(
                name: "IX_TrxApprovalDelegation_ApprovalDelegationPolicyId_Delegation~",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropIndex(
                name: "IX_TrxApprovalDelegation_WorkflowDefinitionId_WorkflowStepId_D~",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxApprovalDelegation_DifferentUser",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxApprovalDelegation_EffectivePeriod",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxApprovalDelegation_StepRequiresDefinition",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxApprovalDelegation_WorkflowScope",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropIndex(
                name: "IX_TrxApprovalAction_ActionReasonId_ActionReasonType",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropIndex(
                name: "IX_TrxApprovalAction_DelegatedFromWorkforceProfileId",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropIndex(
                name: "IX_TrxApprovalAction_WorkflowApproverAssignmentId_ActionType_A~",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxApprovalAction_ActualActor",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "TotalAssignmentCount",
                schema: "public",
                table: "TrxWorkflowStepInstance");

            migrationBuilder.DropColumn(
                name: "ActionReasonCodeSnapshot",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "ActionReasonId",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "ActionReasonNameSnapshot",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "ActionReasonType",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "DelegatedFromWorkforceProfileId",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "WorkflowApproverAssignmentId",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.AlterColumn<string>(
                name: "StepTypeSnapshot",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Approval");

            migrationBuilder.AlterColumn<string>(
                name: "StepStatus",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "ApproverSourceSnapshot",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "RequesterManager");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalModeSnapshot",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Any");

            migrationBuilder.AddColumn<string>(
                name: "AssignedApproverRoleCode",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedApproverUserId",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedApproverWorkforceProfileId",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommentType",
                schema: "public",
                table: "TrxWorkflowComment",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldDefaultValue: "General");

            migrationBuilder.AlterColumn<string>(
                name: "DelegationStatus",
                schema: "public",
                table: "TrxApprovalDelegation",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldDefaultValue: "Draft");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActualActionByUserId",
                schema: "public",
                table: "TrxApprovalAction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldDefaultValue: "Approve");

            migrationBuilder.AlterColumn<string>(
                name: "ActionSource",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Web");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowStepInstance_ApprovalMatrixId",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                column: "ApprovalMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowStepInstance_AssignedApproverUserId_StepStatus_D~",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                columns: new[] { "AssignedApproverUserId", "StepStatus", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowStepInstance_AssignedApproverWorkforceProfileId_~",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                columns: new[] { "AssignedApproverWorkforceProfileId", "StepStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowStepInstance_WorkflowInstanceId_StepOrder",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                columns: new[] { "WorkflowInstanceId", "StepOrder" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowComment_CommentByUserId",
                schema: "public",
                table: "TrxWorkflowComment",
                column: "CommentByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxApprovalDelegation_ApprovalDelegationPolicyId",
                schema: "public",
                table: "TrxApprovalDelegation",
                column: "ApprovalDelegationPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxApprovalDelegation_WorkflowDefinitionId_WorkflowStepId",
                schema: "public",
                table: "TrxApprovalDelegation",
                columns: new[] { "WorkflowDefinitionId", "WorkflowStepId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrxWorkflowStepInstance_AspNetUsers_AssignedApproverUserId",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                column: "AssignedApproverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxWorkflowStepInstance_MstWorkforceProfile_AssignedApprove~",
                schema: "public",
                table: "TrxWorkflowStepInstance",
                column: "AssignedApproverWorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
