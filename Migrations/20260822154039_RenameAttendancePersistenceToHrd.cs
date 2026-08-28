using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class RenameAttendancePersistenceToHrd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PHASE A - Rename the 10 physical tables. No table is dropped or recreated.
            migrationBuilder.RenameTable(
                name: "TrxAttendanceDaily",
                schema: "public",
                newName: "HrdAttendanceDaily");

            migrationBuilder.RenameTable(
                name: "TrxAttendanceDailySegment",
                schema: "public",
                newName: "HrdAttendanceDailySegment");

            migrationBuilder.RenameTable(
                name: "TrxAttendanceException",
                schema: "public",
                newName: "HrdAttendanceException");

            migrationBuilder.RenameTable(
                name: "TrxAttendancePeriod",
                schema: "public",
                newName: "HrdAttendancePeriod");

            migrationBuilder.RenameTable(
                name: "TrxAttendanceProcessingRun",
                schema: "public",
                newName: "HrdAttendanceProcessingRun");

            migrationBuilder.RenameTable(
                name: "TrxAttendanceRawLog",
                schema: "public",
                newName: "HrdAttendanceRawLog");

            migrationBuilder.RenameTable(
                name: "TrxAttendanceSchedulerJob",
                schema: "public",
                newName: "HrdAttendanceSchedulerJob");

            migrationBuilder.RenameTable(
                name: "TrxBusinessTripAttendance",
                schema: "public",
                newName: "HrdBusinessTripAttendance");

            migrationBuilder.RenameTable(
                name: "TrxMissingAttendance",
                schema: "public",
                newName: "HrdMissingAttendance");

            migrationBuilder.RenameTable(
                name: "TrxRemoteAttendance",
                schema: "public",
                newName: "HrdRemoteAttendance");

            // PHASE B - Rename primary-key constraints.
            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"PK_TrxAttendanceProcessingRun\" TO \"PK_HrdAttendanceProcessingRun\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"PK_TrxAttendancePeriod\" TO \"PK_HrdAttendancePeriod\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"PK_TrxAttendanceDaily\" TO \"PK_HrdAttendanceDaily\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"PK_TrxAttendanceSchedulerJob\" TO \"PK_HrdAttendanceSchedulerJob\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"PK_TrxAttendanceException\" TO \"PK_HrdAttendanceException\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"PK_TrxAttendanceRawLog\" TO \"PK_HrdAttendanceRawLog\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"PK_TrxBusinessTripAttendance\" TO \"PK_HrdBusinessTripAttendance\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"PK_TrxRemoteAttendance\" TO \"PK_HrdRemoteAttendance\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"PK_TrxMissingAttendance\" TO \"PK_HrdMissingAttendance\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"PK_TrxAttendanceDailySegment\" TO \"PK_HrdAttendanceDailySegment\";");

            // PHASE C - Rename indexes in-place; definitions, uniqueness and filters are preserved.
            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_AttendancePeriodId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_AttendancePeriodId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_AttendancePolicyId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_AttendancePolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_AttendanceStatus_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_AttendanceStatus_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_DepartmentId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_DoctorId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_EmployeeId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_GracePeriodPolicyId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_GracePeriodPolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_HospitalSiteId_OrganizationUnitId_Depart~",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_HospitalSiteId_OrganizationUnitId_Depart~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_IsLate_IsEarlyLeave_HasMissingPunch_Atte~",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_IsLate_IsEarlyLeave_HasMissingPunch_Atte~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_OrganizationAssignmentId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_OrganizationAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_OrganizationUnitId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_PayrollPeriodId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_PayrollPeriodId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_PositionId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_PrimaryShiftAssignmentId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_PrimaryShiftAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_ProcessingStatus_PayrollInputStatus_Atte~",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_ProcessingStatus_PayrollInputStatus_Atte~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_ScheduleSource_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_ScheduleSource_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_ShiftId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_ShiftId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_UserId_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_UserId_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_WorkforceProfileId_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_WorkLocationId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_WorkScheduleAssignmentId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_WorkScheduleAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDaily_WorkScheduleId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_HrdAttendanceDaily_WorkScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDailySegment_AttendanceDailyId_SegmentOrder",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_HrdAttendanceDailySegment_AttendanceDailyId_SegmentOrder");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDailySegment_EndRawLogId",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_HrdAttendanceDailySegment_EndRawLogId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDailySegment_SegmentType_SegmentStatus",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_HrdAttendanceDailySegment_SegmentType_SegmentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDailySegment_ShiftAssignmentId_SegmentType",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_HrdAttendanceDailySegment_ShiftAssignmentId_SegmentType");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceDailySegment_StartRawLogId",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_HrdAttendanceDailySegment_StartRawLogId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceException_AttendanceDailyId_ExceptionCode",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_HrdAttendanceException_AttendanceDailyId_ExceptionCode");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceException_CorrectionRequestId",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_HrdAttendanceException_CorrectionRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceException_ExceptionStatus_Severity_IsPayrollBl~",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_HrdAttendanceException_ExceptionStatus_Severity_IsPayrollBl~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceException_ResolvedByUserId",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_HrdAttendanceException_ResolvedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceException_WorkforceProfileId_DetectedAt",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_HrdAttendanceException_WorkforceProfileId_DetectedAt");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_ClosedByUserId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_ClosedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_DepartmentId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_HospitalSiteId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_LastProcessingRunId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_LastProcessingRunId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_LegalEntityId_HospitalSiteId_Organizati~",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_LegalEntityId_HospitalSiteId_Organizati~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_OrganizationUnitId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_PeriodCode",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_PeriodCode");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_PeriodStatus_ScheduledCloseAt_IsActive_~",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_PeriodStatus_ScheduledCloseAt_IsActive_~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_ReopenedByUserId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_ReopenedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendancePeriod_StartDate_EndDate_PeriodStatus",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_HrdAttendancePeriod_StartDate_EndDate_PeriodStatus");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_CancelledByUserId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_CancelledByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_CorrelationId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_CorrelationId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_DepartmentId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_HospitalSiteId_OrganizationUnitI~",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_HospitalSiteId_OrganizationUnitI~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_OrganizationUnitId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_RunNumber",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_RunNumber");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_RunStatus_StartedAt",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_RunStatus_StartedAt");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_StartDate_EndDate_ProcessingMode",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_StartDate_EndDate_ProcessingMode");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_TargetWorkforceProfileId_StartDa~",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_TargetWorkforceProfileId_StartDa~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceProcessingRun_TriggeredByUserId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_HrdAttendanceProcessingRun_TriggeredByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_AttendanceDeviceId_ExternalLogId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_AttendanceDeviceId_ExternalLogId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_AttendanceLocationId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_AttendanceLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_DeviceUserKey_EventAt",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_DeviceUserKey_EventAt");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_DoctorId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_EmployeeId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_EventHash",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_EventHash");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_HospitalSiteId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_ProcessedAttendanceDailyId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_ProcessedAttendanceDailyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_ProcessedAttendanceId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_ProcessedAttendanceId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_ProcessingStatus_ReceivedAt",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_ProcessingStatus_ReceivedAt");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_SourceType_EventAt",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_SourceType_EventAt");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_UserId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceRawLog_WorkforceProfileId_EventAt",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_HrdAttendanceRawLog_WorkforceProfileId_EventAt");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_AttendancePeriodId_JobStatus",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_AttendancePeriodId_JobStatus");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_CancelledByUserId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_CancelledByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_CorrelationId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_CorrelationId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_DepartmentId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_HospitalSiteId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_JobNumber",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_JobNumber");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_JobStatus_AvailableAt_Priority",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_JobStatus_AvailableAt_Priority");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_OrganizationUnitId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_ProcessingRunId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_ProcessingRunId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_StartDate_EndDate_JobType",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_StartDate_EndDate_JobType");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_TriggeredByUserId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_TriggeredByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxAttendanceSchedulerJob_WorkforceProfileId_HospitalSiteId~",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_HrdAttendanceSchedulerJob_WorkforceProfileId_HospitalSiteId~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxBusinessTripAttendance_ApprovedByUserId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_HrdBusinessTripAttendance_ApprovedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxBusinessTripAttendance_AttendanceDailyId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_HrdBusinessTripAttendance_AttendanceDailyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxBusinessTripAttendance_AttendanceLocationId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_HrdBusinessTripAttendance_AttendanceLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxBusinessTripAttendance_AttendanceStatus_AttendanceDate",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_HrdBusinessTripAttendance_AttendanceStatus_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_TrxBusinessTripAttendance_HospitalSiteId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_HrdBusinessTripAttendance_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxBusinessTripAttendance_ReferenceType_ReferenceId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_HrdBusinessTripAttendance_ReferenceType_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxBusinessTripAttendance_UserId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_HrdBusinessTripAttendance_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxBusinessTripAttendance_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_HrdBusinessTripAttendance_WorkforceProfileId_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_AttendanceCorrectionRequestId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_AttendanceCorrectionRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_AttendanceDailyId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_AttendanceDailyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_AttendanceExceptionId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_AttendanceExceptionId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_MissingStatus_IsPayrollBlocking_Attend~",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_MissingStatus_IsPayrollBlocking_Attend~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_ResolvedByUserId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_ResolvedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_ShiftId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_ShiftId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_UserId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_WorkforceProfileId_AttendanceDate_Miss~",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_WorkforceProfileId_AttendanceDate_Miss~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxMissingAttendance_WorkScheduleId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_HrdMissingAttendance_WorkScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxRemoteAttendance_ApprovalStatus_AttendanceDate",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_HrdRemoteAttendance_ApprovalStatus_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_TrxRemoteAttendance_ApprovedByUserId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_HrdRemoteAttendance_ApprovedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxRemoteAttendance_AttendanceDailyId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_HrdRemoteAttendance_AttendanceDailyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxRemoteAttendance_AttendanceLocationId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_HrdRemoteAttendance_AttendanceLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxRemoteAttendance_AttendancePolicyId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_HrdRemoteAttendance_AttendancePolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxRemoteAttendance_UserId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_HrdRemoteAttendance_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxRemoteAttendance_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_HrdRemoteAttendance_WorkforceProfileId_AttendanceDate");

            // PHASE D - Rename foreign-key constraints in-place; relationships are unchanged.
            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_TrxAttendanceProcessingRun_AspNetUsers_CancelledByUserId\" TO \"FK_HrdAttendanceProcessingRun_AspNetUsers_CancelledByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_TrxAttendanceProcessingRun_AspNetUsers_TriggeredByUserId\" TO \"FK_HrdAttendanceProcessingRun_AspNetUsers_TriggeredByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_TrxAttendanceProcessingRun_MstDepartment_DepartmentId\" TO \"FK_HrdAttendanceProcessingRun_MstDepartment_DepartmentId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_TrxAttendanceProcessingRun_MstHospitalSite_HospitalSiteId\" TO \"FK_HrdAttendanceProcessingRun_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_TrxAttendanceProcessingRun_MstOrganizationUnit_Organization~\" TO \"FK_HrdAttendanceProcessingRun_MstOrganizationUnit_Organization~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_TrxAttendanceProcessingRun_MstWorkforceProfile_TargetWorkfo~\" TO \"FK_HrdAttendanceProcessingRun_MstWorkforceProfile_TargetWorkfo~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_TrxAttendancePeriod_AspNetUsers_ClosedByUserId\" TO \"FK_HrdAttendancePeriod_AspNetUsers_ClosedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_TrxAttendancePeriod_AspNetUsers_ReopenedByUserId\" TO \"FK_HrdAttendancePeriod_AspNetUsers_ReopenedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_TrxAttendancePeriod_MstDepartment_DepartmentId\" TO \"FK_HrdAttendancePeriod_MstDepartment_DepartmentId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_TrxAttendancePeriod_MstHospitalSite_HospitalSiteId\" TO \"FK_HrdAttendancePeriod_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_TrxAttendancePeriod_MstLegalEntity_LegalEntityId\" TO \"FK_HrdAttendancePeriod_MstLegalEntity_LegalEntityId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_TrxAttendancePeriod_MstOrganizationUnit_OrganizationUnitId\" TO \"FK_HrdAttendancePeriod_MstOrganizationUnit_OrganizationUnitId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_TrxAttendancePeriod_TrxAttendanceProcessingRun_LastProcessi~\" TO \"FK_HrdAttendancePeriod_HrdAttendanceProcessingRun_LastProcessi~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_AspNetUsers_UserId\" TO \"FK_HrdAttendanceDaily_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstAttendancePolicy_AttendancePolicyId\" TO \"FK_HrdAttendanceDaily_MstAttendancePolicy_AttendancePolicyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstDepartment_DepartmentId\" TO \"FK_HrdAttendanceDaily_MstDepartment_DepartmentId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstDoctor_DoctorId\" TO \"FK_HrdAttendanceDaily_MstDoctor_DoctorId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstEmployee_EmployeeId\" TO \"FK_HrdAttendanceDaily_MstEmployee_EmployeeId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstGracePeriodPolicy_GracePeriodPolicyId\" TO \"FK_HrdAttendanceDaily_MstGracePeriodPolicy_GracePeriodPolicyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstHospitalSite_HospitalSiteId\" TO \"FK_HrdAttendanceDaily_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstOrganizationUnit_OrganizationUnitId\" TO \"FK_HrdAttendanceDaily_MstOrganizationUnit_OrganizationUnitId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstPayrollPeriod_PayrollPeriodId\" TO \"FK_HrdAttendanceDaily_MstPayrollPeriod_PayrollPeriodId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstPosition_PositionId\" TO \"FK_HrdAttendanceDaily_MstPosition_PositionId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstShift_ShiftId\" TO \"FK_HrdAttendanceDaily_MstShift_ShiftId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstWorkLocation_WorkLocationId\" TO \"FK_HrdAttendanceDaily_MstWorkLocation_WorkLocationId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstWorkSchedule_WorkScheduleId\" TO \"FK_HrdAttendanceDaily_MstWorkSchedule_WorkScheduleId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_MstWorkforceProfile_WorkforceProfileId\" TO \"FK_HrdAttendanceDaily_MstWorkforceProfile_WorkforceProfileId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_TrxAttendancePeriod_AttendancePeriodId\" TO \"FK_HrdAttendanceDaily_HrdAttendancePeriod_AttendancePeriodId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_TrxShiftAssignment_PrimaryShiftAssignmen~\" TO \"FK_HrdAttendanceDaily_TrxShiftAssignment_PrimaryShiftAssignmen~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_WfpOrganizationAssignment_OrganizationAs~\" TO \"FK_HrdAttendanceDaily_WfpOrganizationAssignment_OrganizationAs~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_TrxAttendanceDaily_WfpWorkScheduleAssignment_WorkScheduleAs~\" TO \"FK_HrdAttendanceDaily_WfpWorkScheduleAssignment_WorkScheduleAs~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_TrxAttendanceSchedulerJob_AspNetUsers_CancelledByUserId\" TO \"FK_HrdAttendanceSchedulerJob_AspNetUsers_CancelledByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_TrxAttendanceSchedulerJob_AspNetUsers_TriggeredByUserId\" TO \"FK_HrdAttendanceSchedulerJob_AspNetUsers_TriggeredByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_TrxAttendanceSchedulerJob_MstDepartment_DepartmentId\" TO \"FK_HrdAttendanceSchedulerJob_MstDepartment_DepartmentId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_TrxAttendanceSchedulerJob_MstHospitalSite_HospitalSiteId\" TO \"FK_HrdAttendanceSchedulerJob_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_TrxAttendanceSchedulerJob_MstOrganizationUnit_OrganizationU~\" TO \"FK_HrdAttendanceSchedulerJob_MstOrganizationUnit_OrganizationU~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_TrxAttendanceSchedulerJob_MstWorkforceProfile_WorkforceProf~\" TO \"FK_HrdAttendanceSchedulerJob_MstWorkforceProfile_WorkforceProf~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_TrxAttendanceSchedulerJob_TrxAttendancePeriod_AttendancePer~\" TO \"FK_HrdAttendanceSchedulerJob_HrdAttendancePeriod_AttendancePer~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_TrxAttendanceSchedulerJob_TrxAttendanceProcessingRun_Proces~\" TO \"FK_HrdAttendanceSchedulerJob_HrdAttendanceProcessingRun_Proces~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"FK_TrxAttendanceException_AspNetUsers_ResolvedByUserId\" TO \"FK_HrdAttendanceException_AspNetUsers_ResolvedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"FK_TrxAttendanceException_HrdAttendanceCorrectionRequest_Corre~\" TO \"FK_HrdAttendanceException_HrdAttendanceCorrectionRequest_Corre~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"FK_TrxAttendanceException_MstWorkforceProfile_WorkforceProfile~\" TO \"FK_HrdAttendanceException_MstWorkforceProfile_WorkforceProfile~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"FK_TrxAttendanceException_TrxAttendanceDaily_AttendanceDailyId\" TO \"FK_HrdAttendanceException_HrdAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_AspNetUsers_UserId\" TO \"FK_HrdAttendanceRawLog_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_HrdAttendance_ProcessedAttendanceId\" TO \"FK_HrdAttendanceRawLog_HrdAttendance_ProcessedAttendanceId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_MstAttendanceDevice_AttendanceDeviceId\" TO \"FK_HrdAttendanceRawLog_MstAttendanceDevice_AttendanceDeviceId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_MstAttendanceLocation_AttendanceLocatio~\" TO \"FK_HrdAttendanceRawLog_MstAttendanceLocation_AttendanceLocatio~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_MstDoctor_DoctorId\" TO \"FK_HrdAttendanceRawLog_MstDoctor_DoctorId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_MstEmployee_EmployeeId\" TO \"FK_HrdAttendanceRawLog_MstEmployee_EmployeeId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_MstHospitalSite_HospitalSiteId\" TO \"FK_HrdAttendanceRawLog_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_MstWorkforceProfile_WorkforceProfileId\" TO \"FK_HrdAttendanceRawLog_MstWorkforceProfile_WorkforceProfileId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_TrxAttendanceRawLog_TrxAttendanceDaily_ProcessedAttendanceD~\" TO \"FK_HrdAttendanceRawLog_HrdAttendanceDaily_ProcessedAttendanceD~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_TrxBusinessTripAttendance_AspNetUsers_ApprovedByUserId\" TO \"FK_HrdBusinessTripAttendance_AspNetUsers_ApprovedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_TrxBusinessTripAttendance_AspNetUsers_UserId\" TO \"FK_HrdBusinessTripAttendance_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_TrxBusinessTripAttendance_MstAttendanceLocation_AttendanceL~\" TO \"FK_HrdBusinessTripAttendance_MstAttendanceLocation_AttendanceL~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_TrxBusinessTripAttendance_MstHospitalSite_HospitalSiteId\" TO \"FK_HrdBusinessTripAttendance_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_TrxBusinessTripAttendance_MstWorkforceProfile_WorkforceProf~\" TO \"FK_HrdBusinessTripAttendance_MstWorkforceProfile_WorkforceProf~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_TrxBusinessTripAttendance_TrxAttendanceDaily_AttendanceDail~\" TO \"FK_HrdBusinessTripAttendance_HrdAttendanceDaily_AttendanceDail~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_TrxRemoteAttendance_AspNetUsers_ApprovedByUserId\" TO \"FK_HrdRemoteAttendance_AspNetUsers_ApprovedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_TrxRemoteAttendance_AspNetUsers_UserId\" TO \"FK_HrdRemoteAttendance_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_TrxRemoteAttendance_MstAttendanceLocation_AttendanceLocatio~\" TO \"FK_HrdRemoteAttendance_MstAttendanceLocation_AttendanceLocatio~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_TrxRemoteAttendance_MstAttendancePolicy_AttendancePolicyId\" TO \"FK_HrdRemoteAttendance_MstAttendancePolicy_AttendancePolicyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_TrxRemoteAttendance_MstWorkforceProfile_WorkforceProfileId\" TO \"FK_HrdRemoteAttendance_MstWorkforceProfile_WorkforceProfileId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_TrxRemoteAttendance_TrxAttendanceDaily_AttendanceDailyId\" TO \"FK_HrdRemoteAttendance_HrdAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_TrxMissingAttendance_AspNetUsers_ResolvedByUserId\" TO \"FK_HrdMissingAttendance_AspNetUsers_ResolvedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_TrxMissingAttendance_AspNetUsers_UserId\" TO \"FK_HrdMissingAttendance_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_TrxMissingAttendance_HrdAttendanceCorrectionRequest_Attenda~\" TO \"FK_HrdMissingAttendance_HrdAttendanceCorrectionRequest_Attenda~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_TrxMissingAttendance_MstShift_ShiftId\" TO \"FK_HrdMissingAttendance_MstShift_ShiftId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_TrxMissingAttendance_MstWorkSchedule_WorkScheduleId\" TO \"FK_HrdMissingAttendance_MstWorkSchedule_WorkScheduleId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_TrxMissingAttendance_MstWorkforceProfile_WorkforceProfileId\" TO \"FK_HrdMissingAttendance_MstWorkforceProfile_WorkforceProfileId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_TrxMissingAttendance_TrxAttendanceDaily_AttendanceDailyId\" TO \"FK_HrdMissingAttendance_HrdAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_TrxMissingAttendance_TrxAttendanceException_AttendanceExcep~\" TO \"FK_HrdMissingAttendance_HrdAttendanceException_AttendanceExcep~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"FK_TrxAttendanceDailySegment_TrxAttendanceDaily_AttendanceDail~\" TO \"FK_HrdAttendanceDailySegment_HrdAttendanceDaily_AttendanceDail~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"FK_TrxAttendanceDailySegment_TrxAttendanceRawLog_EndRawLogId\" TO \"FK_HrdAttendanceDailySegment_HrdAttendanceRawLog_EndRawLogId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"FK_TrxAttendanceDailySegment_TrxAttendanceRawLog_StartRawLogId\" TO \"FK_HrdAttendanceDailySegment_HrdAttendanceRawLog_StartRawLogId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"FK_TrxAttendanceDailySegment_TrxShiftAssignment_ShiftAssignmen~\" TO \"FK_HrdAttendanceDailySegment_TrxShiftAssignment_ShiftAssignmen~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendance\" RENAME CONSTRAINT \"FK_HrdAttendance_TrxAttendanceDaily_AttendanceDailyId\" TO \"FK_HrdAttendance_HrdAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceCorrectionRequest\" RENAME CONSTRAINT \"FK_HrdAttendanceCorrectionRequest_TrxAttendanceDaily_Attendanc~\" TO \"FK_HrdAttendanceCorrectionRequest_HrdAttendanceDaily_Attendanc~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxLeaveAttendanceIntegration\" RENAME CONSTRAINT \"FK_TrxLeaveAttendanceIntegration_TrxAttendanceDaily_Attendance~\" TO \"FK_TrxLeaveAttendanceIntegration_HrdAttendanceDaily_Attendance~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxOvertimeRealization\" RENAME CONSTRAINT \"FK_TrxOvertimeRealization_TrxAttendanceDaily_AttendanceDailyId\" TO \"FK_TrxOvertimeRealization_HrdAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxOvertimeRealizationDetail\" RENAME CONSTRAINT \"FK_TrxOvertimeRealizationDetail_TrxAttendanceDaily_AttendanceD~\" TO \"FK_TrxOvertimeRealizationDetail_HrdAttendanceDaily_AttendanceD~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxOvertimeRequestDetail\" RENAME CONSTRAINT \"FK_TrxOvertimeRequestDetail_TrxAttendanceDaily_AttendanceDaily~\" TO \"FK_TrxOvertimeRequestDetail_HrdAttendanceDaily_AttendanceDaily~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxPayrollAttendanceInput\" RENAME CONSTRAINT \"FK_TrxPayrollAttendanceInput_TrxAttendanceDaily_AttendanceDail~\" TO \"FK_TrxPayrollAttendanceInput_HrdAttendanceDaily_AttendanceDail~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxTravelAttendanceLink\" RENAME CONSTRAINT \"FK_TrxTravelAttendanceLink_TrxAttendanceDaily_AttendanceDailyId\" TO \"FK_TrxTravelAttendanceLink_HrdAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxTravelAttendanceLink\" RENAME CONSTRAINT \"FK_TrxTravelAttendanceLink_TrxBusinessTripAttendance_BusinessT~\" TO \"FK_TrxTravelAttendanceLink_HrdBusinessTripAttendance_BusinessT~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"WfpOvertimeRequest\" RENAME CONSTRAINT \"FK_WfpOvertimeRequest_TrxAttendanceDaily_AttendanceDailyId\" TO \"FK_WfpOvertimeRequest_HrdAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"WfpTransportAllowanceTransaction\" RENAME CONSTRAINT \"FK_WfpTransportAllowanceTransaction_TrxAttendanceDaily_Attenda~\" TO \"FK_WfpTransportAllowanceTransaction_HrdAttendanceDaily_Attenda~\";");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse PHASE D - Restore foreign-key constraint names.
            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"WfpTransportAllowanceTransaction\" RENAME CONSTRAINT \"FK_WfpTransportAllowanceTransaction_HrdAttendanceDaily_Attenda~\" TO \"FK_WfpTransportAllowanceTransaction_TrxAttendanceDaily_Attenda~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"WfpOvertimeRequest\" RENAME CONSTRAINT \"FK_WfpOvertimeRequest_HrdAttendanceDaily_AttendanceDailyId\" TO \"FK_WfpOvertimeRequest_TrxAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxTravelAttendanceLink\" RENAME CONSTRAINT \"FK_TrxTravelAttendanceLink_HrdBusinessTripAttendance_BusinessT~\" TO \"FK_TrxTravelAttendanceLink_TrxBusinessTripAttendance_BusinessT~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxTravelAttendanceLink\" RENAME CONSTRAINT \"FK_TrxTravelAttendanceLink_HrdAttendanceDaily_AttendanceDailyId\" TO \"FK_TrxTravelAttendanceLink_TrxAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxPayrollAttendanceInput\" RENAME CONSTRAINT \"FK_TrxPayrollAttendanceInput_HrdAttendanceDaily_AttendanceDail~\" TO \"FK_TrxPayrollAttendanceInput_TrxAttendanceDaily_AttendanceDail~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxOvertimeRequestDetail\" RENAME CONSTRAINT \"FK_TrxOvertimeRequestDetail_HrdAttendanceDaily_AttendanceDaily~\" TO \"FK_TrxOvertimeRequestDetail_TrxAttendanceDaily_AttendanceDaily~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxOvertimeRealizationDetail\" RENAME CONSTRAINT \"FK_TrxOvertimeRealizationDetail_HrdAttendanceDaily_AttendanceD~\" TO \"FK_TrxOvertimeRealizationDetail_TrxAttendanceDaily_AttendanceD~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxOvertimeRealization\" RENAME CONSTRAINT \"FK_TrxOvertimeRealization_HrdAttendanceDaily_AttendanceDailyId\" TO \"FK_TrxOvertimeRealization_TrxAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"TrxLeaveAttendanceIntegration\" RENAME CONSTRAINT \"FK_TrxLeaveAttendanceIntegration_HrdAttendanceDaily_Attendance~\" TO \"FK_TrxLeaveAttendanceIntegration_TrxAttendanceDaily_Attendance~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceCorrectionRequest\" RENAME CONSTRAINT \"FK_HrdAttendanceCorrectionRequest_HrdAttendanceDaily_Attendanc~\" TO \"FK_HrdAttendanceCorrectionRequest_TrxAttendanceDaily_Attendanc~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendance\" RENAME CONSTRAINT \"FK_HrdAttendance_HrdAttendanceDaily_AttendanceDailyId\" TO \"FK_HrdAttendance_TrxAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"FK_HrdAttendanceDailySegment_TrxShiftAssignment_ShiftAssignmen~\" TO \"FK_TrxAttendanceDailySegment_TrxShiftAssignment_ShiftAssignmen~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"FK_HrdAttendanceDailySegment_HrdAttendanceRawLog_StartRawLogId\" TO \"FK_TrxAttendanceDailySegment_TrxAttendanceRawLog_StartRawLogId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"FK_HrdAttendanceDailySegment_HrdAttendanceRawLog_EndRawLogId\" TO \"FK_TrxAttendanceDailySegment_TrxAttendanceRawLog_EndRawLogId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"FK_HrdAttendanceDailySegment_HrdAttendanceDaily_AttendanceDail~\" TO \"FK_TrxAttendanceDailySegment_TrxAttendanceDaily_AttendanceDail~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_HrdMissingAttendance_HrdAttendanceException_AttendanceExcep~\" TO \"FK_TrxMissingAttendance_TrxAttendanceException_AttendanceExcep~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_HrdMissingAttendance_HrdAttendanceDaily_AttendanceDailyId\" TO \"FK_TrxMissingAttendance_TrxAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_HrdMissingAttendance_MstWorkforceProfile_WorkforceProfileId\" TO \"FK_TrxMissingAttendance_MstWorkforceProfile_WorkforceProfileId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_HrdMissingAttendance_MstWorkSchedule_WorkScheduleId\" TO \"FK_TrxMissingAttendance_MstWorkSchedule_WorkScheduleId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_HrdMissingAttendance_MstShift_ShiftId\" TO \"FK_TrxMissingAttendance_MstShift_ShiftId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_HrdMissingAttendance_HrdAttendanceCorrectionRequest_Attenda~\" TO \"FK_TrxMissingAttendance_HrdAttendanceCorrectionRequest_Attenda~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_HrdMissingAttendance_AspNetUsers_UserId\" TO \"FK_TrxMissingAttendance_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"FK_HrdMissingAttendance_AspNetUsers_ResolvedByUserId\" TO \"FK_TrxMissingAttendance_AspNetUsers_ResolvedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_HrdRemoteAttendance_HrdAttendanceDaily_AttendanceDailyId\" TO \"FK_TrxRemoteAttendance_TrxAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_HrdRemoteAttendance_MstWorkforceProfile_WorkforceProfileId\" TO \"FK_TrxRemoteAttendance_MstWorkforceProfile_WorkforceProfileId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_HrdRemoteAttendance_MstAttendancePolicy_AttendancePolicyId\" TO \"FK_TrxRemoteAttendance_MstAttendancePolicy_AttendancePolicyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_HrdRemoteAttendance_MstAttendanceLocation_AttendanceLocatio~\" TO \"FK_TrxRemoteAttendance_MstAttendanceLocation_AttendanceLocatio~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_HrdRemoteAttendance_AspNetUsers_UserId\" TO \"FK_TrxRemoteAttendance_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"FK_HrdRemoteAttendance_AspNetUsers_ApprovedByUserId\" TO \"FK_TrxRemoteAttendance_AspNetUsers_ApprovedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_HrdBusinessTripAttendance_HrdAttendanceDaily_AttendanceDail~\" TO \"FK_TrxBusinessTripAttendance_TrxAttendanceDaily_AttendanceDail~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_HrdBusinessTripAttendance_MstWorkforceProfile_WorkforceProf~\" TO \"FK_TrxBusinessTripAttendance_MstWorkforceProfile_WorkforceProf~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_HrdBusinessTripAttendance_MstHospitalSite_HospitalSiteId\" TO \"FK_TrxBusinessTripAttendance_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_HrdBusinessTripAttendance_MstAttendanceLocation_AttendanceL~\" TO \"FK_TrxBusinessTripAttendance_MstAttendanceLocation_AttendanceL~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_HrdBusinessTripAttendance_AspNetUsers_UserId\" TO \"FK_TrxBusinessTripAttendance_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"FK_HrdBusinessTripAttendance_AspNetUsers_ApprovedByUserId\" TO \"FK_TrxBusinessTripAttendance_AspNetUsers_ApprovedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_HrdAttendanceDaily_ProcessedAttendanceD~\" TO \"FK_TrxAttendanceRawLog_TrxAttendanceDaily_ProcessedAttendanceD~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_MstWorkforceProfile_WorkforceProfileId\" TO \"FK_TrxAttendanceRawLog_MstWorkforceProfile_WorkforceProfileId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_MstHospitalSite_HospitalSiteId\" TO \"FK_TrxAttendanceRawLog_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_MstEmployee_EmployeeId\" TO \"FK_TrxAttendanceRawLog_MstEmployee_EmployeeId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_MstDoctor_DoctorId\" TO \"FK_TrxAttendanceRawLog_MstDoctor_DoctorId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_MstAttendanceLocation_AttendanceLocatio~\" TO \"FK_TrxAttendanceRawLog_MstAttendanceLocation_AttendanceLocatio~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_MstAttendanceDevice_AttendanceDeviceId\" TO \"FK_TrxAttendanceRawLog_MstAttendanceDevice_AttendanceDeviceId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_HrdAttendance_ProcessedAttendanceId\" TO \"FK_TrxAttendanceRawLog_HrdAttendance_ProcessedAttendanceId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"FK_HrdAttendanceRawLog_AspNetUsers_UserId\" TO \"FK_TrxAttendanceRawLog_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"FK_HrdAttendanceException_HrdAttendanceDaily_AttendanceDailyId\" TO \"FK_TrxAttendanceException_TrxAttendanceDaily_AttendanceDailyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"FK_HrdAttendanceException_MstWorkforceProfile_WorkforceProfile~\" TO \"FK_TrxAttendanceException_MstWorkforceProfile_WorkforceProfile~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"FK_HrdAttendanceException_HrdAttendanceCorrectionRequest_Corre~\" TO \"FK_TrxAttendanceException_HrdAttendanceCorrectionRequest_Corre~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"FK_HrdAttendanceException_AspNetUsers_ResolvedByUserId\" TO \"FK_TrxAttendanceException_AspNetUsers_ResolvedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_HrdAttendanceSchedulerJob_HrdAttendanceProcessingRun_Proces~\" TO \"FK_TrxAttendanceSchedulerJob_TrxAttendanceProcessingRun_Proces~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_HrdAttendanceSchedulerJob_HrdAttendancePeriod_AttendancePer~\" TO \"FK_TrxAttendanceSchedulerJob_TrxAttendancePeriod_AttendancePer~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_HrdAttendanceSchedulerJob_MstWorkforceProfile_WorkforceProf~\" TO \"FK_TrxAttendanceSchedulerJob_MstWorkforceProfile_WorkforceProf~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_HrdAttendanceSchedulerJob_MstOrganizationUnit_OrganizationU~\" TO \"FK_TrxAttendanceSchedulerJob_MstOrganizationUnit_OrganizationU~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_HrdAttendanceSchedulerJob_MstHospitalSite_HospitalSiteId\" TO \"FK_TrxAttendanceSchedulerJob_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_HrdAttendanceSchedulerJob_MstDepartment_DepartmentId\" TO \"FK_TrxAttendanceSchedulerJob_MstDepartment_DepartmentId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_HrdAttendanceSchedulerJob_AspNetUsers_TriggeredByUserId\" TO \"FK_TrxAttendanceSchedulerJob_AspNetUsers_TriggeredByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"FK_HrdAttendanceSchedulerJob_AspNetUsers_CancelledByUserId\" TO \"FK_TrxAttendanceSchedulerJob_AspNetUsers_CancelledByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_WfpWorkScheduleAssignment_WorkScheduleAs~\" TO \"FK_TrxAttendanceDaily_WfpWorkScheduleAssignment_WorkScheduleAs~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_WfpOrganizationAssignment_OrganizationAs~\" TO \"FK_TrxAttendanceDaily_WfpOrganizationAssignment_OrganizationAs~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_TrxShiftAssignment_PrimaryShiftAssignmen~\" TO \"FK_TrxAttendanceDaily_TrxShiftAssignment_PrimaryShiftAssignmen~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_HrdAttendancePeriod_AttendancePeriodId\" TO \"FK_TrxAttendanceDaily_TrxAttendancePeriod_AttendancePeriodId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstWorkforceProfile_WorkforceProfileId\" TO \"FK_TrxAttendanceDaily_MstWorkforceProfile_WorkforceProfileId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstWorkSchedule_WorkScheduleId\" TO \"FK_TrxAttendanceDaily_MstWorkSchedule_WorkScheduleId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstWorkLocation_WorkLocationId\" TO \"FK_TrxAttendanceDaily_MstWorkLocation_WorkLocationId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstShift_ShiftId\" TO \"FK_TrxAttendanceDaily_MstShift_ShiftId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstPosition_PositionId\" TO \"FK_TrxAttendanceDaily_MstPosition_PositionId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstPayrollPeriod_PayrollPeriodId\" TO \"FK_TrxAttendanceDaily_MstPayrollPeriod_PayrollPeriodId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstOrganizationUnit_OrganizationUnitId\" TO \"FK_TrxAttendanceDaily_MstOrganizationUnit_OrganizationUnitId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstHospitalSite_HospitalSiteId\" TO \"FK_TrxAttendanceDaily_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstGracePeriodPolicy_GracePeriodPolicyId\" TO \"FK_TrxAttendanceDaily_MstGracePeriodPolicy_GracePeriodPolicyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstEmployee_EmployeeId\" TO \"FK_TrxAttendanceDaily_MstEmployee_EmployeeId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstDoctor_DoctorId\" TO \"FK_TrxAttendanceDaily_MstDoctor_DoctorId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstDepartment_DepartmentId\" TO \"FK_TrxAttendanceDaily_MstDepartment_DepartmentId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_MstAttendancePolicy_AttendancePolicyId\" TO \"FK_TrxAttendanceDaily_MstAttendancePolicy_AttendancePolicyId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"FK_HrdAttendanceDaily_AspNetUsers_UserId\" TO \"FK_TrxAttendanceDaily_AspNetUsers_UserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_HrdAttendancePeriod_HrdAttendanceProcessingRun_LastProcessi~\" TO \"FK_TrxAttendancePeriod_TrxAttendanceProcessingRun_LastProcessi~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_HrdAttendancePeriod_MstOrganizationUnit_OrganizationUnitId\" TO \"FK_TrxAttendancePeriod_MstOrganizationUnit_OrganizationUnitId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_HrdAttendancePeriod_MstLegalEntity_LegalEntityId\" TO \"FK_TrxAttendancePeriod_MstLegalEntity_LegalEntityId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_HrdAttendancePeriod_MstHospitalSite_HospitalSiteId\" TO \"FK_TrxAttendancePeriod_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_HrdAttendancePeriod_MstDepartment_DepartmentId\" TO \"FK_TrxAttendancePeriod_MstDepartment_DepartmentId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_HrdAttendancePeriod_AspNetUsers_ReopenedByUserId\" TO \"FK_TrxAttendancePeriod_AspNetUsers_ReopenedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"FK_HrdAttendancePeriod_AspNetUsers_ClosedByUserId\" TO \"FK_TrxAttendancePeriod_AspNetUsers_ClosedByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_HrdAttendanceProcessingRun_MstWorkforceProfile_TargetWorkfo~\" TO \"FK_TrxAttendanceProcessingRun_MstWorkforceProfile_TargetWorkfo~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_HrdAttendanceProcessingRun_MstOrganizationUnit_Organization~\" TO \"FK_TrxAttendanceProcessingRun_MstOrganizationUnit_Organization~\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_HrdAttendanceProcessingRun_MstHospitalSite_HospitalSiteId\" TO \"FK_TrxAttendanceProcessingRun_MstHospitalSite_HospitalSiteId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_HrdAttendanceProcessingRun_MstDepartment_DepartmentId\" TO \"FK_TrxAttendanceProcessingRun_MstDepartment_DepartmentId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_HrdAttendanceProcessingRun_AspNetUsers_TriggeredByUserId\" TO \"FK_TrxAttendanceProcessingRun_AspNetUsers_TriggeredByUserId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"FK_HrdAttendanceProcessingRun_AspNetUsers_CancelledByUserId\" TO \"FK_TrxAttendanceProcessingRun_AspNetUsers_CancelledByUserId\";");

            // Reverse PHASE C - Restore index names.
            migrationBuilder.RenameIndex(
                name: "IX_HrdRemoteAttendance_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_TrxRemoteAttendance_WorkforceProfileId_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_HrdRemoteAttendance_UserId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_TrxRemoteAttendance_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdRemoteAttendance_AttendancePolicyId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_TrxRemoteAttendance_AttendancePolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdRemoteAttendance_AttendanceLocationId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_TrxRemoteAttendance_AttendanceLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdRemoteAttendance_AttendanceDailyId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_TrxRemoteAttendance_AttendanceDailyId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdRemoteAttendance_ApprovedByUserId",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_TrxRemoteAttendance_ApprovedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdRemoteAttendance_ApprovalStatus_AttendanceDate",
                schema: "public",
                table: "HrdRemoteAttendance",
                newName: "IX_TrxRemoteAttendance_ApprovalStatus_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_WorkScheduleId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_WorkScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_WorkforceProfileId_AttendanceDate_Miss~",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_WorkforceProfileId_AttendanceDate_Miss~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_UserId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_ShiftId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_ShiftId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_ResolvedByUserId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_ResolvedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_MissingStatus_IsPayrollBlocking_Attend~",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_MissingStatus_IsPayrollBlocking_Attend~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_AttendanceExceptionId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_AttendanceExceptionId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_AttendanceDailyId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_AttendanceDailyId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdMissingAttendance_AttendanceCorrectionRequestId",
                schema: "public",
                table: "HrdMissingAttendance",
                newName: "IX_TrxMissingAttendance_AttendanceCorrectionRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdBusinessTripAttendance_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_TrxBusinessTripAttendance_WorkforceProfileId_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_HrdBusinessTripAttendance_UserId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_TrxBusinessTripAttendance_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdBusinessTripAttendance_ReferenceType_ReferenceId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_TrxBusinessTripAttendance_ReferenceType_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdBusinessTripAttendance_HospitalSiteId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_TrxBusinessTripAttendance_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdBusinessTripAttendance_AttendanceStatus_AttendanceDate",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_TrxBusinessTripAttendance_AttendanceStatus_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_HrdBusinessTripAttendance_AttendanceLocationId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_TrxBusinessTripAttendance_AttendanceLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdBusinessTripAttendance_AttendanceDailyId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_TrxBusinessTripAttendance_AttendanceDailyId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdBusinessTripAttendance_ApprovedByUserId",
                schema: "public",
                table: "HrdBusinessTripAttendance",
                newName: "IX_TrxBusinessTripAttendance_ApprovedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_WorkforceProfileId_HospitalSiteId~",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_WorkforceProfileId_HospitalSiteId~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_TriggeredByUserId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_TriggeredByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_StartDate_EndDate_JobType",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_StartDate_EndDate_JobType");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_ProcessingRunId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_ProcessingRunId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_OrganizationUnitId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_JobStatus_AvailableAt_Priority",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_JobStatus_AvailableAt_Priority");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_JobNumber",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_JobNumber");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_HospitalSiteId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_DepartmentId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_CorrelationId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_CorrelationId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_CancelledByUserId",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_CancelledByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceSchedulerJob_AttendancePeriodId_JobStatus",
                schema: "public",
                table: "HrdAttendanceSchedulerJob",
                newName: "IX_TrxAttendanceSchedulerJob_AttendancePeriodId_JobStatus");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_WorkforceProfileId_EventAt",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_WorkforceProfileId_EventAt");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_UserId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_SourceType_EventAt",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_SourceType_EventAt");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_ProcessingStatus_ReceivedAt",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_ProcessingStatus_ReceivedAt");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_ProcessedAttendanceId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_ProcessedAttendanceId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_ProcessedAttendanceDailyId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_ProcessedAttendanceDailyId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_HospitalSiteId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_EventHash",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_EventHash");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_EmployeeId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_DoctorId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_DeviceUserKey_EventAt",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_DeviceUserKey_EventAt");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_AttendanceLocationId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_AttendanceLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceRawLog_AttendanceDeviceId_ExternalLogId",
                schema: "public",
                table: "HrdAttendanceRawLog",
                newName: "IX_TrxAttendanceRawLog_AttendanceDeviceId_ExternalLogId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_TriggeredByUserId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_TriggeredByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_TargetWorkforceProfileId_StartDa~",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_TargetWorkforceProfileId_StartDa~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_StartDate_EndDate_ProcessingMode",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_StartDate_EndDate_ProcessingMode");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_RunStatus_StartedAt",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_RunStatus_StartedAt");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_RunNumber",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_RunNumber");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_OrganizationUnitId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_HospitalSiteId_OrganizationUnitI~",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_HospitalSiteId_OrganizationUnitI~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_DepartmentId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_CorrelationId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_CorrelationId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceProcessingRun_CancelledByUserId",
                schema: "public",
                table: "HrdAttendanceProcessingRun",
                newName: "IX_TrxAttendanceProcessingRun_CancelledByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_StartDate_EndDate_PeriodStatus",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_StartDate_EndDate_PeriodStatus");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_ReopenedByUserId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_ReopenedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_PeriodStatus_ScheduledCloseAt_IsActive_~",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_PeriodStatus_ScheduledCloseAt_IsActive_~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_PeriodCode",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_PeriodCode");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_OrganizationUnitId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_LegalEntityId_HospitalSiteId_Organizati~",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_LegalEntityId_HospitalSiteId_Organizati~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_LastProcessingRunId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_LastProcessingRunId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_HospitalSiteId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_DepartmentId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendancePeriod_ClosedByUserId",
                schema: "public",
                table: "HrdAttendancePeriod",
                newName: "IX_TrxAttendancePeriod_ClosedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceException_WorkforceProfileId_DetectedAt",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_TrxAttendanceException_WorkforceProfileId_DetectedAt");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceException_ResolvedByUserId",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_TrxAttendanceException_ResolvedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceException_ExceptionStatus_Severity_IsPayrollBl~",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_TrxAttendanceException_ExceptionStatus_Severity_IsPayrollBl~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceException_CorrectionRequestId",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_TrxAttendanceException_CorrectionRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceException_AttendanceDailyId_ExceptionCode",
                schema: "public",
                table: "HrdAttendanceException",
                newName: "IX_TrxAttendanceException_AttendanceDailyId_ExceptionCode");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDailySegment_StartRawLogId",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_TrxAttendanceDailySegment_StartRawLogId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDailySegment_ShiftAssignmentId_SegmentType",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_TrxAttendanceDailySegment_ShiftAssignmentId_SegmentType");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDailySegment_SegmentType_SegmentStatus",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_TrxAttendanceDailySegment_SegmentType_SegmentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDailySegment_EndRawLogId",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_TrxAttendanceDailySegment_EndRawLogId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDailySegment_AttendanceDailyId_SegmentOrder",
                schema: "public",
                table: "HrdAttendanceDailySegment",
                newName: "IX_TrxAttendanceDailySegment_AttendanceDailyId_SegmentOrder");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_WorkScheduleId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_WorkScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_WorkScheduleAssignmentId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_WorkScheduleAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_WorkLocationId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_WorkforceProfileId_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_UserId_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_UserId_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_ShiftId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_ShiftId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_ScheduleSource_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_ScheduleSource_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_ProcessingStatus_PayrollInputStatus_Atte~",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_ProcessingStatus_PayrollInputStatus_Atte~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_PrimaryShiftAssignmentId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_PrimaryShiftAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_PositionId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_PayrollPeriodId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_PayrollPeriodId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_OrganizationUnitId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_OrganizationAssignmentId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_OrganizationAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_IsLate_IsEarlyLeave_HasMissingPunch_Atte~",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_IsLate_IsEarlyLeave_HasMissingPunch_Atte~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_HospitalSiteId_OrganizationUnitId_Depart~",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_HospitalSiteId_OrganizationUnitId_Depart~");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_GracePeriodPolicyId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_GracePeriodPolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_EmployeeId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_DoctorId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_DepartmentId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_AttendanceStatus_AttendanceDate",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_AttendanceStatus_AttendanceDate");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_AttendancePolicyId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_AttendancePolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_HrdAttendanceDaily_AttendancePeriodId",
                schema: "public",
                table: "HrdAttendanceDaily",
                newName: "IX_TrxAttendanceDaily_AttendancePeriodId");

            // Reverse PHASE B - Restore primary-key constraint names.
            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDailySegment\" RENAME CONSTRAINT \"PK_HrdAttendanceDailySegment\" TO \"PK_TrxAttendanceDailySegment\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdMissingAttendance\" RENAME CONSTRAINT \"PK_HrdMissingAttendance\" TO \"PK_TrxMissingAttendance\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdRemoteAttendance\" RENAME CONSTRAINT \"PK_HrdRemoteAttendance\" TO \"PK_TrxRemoteAttendance\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdBusinessTripAttendance\" RENAME CONSTRAINT \"PK_HrdBusinessTripAttendance\" TO \"PK_TrxBusinessTripAttendance\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceRawLog\" RENAME CONSTRAINT \"PK_HrdAttendanceRawLog\" TO \"PK_TrxAttendanceRawLog\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceException\" RENAME CONSTRAINT \"PK_HrdAttendanceException\" TO \"PK_TrxAttendanceException\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceSchedulerJob\" RENAME CONSTRAINT \"PK_HrdAttendanceSchedulerJob\" TO \"PK_TrxAttendanceSchedulerJob\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceDaily\" RENAME CONSTRAINT \"PK_HrdAttendanceDaily\" TO \"PK_TrxAttendanceDaily\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendancePeriod\" RENAME CONSTRAINT \"PK_HrdAttendancePeriod\" TO \"PK_TrxAttendancePeriod\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"public\".\"HrdAttendanceProcessingRun\" RENAME CONSTRAINT \"PK_HrdAttendanceProcessingRun\" TO \"PK_TrxAttendanceProcessingRun\";");

            // Reverse PHASE A - Restore the original physical table names.
            migrationBuilder.RenameTable(
                name: "HrdRemoteAttendance",
                schema: "public",
                newName: "TrxRemoteAttendance");

            migrationBuilder.RenameTable(
                name: "HrdMissingAttendance",
                schema: "public",
                newName: "TrxMissingAttendance");

            migrationBuilder.RenameTable(
                name: "HrdBusinessTripAttendance",
                schema: "public",
                newName: "TrxBusinessTripAttendance");

            migrationBuilder.RenameTable(
                name: "HrdAttendanceSchedulerJob",
                schema: "public",
                newName: "TrxAttendanceSchedulerJob");

            migrationBuilder.RenameTable(
                name: "HrdAttendanceRawLog",
                schema: "public",
                newName: "TrxAttendanceRawLog");

            migrationBuilder.RenameTable(
                name: "HrdAttendanceProcessingRun",
                schema: "public",
                newName: "TrxAttendanceProcessingRun");

            migrationBuilder.RenameTable(
                name: "HrdAttendancePeriod",
                schema: "public",
                newName: "TrxAttendancePeriod");

            migrationBuilder.RenameTable(
                name: "HrdAttendanceException",
                schema: "public",
                newName: "TrxAttendanceException");

            migrationBuilder.RenameTable(
                name: "HrdAttendanceDailySegment",
                schema: "public",
                newName: "TrxAttendanceDailySegment");

            migrationBuilder.RenameTable(
                name: "HrdAttendanceDaily",
                schema: "public",
                newName: "TrxAttendanceDaily");

        }
    }
}
