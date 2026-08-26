using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class NormalizeAttendanceCorrectionFamilyToHrd : Migration
    {
        protected override void Up(MigrationBuilder b)
        {
            Inbound(b, false, true);

            b.RenameTable(
                "TrxAttendanceCorrectionRequest",
                "public",
                "HrdAttendanceCorrectionRequest",
                "public");

            b.RenameTable(
                "TrxAttendanceCorrectionDetail",
                "public",
                "HrdAttendanceCorrectionDetail",
                "public");

            b.RenameTable(
                "TrxAttendanceCorrectionApproval",
                "public",
                "HrdAttendanceCorrectionApproval",
                "public");

            Indexes(b, false);
            Constraints(b, false);
            Inbound(b, true);

            b.Sql("""
        UPDATE public."TrxWorkflowInstance"
        SET "ReferenceType" = 'HrdAttendanceCorrectionRequest'
        WHERE "ReferenceType" = 'TrxAttendanceCorrectionRequest';
        """);
        }

        protected override void Down(MigrationBuilder b)
        {
            b.Sql("""
        UPDATE public."TrxWorkflowInstance"
        SET "ReferenceType" = 'TrxAttendanceCorrectionRequest'
        WHERE "ReferenceType" = 'HrdAttendanceCorrectionRequest';
        """);

            Inbound(b, true, true);
            Indexes(b, true);
            Constraints(b, true);

            b.RenameTable(
                "HrdAttendanceCorrectionApproval",
                "public",
                "TrxAttendanceCorrectionApproval",
                "public");

            b.RenameTable(
                "HrdAttendanceCorrectionDetail",
                "public",
                "TrxAttendanceCorrectionDetail",
                "public");

            b.RenameTable(
                "HrdAttendanceCorrectionRequest",
                "public",
                "TrxAttendanceCorrectionRequest",
                "public");

            Inbound(b, false);
        }

        private static void Inbound(MigrationBuilder b, bool hrd, bool drop = false)
        {
            var target = hrd
                ? "HrdAttendanceCorrectionRequest"
                : "TrxAttendanceCorrectionRequest";

            var exceptionName =
                "FK_TrxAttendanceException_" + target + "_Corre~";

            var missingName =
                "FK_TrxMissingAttendance_" + target + "_Attenda~";

            if (drop)
            {
                b.DropForeignKey(
                    name: exceptionName,
                    table: "TrxAttendanceException",
                    schema: "public");

                b.DropForeignKey(
                    name: missingName,
                    table: "TrxMissingAttendance",
                    schema: "public");

                return;
            }

            b.AddForeignKey(
                name: exceptionName,
                table: "TrxAttendanceException",
                column: "CorrectionRequestId",
                principalTable: target,
                schema: "public",
                principalSchema: "public",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            b.AddForeignKey(
                name: missingName,
                table: "TrxMissingAttendance",
                column: "AttendanceCorrectionRequestId",
                principalTable: target,
                schema: "public",
                principalSchema: "public",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        private static void Indexes(MigrationBuilder b, bool down)
        {
            IndexGroup(b, "HrdAttendanceCorrectionApproval", "TrxAttendanceCorrectionApproval", "HrdAttendanceCorrectionApproval", down, new[] { "ActualActionByUserId", "ActualActionByWorkforceProf~", "AssignedApproverUserId_Appr~", "AssignedApproverWorkforcePr~", "AttendanceCorrectionRequest~", "DelegatedFromUserId", "RejectionReasonId", "WorkflowStepId" });
            IndexGroup(b, "HrdAttendanceCorrectionDetail", "TrxAttendanceCorrectionDetail", "HrdAttendanceCorrectionDetail", down, new[] { "AppliedByUserId", "AttendanceCorrectionRequestId~", "DetailStatus_IsApplied" });
            IndexGroup(b, "HrdAttendanceCorrectionRequest", "TrxAttendanceCorrectionRequest", "HrdAttendanceCorrectionRequest", down, new[] { "AppliedByUserId", "AttendanceDailyId", "AttendanceId", "RejectionReasonId", "RequestedByUserId", "RequestedByWorkforceProfileId", "RequestNumber", "RequestReasonId", "RequestStatus_SubmittedAt", "WorkflowDefinitionId_Request~", "WorkflowInstanceId", "WorkforceProfileId_Attendanc~" });
        }

        private static void IndexGroup(
    MigrationBuilder b,
    string table,
    string trx,
    string hrd,
    bool down,
    string[] suffixes)
        {
            foreach (var suffix in suffixes)
            {
                var oldName = $"IX_{(down ? hrd : trx)}_{suffix}";
                var newName = $"IX_{(down ? trx : hrd)}_{suffix}";

                b.RenameIndex(
                    name: oldName,
                    newName: newName,
                    table: table,
                    schema: "public");
            }
        }

        private static void Constraints(MigrationBuilder b, bool down)
        {
            ConstraintGroup(b, "HrdAttendanceCorrectionRequest", "TrxAttendanceCorrectionRequest", "HrdAttendanceCorrectionRequest", down, new[] { "AspNetUsers_AppliedByUserId", "AspNetUsers_RequestedByUserId", "HrdAttendance_AttendanceId", "MstRejectionReason_Rejection~", "MstRequestReason_RequestReas~", "MstWorkflowDefinition_Workfl~", "MstWorkforceProfile_Requeste~", "MstWorkforceProfile_Workforc~", "TrxAttendanceDaily_Attendanc~", "TrxWorkflowInstance_Workflow~" });
            ConstraintGroup(b, "HrdAttendanceCorrectionApproval", "TrxAttendanceCorrectionApproval", "HrdAttendanceCorrectionApproval", down, new[] { "AspNetUsers_ActualActionByU~", "AspNetUsers_AssignedApprove~", "AspNetUsers_DelegatedFromUs~", "MstRejectionReason_Rejectio~", "MstWorkflowStep_WorkflowSte~", "MstWorkforceProfile_ActualA~", "MstWorkforceProfile_Assigne~" });
            ConstraintGroup(b, "HrdAttendanceCorrectionDetail", "TrxAttendanceCorrectionDetail", "HrdAttendanceCorrectionDetail", down, new[] { "AspNetUsers_AppliedByUserId" });
            RenameConstraint(b, "HrdAttendanceCorrectionRequest", down ? "PK_HrdAttendanceCorrectionRequest" : "PK_TrxAttendanceCorrectionRequest", down ? "PK_TrxAttendanceCorrectionRequest" : "PK_HrdAttendanceCorrectionRequest");
            RenameConstraint(b, "HrdAttendanceCorrectionApproval", down ? "PK_HrdAttendanceCorrectionApproval" : "PK_TrxAttendanceCorrectionApproval", down ? "PK_TrxAttendanceCorrectionApproval" : "PK_HrdAttendanceCorrectionApproval");
            RenameConstraint(b, "HrdAttendanceCorrectionDetail", down ? "PK_HrdAttendanceCorrectionDetail" : "PK_TrxAttendanceCorrectionDetail", down ? "PK_TrxAttendanceCorrectionDetail" : "PK_HrdAttendanceCorrectionDetail");
            RenameConstraint(b, "HrdAttendanceCorrectionApproval", down ? "FK_HrdAttendanceCorrectionApproval_HrdAttendanceCorrectionRequ~" : "FK_TrxAttendanceCorrectionApproval_TrxAttendanceCorrectionRequ~", down ? "FK_TrxAttendanceCorrectionApproval_TrxAttendanceCorrectionRequ~" : "FK_HrdAttendanceCorrectionApproval_HrdAttendanceCorrectionRequ~");
            RenameConstraint(b, "HrdAttendanceCorrectionDetail", down ? "FK_HrdAttendanceCorrectionDetail_HrdAttendanceCorrectionReques~" : "FK_TrxAttendanceCorrectionDetail_TrxAttendanceCorrectionReques~", down ? "FK_TrxAttendanceCorrectionDetail_TrxAttendanceCorrectionReques~" : "FK_HrdAttendanceCorrectionDetail_HrdAttendanceCorrectionReques~");
        }

        private static void ConstraintGroup(MigrationBuilder b, string table, string trx, string hrd, bool down, string[] suffixes)
        {
            foreach (var suffix in suffixes)
            {
                RenameConstraint(b, table, $"FK_{(down ? hrd : trx)}_{suffix}", $"FK_{(down ? trx : hrd)}_{suffix}");
            }
        }

        private static void RenameConstraint(MigrationBuilder b, string table, string oldName, string newName)
        {
            b.Sql($"ALTER TABLE \"public\".\"{table}\" RENAME CONSTRAINT \"{oldName}\" TO \"{newName}\";");
        }
    }
}
