START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix') THEN
    DO $qbe$
    DECLARE
        peta CONSTANT text[] := ARRAY[
                    'TrxClinicalDocumentIntegrity', 'MrcClinicalDocumentIntegrity',
                    'TrxClinicalNoteAddendum', 'MrcClinicalNoteAddendum',
                    'TrxClinicalNoteAuthorDelegation', 'MrcClinicalNoteAuthorDelegation',
                    'TrxMedicalRecordAccessLog', 'MrcAccessLog'
        ];
        lama text;
        baru text;
        i int;
        r record;
    BEGIN
        FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
            lama := peta[i * 2 - 1];
            baru := peta[i * 2];

            IF EXISTS (
                SELECT 1 FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                WHERE n.nspname = 'public' 
                AND c.relname = lama 
                AND c.relkind = 'r'
            ) THEN
                EXECUTE format(
                    'ALTER TABLE public.%I RENAME TO %I',
                    lama,
                    baru
                );
            END IF;
        END LOOP;

        FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
            lama := peta[i * 2 - 1];
            baru := peta[i * 2];

            FOR r IN
                SELECT t.relname AS tabel, c.conname AS nama
                FROM pg_constraint c
                JOIN pg_class t ON t.oid = c.conrelid
                JOIN pg_namespace n ON n.oid = t.relnamespace
                WHERE n.nspname = 'public'
                AND c.conname LIKE '%' || lama || '%'
            LOOP
                EXECUTE format(
                    'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                    r.tabel,
                    r.nama,
                    replace(r.nama, lama, baru)
                );
            END LOOP;
        END LOOP;

        FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
            lama := peta[i * 2 - 1];
            baru := peta[i * 2];

            FOR r IN
                SELECT c.relname AS nama
                FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                WHERE n.nspname = 'public'
                AND c.relkind = 'i'
                AND c.relname LIKE '%' || lama || '%'
            LOOP
                EXECUTE format(
                    'ALTER INDEX public.%I RENAME TO %I',
                    r.nama,
                    replace(r.nama, lama, baru)
                );
            END LOOP;
        END LOOP;
    END
    $qbe$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(
        SELECT 1 
        FROM "__EFMigrationsHistory" 
        WHERE "MigrationId" = '20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix'
    ) THEN

    DO $qbe$
    DECLARE
        peta CONSTANT text[] := ARRAY[
                    'FK_MrcAccessLog_MstMedicalRecordAccessPurpose_Acc~',
                    'FK_MrcAccessLog_MstMedicalRecordAccessPurpose_AccessPurposeId',
                    'IX_MrcAccessLog_IsFlaggedForReview_ReviewedAt_Acc~',
                    'IX_MrcAccessLog_IsFlaggedForReview_ReviewedAt_AccessedAt'
        ];
        lama text;
        baru text;
        i int;
        r record;
    BEGIN
        FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
            lama := peta[i * 2 - 1];
            baru := peta[i * 2];

            FOR r IN
                SELECT t.relname AS tabel
                FROM pg_constraint c
                JOIN pg_class t ON t.oid = c.conrelid
                JOIN pg_namespace n ON n.oid = t.relnamespace
                WHERE n.nspname = 'public'
                AND c.conname = lama
            LOOP
                EXECUTE format(
                    'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                    r.tabel,
                    lama,
                    baru
                );
            END LOOP;

            IF EXISTS (
                SELECT 1
                FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                WHERE n.nspname = 'public'
                AND c.relkind = 'i'
                AND c.relname = lama
            ) THEN
                EXECUTE format(
                    'ALTER INDEX public.%I RENAME TO %I',
                    lama,
                    baru
                );
            END IF;
        END LOOP;
    END
    $qbe$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(
        SELECT 1 
        FROM "__EFMigrationsHistory" 
        WHERE "MigrationId" = '20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix'
    ) THEN

    INSERT INTO "__EFMigrationsHistory"
    (
        "MigrationId",
        "ProductVersion"
    )
    VALUES
    (
        '20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix',
        '9.0.18'
    );

    END IF;
END $EF$;

COMMIT;