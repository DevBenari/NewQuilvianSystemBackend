using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260911000000_RenameNutritionGzPrefixToGzi")]
    public partial class RenameNutritionGzPrefixToGzi : Migration
    {
        // Seluruh entity operasional Gizi pindah dari prefix `Gz` ke `Gzi`, beserta tabel
        // fisiknya. Sepuluh tabel, 28 baris data, sembilan foreign key yang seluruhnya
        // internal terhadap kelompok ini. Nol view, function, dan trigger yang menyebutnya.
        //
        // Registry baris `HealthServices | NutritionManagement / Nutrition` diubah dari `Gz`
        // menjadi `Gzi` pada 11 September 2026 oleh pemilik keputusan modul Gizi, supaya
        // sepadan dengan seluruh prefix lain yang semuanya tiga huruf. Rename di source saja
        // belum menyelesaikan apa pun selama tabel fisiknya belum ikut (QBE-NAM-003), dan
        // migration ini adalah pasangan fisiknya.
        //
        // Seluruhnya RENAME; tidak ada DROP+CREATE (QBE-DB-002), sehingga ke-28 baris data
        // tetap utuh.
        //
        // BERBEDA dari campaign rename sebelumnya di repository ini, di sini nama MEMANJANG.
        // `Gz` dua huruf menjadi `Gzi` tiga huruf, sehingga setiap kemunculannya menambah satu
        // karakter; nama yang memuatnya dua kali bertambah dua. Empat nama akibatnya melewati
        // batas 63 karakter Postgres dan harus dipotong ulang, ditandai akhiran `~` sesuai
        // cara Npgsql memotong identifier.
        //
        // Karena pemotongan itu membuang informasi, petanya ditulis EKSPLISIT kedua arah dan
        // dibangkitkan dari katalog Postgres, bukan dihitung ulang saat migration berjalan.
        // Menghitungnya dengan aturan pada arah balik akan meleset persis pada keempat nama
        // terpotong itu. Sudah diperiksa sebelum ditulis: 75 nama lama menjadi 75 nama baru
        // yang seluruhnya unik, nol tabrakan, nol melebihi 63 karakter.

        private const string Peta = @"
                'GzDietType', 'GziDietType',
                'GzFoodForm', 'GziFoodForm',
                'GzMealDelivery', 'GziMealDelivery',
                'GzMealSchedule', 'GziMealSchedule',
                'GzNutritionCareRecord', 'GziNutritionCareRecord',
                'GzNutritionOrder', 'GziNutritionOrder',
                'GzNutritionOrderHistory', 'GziNutritionOrderHistory',
                'GzPatientDiet', 'GziPatientDiet',
                'GzProductionBatch', 'GziProductionBatch',
                'GzProductionBatchDetail', 'GziProductionBatchDetail',
                'FK_GzMealDelivery_GzProductionBatchDetail_ProductionBatchDetai~', 'FK_GziMealDelivery_GziProductionBatchDetail_ProductionBatchDet~',
                'FK_GzMealDelivery_MstWorkforceProfile_DeliveredByWorkforceId', 'FK_GziMealDelivery_MstWorkforceProfile_DeliveredByWorkforceId',
                'FK_GzNutritionCareRecord_GzNutritionOrder_NutritionOrderId', 'FK_GziNutritionCareRecord_GziNutritionOrder_NutritionOrderId',
                'FK_GzNutritionCareRecord_MstDiagnosis_NutritionDiagnosisId', 'FK_GziNutritionCareRecord_MstDiagnosis_NutritionDiagnosisId',
                'FK_GzNutritionCareRecord_MstWorkforceProfile_RecordedByWorkfor~', 'FK_GziNutritionCareRecord_MstWorkforceProfile_RecordedByWorkfo~',
                'FK_GzNutritionCareRecord_TrxPatientIntegratedProgressNote_Prog~', 'FK_GziNutritionCareRecord_TrxPatientIntegratedProgressNote_Pro~',
                'FK_GzNutritionOrderHistory_GzNutritionOrder_NutritionOrderId', 'FK_GziNutritionOrderHistory_GziNutritionOrder_NutritionOrderId',
                'FK_GzNutritionOrder_MstDoctor_RequesterDoctorId', 'FK_GziNutritionOrder_MstDoctor_RequesterDoctorId',
                'FK_GzNutritionOrder_MstPatient_PatientId', 'FK_GziNutritionOrder_MstPatient_PatientId',
                'FK_GzNutritionOrder_MstWorkforceProfile_AssignedWorkforceId', 'FK_GziNutritionOrder_MstWorkforceProfile_AssignedWorkforceId',
                'FK_GzNutritionOrder_TrxPatientEncounter_EncounterId', 'FK_GziNutritionOrder_TrxPatientEncounter_EncounterId',
                'FK_GzPatientDiet_GzDietType_DietTypeId', 'FK_GziPatientDiet_GziDietType_DietTypeId',
                'FK_GzPatientDiet_GzFoodForm_FoodFormId', 'FK_GziPatientDiet_GziFoodForm_FoodFormId',
                'FK_GzPatientDiet_GzNutritionOrder_NutritionOrderId', 'FK_GziPatientDiet_GziNutritionOrder_NutritionOrderId',
                'FK_GzPatientDiet_MstPatient_PatientId', 'FK_GziPatientDiet_MstPatient_PatientId',
                'FK_GzPatientDiet_MstWorkforceProfile_PrescribedByWorkforceId', 'FK_GziPatientDiet_MstWorkforceProfile_PrescribedByWorkforceId',
                'FK_GzPatientDiet_TrxPatientEncounter_EncounterId', 'FK_GziPatientDiet_TrxPatientEncounter_EncounterId',
                'FK_GzProductionBatchDetail_GzPatientDiet_PatientDietId', 'FK_GziProductionBatchDetail_GziPatientDiet_PatientDietId',
                'FK_GzProductionBatchDetail_GzProductionBatch_ProductionBatchId', 'FK_GziProductionBatchDetail_GziProductionBatch_ProductionBatch~',
                'FK_GzProductionBatchDetail_MstPatient_PatientId', 'FK_GziProductionBatchDetail_MstPatient_PatientId',
                'FK_GzProductionBatchDetail_TrxPatientEncounter_EncounterId', 'FK_GziProductionBatchDetail_TrxPatientEncounter_EncounterId',
                'FK_GzProductionBatch_GzMealSchedule_MealScheduleId', 'FK_GziProductionBatch_GziMealSchedule_MealScheduleId',
                'PK_GzDietType', 'PK_GziDietType',
                'PK_GzFoodForm', 'PK_GziFoodForm',
                'PK_GzMealDelivery', 'PK_GziMealDelivery',
                'PK_GzMealSchedule', 'PK_GziMealSchedule',
                'PK_GzNutritionCareRecord', 'PK_GziNutritionCareRecord',
                'PK_GzNutritionOrder', 'PK_GziNutritionOrder',
                'PK_GzNutritionOrderHistory', 'PK_GziNutritionOrderHistory',
                'PK_GzPatientDiet', 'PK_GziPatientDiet',
                'PK_GzProductionBatch', 'PK_GziProductionBatch',
                'PK_GzProductionBatchDetail', 'PK_GziProductionBatchDetail',
                'IX_GzDietType_DietTypeCode', 'IX_GziDietType_DietTypeCode',
                'IX_GzFoodForm_FoodFormCode', 'IX_GziFoodForm_FoodFormCode',
                'IX_GzMealDelivery_DeliveredByWorkforceId', 'IX_GziMealDelivery_DeliveredByWorkforceId',
                'IX_GzMealDelivery_ProductionBatchDetailId', 'IX_GziMealDelivery_ProductionBatchDetailId',
                'IX_GzMealSchedule_MealScheduleCode', 'IX_GziMealSchedule_MealScheduleCode',
                'IX_GzNutritionCareRecord_NutritionDiagnosisId', 'IX_GziNutritionCareRecord_NutritionDiagnosisId',
                'IX_GzNutritionCareRecord_NutritionOrderId_VisitSequence', 'IX_GziNutritionCareRecord_NutritionOrderId_VisitSequence',
                'IX_GzNutritionCareRecord_ProgressNoteId', 'IX_GziNutritionCareRecord_ProgressNoteId',
                'IX_GzNutritionCareRecord_RecordedByWorkforceId', 'IX_GziNutritionCareRecord_RecordedByWorkforceId',
                'IX_GzNutritionCareRecord_VisitAt', 'IX_GziNutritionCareRecord_VisitAt',
                'IX_GzNutritionOrderHistory_Action_CorrelationId', 'IX_GziNutritionOrderHistory_Action_CorrelationId',
                'IX_GzNutritionOrderHistory_NutritionOrderId_OccurredAt', 'IX_GziNutritionOrderHistory_NutritionOrderId_OccurredAt',
                'IX_GzNutritionOrder_AssignedWorkforceId', 'IX_GziNutritionOrder_AssignedWorkforceId',
                'IX_GzNutritionOrder_EncounterId', 'IX_GziNutritionOrder_EncounterId',
                'IX_GzNutritionOrder_OrderNumber', 'IX_GziNutritionOrder_OrderNumber',
                'IX_GzNutritionOrder_PatientId_RequestedAt', 'IX_GziNutritionOrder_PatientId_RequestedAt',
                'IX_GzNutritionOrder_RequesterDoctorId', 'IX_GziNutritionOrder_RequesterDoctorId',
                'IX_GzNutritionOrder_Status', 'IX_GziNutritionOrder_Status',
                'IX_GzPatientDiet_DietTypeId', 'IX_GziPatientDiet_DietTypeId',
                'IX_GzPatientDiet_EncounterId', 'IX_GziPatientDiet_EncounterId',
                'IX_GzPatientDiet_EncounterId_StartAt', 'IX_GziPatientDiet_EncounterId_StartAt',
                'IX_GzPatientDiet_FoodFormId', 'IX_GziPatientDiet_FoodFormId',
                'IX_GzPatientDiet_NutritionOrderId', 'IX_GziPatientDiet_NutritionOrderId',
                'IX_GzPatientDiet_PatientId', 'IX_GziPatientDiet_PatientId',
                'IX_GzPatientDiet_PrescribedByWorkforceId', 'IX_GziPatientDiet_PrescribedByWorkforceId',
                'IX_GzProductionBatchDetail_EncounterId', 'IX_GziProductionBatchDetail_EncounterId',
                'IX_GzProductionBatchDetail_PatientDietId', 'IX_GziProductionBatchDetail_PatientDietId',
                'IX_GzProductionBatchDetail_PatientId', 'IX_GziProductionBatchDetail_PatientId',
                'IX_GzProductionBatchDetail_ProductionBatchId_EncounterId', 'IX_GziProductionBatchDetail_ProductionBatchId_EncounterId',
                'IX_GzProductionBatch_BatchNumber', 'IX_GziProductionBatch_BatchNumber',
                'IX_GzProductionBatch_MealScheduleId', 'IX_GziProductionBatch_MealScheduleId',
                'IX_GzProductionBatch_ServiceDate_MealScheduleId', 'IX_GziProductionBatch_ServiceDate_MealScheduleId',
                'IX_GzProductionBatch_ServiceDate_Status', 'IX_GziProductionBatch_ServiceDate_Status'
";

        private const string PetaBalik = @"
                'GziDietType', 'GzDietType',
                'GziFoodForm', 'GzFoodForm',
                'GziMealDelivery', 'GzMealDelivery',
                'GziMealSchedule', 'GzMealSchedule',
                'GziNutritionCareRecord', 'GzNutritionCareRecord',
                'GziNutritionOrder', 'GzNutritionOrder',
                'GziNutritionOrderHistory', 'GzNutritionOrderHistory',
                'GziPatientDiet', 'GzPatientDiet',
                'GziProductionBatch', 'GzProductionBatch',
                'GziProductionBatchDetail', 'GzProductionBatchDetail',
                'FK_GziMealDelivery_GziProductionBatchDetail_ProductionBatchDet~', 'FK_GzMealDelivery_GzProductionBatchDetail_ProductionBatchDetai~',
                'FK_GziMealDelivery_MstWorkforceProfile_DeliveredByWorkforceId', 'FK_GzMealDelivery_MstWorkforceProfile_DeliveredByWorkforceId',
                'FK_GziNutritionCareRecord_GziNutritionOrder_NutritionOrderId', 'FK_GzNutritionCareRecord_GzNutritionOrder_NutritionOrderId',
                'FK_GziNutritionCareRecord_MstDiagnosis_NutritionDiagnosisId', 'FK_GzNutritionCareRecord_MstDiagnosis_NutritionDiagnosisId',
                'FK_GziNutritionCareRecord_MstWorkforceProfile_RecordedByWorkfo~', 'FK_GzNutritionCareRecord_MstWorkforceProfile_RecordedByWorkfor~',
                'FK_GziNutritionCareRecord_TrxPatientIntegratedProgressNote_Pro~', 'FK_GzNutritionCareRecord_TrxPatientIntegratedProgressNote_Prog~',
                'FK_GziNutritionOrderHistory_GziNutritionOrder_NutritionOrderId', 'FK_GzNutritionOrderHistory_GzNutritionOrder_NutritionOrderId',
                'FK_GziNutritionOrder_MstDoctor_RequesterDoctorId', 'FK_GzNutritionOrder_MstDoctor_RequesterDoctorId',
                'FK_GziNutritionOrder_MstPatient_PatientId', 'FK_GzNutritionOrder_MstPatient_PatientId',
                'FK_GziNutritionOrder_MstWorkforceProfile_AssignedWorkforceId', 'FK_GzNutritionOrder_MstWorkforceProfile_AssignedWorkforceId',
                'FK_GziNutritionOrder_TrxPatientEncounter_EncounterId', 'FK_GzNutritionOrder_TrxPatientEncounter_EncounterId',
                'FK_GziPatientDiet_GziDietType_DietTypeId', 'FK_GzPatientDiet_GzDietType_DietTypeId',
                'FK_GziPatientDiet_GziFoodForm_FoodFormId', 'FK_GzPatientDiet_GzFoodForm_FoodFormId',
                'FK_GziPatientDiet_GziNutritionOrder_NutritionOrderId', 'FK_GzPatientDiet_GzNutritionOrder_NutritionOrderId',
                'FK_GziPatientDiet_MstPatient_PatientId', 'FK_GzPatientDiet_MstPatient_PatientId',
                'FK_GziPatientDiet_MstWorkforceProfile_PrescribedByWorkforceId', 'FK_GzPatientDiet_MstWorkforceProfile_PrescribedByWorkforceId',
                'FK_GziPatientDiet_TrxPatientEncounter_EncounterId', 'FK_GzPatientDiet_TrxPatientEncounter_EncounterId',
                'FK_GziProductionBatchDetail_GziPatientDiet_PatientDietId', 'FK_GzProductionBatchDetail_GzPatientDiet_PatientDietId',
                'FK_GziProductionBatchDetail_GziProductionBatch_ProductionBatch~', 'FK_GzProductionBatchDetail_GzProductionBatch_ProductionBatchId',
                'FK_GziProductionBatchDetail_MstPatient_PatientId', 'FK_GzProductionBatchDetail_MstPatient_PatientId',
                'FK_GziProductionBatchDetail_TrxPatientEncounter_EncounterId', 'FK_GzProductionBatchDetail_TrxPatientEncounter_EncounterId',
                'FK_GziProductionBatch_GziMealSchedule_MealScheduleId', 'FK_GzProductionBatch_GzMealSchedule_MealScheduleId',
                'PK_GziDietType', 'PK_GzDietType',
                'PK_GziFoodForm', 'PK_GzFoodForm',
                'PK_GziMealDelivery', 'PK_GzMealDelivery',
                'PK_GziMealSchedule', 'PK_GzMealSchedule',
                'PK_GziNutritionCareRecord', 'PK_GzNutritionCareRecord',
                'PK_GziNutritionOrder', 'PK_GzNutritionOrder',
                'PK_GziNutritionOrderHistory', 'PK_GzNutritionOrderHistory',
                'PK_GziPatientDiet', 'PK_GzPatientDiet',
                'PK_GziProductionBatch', 'PK_GzProductionBatch',
                'PK_GziProductionBatchDetail', 'PK_GzProductionBatchDetail',
                'IX_GziDietType_DietTypeCode', 'IX_GzDietType_DietTypeCode',
                'IX_GziFoodForm_FoodFormCode', 'IX_GzFoodForm_FoodFormCode',
                'IX_GziMealDelivery_DeliveredByWorkforceId', 'IX_GzMealDelivery_DeliveredByWorkforceId',
                'IX_GziMealDelivery_ProductionBatchDetailId', 'IX_GzMealDelivery_ProductionBatchDetailId',
                'IX_GziMealSchedule_MealScheduleCode', 'IX_GzMealSchedule_MealScheduleCode',
                'IX_GziNutritionCareRecord_NutritionDiagnosisId', 'IX_GzNutritionCareRecord_NutritionDiagnosisId',
                'IX_GziNutritionCareRecord_NutritionOrderId_VisitSequence', 'IX_GzNutritionCareRecord_NutritionOrderId_VisitSequence',
                'IX_GziNutritionCareRecord_ProgressNoteId', 'IX_GzNutritionCareRecord_ProgressNoteId',
                'IX_GziNutritionCareRecord_RecordedByWorkforceId', 'IX_GzNutritionCareRecord_RecordedByWorkforceId',
                'IX_GziNutritionCareRecord_VisitAt', 'IX_GzNutritionCareRecord_VisitAt',
                'IX_GziNutritionOrderHistory_Action_CorrelationId', 'IX_GzNutritionOrderHistory_Action_CorrelationId',
                'IX_GziNutritionOrderHistory_NutritionOrderId_OccurredAt', 'IX_GzNutritionOrderHistory_NutritionOrderId_OccurredAt',
                'IX_GziNutritionOrder_AssignedWorkforceId', 'IX_GzNutritionOrder_AssignedWorkforceId',
                'IX_GziNutritionOrder_EncounterId', 'IX_GzNutritionOrder_EncounterId',
                'IX_GziNutritionOrder_OrderNumber', 'IX_GzNutritionOrder_OrderNumber',
                'IX_GziNutritionOrder_PatientId_RequestedAt', 'IX_GzNutritionOrder_PatientId_RequestedAt',
                'IX_GziNutritionOrder_RequesterDoctorId', 'IX_GzNutritionOrder_RequesterDoctorId',
                'IX_GziNutritionOrder_Status', 'IX_GzNutritionOrder_Status',
                'IX_GziPatientDiet_DietTypeId', 'IX_GzPatientDiet_DietTypeId',
                'IX_GziPatientDiet_EncounterId', 'IX_GzPatientDiet_EncounterId',
                'IX_GziPatientDiet_EncounterId_StartAt', 'IX_GzPatientDiet_EncounterId_StartAt',
                'IX_GziPatientDiet_FoodFormId', 'IX_GzPatientDiet_FoodFormId',
                'IX_GziPatientDiet_NutritionOrderId', 'IX_GzPatientDiet_NutritionOrderId',
                'IX_GziPatientDiet_PatientId', 'IX_GzPatientDiet_PatientId',
                'IX_GziPatientDiet_PrescribedByWorkforceId', 'IX_GzPatientDiet_PrescribedByWorkforceId',
                'IX_GziProductionBatchDetail_EncounterId', 'IX_GzProductionBatchDetail_EncounterId',
                'IX_GziProductionBatchDetail_PatientDietId', 'IX_GzProductionBatchDetail_PatientDietId',
                'IX_GziProductionBatchDetail_PatientId', 'IX_GzProductionBatchDetail_PatientId',
                'IX_GziProductionBatchDetail_ProductionBatchId_EncounterId', 'IX_GzProductionBatchDetail_ProductionBatchId_EncounterId',
                'IX_GziProductionBatch_BatchNumber', 'IX_GzProductionBatch_BatchNumber',
                'IX_GziProductionBatch_MealScheduleId', 'IX_GzProductionBatch_MealScheduleId',
                'IX_GziProductionBatch_ServiceDate_MealScheduleId', 'IX_GzProductionBatch_ServiceDate_MealScheduleId',
                'IX_GziProductionBatch_ServiceDate_Status', 'IX_GzProductionBatch_ServiceDate_Status'
";

        private static string Skrip(string peta) => $$"""
            DO $qbe$
            DECLARE
                peta CONSTANT text[] := ARRAY[{{peta}}
                ];
                lama text;
                baru text;
                i int;
                r record;
            BEGIN
                -- 1. Nama tabel.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = lama AND c.relkind = 'r'
                    ) THEN
                        EXECUTE format('ALTER TABLE public.%I RENAME TO %I', lama, baru);
                    END IF;
                END LOOP;

                -- 2. Constraint, dicocokkan PERSIS dengan namanya. Mengganti nama constraint
                --    PK atau unique sekaligus mengganti nama index penopangnya.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT t.relname AS tabel
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = 'public' AND c.conname = lama
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                            r.tabel, lama, baru);
                    END LOOP;
                END LOOP;

                -- 3. Sisa index yang tidak ditopang constraint.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relkind = 'i' AND c.relname = lama
                    ) THEN
                        EXECUTE format('ALTER INDEX public.%I RENAME TO %I', lama, baru);
                    END IF;
                END LOOP;
            END
            $qbe$;
            """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(Peta));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
