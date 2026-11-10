using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261105110000_AddAttendanceSliceB1Core")]
    public partial class AddAttendanceSliceB1Core : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // 1. TABEL BARU: HrdAttendanceSetting (HRD-DEC-105, HRD-DEC-106, BE-HRD-104)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdAttendanceSetting') THEN
        CREATE TABLE public.""HrdAttendanceSetting"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""HospitalCode"" character varying(50) NOT NULL,
            ""GracePeriodMinutes"" integer NOT NULL DEFAULT 10,
            ""MaxBackdatedCorrectionDays"" integer NOT NULL DEFAULT 3,
            ""RequireAttachmentForCorrection"" boolean NOT NULL DEFAULT true,
            ""AutoConvertIncompleteToAbsent"" boolean NOT NULL DEFAULT true,
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
            CONSTRAINT ""PK_HrdAttendanceSetting"" PRIMARY KEY (""Id"")
        );

        CREATE UNIQUE INDEX ""IX_HrdAttendanceSetting_HospitalCode"" 
            ON public.""HrdAttendanceSetting"" (""HospitalCode"") 
            WHERE ""IsDelete"" = false;

        -- Seed Default Setting Korporat RS MMC
        INSERT INTO public.""HrdAttendanceSetting"" (
            ""Id"", ""HospitalCode"", ""GracePeriodMinutes"", ""MaxBackdatedCorrectionDays"",
            ""RequireAttachmentForCorrection"", ""AutoConvertIncompleteToAbsent"", ""IsActive"", ""CreateDateTime""
        ) VALUES (
            gen_random_uuid(), 'RSMMC', 10, 3, true, true, true, CURRENT_TIMESTAMP
        );
    END IF;
END $$;
");

            // =========================================================================
            // 2. TABEL BARU: HrdEarlyLeavePermission (HRD-DEC-107, BE-HRD-104)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdEarlyLeavePermission') THEN
        CREATE TABLE public.""HrdEarlyLeavePermission"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""AttendanceDailyId"" uuid NOT NULL,
            ""WorkforceProfileId"" uuid NOT NULL,
            ""RequestedEarlyLeaveAt"" timestamptz NOT NULL,
            ""Reason"" character varying(500) NOT NULL,
            ""Status"" character varying(50) NOT NULL DEFAULT 'Draft',
            ""ApprovedByWorkforceId"" uuid NULL,
            ""ApprovedAt"" timestamptz NULL,
            ""RejectionReason"" character varying(500) NULL,
            ""IsDeductible"" boolean NOT NULL DEFAULT false,
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
            CONSTRAINT ""PK_HrdEarlyLeavePermission"" PRIMARY KEY (""Id"")
        );

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdAttendanceDaily') THEN
            ALTER TABLE public.""HrdEarlyLeavePermission""
                ADD CONSTRAINT ""FK_HrdEarlyLeavePermission_HrdAttendanceDaily_AttendanceDailyId""
                FOREIGN KEY (""AttendanceDailyId"") REFERENCES public.""HrdAttendanceDaily"" (""Id"") ON DELETE RESTRICT;
        END IF;

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstWorkforceProfiles') THEN
            ALTER TABLE public.""HrdEarlyLeavePermission""
                ADD CONSTRAINT ""FK_HrdEarlyLeavePermission_MstWorkforceProfiles_WorkforceProfileId""
                FOREIGN KEY (""WorkforceProfileId"") REFERENCES public.""MstWorkforceProfiles"" (""Id"") ON DELETE RESTRICT;

            ALTER TABLE public.""HrdEarlyLeavePermission""
                ADD CONSTRAINT ""FK_HrdEarlyLeavePermission_MstWorkforceProfiles_ApprovedByWorkforceId""
                FOREIGN KEY (""ApprovedByWorkforceId"") REFERENCES public.""MstWorkforceProfiles"" (""Id"") ON DELETE RESTRICT;
        END IF;

        CREATE INDEX ""IX_HrdEarlyLeavePermission_AttendanceDailyId""
            ON public.""HrdEarlyLeavePermission"" (""AttendanceDailyId"")
            WHERE ""IsDelete"" = false;

        CREATE INDEX ""IX_HrdEarlyLeavePermission_WorkforceProfileId""
            ON public.""HrdEarlyLeavePermission"" (""WorkforceProfileId"")
            WHERE ""IsDelete"" = false;
    END IF;
END $$;
");

            // =========================================================================
            // 3. TABEL BARU: HrdOfficialDutyRequest (HRD-DEC-109, BE-HRD-104)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdOfficialDutyRequest') THEN
        CREATE TABLE public.""HrdOfficialDutyRequest"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""WorkforceProfileId"" uuid NOT NULL,
            ""DutyCategory"" character varying(50) NOT NULL,
            ""AssignmentOrderNumber"" character varying(100) NOT NULL,
            ""StartAt"" timestamptz NOT NULL,
            ""EndAt"" timestamptz NOT NULL,
            ""Destination"" character varying(255) NOT NULL,
            ""Purpose"" text NOT NULL,
            ""Latitude"" numeric(10,8) NULL,
            ""Longitude"" numeric(11,8) NULL,
            ""AttachmentUrl"" character varying(500) NULL,
            ""Status"" character varying(50) NOT NULL DEFAULT 'Draft',
            ""ApprovedByWorkforceId"" uuid NULL,
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
            CONSTRAINT ""PK_HrdOfficialDutyRequest"" PRIMARY KEY (""Id""),
            CONSTRAINT ""CHK_HrdOfficialDutyRequest_Time"" CHECK (""EndAt"" > ""StartAt"")
        );

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstWorkforceProfiles') THEN
            ALTER TABLE public.""HrdOfficialDutyRequest""
                ADD CONSTRAINT ""FK_HrdOfficialDutyRequest_MstWorkforceProfiles_WorkforceProfileId""
                FOREIGN KEY (""WorkforceProfileId"") REFERENCES public.""MstWorkforceProfiles"" (""Id"") ON DELETE RESTRICT;

            ALTER TABLE public.""HrdOfficialDutyRequest""
                ADD CONSTRAINT ""FK_HrdOfficialDutyRequest_MstWorkforceProfiles_ApprovedByWorkforceId""
                FOREIGN KEY (""ApprovedByWorkforceId"") REFERENCES public.""MstWorkforceProfiles"" (""Id"") ON DELETE RESTRICT;
        END IF;

        CREATE INDEX ""IX_HrdOfficialDutyRequest_Workforce_Date""
            ON public.""HrdOfficialDutyRequest"" (""WorkforceProfileId"", ""StartAt"", ""EndAt"")
            WHERE ""IsDelete"" = false;
    END IF;
END $$;
");

            // =========================================================================
            // 4. TABEL BARU: HrdAttendancePeriodReopenLog (HRD-DEC-108, BE-HRD-104)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdAttendancePeriodReopenLog') THEN
        CREATE TABLE public.""HrdAttendancePeriodReopenLog"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""AttendancePeriodId"" uuid NOT NULL,
            ""ReopenedByUserId"" uuid NOT NULL,
            ""ReopenedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ""ReopenReason"" character varying(1000) NOT NULL,
            ""PreviousClosedAt"" timestamptz NULL,
            ""PreviousClosedByUserId"" uuid NULL,
            ""IsRollbackPayrollExecuted"" boolean NOT NULL DEFAULT false,
            ""RollbackPayrollNotes"" text NULL,
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
            CONSTRAINT ""PK_HrdAttendancePeriodReopenLog"" PRIMARY KEY (""Id"")
        );

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdAttendancePeriod') THEN
            ALTER TABLE public.""HrdAttendancePeriodReopenLog""
                ADD CONSTRAINT ""FK_HrdAttendancePeriodReopenLog_HrdAttendancePeriod_AttendancePeriodId""
                FOREIGN KEY (""AttendancePeriodId"") REFERENCES public.""HrdAttendancePeriod"" (""Id"") ON DELETE RESTRICT;
        END IF;

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'AspNetUsers') THEN
            ALTER TABLE public.""HrdAttendancePeriodReopenLog""
                ADD CONSTRAINT ""FK_HrdAttendancePeriodReopenLog_AspNetUsers_ReopenedByUserId""
                FOREIGN KEY (""ReopenedByUserId"") REFERENCES public.""AspNetUsers"" (""Id"") ON DELETE RESTRICT;
        END IF;

        CREATE INDEX ""IX_HrdAttendancePeriodReopenLog_PeriodId""
            ON public.""HrdAttendancePeriodReopenLog"" (""AttendancePeriodId"")
            WHERE ""IsDelete"" = false;
    END IF;
END $$;
");

            // =========================================================================
            // 5. ALTER TABEL EXISTING: HrdAttendanceDaily (Kolom Ekstensi S-B1)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdAttendanceDaily') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'HrdAttendanceDaily' AND column_name = 'EarlyLeavePermissionId') THEN
            ALTER TABLE public.""HrdAttendanceDaily"" ADD COLUMN ""EarlyLeavePermissionId"" uuid NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'HrdAttendanceDaily' AND column_name = 'OfficialDutyRequestId') THEN
            ALTER TABLE public.""HrdAttendanceDaily"" ADD COLUMN ""OfficialDutyRequestId"" uuid NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'HrdAttendanceDaily' AND column_name = 'IncompletePunchReason') THEN
            ALTER TABLE public.""HrdAttendanceDaily"" ADD COLUMN ""IncompletePunchReason"" character varying(500) NULL;
        END IF;

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdEarlyLeavePermission') THEN
            IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_HrdAttendanceDaily_HrdEarlyLeavePermission_EarlyLeavePermissionId') THEN
                ALTER TABLE public.""HrdAttendanceDaily""
                    ADD CONSTRAINT ""FK_HrdAttendanceDaily_HrdEarlyLeavePermission_EarlyLeavePermissionId""
                    FOREIGN KEY (""EarlyLeavePermissionId"") REFERENCES public.""HrdEarlyLeavePermission"" (""Id"") ON DELETE SET NULL;
            END IF;
        END IF;

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdOfficialDutyRequest') THEN
            IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_HrdAttendanceDaily_HrdOfficialDutyRequest_OfficialDutyRequestId') THEN
                ALTER TABLE public.""HrdAttendanceDaily""
                    ADD CONSTRAINT ""FK_HrdAttendanceDaily_HrdOfficialDutyRequest_OfficialDutyRequestId""
                    FOREIGN KEY (""OfficialDutyRequestId"") REFERENCES public.""HrdOfficialDutyRequest"" (""Id"") ON DELETE SET NULL;
            END IF;
        END IF;
    END IF;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HrdAttendancePeriodReopenLog", schema: "public");
            migrationBuilder.DropTable(name: "HrdOfficialDutyRequest", schema: "public");
            migrationBuilder.DropTable(name: "HrdEarlyLeavePermission", schema: "public");
            migrationBuilder.DropTable(name: "HrdAttendanceSetting", schema: "public");
        }
    }
}
