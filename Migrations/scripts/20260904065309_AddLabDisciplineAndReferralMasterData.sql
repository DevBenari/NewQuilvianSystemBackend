START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    ALTER TABLE public."MstProcedure" ADD "LabDiscipline" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    CREATE TABLE public."MstReferralInstitution" (
        "Id" uuid NOT NULL,
        "InstitutionCode" character varying(50) NOT NULL,
        "InstitutionName" character varying(200) NOT NULL,
        "Address" character varying(500),
        "PhoneNumber" character varying(50),
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreateDateTime" timestamp with time zone NOT NULL,
        "CreateBy" uuid NOT NULL,
        "UpdateDateTime" timestamp with time zone,
        "UpdateBy" uuid NOT NULL,
        "DeleteDateTime" timestamp with time zone,
        "DeleteBy" uuid NOT NULL,
        "CancelDateTime" timestamp with time zone,
        "CancelBy" uuid NOT NULL,
        "IsCancel" boolean NOT NULL,
        "IsDelete" boolean NOT NULL,
        CONSTRAINT "PK_MstReferralInstitution" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    CREATE TABLE public."MstReferralDoctor" (
        "Id" uuid NOT NULL,
        "ReferralInstitutionId" uuid NOT NULL,
        "DoctorName" character varying(200) NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreateDateTime" timestamp with time zone NOT NULL,
        "CreateBy" uuid NOT NULL,
        "UpdateDateTime" timestamp with time zone,
        "UpdateBy" uuid NOT NULL,
        "DeleteDateTime" timestamp with time zone,
        "DeleteBy" uuid NOT NULL,
        "CancelDateTime" timestamp with time zone,
        "CancelBy" uuid NOT NULL,
        "IsCancel" boolean NOT NULL,
        "IsDelete" boolean NOT NULL,
        CONSTRAINT "PK_MstReferralDoctor" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MstReferralDoctor_MstReferralInstitution_ReferralInstitutio~" FOREIGN KEY ("ReferralInstitutionId") REFERENCES public."MstReferralInstitution" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    CREATE INDEX "IX_MstProcedure_LabDiscipline" ON public."MstProcedure" ("LabDiscipline") WHERE "LabDiscipline" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    CREATE INDEX "IX_MstReferralDoctor_DoctorName" ON public."MstReferralDoctor" ("DoctorName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    CREATE INDEX "IX_MstReferralDoctor_ReferralInstitutionId" ON public."MstReferralDoctor" ("ReferralInstitutionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    CREATE UNIQUE INDEX "IX_MstReferralInstitution_InstitutionCode" ON public."MstReferralInstitution" ("InstitutionCode") WHERE "IsDelete" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    CREATE INDEX "IX_MstReferralInstitution_InstitutionName" ON public."MstReferralInstitution" ("InstitutionName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904065309_AddLabDisciplineAndReferralMasterData') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904065309_AddLabDisciplineAndReferralMasterData', '9.0.18');
    END IF;
END $EF$;
COMMIT;

