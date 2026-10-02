START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE TABLE public."RadReport" (
        "Id" uuid NOT NULL,
        "RadStudyId" uuid NOT NULL,
        "RadOrderId" uuid NOT NULL,
        "EncounterId" uuid NOT NULL,
        "ReportNumber" character varying(64) NOT NULL,
        "ReportStatus" integer NOT NULL,
        "CurrentVersionNumber" integer NOT NULL,
        "FirstReleasedAt" timestamp with time zone,
        "LastReleasedAt" timestamp with time zone,
        "Version" integer NOT NULL,
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
        CONSTRAINT "PK_RadReport" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RadReport_RadOrder_RadOrderId" FOREIGN KEY ("RadOrderId") REFERENCES public."RadOrder" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RadReport_RadStudy_RadStudyId" FOREIGN KEY ("RadStudyId") REFERENCES public."RadStudy" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE TABLE public."RadReportVersion" (
        "Id" uuid NOT NULL,
        "RadReportId" uuid NOT NULL,
        "VersionNumber" integer NOT NULL,
        "PreviousVersionId" uuid,
        "VersionStatus" integer NOT NULL,
        "IsAmendment" boolean NOT NULL,
        "Findings" character varying(8000),
        "Impression" character varying(4000) NOT NULL,
        "Recommendation" character varying(2000),
        "AuthorUserId" uuid NOT NULL,
        "AuthorRoleSnapshot" integer NOT NULL,
        "DraftedAt" timestamp with time zone NOT NULL,
        "ValidatorUserId" uuid,
        "ValidatedAt" timestamp with time zone,
        "ReleasedAt" timestamp with time zone,
        "AmendmentReason" character varying(1000),
        "Version" integer NOT NULL,
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
        CONSTRAINT "PK_RadReportVersion" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RadReportVersion_RadReportVersion_PreviousVersionId" FOREIGN KEY ("PreviousVersionId") REFERENCES public."RadReportVersion" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RadReportVersion_RadReport_RadReportId" FOREIGN KEY ("RadReportId") REFERENCES public."RadReport" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE INDEX "IX_RadReport_EncounterId" ON public."RadReport" ("EncounterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE INDEX "IX_RadReport_RadOrderId" ON public."RadReport" ("RadOrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE UNIQUE INDEX "IX_RadReport_RadStudyId" ON public."RadReport" ("RadStudyId") WHERE "IsDelete" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE UNIQUE INDEX "IX_RadReport_ReportNumber" ON public."RadReport" ("ReportNumber") WHERE "IsDelete" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE INDEX "IX_RadReport_ReportStatus" ON public."RadReport" ("ReportStatus");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE INDEX "IX_RadReportVersion_AuthorUserId" ON public."RadReportVersion" ("AuthorUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE INDEX "IX_RadReportVersion_PreviousVersionId" ON public."RadReportVersion" ("PreviousVersionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    CREATE UNIQUE INDEX "IX_RadReportVersion_RadReportId_VersionNumber" ON public."RadReportVersion" ("RadReportId", "VersionNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911025734_AddRadReport') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260911025734_AddRadReport', '9.0.18');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911045053_AddRadOrderUrgency') THEN
    ALTER TABLE public."RadOrder" ADD "IsUrgent" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911045053_AddRadOrderUrgency') THEN
    ALTER TABLE public."RadOrder" ADD "UrgentMarkedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911045053_AddRadOrderUrgency') THEN
    ALTER TABLE public."RadOrder" ADD "UrgentMarkedByUserId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911045053_AddRadOrderUrgency') THEN
    CREATE INDEX "IX_RadOrder_ModalityId_IsUrgent_OrderStatus" ON public."RadOrder" ("ModalityId", "IsUrgent", "OrderStatus");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911045053_AddRadOrderUrgency') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260911045053_AddRadOrderUrgency', '9.0.18');
    END IF;
END $EF$;
COMMIT;

