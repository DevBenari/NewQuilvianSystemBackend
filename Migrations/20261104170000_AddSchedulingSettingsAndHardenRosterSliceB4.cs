using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261104170000_AddSchedulingSettingsAndHardenRosterSliceB4")]
    public partial class AddSchedulingSettingsAndHardenRosterSliceB4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // 1. TABEL BARU: HrdSchedulingSettings (HRD-DEC-099, HRD-DEC-102)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdSchedulingSettings') THEN
        CREATE TABLE public.""HrdSchedulingSettings"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""HospitalSiteId"" uuid NOT NULL,
            ""MinRestHoursBetweenShifts"" integer NOT NULL DEFAULT 11,
            ""MaxConsecutiveWorkDays"" integer NOT NULL DEFAULT 6,
            ""MaxConsecutiveNightShifts"" integer NOT NULL DEFAULT 2,
            ""RetroactiveCorrectionWindowDays"" integer NOT NULL DEFAULT 3,
            ""RestHoursEscalatedToBlock"" boolean NOT NULL DEFAULT false,
            ""ConsecutiveDaysEscalatedToBlock"" boolean NOT NULL DEFAULT false,
            ""NightShiftsEscalatedToBlock"" boolean NOT NULL DEFAULT false,
            ""RowVersion"" uuid NOT NULL DEFAULT gen_random_uuid(),
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
            CONSTRAINT ""PK_HrdSchedulingSettings"" PRIMARY KEY (""Id"")
        );

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstHospitalSites') THEN
            ALTER TABLE public.""HrdSchedulingSettings""
                ADD CONSTRAINT ""FK_HrdSchedulingSettings_MstHospitalSites_HospitalSiteId""
                FOREIGN KEY (""HospitalSiteId"") REFERENCES public.""MstHospitalSites"" (""Id"") ON DELETE RESTRICT;
        END IF;

        CREATE UNIQUE INDEX ""IX_HrdSchedulingSettings_HospitalSiteId"" 
            ON public.""HrdSchedulingSettings"" (""HospitalSiteId"") 
            WHERE ""IsDelete"" = false;
    END IF;
END $$;
");

            // =========================================================================
            // 2. ALTER TABEL EXISTING: TrxRosterPeriod (HRD-DEC-098, HRD-DEC-102)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxRosterPeriod') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxRosterPeriod' AND column_name = 'IsRecheckPassed') THEN
            ALTER TABLE public.""TrxRosterPeriod"" ADD COLUMN ""IsRecheckPassed"" boolean NOT NULL DEFAULT false;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxRosterPeriod' AND column_name = 'WarningCount') THEN
            ALTER TABLE public.""TrxRosterPeriod"" ADD COLUMN ""WarningCount"" integer NOT NULL DEFAULT 0;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxRosterPeriod' AND column_name = 'ClosedByUserId') THEN
            ALTER TABLE public.""TrxRosterPeriod"" ADD COLUMN ""ClosedByUserId"" uuid NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxRosterPeriod' AND column_name = 'ClosedAt') THEN
            ALTER TABLE public.""TrxRosterPeriod"" ADD COLUMN ""ClosedAt"" timestamptz NULL;
        END IF;
    END IF;
END $$;
");

            // =========================================================================
            // 3. ALTER TABEL EXISTING: TrxShiftAssignment (HRD-DEC-087, HRD-DEC-100)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxShiftAssignment') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxShiftAssignment' AND column_name = 'OriginalEmployeeId') THEN
            ALTER TABLE public.""TrxShiftAssignment"" ADD COLUMN ""OriginalEmployeeId"" uuid NULL;
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstWorkforceProfiles') THEN
                ALTER TABLE public.""TrxShiftAssignment""
                    ADD CONSTRAINT ""FK_TrxShiftAssignment_MstWorkforceProfiles_OriginalEmployeeId""
                    FOREIGN KEY (""OriginalEmployeeId"") REFERENCES public.""MstWorkforceProfiles"" (""Id"") ON DELETE RESTRICT;
            END IF;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxShiftAssignment' AND column_name = 'IsAccompanied') THEN
            ALTER TABLE public.""TrxShiftAssignment"" ADD COLUMN ""IsAccompanied"" boolean NOT NULL DEFAULT false;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxShiftAssignment' AND column_name = 'PreceptorEmployeeId') THEN
            ALTER TABLE public.""TrxShiftAssignment"" ADD COLUMN ""PreceptorEmployeeId"" uuid NULL;
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstWorkforceProfiles') THEN
                ALTER TABLE public.""TrxShiftAssignment""
                    ADD CONSTRAINT ""FK_TrxShiftAssignment_MstWorkforceProfiles_PreceptorEmployeeId""
                    FOREIGN KEY (""PreceptorEmployeeId"") REFERENCES public.""MstWorkforceProfiles"" (""Id"") ON DELETE RESTRICT;
            END IF;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND tablename = 'TrxShiftAssignment' AND indexname = 'IX_TrxShiftAssignment_DateUnit') THEN
            CREATE INDEX ""IX_TrxShiftAssignment_DateUnit"" ON public.""TrxShiftAssignment"" (""ShiftDate"", ""AssignmentStatus"") WHERE ""IsDelete"" = false;
        END IF;
    END IF;
END $$;
");

            // =========================================================================
            // 4. ALTER TABEL EXISTING: TrxShiftReplacement (HRD-DEC-101)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxShiftReplacement') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxShiftReplacement' AND column_name = 'RosterPeriodId') THEN
            ALTER TABLE public.""TrxShiftReplacement"" ADD COLUMN ""RosterPeriodId"" uuid NULL;
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxRosterPeriod') THEN
                ALTER TABLE public.""TrxShiftReplacement""
                    ADD CONSTRAINT ""FK_TrxShiftReplacement_TrxRosterPeriod_RosterPeriodId""
                    FOREIGN KEY (""RosterPeriodId"") REFERENCES public.""TrxRosterPeriod"" (""Id"") ON DELETE RESTRICT;
            END IF;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxShiftReplacement' AND column_name = 'ReasonCategory') THEN
            ALTER TABLE public.""TrxShiftReplacement"" ADD COLUMN ""ReasonCategory"" varchar(100) NOT NULL DEFAULT 'Sakit Mendadak';
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxShiftReplacement' AND column_name = 'Notes') THEN
            ALTER TABLE public.""TrxShiftReplacement"" ADD COLUMN ""Notes"" varchar(500) NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxShiftReplacement' AND column_name = 'ExecutedByUserId') THEN
            ALTER TABLE public.""TrxShiftReplacement"" ADD COLUMN ""ExecutedByUserId"" uuid NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxShiftReplacement' AND column_name = 'ExecutedAt') THEN
            ALTER TABLE public.""TrxShiftReplacement"" ADD COLUMN ""ExecutedAt"" timestamptz NULL;
        END IF;
    END IF;
END $$;
");

            // =========================================================================
            // 5. ALTER TABEL EXISTING: TrxOnCallAssignment (HRD-DEC-103)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment' AND column_name = 'UnitId') THEN
            ALTER TABLE public.""TrxOnCallAssignment"" ADD COLUMN ""UnitId"" uuid NULL;
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstOrganizationUnits') THEN
                ALTER TABLE public.""TrxOnCallAssignment""
                    ADD CONSTRAINT ""FK_TrxOnCallAssignment_MstOrganizationUnits_UnitId""
                    FOREIGN KEY (""UnitId"") REFERENCES public.""MstOrganizationUnits"" (""Id"") ON DELETE RESTRICT;
            END IF;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment' AND column_name = 'IsCalledIn') THEN
            ALTER TABLE public.""TrxOnCallAssignment"" ADD COLUMN ""IsCalledIn"" boolean NOT NULL DEFAULT false;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment' AND column_name = 'CallInTime') THEN
            ALTER TABLE public.""TrxOnCallAssignment"" ADD COLUMN ""CallInTime"" timestamptz NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment' AND column_name = 'ArrivalTime') THEN
            ALTER TABLE public.""TrxOnCallAssignment"" ADD COLUMN ""ArrivalTime"" timestamptz NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment' AND column_name = 'ResponseMinutes') THEN
            ALTER TABLE public.""TrxOnCallAssignment"" ADD COLUMN ""ResponseMinutes"" integer NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment' AND column_name = 'IsSlaMet') THEN
            ALTER TABLE public.""TrxOnCallAssignment"" ADD COLUMN ""IsSlaMet"" boolean NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment' AND column_name = 'CalledByUnitId') THEN
            ALTER TABLE public.""TrxOnCallAssignment"" ADD COLUMN ""CalledByUnitId"" uuid NULL;
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstOrganizationUnits') THEN
                ALTER TABLE public.""TrxOnCallAssignment""
                    ADD CONSTRAINT ""FK_TrxOnCallAssignment_MstOrganizationUnits_CalledByUnitId""
                    FOREIGN KEY (""CalledByUnitId"") REFERENCES public.""MstOrganizationUnits"" (""Id"") ON DELETE RESTRICT;
            END IF;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TrxOnCallAssignment' AND column_name = 'CallNotes') THEN
            ALTER TABLE public.""TrxOnCallAssignment"" ADD COLUMN ""CallNotes"" varchar(500) NULL;
        END IF;

        IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND tablename = 'TrxOnCallAssignment' AND indexname = 'IX_TrxOnCallAssignment_ActiveStatus') THEN
            CREATE INDEX ""IX_TrxOnCallAssignment_ActiveStatus"" ON public.""TrxOnCallAssignment"" (""UnitId"", ""AssignmentStatus"", ""StartAt"", ""EndAt"") WHERE ""IsDelete"" = false;
        END IF;
    END IF;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrdSchedulingSettings",
                schema: "public");
        }
    }
}
