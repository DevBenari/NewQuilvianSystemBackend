using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260826090500_ImplementIgdFullPatientJourney")]
    public partial class ImplementIgdFullPatientJourney : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE public."TrxPatientAssessment" ALTER COLUMN "QueueId" DROP NOT NULL;
                ALTER TABLE public."TrxDoctorConsultation" ALTER COLUMN "QueueId" DROP NOT NULL;

                ALTER TABLE public."MstServiceUnit" ADD COLUMN IF NOT EXISTS "OrganizationUnitId" uuid NULL;
                CREATE INDEX IF NOT EXISTS "IX_MstServiceUnit_OrganizationUnitId"
                    ON public."MstServiceUnit" ("OrganizationUnitId");
                ALTER TABLE public."MstServiceUnit"
                    ADD CONSTRAINT "FK_MstServiceUnit_MstOrganizationUnit_OrganizationUnitId"
                    FOREIGN KEY ("OrganizationUnitId") REFERENCES public."MstOrganizationUnit" ("Id")
                    ON DELETE RESTRICT;

                ALTER TABLE public."TrxEmergencyVisit" ADD COLUMN IF NOT EXISTS "DuplicateEpisodeOverrideReason" varchar(1000) NULL;
                ALTER TABLE public."TrxEmergencyVisit" ADD COLUMN IF NOT EXISTS "DuplicateEpisodeOverrideByUserId" uuid NULL;
                ALTER TABLE public."TrxEmergencyVisit" ADD COLUMN IF NOT EXISTS "DuplicateEpisodeOverrideAt" timestamptz NULL;
                ALTER TABLE public."TrxEmergencyVisit" ADD COLUMN IF NOT EXISTS "DuplicateEpisodeOverrideOfVisitId" uuid NULL;
                CREATE INDEX IF NOT EXISTS "IX_TrxEmergencyVisit_DuplicateEpisodeOverrideAt"
                    ON public."TrxEmergencyVisit" ("DuplicateEpisodeOverrideAt")
                    WHERE "DuplicateEpisodeOverrideAt" IS NOT NULL;
                CREATE INDEX IF NOT EXISTS "IX_TrxEmergencyVisit_DuplicateEpisodeOverrideOfVisitId"
                    ON public."TrxEmergencyVisit" ("DuplicateEpisodeOverrideOfVisitId")
                    WHERE "DuplicateEpisodeOverrideOfVisitId" IS NOT NULL;

                UPDATE public."TrxPatientEncounter" e
                SET "EncounterType" = 2
                WHERE e."Id" IN (
                    SELECT v."EncounterId" FROM public."TrxEmergencyVisit" v
                    WHERE v."EncounterId" IS NOT NULL
                ) AND e."EncounterType" = 1;

                ALTER TABLE public."TrxEmergencyTransfer" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyTransfer_AspNetUsers_AcceptedByUserId";
                ALTER TABLE public."TrxEmergencyTransfer" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyTransfer_AspNetUsers_ReceivingNurseUserId";
                ALTER TABLE public."TrxEmergencyTransfer" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyTransfer_AspNetUsers_RequestedByUserId";
                ALTER TABLE public."TrxEmergencyTransfer" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyTransfer_AspNetUsers_SendingNurseUserId";
                ALTER TABLE public."TrxEmergencyTransfer" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyTransfer_MstServiceUnit_FromServiceUnitId";
                ALTER TABLE public."TrxEmergencyTransfer" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyTransfer_MstServiceUnit_ToServiceUnitId";
                ALTER TABLE public."TrxEmergencyTransfer" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyTransfer_TrxEmergencyVisit_EmergencyVisitId";

                ALTER TABLE public."TrxEmergencyTransfer" RENAME TO "TrxEmergencyDeparture";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME COLUMN "TransferNumber" TO "DepartureNumber";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME COLUMN "TransferReason" TO "DepartureReason";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME COLUMN "HandoverSummary" TO "SituationSummary";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME COLUMN "RejectionReason" TO "HandoverRejectionReason";

                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "PhysicalStatus" integer NOT NULL DEFAULT 1;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "HandoverStatus" integer NOT NULL DEFAULT 1;
                UPDATE public."TrxEmergencyDeparture"
                SET "PhysicalStatus" = CASE "TransferStatus"
                        WHEN 3 THEN 2 WHEN 4 THEN 3 WHEN 6 THEN 9 ELSE 1 END,
                    "HandoverStatus" = CASE "TransferStatus"
                        WHEN 2 THEN 3 WHEN 4 THEN 3 WHEN 5 THEN 4 WHEN 6 THEN 9 ELSE 1 END;

                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "BackgroundSummary" varchar(2000) NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "AssessmentSummary" varchar(2000) NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "RecommendationSummary" varchar(2000) NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "UnavailableSectionReason" varchar(1000) NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "UnavailableSections" varchar(250) NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "AllergySnapshot" varchar(1000) NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "LastVitalSignId" uuid NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "TriageLevelSnapshot" varchar(150) NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "CancellationReason" varchar(1000) NULL;

                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_AcceptedByUserId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_EmergencyVisitId_TransferStatus_RequestedAt";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_FromBedId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_FromRoomId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_FromServiceUnitId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_ReceivingNurseUserId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_RequestedByUserId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_SendingNurseUserId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_ToBedId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_ToRoomId";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_ToServiceUnitId_TransferStatus";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyTransfer_TransferNumber";

                -- Empat kolom penempatan diarsipkan SEBELUM dibuang. 02-backend-architecture
                -- bagian 6.2 melarang langkah ini dijalankan selama IGD-UNK-03 belum terjawab,
                -- yaitu berapa baris yang keempat kolomnya masih terisi. Pertanyaan itu hanya
                -- dapat dijawab dengan kueri ke basis data bersama, dan otorisasinya belum ada.
                --
                -- Mengarsipkan lebih dulu membuat migration ini aman dijalankan tanpa menunggu
                -- jawabannya: bila ternyata nol baris, tabel arsipnya kosong dan tidak
                -- merugikan siapa pun; bila ternyata ada isinya, riwayat penempatan pasien
                -- tetap utuh dan Down dapat memulihkannya. Tanpa ini, satu baris yang terisi
                -- berarti kehilangan permanen.
                CREATE TABLE IF NOT EXISTS public."TrxEmergencyDepartureLegacyPlacement" (
                    "EmergencyDepartureId" uuid NOT NULL PRIMARY KEY,
                    "FromRoomId" uuid NULL,
                    "ToRoomId" uuid NULL,
                    "FromBedId" uuid NULL,
                    "ToBedId" uuid NULL,
                    "ArchivedAt" timestamptz NOT NULL DEFAULT now()
                );

                INSERT INTO public."TrxEmergencyDepartureLegacyPlacement"
                    ("EmergencyDepartureId", "FromRoomId", "ToRoomId", "FromBedId", "ToBedId")
                SELECT "Id", "FromRoomId", "ToRoomId", "FromBedId", "ToBedId"
                FROM public."TrxEmergencyDeparture"
                WHERE "FromRoomId" IS NOT NULL
                   OR "ToRoomId" IS NOT NULL
                   OR "FromBedId" IS NOT NULL
                   OR "ToBedId" IS NOT NULL
                ON CONFLICT ("EmergencyDepartureId") DO NOTHING;

                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "AcceptedAt";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "AcceptedByUserId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "FromRoomId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "ToRoomId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "FromBedId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "ToBedId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "TransferStatus";

                ALTER TABLE public."TrxEmergencyDeparture"
                    ADD CONSTRAINT "FK_TrxEmergencyDeparture_TrxEmergencyVisit_EmergencyVisitId"
                    FOREIGN KEY ("EmergencyVisitId") REFERENCES public."TrxEmergencyVisit" ("Id") ON DELETE RESTRICT;
                ALTER TABLE public."TrxEmergencyDeparture"
                    ADD CONSTRAINT "FK_TrxEmergencyDeparture_MstServiceUnit_FromServiceUnitId"
                    FOREIGN KEY ("FromServiceUnitId") REFERENCES public."MstServiceUnit" ("Id") ON DELETE RESTRICT;
                ALTER TABLE public."TrxEmergencyDeparture"
                    ADD CONSTRAINT "FK_TrxEmergencyDeparture_MstServiceUnit_ToServiceUnitId"
                    FOREIGN KEY ("ToServiceUnitId") REFERENCES public."MstServiceUnit" ("Id") ON DELETE RESTRICT;
                CREATE UNIQUE INDEX "IX_TrxEmergencyDeparture_DepartureNumber"
                    ON public."TrxEmergencyDeparture" ("DepartureNumber");
                CREATE INDEX "IX_TrxEmergencyDeparture_EmergencyVisitId_PhysicalStatus_RequestedAt"
                    ON public."TrxEmergencyDeparture" ("EmergencyVisitId", "PhysicalStatus", "RequestedAt");
                CREATE INDEX "IX_TrxEmergencyDeparture_ToServiceUnitId_HandoverStatus"
                    ON public."TrxEmergencyDeparture" ("ToServiceUnitId", "HandoverStatus");
                CREATE INDEX "IX_TrxEmergencyDeparture_FromServiceUnitId"
                    ON public."TrxEmergencyDeparture" ("FromServiceUnitId");

                CREATE TABLE public."TrxEmergencyDepartureEvent" (
                    "Id" uuid NOT NULL, "EmergencyDepartureId" uuid NOT NULL,
                    "EventType" integer NOT NULL, "OccurredAt" timestamptz NOT NULL,
                    "RecordedAt" timestamptz NOT NULL, "RecordedByUserId" uuid NOT NULL,
                    "Reason" varchar(1000) NULL, "DowntimeReference" varchar(250) NULL,
                    "IsEffective" boolean NOT NULL, "SupersedesEventId" uuid NULL,
                    "ApprovedByUserId" uuid NULL, "IsActive" boolean NOT NULL,
                    "CreateDateTime" timestamptz NOT NULL, "CreateBy" uuid NOT NULL,
                    "UpdateDateTime" timestamptz NULL, "UpdateBy" uuid NOT NULL,
                    "DeleteDateTime" timestamptz NULL, "DeleteBy" uuid NOT NULL,
                    "CancelDateTime" timestamptz NULL, "CancelBy" uuid NOT NULL,
                    "IsCancel" boolean NOT NULL, "IsDelete" boolean NOT NULL,
                    CONSTRAINT "PK_TrxEmergencyDepartureEvent" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_TrxEmergencyDepartureEvent_Departure" FOREIGN KEY ("EmergencyDepartureId")
                        REFERENCES public."TrxEmergencyDeparture" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_TrxEmergencyDepartureEvent_Supersedes" FOREIGN KEY ("SupersedesEventId")
                        REFERENCES public."TrxEmergencyDepartureEvent" ("Id") ON DELETE RESTRICT
                );
                CREATE INDEX "IX_TrxEmergencyDepartureEvent_Departure_OccurredAt"
                    ON public."TrxEmergencyDepartureEvent" ("EmergencyDepartureId", "OccurredAt");
                CREATE INDEX "IX_TrxEmergencyDepartureEvent_Departure_IsEffective"
                    ON public."TrxEmergencyDepartureEvent" ("EmergencyDepartureId", "IsEffective");

                INSERT INTO public."TrxEmergencyDepartureEvent" (
                    "Id", "EmergencyDepartureId", "EventType", "OccurredAt", "RecordedAt",
                    "RecordedByUserId", "IsEffective", "IsActive", "CreateDateTime", "CreateBy",
                    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
                SELECT gen_random_uuid(), "Id",
                    CASE "PhysicalStatus" WHEN 2 THEN 2 WHEN 3 THEN 3 WHEN 9 THEN 9 ELSE 1 END,
                    COALESCE("ArrivedAt", "DepartedAt", "RequestedAt"),
                    COALESCE("ArrivedAt", "DepartedAt", "RequestedAt"),
                    "RequestedByUserId", true, true,
                    COALESCE("CreateDateTime", "RequestedAt"), "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
                    false, false
                FROM public."TrxEmergencyDeparture";

                CREATE TABLE public."TrxEmergencyHandoverOrderItem" (
                    "Id" uuid NOT NULL, "EmergencyDepartureId" uuid NOT NULL,
                    "OrderKind" integer NOT NULL, "OrderSource" integer NOT NULL,
                    "OrderReferenceId" uuid NULL, "ExternalReference" varchar(150) NULL,
                    "OrderDescription" varchar(500) NOT NULL, "Action" integer NOT NULL,
                    "ActionReason" varchar(1000) NULL, "ActionByUserId" uuid NOT NULL,
                    "ActionAt" timestamptz NOT NULL, "ToServiceUnitId" uuid NULL,
                    "AcceptanceStatus" integer NOT NULL, "AcceptedByUserId" uuid NULL,
                    "AcceptedAt" timestamptz NULL, "RejectionReason" varchar(1000) NULL,
                    "IsEffective" boolean NOT NULL, "SupersedesOrderItemId" uuid NULL,
                    "IsActive" boolean NOT NULL, "CreateDateTime" timestamptz NOT NULL,
                    "CreateBy" uuid NOT NULL, "UpdateDateTime" timestamptz NULL,
                    "UpdateBy" uuid NOT NULL, "DeleteDateTime" timestamptz NULL,
                    "DeleteBy" uuid NOT NULL, "CancelDateTime" timestamptz NULL,
                    "CancelBy" uuid NOT NULL, "IsCancel" boolean NOT NULL, "IsDelete" boolean NOT NULL,
                    CONSTRAINT "PK_TrxEmergencyHandoverOrderItem" PRIMARY KEY ("Id"),
                    CONSTRAINT "CK_EmergencyOrderItem_Reference" CHECK (
                        ("OrderSource" = 1 AND "OrderReferenceId" IS NOT NULL AND "ExternalReference" IS NULL)
                        OR ("OrderSource" = 2 AND "OrderReferenceId" IS NULL AND "ExternalReference" IS NOT NULL)),
                    CONSTRAINT "FK_EmergencyOrderItem_Departure" FOREIGN KEY ("EmergencyDepartureId")
                        REFERENCES public."TrxEmergencyDeparture" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_EmergencyOrderItem_ToServiceUnit" FOREIGN KEY ("ToServiceUnitId")
                        REFERENCES public."MstServiceUnit" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_EmergencyOrderItem_Supersedes" FOREIGN KEY ("SupersedesOrderItemId")
                        REFERENCES public."TrxEmergencyHandoverOrderItem" ("Id") ON DELETE RESTRICT
                );
                CREATE UNIQUE INDEX "UX_EmergencyHandoverOrderItem_Internal"
                    ON public."TrxEmergencyHandoverOrderItem" ("EmergencyDepartureId", "OrderKind", "OrderReferenceId")
                    WHERE "IsEffective" AND "OrderSource" = 1 AND NOT "IsDelete";
                CREATE UNIQUE INDEX "UX_EmergencyHandoverOrderItem_External"
                    ON public."TrxEmergencyHandoverOrderItem" ("EmergencyDepartureId", "OrderKind", "ExternalReference")
                    WHERE "IsEffective" AND "OrderSource" = 2 AND NOT "IsDelete";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS public."TrxEmergencyHandoverOrderItem";
                DROP TABLE IF EXISTS public."TrxEmergencyDepartureEvent";

                DROP INDEX IF EXISTS public."IX_TrxEmergencyDeparture_DepartureNumber";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyDeparture_EmergencyVisitId_PhysicalStatus_RequestedAt";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyDeparture_ToServiceUnitId_HandoverStatus";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyDeparture_FromServiceUnitId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyDeparture_TrxEmergencyVisit_EmergencyVisitId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyDeparture_MstServiceUnit_FromServiceUnitId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP CONSTRAINT IF EXISTS "FK_TrxEmergencyDeparture_MstServiceUnit_ToServiceUnitId";

                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "AcceptedAt" timestamptz NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "AcceptedByUserId" uuid NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "FromRoomId" uuid NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "ToRoomId" uuid NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "FromBedId" uuid NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "ToBedId" uuid NULL;
                ALTER TABLE public."TrxEmergencyDeparture" ADD COLUMN "TransferStatus" integer NOT NULL DEFAULT 1;
                UPDATE public."TrxEmergencyDeparture"
                SET "TransferStatus" = CASE
                    WHEN "PhysicalStatus" = 9 THEN 6
                    WHEN "PhysicalStatus" = 3 THEN 4
                    WHEN "PhysicalStatus" = 2 THEN 3
                    WHEN "HandoverStatus" = 4 THEN 5
                    WHEN "HandoverStatus" = 3 THEN 2
                    ELSE 1 END;

                -- Memulihkan penempatan yang diarsipkan Up. Tabel arsipnya sengaja TIDAK
                -- ikut dihapus: ia satu-satunya salinan riwayat itu, dan membuangnya saat
                -- mundur berarti pemulihan hanya dapat dilakukan sekali.
                UPDATE public."TrxEmergencyDeparture" d
                SET "FromRoomId" = a."FromRoomId",
                    "ToRoomId" = a."ToRoomId",
                    "FromBedId" = a."FromBedId",
                    "ToBedId" = a."ToBedId"
                FROM public."TrxEmergencyDepartureLegacyPlacement" a
                WHERE a."EmergencyDepartureId" = d."Id";

                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "PhysicalStatus";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "HandoverStatus";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "BackgroundSummary";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "AssessmentSummary";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "RecommendationSummary";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "UnavailableSectionReason";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "UnavailableSections";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "AllergySnapshot";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "LastVitalSignId";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "TriageLevelSnapshot";
                ALTER TABLE public."TrxEmergencyDeparture" DROP COLUMN "CancellationReason";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME COLUMN "HandoverRejectionReason" TO "RejectionReason";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME COLUMN "SituationSummary" TO "HandoverSummary";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME COLUMN "DepartureReason" TO "TransferReason";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME COLUMN "DepartureNumber" TO "TransferNumber";
                ALTER TABLE public."TrxEmergencyDeparture" RENAME TO "TrxEmergencyTransfer";

                UPDATE public."TrxPatientEncounter" e SET "EncounterType" = 1
                WHERE e."Id" IN (SELECT v."EncounterId" FROM public."TrxEmergencyVisit" v WHERE v."EncounterId" IS NOT NULL)
                    AND e."EncounterType" = 2;

                DROP INDEX IF EXISTS public."IX_TrxEmergencyVisit_DuplicateEpisodeOverrideAt";
                DROP INDEX IF EXISTS public."IX_TrxEmergencyVisit_DuplicateEpisodeOverrideOfVisitId";
                ALTER TABLE public."TrxEmergencyVisit" DROP COLUMN IF EXISTS "DuplicateEpisodeOverrideReason";
                ALTER TABLE public."TrxEmergencyVisit" DROP COLUMN IF EXISTS "DuplicateEpisodeOverrideByUserId";
                ALTER TABLE public."TrxEmergencyVisit" DROP COLUMN IF EXISTS "DuplicateEpisodeOverrideAt";
                ALTER TABLE public."TrxEmergencyVisit" DROP COLUMN IF EXISTS "DuplicateEpisodeOverrideOfVisitId";

                ALTER TABLE public."MstServiceUnit" DROP CONSTRAINT IF EXISTS "FK_MstServiceUnit_MstOrganizationUnit_OrganizationUnitId";
                DROP INDEX IF EXISTS public."IX_MstServiceUnit_OrganizationUnitId";
                ALTER TABLE public."MstServiceUnit" DROP COLUMN IF EXISTS "OrganizationUnitId";

                ALTER TABLE public."TrxPatientAssessment" ALTER COLUMN "QueueId" SET NOT NULL;
                ALTER TABLE public."TrxDoctorConsultation" ALTER COLUMN "QueueId" SET NOT NULL;
                """);
        }
    }
}
