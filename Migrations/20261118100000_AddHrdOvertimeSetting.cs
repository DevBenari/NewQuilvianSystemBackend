using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Register masalah SDM <c>MSL-10</c> (18 November 2026): parameter lembur pindah dari memori statis ke tabel
    /// <c>HrdOvertimeSetting</c>. Ditulis manual dan idempoten (pola <c>20261105110000_AddAttendanceSliceB1Core</c>),
    /// karena snapshot model tidak sinkron dengan migration SDM manual. Baris awal memakai nilai bawaan yang selama ini
    /// berlaku di memori, sehingga perilaku tidak berubah saat migration dijalankan.
    /// </summary>
    /// <remarks>Dibuat atas izin pemilik 18 November 2026; BELUM dijalankan ke database mana pun.</remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261118100000_AddHrdOvertimeSetting")]
    public partial class AddHrdOvertimeSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'HrdOvertimeSetting') THEN
        CREATE TABLE public.""HrdOvertimeSetting"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""HospitalCode"" character varying(50) NOT NULL,
            ""DailyMaxHoursRegular"" numeric(5,2) NOT NULL DEFAULT 4,
            ""WeeklyMaxHoursRegular"" numeric(5,2) NOT NULL DEFAULT 18,
            ""RoundingIntervalMinutes"" integer NOT NULL DEFAULT 30,
            ""MinimumThresholdMinutes"" integer NOT NULL DEFAULT 30,
            ""CtoValidityDays"" integer NOT NULL DEFAULT 90,
            ""MonthlyCutoffDay"" integer NOT NULL DEFAULT 20,
            ""BiometricDiscrepancyTolerance"" integer NOT NULL DEFAULT 30,
            ""PostClaimCitoGraceDays"" integer NOT NULL DEFAULT 1,
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
            CONSTRAINT ""PK_HrdOvertimeSetting"" PRIMARY KEY (""Id""),
            CONSTRAINT ""CK_HrdOvertimeSetting_CutoffDay"" CHECK (""MonthlyCutoffDay"" BETWEEN 1 AND 28),
            CONSTRAINT ""CK_HrdOvertimeSetting_PositiveLimits"" CHECK (
                ""DailyMaxHoursRegular"" > 0 AND ""WeeklyMaxHoursRegular"" > 0 AND ""RoundingIntervalMinutes"" > 0
                AND ""MinimumThresholdMinutes"" > 0 AND ""CtoValidityDays"" > 0 AND ""BiometricDiscrepancyTolerance"" > 0
                AND ""PostClaimCitoGraceDays"" > 0)
        );

        CREATE UNIQUE INDEX ""IX_HrdOvertimeSetting_HospitalCode""
            ON public.""HrdOvertimeSetting"" (""HospitalCode"")
            WHERE ""IsDelete"" = false;

        INSERT INTO public.""HrdOvertimeSetting"" (""Id"", ""HospitalCode"", ""CreateDateTime"")
        VALUES (gen_random_uuid(), 'RSMMC', CURRENT_TIMESTAMP);
    END IF;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS public.""HrdOvertimeSetting"";");
        }
    }
}
