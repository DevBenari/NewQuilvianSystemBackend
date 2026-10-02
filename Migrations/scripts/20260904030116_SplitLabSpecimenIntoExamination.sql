START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN

    DO $$
    DECLARE jumlah bigint;
    BEGIN
        SELECT COUNT(*) INTO jumlah FROM public."LabSpecimen";

        IF jumlah > 0 THEN
            RAISE EXCEPTION
                'LAB-OPEN-012: tabel LabSpecimen masih memuat % baris, sehingga keenam kolom salinan tarif tidak dihapus. Pindahkan lebih dahulu setiap baris menjadi baris LabExamination yang MEMAKAI KEMBALI identitas wadah lama. Lihat 02-backend-architecture.md bagian 7 langkah 3.', jumlah;
        END IF;
    END $$;

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" DROP CONSTRAINT "FK_LabSpecimen_MstProcedure_ProcedureId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    DROP INDEX public."IX_LabSpecimen_ProcedureId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" DROP COLUMN "ProcedureCodeSnapshot";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" DROP COLUMN "ProcedureId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" DROP COLUMN "ProcedureNameSnapshot";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" DROP COLUMN "TariffCodeSnapshot";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" DROP COLUMN "TariffId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" DROP COLUMN "UnitPriceSnapshot";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904030116_SplitLabSpecimenIntoExamination', '9.0.18');
    END IF;
END $EF$;
COMMIT;

