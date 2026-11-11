using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Migrasi Skema DB Ekstensi Model WorkflowManagement V2 & Index Komposit Performa Tinggi (BE-HRD-302, HRD-DEC-182 s.d. 187)
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261109200000_AddWorkflowAndUnifiedApprovalV2")]
    public partial class AddWorkflowAndUnifiedApprovalV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- BE-HRD-302: Ekstensi Kolom TrxWorkflowInstance V2
                ALTER TABLE ""TrxWorkflowInstance"" ADD COLUMN IF NOT EXISTS ""PriorityLevel"" character varying(30) DEFAULT 'Normal';
                ALTER TABLE ""TrxWorkflowInstance"" ADD COLUMN IF NOT EXISTS ""SlaDeadline"" timestamp with time zone;
                ALTER TABLE ""TrxWorkflowInstance"" ADD COLUMN IF NOT EXISTS ""IsOverdue"" boolean DEFAULT false;
                ALTER TABLE ""TrxWorkflowInstance"" ADD COLUMN IF NOT EXISTS ""IsDelegatedAction"" boolean DEFAULT false;
                ALTER TABLE ""TrxWorkflowInstance"" ADD COLUMN IF NOT EXISTS ""OnBehalfOfEmployeeId"" uuid;
                ALTER TABLE ""TrxWorkflowInstance"" ADD COLUMN IF NOT EXISTS ""Amount"" numeric(18,2);
                ALTER TABLE ""TrxWorkflowInstance"" ADD COLUMN IF NOT EXISTS ""TotalSteps"" integer DEFAULT 1;
                ALTER TABLE ""TrxWorkflowInstance"" ADD COLUMN IF NOT EXISTS ""FinalizedAt"" timestamp with time zone;

                -- BE-HRD-302: Ekstensi Kolom TrxWorkflowApproverAssignment V2
                ALTER TABLE ""TrxWorkflowApproverAssignment"" ADD COLUMN IF NOT EXISTS ""ApproverEmployeeId"" uuid;
                ALTER TABLE ""TrxWorkflowApproverAssignment"" ADD COLUMN IF NOT EXISTS ""DelegatedEmployeeId"" uuid;

                -- BE-HRD-302: Ekstensi Kolom TrxApprovalAction V2
                ALTER TABLE ""TrxApprovalAction"" ADD COLUMN IF NOT EXISTS ""ActionEmployeeId"" uuid;
                ALTER TABLE ""TrxApprovalAction"" ADD COLUMN IF NOT EXISTS ""OnBehalfOfEmployeeId"" uuid;
                ALTER TABLE ""TrxApprovalAction"" ADD COLUMN IF NOT EXISTS ""DigitalSignatureHash"" character varying(256);
                ALTER TABLE ""TrxApprovalAction"" ADD COLUMN IF NOT EXISTS ""ReasonComment"" character varying(2000);

                -- BE-HRD-302: Ekstensi Kolom TrxApprovalDelegation V2
                ALTER TABLE ""TrxApprovalDelegation"" ADD COLUMN IF NOT EXISTS ""PrincipalEmployeeId"" uuid;
                ALTER TABLE ""TrxApprovalDelegation"" ADD COLUMN IF NOT EXISTS ""DelegateEmployeeId"" uuid;
                ALTER TABLE ""TrxApprovalDelegation"" ADD COLUMN IF NOT EXISTS ""DelegatedRequestTypes"" character varying(500);
                ALTER TABLE ""TrxApprovalDelegation"" ADD COLUMN IF NOT EXISTS ""StartDate"" timestamp with time zone;
                ALTER TABLE ""TrxApprovalDelegation"" ADD COLUMN IF NOT EXISTS ""EndDate"" timestamp with time zone;

                -- Index Gabungan Performa Tinggi Pemanggilan Inbox Atasan & SLA Overdue (BE-HRD-302)
                CREATE INDEX IF NOT EXISTS ix_trx_workflow_approver_assignment_approver_status 
                    ON ""TrxWorkflowApproverAssignment"" (""ApproverEmployeeId"", ""AssignmentStatus"");

                CREATE INDEX IF NOT EXISTS ix_trx_workflow_instance_applicant_status 
                    ON ""TrxWorkflowInstance"" (""RequestedByEmployeeId"", ""WorkflowStatus"");

                CREATE INDEX IF NOT EXISTS ix_trx_workflow_instance_overdue_deadline 
                    ON ""TrxWorkflowInstance"" (""IsOverdue"", ""SlaDeadline"");

                CREATE INDEX IF NOT EXISTS ix_trx_approval_delegation_active_dates 
                    ON ""TrxApprovalDelegation"" (""DelegateEmployeeId"", ""IsActive"", ""StartDate"", ""EndDate"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ix_trx_workflow_approver_assignment_approver_status;
                DROP INDEX IF EXISTS ix_trx_workflow_instance_applicant_status;
                DROP INDEX IF EXISTS ix_trx_workflow_instance_overdue_deadline;
                DROP INDEX IF EXISTS ix_trx_approval_delegation_active_dates;
            ");
        }
    }
}
