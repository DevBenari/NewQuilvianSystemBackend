START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904072427_AddReferralPointerToPatientEncounter') THEN
    ALTER TABLE public."TrxPatientEncounter" ADD "ReferralDoctorId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904072427_AddReferralPointerToPatientEncounter') THEN
    ALTER TABLE public."TrxPatientEncounter" ADD "ReferralInstitutionId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904072427_AddReferralPointerToPatientEncounter') THEN
    CREATE INDEX "IX_TrxPatientEncounter_ReferralDoctorId" ON public."TrxPatientEncounter" ("ReferralDoctorId") WHERE "ReferralDoctorId" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904072427_AddReferralPointerToPatientEncounter') THEN
    CREATE INDEX "IX_TrxPatientEncounter_ReferralInstitutionId" ON public."TrxPatientEncounter" ("ReferralInstitutionId") WHERE "ReferralInstitutionId" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904072427_AddReferralPointerToPatientEncounter') THEN
    ALTER TABLE public."TrxPatientEncounter" ADD CONSTRAINT "FK_TrxPatientEncounter_MstReferralDoctor_ReferralDoctorId" FOREIGN KEY ("ReferralDoctorId") REFERENCES public."MstReferralDoctor" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904072427_AddReferralPointerToPatientEncounter') THEN
    ALTER TABLE public."TrxPatientEncounter" ADD CONSTRAINT "FK_TrxPatientEncounter_MstReferralInstitution_ReferralInstitut~" FOREIGN KEY ("ReferralInstitutionId") REFERENCES public."MstReferralInstitution" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904072427_AddReferralPointerToPatientEncounter') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904072427_AddReferralPointerToPatientEncounter', '9.0.18');
    END IF;
END $EF$;
COMMIT;

