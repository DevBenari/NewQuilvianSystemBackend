using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonnelActionOrderAndAssignmentApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonnelActionOrderId",
                schema: "public",
                table: "WfpSalaryAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "public",
                table: "WfpPositionAssignment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                schema: "public",
                table: "WfpPositionAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonnelActionOrderId",
                schema: "public",
                table: "WfpPositionAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "public",
                table: "WfpOrganizationAssignment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                schema: "public",
                table: "WfpOrganizationAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonnelActionOrderId",
                schema: "public",
                table: "WfpOrganizationAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "public",
                table: "WfpManagerAssignment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                schema: "public",
                table: "WfpManagerAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonnelActionOrderId",
                schema: "public",
                table: "WfpManagerAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceFileName",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceFilePath",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubmittedOnBehalf",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnBehalfNotifiedAt",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnBehalfReason",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByWorkforceProfileId",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HrdInitialPlacementBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BatchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BatchStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    ValidatedRows = table.Column<int>(type: "integer", nullable: false),
                    FailedRows = table.Column<int>(type: "integer", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutionSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdInitialPlacementBatch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdPersonnelActionOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionOrderNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionCategory = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "date", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReasonText = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    SignedDocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SignedDocumentFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TargetOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetManagerProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedSalaryAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetBaseSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancellationRequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationRequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CounterOrderNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdPersonnelActionOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrder_MstDepartment_TargetDepartmentId",
                        column: x => x.TargetDepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrder_MstOrganizationUnit_TargetOrganizat~",
                        column: x => x.TargetOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrder_MstPosition_TargetPositionId",
                        column: x => x.TargetPositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrder_MstWorkflowDefinition_WorkflowDefin~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrder_MstWorkforceProfile_TargetManagerPr~",
                        column: x => x.TargetManagerProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrder_MstWorkforceProfile_WorkforceProfil~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrder_TrxWorkflowInstance_WorkflowInstanc~",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "public",
                        principalTable: "TrxWorkflowInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrder_WfpSalaryAssignment_LinkedSalaryAss~",
                        column: x => x.LinkedSalaryAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpSalaryAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdInitialPlacementBatchRow",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalaryGradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeneratedOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdInitialPlacementBatchRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdInitialPlacementBatchRow_HrdInitialPlacementBatch_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "public",
                        principalTable: "HrdInitialPlacementBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdInitialPlacementBatchRow_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialPlacementBatchRow_MstOrganizationUnit_Organizatio~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialPlacementBatchRow_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialPlacementBatchRow_MstWorkforceProfile_ManagerWork~",
                        column: x => x.ManagerWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialPlacementBatchRow_MstWorkforceProfile_WorkforcePr~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdPersonnelActionOrderAttachment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonnelActionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdPersonnelActionOrderAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdPersonnelActionOrderAttachment_HrdPersonnelActionOrder_P~",
                        column: x => x.PersonnelActionOrderId,
                        principalSchema: "public",
                        principalTable: "HrdPersonnelActionOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpSalaryAssignment_PersonnelActionOrderId",
                schema: "public",
                table: "WfpSalaryAssignment",
                column: "PersonnelActionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPositionAssignment_PersonnelActionOrderId",
                schema: "public",
                table: "WfpPositionAssignment",
                column: "PersonnelActionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOrganizationAssignment_PersonnelActionOrderId",
                schema: "public",
                table: "WfpOrganizationAssignment",
                column: "PersonnelActionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpManagerAssignment_PersonnelActionOrderId",
                schema: "public",
                table: "WfpManagerAssignment",
                column: "PersonnelActionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeProfileChangeRequest_SubmittedByWorkforceProfile~",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest",
                column: "SubmittedByWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialPlacementBatch_BatchNumber",
                schema: "public",
                table: "HrdInitialPlacementBatch",
                column: "BatchNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialPlacementBatch_BatchStatus",
                schema: "public",
                table: "HrdInitialPlacementBatch",
                column: "BatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialPlacementBatchRow_BatchId_WorkforceProfileId",
                schema: "public",
                table: "HrdInitialPlacementBatchRow",
                columns: new[] { "BatchId", "WorkforceProfileId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialPlacementBatchRow_DepartmentId",
                schema: "public",
                table: "HrdInitialPlacementBatchRow",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialPlacementBatchRow_ManagerWorkforceProfileId",
                schema: "public",
                table: "HrdInitialPlacementBatchRow",
                column: "ManagerWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialPlacementBatchRow_OrganizationUnitId",
                schema: "public",
                table: "HrdInitialPlacementBatchRow",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialPlacementBatchRow_PositionId",
                schema: "public",
                table: "HrdInitialPlacementBatchRow",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialPlacementBatchRow_WorkforceProfileId",
                schema: "public",
                table: "HrdInitialPlacementBatchRow",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_ActionOrderNumber",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                column: "ActionOrderNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_LinkedSalaryAssignmentId",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                column: "LinkedSalaryAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_OrderStatus_EffectiveDate",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                columns: new[] { "OrderStatus", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_TargetDepartmentId",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                column: "TargetDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_TargetManagerProfileId",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                column: "TargetManagerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_TargetOrganizationUnitId",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                column: "TargetOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_TargetPositionId",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                column: "TargetPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_WorkflowDefinitionId",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_WorkflowInstanceId",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                column: "WorkflowInstanceId",
                unique: true,
                filter: "\"WorkflowInstanceId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrder_WorkforceProfileId_EffectiveDate",
                schema: "public",
                table: "HrdPersonnelActionOrder",
                columns: new[] { "WorkforceProfileId", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdPersonnelActionOrderAttachment_PersonnelActionOrderId",
                schema: "public",
                table: "HrdPersonnelActionOrderAttachment",
                column: "PersonnelActionOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxEmployeeProfileChangeRequest_MstWorkforceProfile_Submitt~",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest",
                column: "SubmittedByWorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpManagerAssignment_HrdPersonnelActionOrder_PersonnelActio~",
                schema: "public",
                table: "WfpManagerAssignment",
                column: "PersonnelActionOrderId",
                principalSchema: "public",
                principalTable: "HrdPersonnelActionOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpOrganizationAssignment_HrdPersonnelActionOrder_Personnel~",
                schema: "public",
                table: "WfpOrganizationAssignment",
                column: "PersonnelActionOrderId",
                principalSchema: "public",
                principalTable: "HrdPersonnelActionOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpPositionAssignment_HrdPersonnelActionOrder_PersonnelActi~",
                schema: "public",
                table: "WfpPositionAssignment",
                column: "PersonnelActionOrderId",
                principalSchema: "public",
                principalTable: "HrdPersonnelActionOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpSalaryAssignment_HrdPersonnelActionOrder_PersonnelAction~",
                schema: "public",
                table: "WfpSalaryAssignment",
                column: "PersonnelActionOrderId",
                principalSchema: "public",
                principalTable: "HrdPersonnelActionOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxEmployeeProfileChangeRequest_MstWorkforceProfile_Submitt~",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpManagerAssignment_HrdPersonnelActionOrder_PersonnelActio~",
                schema: "public",
                table: "WfpManagerAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpOrganizationAssignment_HrdPersonnelActionOrder_Personnel~",
                schema: "public",
                table: "WfpOrganizationAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpPositionAssignment_HrdPersonnelActionOrder_PersonnelActi~",
                schema: "public",
                table: "WfpPositionAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpSalaryAssignment_HrdPersonnelActionOrder_PersonnelAction~",
                schema: "public",
                table: "WfpSalaryAssignment");

            migrationBuilder.DropTable(
                name: "HrdInitialPlacementBatchRow",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPersonnelActionOrderAttachment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdInitialPlacementBatch",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPersonnelActionOrder",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_WfpSalaryAssignment_PersonnelActionOrderId",
                schema: "public",
                table: "WfpSalaryAssignment");

            migrationBuilder.DropIndex(
                name: "IX_WfpPositionAssignment_PersonnelActionOrderId",
                schema: "public",
                table: "WfpPositionAssignment");

            migrationBuilder.DropIndex(
                name: "IX_WfpOrganizationAssignment_PersonnelActionOrderId",
                schema: "public",
                table: "WfpOrganizationAssignment");

            migrationBuilder.DropIndex(
                name: "IX_WfpManagerAssignment_PersonnelActionOrderId",
                schema: "public",
                table: "WfpManagerAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TrxEmployeeProfileChangeRequest_SubmittedByWorkforceProfile~",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest");

            migrationBuilder.DropColumn(
                name: "PersonnelActionOrderId",
                schema: "public",
                table: "WfpSalaryAssignment");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "public",
                table: "WfpPositionAssignment");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "public",
                table: "WfpPositionAssignment");

            migrationBuilder.DropColumn(
                name: "PersonnelActionOrderId",
                schema: "public",
                table: "WfpPositionAssignment");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "public",
                table: "WfpOrganizationAssignment");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "public",
                table: "WfpOrganizationAssignment");

            migrationBuilder.DropColumn(
                name: "PersonnelActionOrderId",
                schema: "public",
                table: "WfpOrganizationAssignment");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "public",
                table: "WfpManagerAssignment");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "public",
                table: "WfpManagerAssignment");

            migrationBuilder.DropColumn(
                name: "PersonnelActionOrderId",
                schema: "public",
                table: "WfpManagerAssignment");

            migrationBuilder.DropColumn(
                name: "EvidenceFileName",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest");

            migrationBuilder.DropColumn(
                name: "EvidenceFilePath",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest");

            migrationBuilder.DropColumn(
                name: "IsSubmittedOnBehalf",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest");

            migrationBuilder.DropColumn(
                name: "OnBehalfNotifiedAt",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest");

            migrationBuilder.DropColumn(
                name: "OnBehalfReason",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest");

            migrationBuilder.DropColumn(
                name: "SubmittedByWorkforceProfileId",
                schema: "public",
                table: "TrxEmployeeProfileChangeRequest");
        }
    }
}
