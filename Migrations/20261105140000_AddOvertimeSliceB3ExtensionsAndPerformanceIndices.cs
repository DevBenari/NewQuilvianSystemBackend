using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261105140000_AddOvertimeSliceB3ExtensionsAndPerformanceIndices")]
    public partial class AddOvertimeSliceB3ExtensionsAndPerformanceIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // 1. EKSTENSI TABEL: TrxOvertimePlan (HRD-DEC-116, BE-HRD-131)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOvertimePlan') THEN
        ALTER TABLE public.""TrxOvertimePlan"" ADD COLUMN IF NOT EXISTS ""CompensationType"" character varying(50) NOT NULL DEFAULT 'FinancialPayout';
        ALTER TABLE public.""TrxOvertimePlan"" ADD COLUMN IF NOT EXISTS ""IsEmergencyEscalation"" boolean NOT NULL DEFAULT false;
        ALTER TABLE public.""TrxOvertimePlan"" ADD COLUMN IF NOT EXISTS ""WeeklyQuotaHoursAtPlan"" numeric(5,2) NULL;

        CREATE INDEX IF NOT EXISTS ""idx_ot_plan_date_unit"" 
            ON public.""TrxOvertimePlan"" (""OrganizationUnitId"", ""PlanStartDate"", ""PlanStatus"");
    END IF;
END $$;
");

            // =========================================================================
            // 2. EKSTENSI TABEL: TrxOvertimeRealization (HRD-DEC-116, HRD-DEC-117, BE-HRD-131)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOvertimeRealization') THEN
        ALTER TABLE public.""TrxOvertimeRealization"" ADD COLUMN IF NOT EXISTS ""OvertimeCategory"" character varying(50) NOT NULL DEFAULT 'Planned';
        ALTER TABLE public.""TrxOvertimeRealization"" ADD COLUMN IF NOT EXISTS ""EmergencyClinicalReferenceNo"" character varying(100) NULL;
        ALTER TABLE public.""TrxOvertimeRealization"" ADD COLUMN IF NOT EXISTS ""ClinicalActionSummary"" character varying(2000) NULL;
        ALTER TABLE public.""TrxOvertimeRealization"" ADD COLUMN IF NOT EXISTS ""CompensationType"" character varying(50) NOT NULL DEFAULT 'FinancialPayout';
        ALTER TABLE public.""TrxOvertimeRealization"" ADD COLUMN IF NOT EXISTS ""IsMedicalDirectorEscalation"" boolean NOT NULL DEFAULT false;
        ALTER TABLE public.""TrxOvertimeRealization"" ADD COLUMN IF NOT EXISTS ""MedicalDirectorApprovedById"" uuid NULL;
        ALTER TABLE public.""TrxOvertimeRealization"" ADD COLUMN IF NOT EXISTS ""IsRetrospectiveAdjustment"" boolean NOT NULL DEFAULT false;

        CREATE INDEX IF NOT EXISTS ""idx_ot_realization_emp_date"" 
            ON public.""TrxOvertimeRealization"" (""WorkforceProfileId"", ""ActualStartDate"", ""RealizationStatus"");
    END IF;
END $$;
");

            // =========================================================================
            // 3. EKSTENSI TABEL: TrxOvertimeVerification (HRD-DEC-119, BE-HRD-131)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOvertimeVerification') THEN
        ALTER TABLE public.""TrxOvertimeVerification"" ADD COLUMN IF NOT EXISTS ""BiometricCheckIn"" timestamptz NULL;
        ALTER TABLE public.""TrxOvertimeVerification"" ADD COLUMN IF NOT EXISTS ""BiometricCheckOut"" timestamptz NULL;
        ALTER TABLE public.""TrxOvertimeVerification"" ADD COLUMN IF NOT EXISTS ""BiometricExcessMinutes"" integer NOT NULL DEFAULT 0;
        ALTER TABLE public.""TrxOvertimeVerification"" ADD COLUMN IF NOT EXISTS ""VerifiedPayableMinutes"" integer NOT NULL DEFAULT 0;
        ALTER TABLE public.""TrxOvertimeVerification"" ADD COLUMN IF NOT EXISTS ""DiscrepancyReason"" character varying(255) NULL;
        ALTER TABLE public.""TrxOvertimeVerification"" ADD COLUMN IF NOT EXISTS ""ManualAdjustmentNotes"" character varying(2000) NULL;
        ALTER TABLE public.""TrxOvertimeVerification"" ADD COLUMN IF NOT EXISTS ""VerifiedByHrAdminId"" uuid NULL;

        CREATE INDEX IF NOT EXISTS ""idx_ot_verification_state"" 
            ON public.""TrxOvertimeVerification"" (""VerificationStatus"", ""IsAttendanceMatched"", ""HasVariance"");
    END IF;
END $$;
");

            // =========================================================================
            // 4. EKSTENSI TABEL: TrxCompensatoryTimeOff (HRD-DEC-118, BE-HRD-131)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxCompensatoryTimeOff') THEN
        ALTER TABLE public.""TrxCompensatoryTimeOff"" ADD COLUMN IF NOT EXISTS ""GrantedDays"" numeric(4,1) NOT NULL DEFAULT 0;
        ALTER TABLE public.""TrxCompensatoryTimeOff"" ADD COLUMN IF NOT EXISTS ""RemainingHours"" numeric(5,2) NOT NULL DEFAULT 0;
        ALTER TABLE public.""TrxCompensatoryTimeOff"" ADD COLUMN IF NOT EXISTS ""IsExpired"" boolean NOT NULL DEFAULT false;

        CREATE INDEX IF NOT EXISTS ""idx_cto_emp_expiry"" 
            ON public.""TrxCompensatoryTimeOff"" (""WorkforceProfileId"", ""ExpiryDate"", ""IsExpired"");
    END IF;
END $$;
");

            // =========================================================================
            // 5. EKSTENSI TABEL: TrxOvertimePeriod (HRD-DEC-121, BE-HRD-131)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOvertimePeriod') THEN
        ALTER TABLE public.""TrxOvertimePeriod"" ADD COLUMN IF NOT EXISTS ""CutoffDate"" date NULL;
        ALTER TABLE public.""TrxOvertimePeriod"" ADD COLUMN IF NOT EXISTS ""TotalCtoGrantedHours"" numeric(6,2) NOT NULL DEFAULT 0;
        ALTER TABLE public.""TrxOvertimePeriod"" ADD COLUMN IF NOT EXISTS ""IsHandoffExecuted"" boolean NOT NULL DEFAULT false;

        CREATE INDEX IF NOT EXISTS ""idx_ot_period_lookup"" 
            ON public.""TrxOvertimePeriod"" (""StartDate"", ""EndDate"", ""PeriodStatus"");
    END IF;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOvertimePlan') THEN
        DROP INDEX IF EXISTS ""idx_ot_plan_date_unit"";
        ALTER TABLE public.""TrxOvertimePlan"" DROP COLUMN IF EXISTS ""CompensationType"";
        ALTER TABLE public.""TrxOvertimePlan"" DROP COLUMN IF EXISTS ""IsEmergencyEscalation"";
        ALTER TABLE public.""TrxOvertimePlan"" DROP COLUMN IF EXISTS ""WeeklyQuotaHoursAtPlan"";
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOvertimeRealization') THEN
        DROP INDEX IF EXISTS ""idx_ot_realization_emp_date"";
        ALTER TABLE public.""TrxOvertimeRealization"" DROP COLUMN IF EXISTS ""OvertimeCategory"";
        ALTER TABLE public.""TrxOvertimeRealization"" DROP COLUMN IF EXISTS ""EmergencyClinicalReferenceNo"";
        ALTER TABLE public.""TrxOvertimeRealization"" DROP COLUMN IF EXISTS ""ClinicalActionSummary"";
        ALTER TABLE public.""TrxOvertimeRealization"" DROP COLUMN IF EXISTS ""CompensationType"";
        ALTER TABLE public.""TrxOvertimeRealization"" DROP COLUMN IF EXISTS ""IsMedicalDirectorEscalation"";
        ALTER TABLE public.""TrxOvertimeRealization"" DROP COLUMN IF EXISTS ""MedicalDirectorApprovedById"";
        ALTER TABLE public.""TrxOvertimeRealization"" DROP COLUMN IF EXISTS ""IsRetrospectiveAdjustment"";
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOvertimeVerification') THEN
        DROP INDEX IF EXISTS ""idx_ot_verification_state"";
        ALTER TABLE public.""TrxOvertimeVerification"" DROP COLUMN IF EXISTS ""BiometricCheckIn"";
        ALTER TABLE public.""TrxOvertimeVerification"" DROP COLUMN IF EXISTS ""BiometricCheckOut"";
        ALTER TABLE public.""TrxOvertimeVerification"" DROP COLUMN IF EXISTS ""BiometricExcessMinutes"";
        ALTER TABLE public.""TrxOvertimeVerification"" DROP COLUMN IF EXISTS ""VerifiedPayableMinutes"";
        ALTER TABLE public.""TrxOvertimeVerification"" DROP COLUMN IF EXISTS ""DiscrepancyReason"";
        ALTER TABLE public.""TrxOvertimeVerification"" DROP COLUMN IF EXISTS ""ManualAdjustmentNotes"";
        ALTER TABLE public.""TrxOvertimeVerification"" DROP COLUMN IF EXISTS ""VerifiedByHrAdminId"";
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxCompensatoryTimeOff') THEN
        DROP INDEX IF EXISTS ""idx_cto_emp_expiry"";
        ALTER TABLE public.""TrxCompensatoryTimeOff"" DROP COLUMN IF EXISTS ""GrantedDays"";
        ALTER TABLE public.""TrxCompensatoryTimeOff"" DROP COLUMN IF EXISTS ""RemainingHours"";
        ALTER TABLE public.""TrxCompensatoryTimeOff"" DROP COLUMN IF EXISTS ""IsExpired"";
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TrxOvertimePeriod') THEN
        DROP INDEX IF EXISTS ""idx_ot_period_lookup"";
        ALTER TABLE public.""TrxOvertimePeriod"" DROP COLUMN IF EXISTS ""CutoffDate"";
        ALTER TABLE public.""TrxOvertimePeriod"" DROP COLUMN IF EXISTS ""TotalCtoGrantedHours"";
        ALTER TABLE public.""TrxOvertimePeriod"" DROP COLUMN IF EXISTS ""IsHandoffExecuted"";
    END IF;
END $$;
");
        }
    }
}
