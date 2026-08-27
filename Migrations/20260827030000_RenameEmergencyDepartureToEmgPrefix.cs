using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260827030000_RenameEmergencyDepartureToEmgPrefix")]
    public partial class RenameEmergencyDepartureToEmgPrefix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- QBE-NAM-001 melarang `Trx*` untuk entitas baru, dan registry menetapkan
                -- prefix modul EmergencyInstallationManagement adalah `Emg` (ACTIVE sejak
                -- 24 Agt 2026). Tiga entitas kepergian yang lahir di 20260826090500 memakai
                -- prefix yang salah. QBE-NAM-003 mewajibkan sumber dan tabel fisik dinormalkan
                -- bersama, jadi migration ini adalah pasangan fisik dari rename di sumber.
                --
                -- Batas lingkup yang disengaja:
                --
                -- 1. Tabel arsip "TrxEmergencyDepartureLegacyPlacement" TIDAK ikut diganti
                --    nama. Ia tidak punya entity, tidak pernah dirujuk kode aplikasi, dan Down
                --    migration 20260826090500 bergantung pada namanya untuk memulihkan
                --    penempatan lama. Mengganti namanya memutus satu-satunya jalan pemulihan
                --    itu tanpa memberi keuntungan konformansi apa pun.
                --
                -- 2. Nama PK dan sisa objek "EmgDeparture" yang diwarisi dari
                --    "TrxEmergencyTransfer" dibiarkan apa adanya. PostgreSQL tidak ikut
                --    mengganti nama constraint saat tabel di-rename, jadi kemelencengan itu
                --    sudah ada sebelum PR ini dan bukan buatannya. Yang dinormalkan di sini
                --    hanya objek yang memang dibuat oleh 20260826090500.

                ALTER TABLE IF EXISTS public."TrxEmergencyDeparture" RENAME TO "EmgDeparture";
                ALTER TABLE IF EXISTS public."TrxEmergencyDepartureEvent" RENAME TO "EmgDepartureEvent";
                ALTER TABLE IF EXISTS public."TrxEmergencyHandoverOrderItem" RENAME TO "EmgHandoverOrderItem";

                ALTER INDEX IF EXISTS public."IX_TrxEmergencyDeparture_DepartureNumber"
                    RENAME TO "IX_EmgDeparture_DepartureNumber";
                ALTER INDEX IF EXISTS public."IX_TrxEmergencyDeparture_EmergencyVisitId_PhysicalStatus_RequestedAt"
                    RENAME TO "IX_EmgDeparture_EmergencyVisitId_PhysicalStatus_RequestedAt";
                ALTER INDEX IF EXISTS public."IX_TrxEmergencyDeparture_ToServiceUnitId_HandoverStatus"
                    RENAME TO "IX_EmgDeparture_ToServiceUnitId_HandoverStatus";
                ALTER INDEX IF EXISTS public."IX_TrxEmergencyDeparture_FromServiceUnitId"
                    RENAME TO "IX_EmgDeparture_FromServiceUnitId";
                ALTER INDEX IF EXISTS public."IX_TrxEmergencyDepartureEvent_Departure_OccurredAt"
                    RENAME TO "IX_EmgDepartureEvent_Departure_OccurredAt";
                ALTER INDEX IF EXISTS public."IX_TrxEmergencyDepartureEvent_Departure_IsEffective"
                    RENAME TO "IX_EmgDepartureEvent_Departure_IsEffective";

                -- "TrxEmergencyVisit" tetap disebut apa adanya: tabel itu legacy dan
                -- normalisasinya kampanye tersendiri, bukan bagian PR ini.
                ALTER TABLE public."EmgDeparture"
                    RENAME CONSTRAINT "FK_TrxEmergencyDeparture_TrxEmergencyVisit_EmergencyVisitId"
                    TO "FK_EmgDeparture_TrxEmergencyVisit_EmergencyVisitId";
                ALTER TABLE public."EmgDeparture"
                    RENAME CONSTRAINT "FK_TrxEmergencyDeparture_MstServiceUnit_FromServiceUnitId"
                    TO "FK_EmgDeparture_MstServiceUnit_FromServiceUnitId";
                ALTER TABLE public."EmgDeparture"
                    RENAME CONSTRAINT "FK_TrxEmergencyDeparture_MstServiceUnit_ToServiceUnitId"
                    TO "FK_EmgDeparture_MstServiceUnit_ToServiceUnitId";

                ALTER TABLE public."EmgDepartureEvent"
                    RENAME CONSTRAINT "PK_TrxEmergencyDepartureEvent" TO "PK_EmgDepartureEvent";
                ALTER TABLE public."EmgDepartureEvent"
                    RENAME CONSTRAINT "FK_TrxEmergencyDepartureEvent_Departure" TO "FK_EmgDepartureEvent_Departure";
                ALTER TABLE public."EmgDepartureEvent"
                    RENAME CONSTRAINT "FK_TrxEmergencyDepartureEvent_Supersedes" TO "FK_EmgDepartureEvent_Supersedes";

                ALTER TABLE public."EmgHandoverOrderItem"
                    RENAME CONSTRAINT "PK_TrxEmergencyHandoverOrderItem" TO "PK_EmgHandoverOrderItem";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE public."EmgHandoverOrderItem"
                    RENAME CONSTRAINT "PK_EmgHandoverOrderItem" TO "PK_TrxEmergencyHandoverOrderItem";

                ALTER TABLE public."EmgDepartureEvent"
                    RENAME CONSTRAINT "FK_EmgDepartureEvent_Supersedes" TO "FK_TrxEmergencyDepartureEvent_Supersedes";
                ALTER TABLE public."EmgDepartureEvent"
                    RENAME CONSTRAINT "FK_EmgDepartureEvent_Departure" TO "FK_TrxEmergencyDepartureEvent_Departure";
                ALTER TABLE public."EmgDepartureEvent"
                    RENAME CONSTRAINT "PK_EmgDepartureEvent" TO "PK_TrxEmergencyDepartureEvent";

                ALTER TABLE public."EmgDeparture"
                    RENAME CONSTRAINT "FK_EmgDeparture_MstServiceUnit_ToServiceUnitId"
                    TO "FK_TrxEmergencyDeparture_MstServiceUnit_ToServiceUnitId";
                ALTER TABLE public."EmgDeparture"
                    RENAME CONSTRAINT "FK_EmgDeparture_MstServiceUnit_FromServiceUnitId"
                    TO "FK_TrxEmergencyDeparture_MstServiceUnit_FromServiceUnitId";
                ALTER TABLE public."EmgDeparture"
                    RENAME CONSTRAINT "FK_EmgDeparture_TrxEmergencyVisit_EmergencyVisitId"
                    TO "FK_TrxEmergencyDeparture_TrxEmergencyVisit_EmergencyVisitId";

                ALTER INDEX IF EXISTS public."IX_EmgDepartureEvent_Departure_IsEffective"
                    RENAME TO "IX_TrxEmergencyDepartureEvent_Departure_IsEffective";
                ALTER INDEX IF EXISTS public."IX_EmgDepartureEvent_Departure_OccurredAt"
                    RENAME TO "IX_TrxEmergencyDepartureEvent_Departure_OccurredAt";
                ALTER INDEX IF EXISTS public."IX_EmgDeparture_FromServiceUnitId"
                    RENAME TO "IX_TrxEmergencyDeparture_FromServiceUnitId";
                ALTER INDEX IF EXISTS public."IX_EmgDeparture_ToServiceUnitId_HandoverStatus"
                    RENAME TO "IX_TrxEmergencyDeparture_ToServiceUnitId_HandoverStatus";
                ALTER INDEX IF EXISTS public."IX_EmgDeparture_EmergencyVisitId_PhysicalStatus_RequestedAt"
                    RENAME TO "IX_TrxEmergencyDeparture_EmergencyVisitId_PhysicalStatus_RequestedAt";
                ALTER INDEX IF EXISTS public."IX_EmgDeparture_DepartureNumber"
                    RENAME TO "IX_TrxEmergencyDeparture_DepartureNumber";

                ALTER TABLE IF EXISTS public."EmgHandoverOrderItem" RENAME TO "TrxEmergencyHandoverOrderItem";
                ALTER TABLE IF EXISTS public."EmgDepartureEvent" RENAME TO "TrxEmergencyDepartureEvent";
                ALTER TABLE IF EXISTS public."EmgDeparture" RENAME TO "TrxEmergencyDeparture";
                """);
        }
    }
}
