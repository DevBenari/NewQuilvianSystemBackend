using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class InitializeWfpOnBoardingOffBoardingContractHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpContractHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContractType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    ContractStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WfpContractHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpContractHistory_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpEmploymentHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HistoryType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OldDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_WfpEmploymentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpEmploymentHistory_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpEmploymentHistory_MstDepartment_NewDepartmentId",
                        column: x => x.NewDepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpEmploymentHistory_MstDepartment_OldDepartmentId",
                        column: x => x.OldDepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpEmploymentHistory_MstPosition_NewPositionId",
                        column: x => x.NewPositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpEmploymentHistory_MstPosition_OldPositionId",
                        column: x => x.OldPositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpEmploymentHistory_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpOffboardingChecklist",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OffboardingType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WfpOffboardingChecklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpOffboardingChecklist_MstWorkforceProfile_WorkforceProfil~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpOnboardingChecklist",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "date", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WfpOnboardingChecklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_MstWorkforceProfile_WorkforceProfile~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpOffboardingTask",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OffboardingChecklistId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaskName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaskCategory = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WfpOffboardingTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpOffboardingTask_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOffboardingTask_WfpOffboardingChecklist_OffboardingCheck~",
                        column: x => x.OffboardingChecklistId,
                        principalSchema: "public",
                        principalTable: "WfpOffboardingChecklist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpOnboardingTask",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingChecklistId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaskName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaskCategory = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WfpOnboardingTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingTask_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingTask_WfpOnboardingChecklist_OnboardingChecklis~",
                        column: x => x.OnboardingChecklistId,
                        principalSchema: "public",
                        principalTable: "WfpOnboardingChecklist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_ContractStatus",
                schema: "public",
                table: "WfpContractHistory",
                column: "ContractStatus");

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_ContractType",
                schema: "public",
                table: "WfpContractHistory",
                column: "ContractType");

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_EndDate",
                schema: "public",
                table: "WfpContractHistory",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_WorkforceProfileId",
                schema: "public",
                table: "WfpContractHistory",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_WorkforceProfileId_ContractNumber_IsDele~",
                schema: "public",
                table: "WfpContractHistory",
                columns: new[] { "WorkforceProfileId", "ContractNumber", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_WorkforceProfileId_ContractStatus_IsActi~",
                schema: "public",
                table: "WfpContractHistory",
                columns: new[] { "WorkforceProfileId", "ContractStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_WorkforceProfileId_StartDate_EndDate",
                schema: "public",
                table: "WfpContractHistory",
                columns: new[] { "WorkforceProfileId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_ApprovedByUserId",
                schema: "public",
                table: "WfpEmploymentHistory",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_EffectiveDate",
                schema: "public",
                table: "WfpEmploymentHistory",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_HistoryType",
                schema: "public",
                table: "WfpEmploymentHistory",
                column: "HistoryType");

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_NewDepartmentId_NewPositionId",
                schema: "public",
                table: "WfpEmploymentHistory",
                columns: new[] { "NewDepartmentId", "NewPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_NewPositionId",
                schema: "public",
                table: "WfpEmploymentHistory",
                column: "NewPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_OldDepartmentId",
                schema: "public",
                table: "WfpEmploymentHistory",
                column: "OldDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_OldPositionId",
                schema: "public",
                table: "WfpEmploymentHistory",
                column: "OldPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_WorkforceProfileId",
                schema: "public",
                table: "WfpEmploymentHistory",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_WorkforceProfileId_HistoryType_Effecti~",
                schema: "public",
                table: "WfpEmploymentHistory",
                columns: new[] { "WorkforceProfileId", "HistoryType", "EffectiveDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpEmploymentHistory_WorkforceProfileId_IsActive_IsDelete",
                schema: "public",
                table: "WfpEmploymentHistory",
                columns: new[] { "WorkforceProfileId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingChecklist_Status_EffectiveEndDate_IsActive_Is~",
                schema: "public",
                table: "WfpOffboardingChecklist",
                columns: new[] { "Status", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingChecklist_WorkforceProfileId",
                schema: "public",
                table: "WfpOffboardingChecklist",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingChecklist_WorkforceProfileId_OffboardingType_~",
                schema: "public",
                table: "WfpOffboardingChecklist",
                columns: new[] { "WorkforceProfileId", "OffboardingType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingChecklist_WorkforceProfileId_Status",
                schema: "public",
                table: "WfpOffboardingChecklist",
                columns: new[] { "WorkforceProfileId", "Status" },
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingChecklist_WorkforceProfileId_Status_IsActive_~",
                schema: "public",
                table: "WfpOffboardingChecklist",
                columns: new[] { "WorkforceProfileId", "Status", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingTask_CompletedByUserId",
                schema: "public",
                table: "WfpOffboardingTask",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingTask_IsRequired_IsCompleted_IsActive_IsDelete",
                schema: "public",
                table: "WfpOffboardingTask",
                columns: new[] { "IsRequired", "IsCompleted", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingTask_OffboardingChecklistId",
                schema: "public",
                table: "WfpOffboardingTask",
                column: "OffboardingChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingTask_OffboardingChecklistId_SortOrder",
                schema: "public",
                table: "WfpOffboardingTask",
                columns: new[] { "OffboardingChecklistId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingTask_OffboardingChecklistId_TaskCategory_IsCo~",
                schema: "public",
                table: "WfpOffboardingTask",
                columns: new[] { "OffboardingChecklistId", "TaskCategory", "IsCompleted", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOffboardingTask_OffboardingChecklistId_TaskCode",
                schema: "public",
                table: "WfpOffboardingTask",
                columns: new[] { "OffboardingChecklistId", "TaskCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_AssignedToUserId",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_Status_TargetCompletionDate_IsActive~",
                schema: "public",
                table: "WfpOnboardingChecklist",
                columns: new[] { "Status", "TargetCompletionDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_WorkforceProfileId",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_WorkforceProfileId_OnboardingType_Is~",
                schema: "public",
                table: "WfpOnboardingChecklist",
                columns: new[] { "WorkforceProfileId", "OnboardingType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_WorkforceProfileId_Status",
                schema: "public",
                table: "WfpOnboardingChecklist",
                columns: new[] { "WorkforceProfileId", "Status" },
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_WorkforceProfileId_Status_IsActive_I~",
                schema: "public",
                table: "WfpOnboardingChecklist",
                columns: new[] { "WorkforceProfileId", "Status", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_CompletedByUserId",
                schema: "public",
                table: "WfpOnboardingTask",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_IsRequired_IsCompleted_IsActive_IsDelete",
                schema: "public",
                table: "WfpOnboardingTask",
                columns: new[] { "IsRequired", "IsCompleted", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_OnboardingChecklistId",
                schema: "public",
                table: "WfpOnboardingTask",
                column: "OnboardingChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_OnboardingChecklistId_SortOrder",
                schema: "public",
                table: "WfpOnboardingTask",
                columns: new[] { "OnboardingChecklistId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_OnboardingChecklistId_TaskCategory_IsComp~",
                schema: "public",
                table: "WfpOnboardingTask",
                columns: new[] { "OnboardingChecklistId", "TaskCategory", "IsCompleted", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_OnboardingChecklistId_TaskCode",
                schema: "public",
                table: "WfpOnboardingTask",
                columns: new[] { "OnboardingChecklistId", "TaskCode" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpContractHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpEmploymentHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpOffboardingTask",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpOnboardingTask",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpOffboardingChecklist",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpOnboardingChecklist",
                schema: "public");
        }
    }
}
