START TRANSACTION;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" ADD "ProcedureCodeSnapshot" character varying(50);
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" ADD "ProcedureId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" ADD "ProcedureNameSnapshot" character varying(200);
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" ADD "TariffCodeSnapshot" character varying(50);
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" ADD "TariffId" uuid;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" ADD "UnitPriceSnapshot" numeric(18,2);
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    CREATE INDEX "IX_LabSpecimen_ProcedureId" ON public."LabSpecimen" ("ProcedureId");
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    ALTER TABLE public."LabSpecimen" ADD CONSTRAINT "FK_LabSpecimen_MstProcedure_ProcedureId" FOREIGN KEY ("ProcedureId") REFERENCES public."MstProcedure" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination') THEN
    DELETE FROM "__EFMigrationsHistory"
    WHERE "MigrationId" = '20260904030116_SplitLabSpecimenIntoExamination';
    END IF;
END $EF$;
COMMIT;

