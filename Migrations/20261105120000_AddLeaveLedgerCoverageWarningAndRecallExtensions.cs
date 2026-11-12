using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261105120000_AddLeaveLedgerCoverageWarningAndRecallExtensions")]
    public partial class AddLeaveLedgerCoverageWarningAndRecallExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // 1. TABEL BARU: TrxLeaveRecallRequest (HRD-DEC-113, BE-HRD-117)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxLeaveRecallRequest') THEN
        CREATE TABLE public.""TrxLeaveRecallRequest"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""RecallNumber"" character varying(100) NOT NULL,
            ""LeaveRequestId"" uuid NOT NULL,
            ""WorkforceProfileId"" uuid NOT NULL,
            ""EffectiveRecallDate"" date NOT NULL,
            ""RestoredDays"" numeric(4,1) NOT NULL,
            ""EmergencyCategory"" character varying(50) NOT NULL,
            ""RecallReason"" text NOT NULL,
            ""Status"" character varying(50) NOT NULL DEFAULT 'RequestedByHeadUnit',
            ""RequestedByHeadUnitId"" uuid NOT NULL,
            ""RequestedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ""ApprovedByHrManagerId"" uuid NULL,
            ""ApprovedAt"" timestamptz NULL,
            ""RejectionReason"" character varying(500) NULL,
            ""IsActive"" boolean NOT NULL DEFAULT true,
            ""CreateDateTime"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ""CreateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""UpdateDateTime"" timestamptz NULL,
            ""UpdateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""DeleteDateTime"" timestamptz NULL,
            ""DeleteBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""CancelDateTime"" timestamptz NULL,
            ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""IsCancel"" boolean NOT NULL DEFAULT false,
            ""IsDelete"" boolean NOT NULL DEFAULT false,
            CONSTRAINT ""PK_TrxLeaveRecallRequest"" PRIMARY KEY (""Id"")
        );

        CREATE UNIQUE INDEX ""IX_TrxLeaveRecallRequest_RecallNumber"" 
            ON public.""TrxLeaveRecallRequest"" (""RecallNumber"") 
            WHERE ""IsDelete"" = false;

        CREATE INDEX ""IX_TrxLeaveRecallRequest_LeaveRequestId_Status"" 
            ON public.""TrxLeaveRecallRequest"" (""LeaveRequestId"", ""Status"", ""IsDelete"");

        CREATE INDEX ""IX_TrxLeaveRecallRequest_Workforce_RecallDate"" 
            ON public.""TrxLeaveRecallRequest"" (""WorkforceProfileId"", ""EffectiveRecallDate"", ""IsDelete"");
    END IF;
END $$;
");

            // =========================================================================
            // 2. TABEL BARU: TrxLeaveBalanceLedger (HRD-DEC-110, BE-HRD-117)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxLeaveBalanceLedger') THEN
        CREATE TABLE public.""TrxLeaveBalanceLedger"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""WorkforceProfileId"" uuid NOT NULL,
            ""LeaveTypeId"" uuid NOT NULL,
            ""PeriodYear"" integer NOT NULL,
            ""TransactionType"" character varying(50) NOT NULL,
            ""AmountDays"" numeric(4,1) NOT NULL,
            ""BalanceBefore"" numeric(4,1) NOT NULL,
            ""BalanceAfter"" numeric(4,1) NOT NULL,
            ""TransactionDate"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ""ReferenceRequestId"" uuid NULL,
            ""ReferenceRecallId"" uuid NULL,
            ""ReferenceAdjustmentId"" uuid NULL,
            ""ExpiryDate"" timestamptz NULL,
            ""IsForfeited"" boolean NOT NULL DEFAULT false,
            ""Notes"" character varying(500) NULL,
            ""IsActive"" boolean NOT NULL DEFAULT true,
            ""CreateDateTime"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ""CreateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""UpdateDateTime"" timestamptz NULL,
            ""UpdateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""DeleteDateTime"" timestamptz NULL,
            ""DeleteBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""CancelDateTime"" timestamptz NULL,
            ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ""IsCancel"" boolean NOT NULL DEFAULT false,
            ""IsDelete"" boolean NOT NULL DEFAULT false,
            CONSTRAINT ""PK_TrxLeaveBalanceLedger"" PRIMARY KEY (""Id"")
        );

        CREATE INDEX ""IX_TrxLeaveBalanceLedger_Workforce_LeaveType_Year"" 
            ON public.""TrxLeaveBalanceLedger"" (""WorkforceProfileId"", ""LeaveTypeId"", ""PeriodYear"", ""IsDelete"");

        CREATE INDEX ""IX_TrxLeaveBalanceLedger_PeriodYear_Type"" 
            ON public.""TrxLeaveBalanceLedger"" (""PeriodYear"", ""TransactionType"", ""IsDelete"");

        CREATE INDEX ""IX_TrxLeaveBalanceLedger_ExpiryDate_IsForfeited"" 
            ON public.""TrxLeaveBalanceLedger"" (""ExpiryDate"", ""IsForfeited"", ""IsDelete"");
    END IF;
END $$;
");

            // =========================================================================
            // 3. PERLUASAN KOLOM: TrxLeaveAdjustment (HRD-DEC-115, BE-HRD-117)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'TrxLeaveAdjustment' AND column_name = 'AuditDocumentNumber'
    ) THEN
        ALTER TABLE public.""TrxLeaveAdjustment"" 
            ADD COLUMN ""AuditDocumentNumber"" character varying(100) NULL;
    END IF;
END $$;
");

            // =========================================================================
            // 4. PERLUASAN KOLOM: WfpLeaveRequest (HRD-DEC-111, HRD-DEC-112, HRD-DEC-114, HRD-DEC-115)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'WfpLeaveRequest' AND column_name = 'HasStaffingCoverageWarning'
    ) THEN
        ALTER TABLE public.""WfpLeaveRequest"" 
            ADD COLUMN ""HasStaffingCoverageWarning"" boolean NOT NULL DEFAULT false;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'WfpLeaveRequest' AND column_name = 'CoverageWarningDetails'
    ) THEN
        ALTER TABLE public.""WfpLeaveRequest"" 
            ADD COLUMN ""CoverageWarningDetails"" text NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'WfpLeaveRequest' AND column_name = 'CoverageMitigationNotes'
    ) THEN
        ALTER TABLE public.""WfpLeaveRequest"" 
            ADD COLUMN ""CoverageMitigationNotes"" character varying(500) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'WfpLeaveRequest' AND column_name = 'IsDoctorNoteVerified'
    ) THEN
        ALTER TABLE public.""WfpLeaveRequest"" 
            ADD COLUMN ""IsDoctorNoteVerified"" boolean NOT NULL DEFAULT false;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'WfpLeaveRequest' AND column_name = 'DoctorNoteFaskesName'
    ) THEN
        ALTER TABLE public.""WfpLeaveRequest"" 
            ADD COLUMN ""DoctorNoteFaskesName"" character varying(255) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'WfpLeaveRequest' AND column_name = 'DoctorSipNumber'
    ) THEN
        ALTER TABLE public.""WfpLeaveRequest"" 
            ADD COLUMN ""DoctorSipNumber"" character varying(100) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'WfpLeaveRequest' AND column_name = 'IsPendingPayrollReconciliation'
    ) THEN
        ALTER TABLE public.""WfpLeaveRequest"" 
            ADD COLUMN ""IsPendingPayrollReconciliation"" boolean NOT NULL DEFAULT false;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'WfpLeaveRequest' AND column_name = 'IsPayrollClosed'
    ) THEN
        ALTER TABLE public.""WfpLeaveRequest"" 
            ADD COLUMN ""IsPayrollClosed"" boolean NOT NULL DEFAULT false;
    END IF;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TrxLeaveBalanceLedger", schema: "public");
            migrationBuilder.DropTable(name: "TrxLeaveRecallRequest", schema: "public");
        }
    }
}
