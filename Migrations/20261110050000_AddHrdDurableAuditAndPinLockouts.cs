using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Migrasi remediasi Gelombang 1 audit backend HR (10 November 2026, <c>backend-unfinished-work.md</c>):
    /// delapan tabel <c>Hrd*</c> pengganti penyimpanan in-memory (BE-HRD-159, 204, 229, 234, 235, 241),
    /// kolom dosis radiasi terukur (BE-HRD-298), dan index unik nomor penugasan FPPE (QBE-CODE-004, BE-HRD-230).
    /// </summary>
    /// <remarks>
    /// Ditulis manual dengan <c>IF NOT EXISTS</c> mengikuti pola migration HR terdahulu, karena snapshot model EF
    /// sudah lama tidak sinkron dengan migration SQL manual sebelumnya; scaffold otomatis menghasilkan ratusan operasi
    /// drop/create di luar lingkup task dan tidak aman dijalankan. Tidak ada data yang dihapus oleh migration ini.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261110050000_AddHrdDurableAuditAndPinLockouts")]
    public partial class AddHrdDurableAuditAndPinLockouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- ============================================================================================
                -- BE-HRD-234: Jejak audit diskresi penyesuaian tier rekredensialing (sebelumnya in-memory)
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdRecredentialingTierOverride"" (
                    ""Id"" uuid NOT NULL,
                    ""WorkforceProfileId"" uuid NOT NULL,
                    ""CalculatedTier"" character varying(40) NOT NULL,
                    ""NewTier"" character varying(40) NOT NULL,
                    ""JustificationReason"" character varying(2000) NOT NULL,
                    ""ApprovedByCommitteeChairmanId"" uuid NOT NULL,
                    ""ChairmanNameSnapshot"" character varying(200) NULL,
                    ""OverriddenAt"" timestamp with time zone NOT NULL,
                    ""IsActive"" boolean NOT NULL DEFAULT true,
                    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreateBy"" uuid NOT NULL,
                    ""UpdateDateTime"" timestamp with time zone NULL,
                    ""UpdateBy"" uuid NOT NULL,
                    ""DeleteDateTime"" timestamp with time zone NULL,
                    ""DeleteBy"" uuid NOT NULL,
                    ""CancelDateTime"" timestamp with time zone NULL,
                    ""CancelBy"" uuid NOT NULL,
                    ""IsCancel"" boolean NOT NULL DEFAULT false,
                    ""IsDelete"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_HrdRecredentialingTierOverride"" PRIMARY KEY (""Id"")
                );
                CREATE INDEX IF NOT EXISTS ""IX_HrdRecredentialingTierOverride_WorkforceProfileId_IsActive""
                    ON public.""HrdRecredentialingTierOverride"" (""WorkforceProfileId"", ""IsActive"");
                CREATE INDEX IF NOT EXISTS ""IX_HrdRecredentialingTierOverride_WorkforceProfileId_OverriddenAt""
                    ON public.""HrdRecredentialingTierOverride"" (""WorkforceProfileId"", ""OverriddenAt"");

                -- ============================================================================================
                -- BE-HRD-235: Putusan sidang rekredensialing (sebelumnya in-memory)
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdRecredentialingVerdict"" (
                    ""Id"" uuid NOT NULL,
                    ""WorkforceProfileId"" uuid NOT NULL,
                    ""DoctorCodeSnapshot"" character varying(50) NULL,
                    ""DoctorNameSnapshot"" character varying(200) NULL,
                    ""Verdict"" character varying(30) NOT NULL,
                    ""CommitteeNotes"" character varying(2000) NULL,
                    ""ApprovedByCommitteeChairmanId"" uuid NOT NULL,
                    ""DecidedAt"" timestamp with time zone NOT NULL,
                    ""SignatureHash"" character varying(64) NOT NULL,
                    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreateBy"" uuid NOT NULL,
                    ""UpdateDateTime"" timestamp with time zone NULL,
                    ""UpdateBy"" uuid NOT NULL,
                    ""DeleteDateTime"" timestamp with time zone NULL,
                    ""DeleteBy"" uuid NOT NULL,
                    ""CancelDateTime"" timestamp with time zone NULL,
                    ""CancelBy"" uuid NOT NULL,
                    ""IsCancel"" boolean NOT NULL DEFAULT false,
                    ""IsDelete"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_HrdRecredentialingVerdict"" PRIMARY KEY (""Id"")
                );
                CREATE INDEX IF NOT EXISTS ""IX_HrdRecredentialingVerdict_WorkforceProfileId_DecidedAt""
                    ON public.""HrdRecredentialingVerdict"" (""WorkforceProfileId"", ""DecidedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_HrdRecredentialingVerdict_Verdict""
                    ON public.""HrdRecredentialingVerdict"" (""Verdict"");

                -- ============================================================================================
                -- BE-HRD-235: Log pengiriman wewenang klinis ke SIMRS EMR (sebelumnya in-memory, latensi acak)
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdSimrsPrivilegeSyncLog"" (
                    ""Id"" uuid NOT NULL,
                    ""VerdictId"" uuid NOT NULL,
                    ""DoctorCode"" character varying(50) NOT NULL,
                    ""TargetLevel"" integer NOT NULL,
                    ""IsHardStopBlocked"" boolean NOT NULL,
                    ""PayloadJson"" text NOT NULL,
                    ""ResponseStatusCode"" integer NOT NULL,
                    ""LatencyMs"" bigint NOT NULL,
                    ""DispatchedAt"" timestamp with time zone NOT NULL,
                    ""Status"" character varying(30) NOT NULL,
                    ""Message"" character varying(1000) NULL,
                    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreateBy"" uuid NOT NULL,
                    ""UpdateDateTime"" timestamp with time zone NULL,
                    ""UpdateBy"" uuid NOT NULL,
                    ""DeleteDateTime"" timestamp with time zone NULL,
                    ""DeleteBy"" uuid NOT NULL,
                    ""CancelDateTime"" timestamp with time zone NULL,
                    ""CancelBy"" uuid NOT NULL,
                    ""IsCancel"" boolean NOT NULL DEFAULT false,
                    ""IsDelete"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_HrdSimrsPrivilegeSyncLog"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_HrdSimrsPrivilegeSyncLog_HrdRecredentialingVerdict_VerdictId""
                        FOREIGN KEY (""VerdictId"") REFERENCES public.""HrdRecredentialingVerdict"" (""Id"") ON DELETE RESTRICT
                );
                CREATE INDEX IF NOT EXISTS ""IX_HrdSimrsPrivilegeSyncLog_VerdictId_DispatchedAt""
                    ON public.""HrdSimrsPrivilegeSyncLog"" (""VerdictId"", ""DispatchedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_HrdSimrsPrivilegeSyncLog_DispatchedAt""
                    ON public.""HrdSimrsPrivilegeSyncLog"" (""DispatchedAt"");

                -- ============================================================================================
                -- BE-HRD-229: Event pemicu kritis FPPE yang diterima dari modul lain (sebelumnya in-memory)
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdFppeTriggerEvent"" (
                    ""Id"" uuid NOT NULL,
                    ""WorkforceProfileId"" uuid NOT NULL,
                    ""DoctorNameSnapshot"" character varying(200) NULL,
                    ""DoctorCodeSnapshot"" character varying(50) NULL,
                    ""SpecializationSnapshot"" character varying(150) NULL,
                    ""TriggerType"" character varying(50) NOT NULL,
                    ""Severity"" character varying(20) NOT NULL,
                    ""IncidentDate"" timestamp with time zone NOT NULL,
                    ""Description"" character varying(2000) NULL,
                    ""MetricEvidence"" character varying(2000) NULL,
                    ""RecommendedAction"" character varying(500) NULL,
                    ""DetectedAt"" timestamp with time zone NOT NULL,
                    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreateBy"" uuid NOT NULL,
                    ""UpdateDateTime"" timestamp with time zone NULL,
                    ""UpdateBy"" uuid NOT NULL,
                    ""DeleteDateTime"" timestamp with time zone NULL,
                    ""DeleteBy"" uuid NOT NULL,
                    ""CancelDateTime"" timestamp with time zone NULL,
                    ""CancelBy"" uuid NOT NULL,
                    ""IsCancel"" boolean NOT NULL DEFAULT false,
                    ""IsDelete"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_HrdFppeTriggerEvent"" PRIMARY KEY (""Id"")
                );
                CREATE INDEX IF NOT EXISTS ""IX_HrdFppeTriggerEvent_WorkforceProfileId_TriggerType""
                    ON public.""HrdFppeTriggerEvent"" (""WorkforceProfileId"", ""TriggerType"");
                CREATE INDEX IF NOT EXISTS ""IX_HrdFppeTriggerEvent_DetectedAt""
                    ON public.""HrdFppeTriggerEvent"" (""DetectedAt"");

                -- ============================================================================================
                -- BE-HRD-230 / QBE-CODE-004: nomor penugasan FPPE kini urut dari provider bersama; wajib unik
                -- ============================================================================================
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_trx_fppe_engagements_EngagementNumber""
                    ON public.""trx_fppe_engagements"" (""EngagementNumber"") WHERE ""IsDelete"" = false;

                -- ============================================================================================
                -- BE-HRD-241: Status pengamanan shift buddy system (sebelumnya in-memory)
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdShiftBuddySafeguard"" (
                    ""Id"" uuid NOT NULL,
                    ""WorkforceProfileId"" uuid NOT NULL,
                    ""StaffNameSnapshot"" character varying(200) NULL,
                    ""IsOverdueFlagged"" boolean NOT NULL DEFAULT true,
                    ""GracePeriodDaysRemaining"" integer NOT NULL,
                    ""IsBuddySystemEnforced"" boolean NOT NULL DEFAULT true,
                    ""IsRkkRestrictedLevel3"" boolean NOT NULL DEFAULT false,
                    ""ActivatedAt"" timestamp with time zone NOT NULL,
                    ""ReleasedAt"" timestamp with time zone NULL,
                    ""IsActive"" boolean NOT NULL DEFAULT true,
                    ""Message"" character varying(1000) NULL,
                    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreateBy"" uuid NOT NULL,
                    ""UpdateDateTime"" timestamp with time zone NULL,
                    ""UpdateBy"" uuid NOT NULL,
                    ""DeleteDateTime"" timestamp with time zone NULL,
                    ""DeleteBy"" uuid NOT NULL,
                    ""CancelDateTime"" timestamp with time zone NULL,
                    ""CancelBy"" uuid NOT NULL,
                    ""IsCancel"" boolean NOT NULL DEFAULT false,
                    ""IsDelete"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_HrdShiftBuddySafeguard"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_HrdShiftBuddySafeguard_MstWorkforceProfile_WorkforceProfileId""
                        FOREIGN KEY (""WorkforceProfileId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_HrdShiftBuddySafeguard_WorkforceProfileId""
                    ON public.""HrdShiftBuddySafeguard"" (""WorkforceProfileId"") WHERE ""IsActive"" = true AND ""IsDelete"" = false;

                -- ============================================================================================
                -- BE-HRD-159: Penguncian PIN slip gaji & sesi 15 menit (sebelumnya in-memory)
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdPayslipPinLockout"" (
                    ""Id"" uuid NOT NULL,
                    ""EmployeeKey"" uuid NOT NULL,
                    ""FailedAttempts"" integer NOT NULL,
                    ""LockoutExpiresAt"" timestamp with time zone NULL,
                    ""LastFailedAt"" timestamp with time zone NOT NULL,
                    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreateBy"" uuid NOT NULL,
                    ""UpdateDateTime"" timestamp with time zone NULL,
                    ""UpdateBy"" uuid NOT NULL,
                    ""DeleteDateTime"" timestamp with time zone NULL,
                    ""DeleteBy"" uuid NOT NULL,
                    ""CancelDateTime"" timestamp with time zone NULL,
                    ""CancelBy"" uuid NOT NULL,
                    ""IsCancel"" boolean NOT NULL DEFAULT false,
                    ""IsDelete"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_HrdPayslipPinLockout"" PRIMARY KEY (""Id"")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS uq_payslip_pin_lockout_employee
                    ON public.""HrdPayslipPinLockout"" (""EmployeeKey"");

                CREATE TABLE IF NOT EXISTS public.""HrdPayslipPinSession"" (
                    ""Id"" uuid NOT NULL,
                    ""TokenHash"" character varying(64) NOT NULL,
                    ""EmployeeKey"" uuid NOT NULL,
                    ""IssuedAt"" timestamp with time zone NOT NULL,
                    ""ExpiresAt"" timestamp with time zone NOT NULL,
                    ""RevokedAt"" timestamp with time zone NULL,
                    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreateBy"" uuid NOT NULL,
                    ""UpdateDateTime"" timestamp with time zone NULL,
                    ""UpdateBy"" uuid NOT NULL,
                    ""DeleteDateTime"" timestamp with time zone NULL,
                    ""DeleteBy"" uuid NOT NULL,
                    ""CancelDateTime"" timestamp with time zone NULL,
                    ""CancelBy"" uuid NOT NULL,
                    ""IsCancel"" boolean NOT NULL DEFAULT false,
                    ""IsDelete"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_HrdPayslipPinSession"" PRIMARY KEY (""Id"")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS uq_payslip_pin_session_token
                    ON public.""HrdPayslipPinSession"" (""TokenHash"");
                CREATE INDEX IF NOT EXISTS ""IX_HrdPayslipPinSession_EmployeeKey_ExpiresAt""
                    ON public.""HrdPayslipPinSession"" (""EmployeeKey"", ""ExpiresAt"");

                -- ============================================================================================
                -- BE-HRD-204: Penghitung salah-PIN portal verifikasi publik per dokumen per IP (sebelumnya in-memory)
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdPinAttemptRecord"" (
                    ""Id"" uuid NOT NULL,
                    ""AttemptKey"" character varying(200) NOT NULL,
                    ""VerificationHash"" character varying(128) NOT NULL,
                    ""ClientIp"" character varying(64) NOT NULL,
                    ""FailedCount"" integer NOT NULL,
                    ""LockedUntilUtc"" timestamp with time zone NULL,
                    ""LastAttemptUtc"" timestamp with time zone NOT NULL,
                    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""CreateBy"" uuid NOT NULL,
                    ""UpdateDateTime"" timestamp with time zone NULL,
                    ""UpdateBy"" uuid NOT NULL,
                    ""DeleteDateTime"" timestamp with time zone NULL,
                    ""DeleteBy"" uuid NOT NULL,
                    ""CancelDateTime"" timestamp with time zone NULL,
                    ""CancelBy"" uuid NOT NULL,
                    ""IsCancel"" boolean NOT NULL DEFAULT false,
                    ""IsDelete"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_HrdPinAttemptRecord"" PRIMARY KEY (""Id"")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS uq_pin_attempt_record_key
                    ON public.""HrdPinAttemptRecord"" (""AttemptKey"");
                CREATE INDEX IF NOT EXISTS ""IX_HrdPinAttemptRecord_LastAttemptUtc""
                    ON public.""HrdPinAttemptRecord"" (""LastAttemptUtc"");

                -- ============================================================================================
                -- BE-HRD-298: Dosis radiasi terukur per catatan surveilans TLD (radar dihitung dari data nyata)
                -- ============================================================================================
                ALTER TABLE public.""TrxEmployeeHealthSurveillance"" ADD COLUMN IF NOT EXISTS ""MeasuredDoseMsv"" numeric(10,3) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE public.""TrxEmployeeHealthSurveillance"" DROP COLUMN IF EXISTS ""MeasuredDoseMsv"";
                DROP TABLE IF EXISTS public.""HrdPinAttemptRecord"";
                DROP TABLE IF EXISTS public.""HrdPayslipPinSession"";
                DROP TABLE IF EXISTS public.""HrdPayslipPinLockout"";
                DROP TABLE IF EXISTS public.""HrdShiftBuddySafeguard"";
                DROP INDEX IF EXISTS public.""IX_trx_fppe_engagements_EngagementNumber"";
                DROP TABLE IF EXISTS public.""HrdFppeTriggerEvent"";
                DROP TABLE IF EXISTS public.""HrdSimrsPrivilegeSyncLog"";
                DROP TABLE IF EXISTS public.""HrdRecredentialingVerdict"";
                DROP TABLE IF EXISTS public.""HrdRecredentialingTierOverride"";
            ");
        }
    }
}
