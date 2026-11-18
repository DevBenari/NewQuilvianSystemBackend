using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingAttendanceAmendmentA52 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerTimeoutMinutes",
                schema: "public",
                table: "MstOnCallType",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "ScanWindowAfterMinutes",
                schema: "public",
                table: "HrdAttendanceSetting",
                type: "integer",
                nullable: false,
                defaultValue: 480);

            migrationBuilder.AddColumn<int>(
                name: "ScanWindowBeforeMinutes",
                schema: "public",
                table: "HrdAttendanceSetting",
                type: "integer",
                nullable: false,
                defaultValue: 240);

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceIncidentId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HrdAttendanceDeviceIncident",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HrdAttendanceDeviceIncident", x => x.Id);
                    table.CheckConstraint("CK_HrdAttendanceDeviceIncident_Range", "\"EndAt\" > \"StartAt\"");
                    table.ForeignKey(
                        name: "FK_HrdAttendanceDeviceIncident_MstAttendanceDevice_AttendanceD~",
                        column: x => x.AttendanceDeviceId,
                        principalSchema: "public",
                        principalTable: "MstAttendanceDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdAttendanceDispensation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DispensationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Submitted"),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOnBehalf = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApproverWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdAttendanceDispensation", x => x.Id);
                    table.CheckConstraint("CK_HrdAttendanceDispensation_Range", "\"EndAt\" > \"StartAt\"");
                    table.ForeignKey(
                        name: "FK_HrdAttendanceDispensation_MstWorkforceProfile_ApproverWorkf~",
                        column: x => x.ApproverWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdAttendanceDispensation_MstWorkforceProfile_WorkforceProf~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdOnCallBackup",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnCallAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BackupSequence = table.Column<int>(type: "integer", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdOnCallBackup", x => x.Id);
                    table.CheckConstraint("CK_HrdOnCallBackup_BackupSequence", "\"BackupSequence\" >= 1");
                    table.ForeignKey(
                        name: "FK_HrdOnCallBackup_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnCallBackup_TrxOnCallAssignment_OnCallAssignmentId",
                        column: x => x.OnCallAssignmentId,
                        principalSchema: "public",
                        principalTable: "TrxOnCallAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdOnCallCall",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnCallAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalledWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    BackupSequence = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CallingOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CallReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CallStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Sent"),
                    CalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnswerRecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeclineReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArrivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponseMinutes = table.Column<int>(type: "integer", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdOnCallCall", x => x.Id);
                    table.CheckConstraint("CK_HrdOnCallCall_BackupSequence", "\"BackupSequence\" >= 0");
                    table.ForeignKey(
                        name: "FK_HrdOnCallCall_MstOrganizationUnit_CallingOrganizationUnitId",
                        column: x => x.CallingOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnCallCall_MstWorkforceProfile_CalledWorkforceProfileId",
                        column: x => x.CalledWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnCallCall_TrxOnCallAssignment_OnCallAssignmentId",
                        column: x => x.OnCallAssignmentId,
                        principalSchema: "public",
                        principalTable: "TrxOnCallAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdShiftAssignmentRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RosterPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PreviousWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreviousEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReasonCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReasonNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ShiftReplacementId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_HrdShiftAssignmentRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdShiftAssignmentRevision_MstShift_NewShiftId",
                        column: x => x.NewShiftId,
                        principalSchema: "public",
                        principalTable: "MstShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdShiftAssignmentRevision_MstShift_PreviousShiftId",
                        column: x => x.PreviousShiftId,
                        principalSchema: "public",
                        principalTable: "MstShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdShiftAssignmentRevision_MstWorkforceProfile_NewWorkforce~",
                        column: x => x.NewWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdShiftAssignmentRevision_MstWorkforceProfile_PreviousWork~",
                        column: x => x.PreviousWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdShiftAssignmentRevision_TrxRosterPeriod_RosterPeriodId",
                        column: x => x.RosterPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxRosterPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdShiftAssignmentRevision_TrxShiftAssignment_ShiftAssignme~",
                        column: x => x.ShiftAssignmentId,
                        principalSchema: "public",
                        principalTable: "TrxShiftAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdShiftAssignmentRevision_TrxShiftReplacement_ShiftReplace~",
                        column: x => x.ShiftReplacementId,
                        principalSchema: "public",
                        principalTable: "TrxShiftReplacement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdStudyAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecreeNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InstitutionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProgramName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    LastChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HrdStudyAssignment", x => x.Id);
                    table.CheckConstraint("CK_HrdStudyAssignment_Range", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_HrdStudyAssignment_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceCorrectionRequest_DeviceIncidentId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                column: "DeviceIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceDeviceIncident_AttendanceDeviceId_StartAt",
                schema: "public",
                table: "HrdAttendanceDeviceIncident",
                columns: new[] { "AttendanceDeviceId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceDispensation_ApproverWorkforceProfileId",
                schema: "public",
                table: "HrdAttendanceDispensation",
                column: "ApproverWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceDispensation_Status",
                schema: "public",
                table: "HrdAttendanceDispensation",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceDispensation_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDispensation",
                columns: new[] { "WorkforceProfileId", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnCallBackup_OnCallAssignmentId_BackupSequence",
                schema: "public",
                table: "HrdOnCallBackup",
                columns: new[] { "OnCallAssignmentId", "BackupSequence" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnCallBackup_OnCallAssignmentId_WorkforceProfileId",
                schema: "public",
                table: "HrdOnCallBackup",
                columns: new[] { "OnCallAssignmentId", "WorkforceProfileId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnCallBackup_WorkforceProfileId",
                schema: "public",
                table: "HrdOnCallBackup",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnCallCall_CalledWorkforceProfileId",
                schema: "public",
                table: "HrdOnCallCall",
                column: "CalledWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnCallCall_CallingOrganizationUnitId",
                schema: "public",
                table: "HrdOnCallCall",
                column: "CallingOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnCallCall_OnCallAssignmentId_CalledAt",
                schema: "public",
                table: "HrdOnCallCall",
                columns: new[] { "OnCallAssignmentId", "CalledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdShiftAssignmentRevision_NewShiftId",
                schema: "public",
                table: "HrdShiftAssignmentRevision",
                column: "NewShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdShiftAssignmentRevision_NewWorkforceProfileId",
                schema: "public",
                table: "HrdShiftAssignmentRevision",
                column: "NewWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdShiftAssignmentRevision_PreviousShiftId",
                schema: "public",
                table: "HrdShiftAssignmentRevision",
                column: "PreviousShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdShiftAssignmentRevision_PreviousWorkforceProfileId",
                schema: "public",
                table: "HrdShiftAssignmentRevision",
                column: "PreviousWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdShiftAssignmentRevision_RosterPeriodId_ShiftDate",
                schema: "public",
                table: "HrdShiftAssignmentRevision",
                columns: new[] { "RosterPeriodId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdShiftAssignmentRevision_ShiftAssignmentId_ChangedAt",
                schema: "public",
                table: "HrdShiftAssignmentRevision",
                columns: new[] { "ShiftAssignmentId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdShiftAssignmentRevision_ShiftReplacementId",
                schema: "public",
                table: "HrdShiftAssignmentRevision",
                column: "ShiftReplacementId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStudyAssignment_WorkforceProfileId_StartDate_EndDate",
                schema: "public",
                table: "HrdStudyAssignment",
                columns: new[] { "WorkforceProfileId", "StartDate", "EndDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_HrdAttendanceCorrectionRequest_HrdAttendanceDeviceIncident_~",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                column: "DeviceIncidentId",
                principalSchema: "public",
                principalTable: "HrdAttendanceDeviceIncident",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HrdAttendanceCorrectionRequest_HrdAttendanceDeviceIncident_~",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");

            migrationBuilder.DropTable(
                name: "HrdAttendanceDeviceIncident",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdAttendanceDispensation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdOnCallBackup",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdOnCallCall",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdShiftAssignmentRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdStudyAssignment",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_HrdAttendanceCorrectionRequest_DeviceIncidentId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");

            migrationBuilder.DropColumn(
                name: "AnswerTimeoutMinutes",
                schema: "public",
                table: "MstOnCallType");

            migrationBuilder.DropColumn(
                name: "ScanWindowAfterMinutes",
                schema: "public",
                table: "HrdAttendanceSetting");

            migrationBuilder.DropColumn(
                name: "ScanWindowBeforeMinutes",
                schema: "public",
                table: "HrdAttendanceSetting");

            migrationBuilder.DropColumn(
                name: "DeviceIncidentId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");
        }
    }
}
