using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Migrasi remediasi Gelombang 2 audit backend HR (11 November 2026, <c>backend-unfinished-work.md</c>):
    /// tabel pusat pemberitahuan dalam aplikasi <c>HrdNotification</c> (BE-HRD-314, kelompok D), tabel jejak kunci/pulih
    /// akun skorsing <c>HrdSuspensionAccessAction</c> (BE-HRD-285, INT-HR-88), dan pemetaan ulang kolom identitas pegawai
    /// yang terlanjur berisi ID akun karena controller membaca klaim yang tidak pernah ada (kelompok G).
    /// </summary>
    /// <remarks>
    /// Ditulis manual dengan <c>IF NOT EXISTS</c> mengikuti pola migration HR terdahulu (snapshot EF tidak sinkron;
    /// scaffold otomatis tidak aman dijalankan). Pemetaan ulang identitas hanya menyentuh baris yang kolomnya persis sama
    /// dengan ID akun yang punya profil tenaga kerja; baris lain tidak diubah. Tidak ada data yang dihapus.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261111020000_AddHrdNotificationAndSuspensionAccess")]
    public partial class AddHrdNotificationAndSuspensionAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- ============================================================================================
                -- BE-HRD-314: Pusat pemberitahuan dalam aplikasi (kanal InApp + SignalR)
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdNotification"" (
                    ""Id"" uuid NOT NULL,
                    ""RecipientUserId"" uuid NOT NULL,
                    ""RecipientWorkforceProfileId"" uuid NULL,
                    ""Title"" character varying(200) NOT NULL,
                    ""Body"" character varying(2000) NOT NULL,
                    ""Category"" character varying(60) NOT NULL,
                    ""Priority"" character varying(20) NOT NULL,
                    ""Channel"" character varying(30) NOT NULL,
                    ""RequestedChannel"" character varying(30) NULL,
                    ""SourceModule"" character varying(80) NULL,
                    ""ReferenceType"" character varying(100) NULL,
                    ""ReferenceId"" uuid NULL,
                    ""DeepLinkUrl"" character varying(300) NULL,
                    ""DedupKey"" character varying(250) NULL,
                    ""IsRead"" boolean NOT NULL DEFAULT false,
                    ""ReadAt"" timestamp with time zone NULL,
                    ""DeliveredAt"" timestamp with time zone NOT NULL,
                    ""IsPushedRealtime"" boolean NOT NULL DEFAULT false,
                    ""ExpiresAt"" timestamp with time zone NULL,
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
                    CONSTRAINT ""PK_HrdNotification"" PRIMARY KEY (""Id"")
                );
                CREATE INDEX IF NOT EXISTS ""IX_HrdNotification_Recipient_IsRead_DeliveredAt""
                    ON public.""HrdNotification"" (""RecipientUserId"", ""IsRead"", ""DeliveredAt"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""uq_hrd_notification_dedup_key""
                    ON public.""HrdNotification"" (""DedupKey"") WHERE ""DedupKey"" IS NOT NULL;
                CREATE INDEX IF NOT EXISTS ""IX_HrdNotification_Reference""
                    ON public.""HrdNotification"" (""ReferenceType"", ""ReferenceId"");

                -- ============================================================================================
                -- BE-HRD-285 / INT-HR-88: Jejak kunci dan pulih akun aplikasi selama skorsing
                -- ============================================================================================
                CREATE TABLE IF NOT EXISTS public.""HrdSuspensionAccessAction"" (
                    ""Id"" uuid NOT NULL,
                    ""SuspensionId"" uuid NOT NULL,
                    ""EmployeeId"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""ActionType"" character varying(20) NOT NULL,
                    ""WasActiveBefore"" boolean NOT NULL DEFAULT true,
                    ""ActionAt"" timestamp with time zone NOT NULL,
                    ""LockoutUntil"" timestamp with time zone NULL,
                    ""Notes"" character varying(500) NULL,
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
                    CONSTRAINT ""PK_HrdSuspensionAccessAction"" PRIMARY KEY (""Id"")
                );
                CREATE INDEX IF NOT EXISTS ""IX_HrdSuspensionAccessAction_Suspension_User_Action""
                    ON public.""HrdSuspensionAccessAction"" (""SuspensionId"", ""UserId"", ""ActionType"");
                CREATE INDEX IF NOT EXISTS ""IX_HrdSuspensionAccessAction_UserId""
                    ON public.""HrdSuspensionAccessAction"" (""UserId"");
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'trx_disciplinary_suspensions')
                       AND NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdSuspensionAccessAction_Suspension') THEN
                        ALTER TABLE public.""HrdSuspensionAccessAction""
                            ADD CONSTRAINT ""FK_HrdSuspensionAccessAction_Suspension""
                            FOREIGN KEY (""SuspensionId"") REFERENCES public.""trx_disciplinary_suspensions"" (""suspension_id"") ON DELETE RESTRICT;
                    END IF;
                EXCEPTION WHEN OTHERS THEN
                    RAISE NOTICE 'FK_HrdSuspensionAccessAction_Suspension dilewati: %', SQLERRM;
                END $$;

                -- ============================================================================================
                -- Kelompok G: pemetaan ulang kolom WorkforceProfileId yang terlanjur berisi ID akun
                -- (controller lama membaca klaim ""WorkforceProfileId""/""EmployeeId"" yang tidak pernah ada di token,
                -- sehingga jatuh ke ID akun). Hanya baris yang nilainya persis ID akun ber-profil yang dipetakan.
                -- ============================================================================================
                UPDATE public.""TrxBusinessTravelRequest"" t SET ""WorkforceProfileId"" = u.""WorkforceProfileId""
                    FROM public.""AspNetUsers"" u WHERE t.""WorkforceProfileId"" = u.""Id"" AND u.""WorkforceProfileId"" IS NOT NULL;
                UPDATE public.""TrxTravelAdvanceRequest"" t SET ""WorkforceProfileId"" = u.""WorkforceProfileId""
                    FROM public.""AspNetUsers"" u WHERE t.""WorkforceProfileId"" = u.""Id"" AND u.""WorkforceProfileId"" IS NOT NULL;
                UPDATE public.""TrxTravelExpenseClaim"" t SET ""WorkforceProfileId"" = u.""WorkforceProfileId""
                    FROM public.""AspNetUsers"" u WHERE t.""WorkforceProfileId"" = u.""Id"" AND u.""WorkforceProfileId"" IS NOT NULL;
                UPDATE public.""TrxHrServiceRequest"" t SET ""RequestedByWorkforceProfileId"" = u.""WorkforceProfileId""
                    FROM public.""AspNetUsers"" u WHERE t.""RequestedByWorkforceProfileId"" = u.""Id"" AND u.""WorkforceProfileId"" IS NOT NULL;
                UPDATE public.""TrxEmployeeGrievance"" t SET ""ComplainantWorkforceProfileId"" = u.""WorkforceProfileId"", ""ComplainantUserId"" = COALESCE(t.""ComplainantUserId"", u.""Id"")
                    FROM public.""AspNetUsers"" u WHERE t.""ComplainantWorkforceProfileId"" = u.""Id"" AND u.""WorkforceProfileId"" IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Pemetaan ulang identitas tidak dibalik: nilai lama adalah kesalahan data, bukan keadaan yang ingin dipulihkan.
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS public.""HrdSuspensionAccessAction"";
                DROP TABLE IF EXISTS public.""HrdNotification"";
            ");
        }
    }
}
