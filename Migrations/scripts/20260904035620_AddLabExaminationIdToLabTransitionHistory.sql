START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904035620_AddLabExaminationIdToLabTransitionHistory') THEN
    ALTER TABLE public."LabTransitionHistory" ADD "LabExaminationId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904035620_AddLabExaminationIdToLabTransitionHistory') THEN
    CREATE INDEX "IX_LabTransitionHistory_LabExaminationId" ON public."LabTransitionHistory" ("LabExaminationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904035620_AddLabExaminationIdToLabTransitionHistory') THEN
    ALTER TABLE public."LabTransitionHistory" ADD CONSTRAINT "FK_LabTransitionHistory_LabExamination_LabExaminationId" FOREIGN KEY ("LabExaminationId") REFERENCES public."LabExamination" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904035620_AddLabExaminationIdToLabTransitionHistory') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904035620_AddLabExaminationIdToLabTransitionHistory', '9.0.18');
    END IF;
END $EF$;
COMMIT;

