START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprCase" (
        "Id" uuid NOT NULL,
        "CaseNumber" character varying(50) NOT NULL,
        "PatientId" uuid NOT NULL,
        "EncounterId" uuid NOT NULL,
        "RequesterDoctorId" uuid NOT NULL,
        "PrimarySurgeonId" uuid NOT NULL,
        "CaseType" integer NOT NULL,
        "Priority" integer NOT NULL,
        "Status" integer NOT NULL,
        "Outcome" integer,
        "Indication" character varying(4000) NOT NULL,
        "Laterality" character varying(30),
        "EstimatedMinutes" integer NOT NULL,
        "RequestedAt" timestamp with time zone NOT NULL,
        "PreferredAt" timestamp with time zone,
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
        CONSTRAINT "PK_OprCase" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprCase_MstDoctor_PrimarySurgeonId" FOREIGN KEY ("PrimarySurgeonId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OprCase_MstDoctor_RequesterDoctorId" FOREIGN KEY ("RequesterDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OprCase_MstPatient_PatientId" FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OprCase_TrxPatientEncounter_EncounterId" FOREIGN KEY ("EncounterId") REFERENCES public."TrxPatientEncounter" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprAnesthesiaRecord" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "AssessmentSummary" character varying(4000) NOT NULL,
        "Technique" character varying(4000) NOT NULL,
        "MedicationFluidSummary" character varying(8000) NOT NULL,
        "AirwaySummary" character varying(4000) NOT NULL,
        "MonitoringSummary" character varying(8000) NOT NULL,
        "EventSummary" character varying(4000),
        "FinalCondition" character varying(4000) NOT NULL,
        "FinalizedBy" uuid,
        "FinalizedAt" timestamp with time zone,
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
        CONSTRAINT "PK_OprAnesthesiaRecord" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprAnesthesiaRecord_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprCaseProcedure" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "PatientProcedureId" uuid NOT NULL,
        "IsPrimary" boolean NOT NULL,
        "Sequence" integer NOT NULL,
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
        CONSTRAINT "PK_OprCaseProcedure" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprCaseProcedure_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OprCaseProcedure_TrxPatientProcedure_PatientProcedureId" FOREIGN KEY ("PatientProcedureId") REFERENCES public."TrxPatientProcedure" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprExecutionRecord" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "PreDiagnosis" character varying(4000) NOT NULL,
        "PostDiagnosis" character varying(4000) NOT NULL,
        "Findings" character varying(8000) NOT NULL,
        "Technique" character varying(8000) NOT NULL,
        "Complications" character varying(4000),
        "BloodLossMl" numeric,
        "SpecimenNote" character varying(2000),
        "ImplantDrainNote" character varying(2000),
        "PostPlan" character varying(4000) NOT NULL,
        "StartedAt" timestamp with time zone NOT NULL,
        "FinishedAt" timestamp with time zone,
        "FinalizedBy" uuid,
        "FinalizedAt" timestamp with time zone,
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
        CONSTRAINT "PK_OprExecutionRecord" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprExecutionRecord_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprHandover" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "DestinationUnitId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "ConditionSummary" character varying(4000) NOT NULL,
        "DeviceTherapySummary" character varying(4000),
        "RiskSummary" character varying(4000),
        "InstructionSummary" character varying(4000),
        "SentBy" uuid NOT NULL,
        "SentAt" timestamp with time zone NOT NULL,
        "ReceivedBy" uuid,
        "AcceptedAt" timestamp with time zone,
        "RejectionReason" character varying(2000),
        "Revision" integer NOT NULL,
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
        CONSTRAINT "PK_OprHandover" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprHandover_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprIntegrationDelivery" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "Destination" character varying(50) NOT NULL,
        "MessageType" character varying(100) NOT NULL,
        "IdempotencyKey" character varying(150) NOT NULL,
        "CorrelationId" character varying(100) NOT NULL,
        "PayloadReference" character varying(250) NOT NULL,
        "Status" integer NOT NULL,
        "RetryCount" integer NOT NULL,
        "LastAttemptAt" timestamp with time zone,
        "LastErrorCode" character varying(100),
        "AcceptedReference" character varying(150),
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
        CONSTRAINT "PK_OprIntegrationDelivery" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprIntegrationDelivery_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprMaterialUsage" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "ExternalItemId" uuid NOT NULL,
        "ItemType" integer NOT NULL,
        "Quantity" numeric(18,4) NOT NULL,
        "UnitCode" character varying(30) NOT NULL,
        "Outcome" integer NOT NULL,
        "BatchNumber" character varying(100),
        "SerialNumber" character varying(150),
        "OccurredAt" timestamp with time zone NOT NULL,
        "RecordedBy" uuid NOT NULL,
        "Revision" integer NOT NULL,
        "CorrectionReason" character varying(2000),
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
        CONSTRAINT "PK_OprMaterialUsage" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprMaterialUsage_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprRecovery" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "ScoreSystem" character varying(100) NOT NULL,
        "ScoreValue" numeric(18,4),
        "ObservationJson" jsonb NOT NULL,
        "Decision" integer NOT NULL,
        "DecisionNote" character varying(2000),
        "ReleasedBy" uuid,
        "ReleasedAt" timestamp with time zone,
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
        CONSTRAINT "PK_OprRecovery" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprRecovery_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprSafetyChecklist" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "Phase" integer NOT NULL,
        "TemplateVersion" character varying(50) NOT NULL,
        "Revision" integer NOT NULL,
        "Status" integer NOT NULL,
        "ItemsJson" jsonb NOT NULL,
        "SignedByUserId" uuid,
        "SignedAt" timestamp with time zone,
        "IsEmergencyBypass" boolean NOT NULL,
        "BypassReason" character varying(2000),
        "BypassResponsibleUserId" uuid,
        "CompletedAfterStableAt" timestamp with time zone,
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
        CONSTRAINT "PK_OprSafetyChecklist" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprSafetyChecklist_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprSchedule" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "RoomId" uuid NOT NULL,
        "StartAt" timestamp with time zone NOT NULL,
        "EndAt" timestamp with time zone NOT NULL,
        "BufferBeforeMinutes" integer NOT NULL,
        "BufferAfterMinutes" integer NOT NULL,
        "Revision" integer NOT NULL,
        "IsCurrent" boolean NOT NULL,
        "ChangeReason" character varying(500),
        "ChangedByUserId" uuid NOT NULL,
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
        CONSTRAINT "PK_OprSchedule" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprSchedule_MstRoom_RoomId" FOREIGN KEY ("RoomId") REFERENCES public."MstRoom" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OprSchedule_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprStatusHistory" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "FromStatus" integer,
        "ToStatus" integer NOT NULL,
        "Action" character varying(50) NOT NULL,
        "Reason" character varying(1000),
        "ActorUserId" uuid NOT NULL,
        "OccurredAt" timestamp with time zone NOT NULL,
        "Source" character varying(50) NOT NULL,
        "CorrelationId" character varying(100),
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
        CONSTRAINT "PK_OprStatusHistory" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprStatusHistory_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprExecutionAddendum" (
        "Id" uuid NOT NULL,
        "ExecutionRecordId" uuid NOT NULL,
        "Content" character varying(8000) NOT NULL,
        "Reason" character varying(2000) NOT NULL,
        "AuthoredBy" uuid NOT NULL,
        "AuthoredAt" timestamp with time zone NOT NULL,
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
        CONSTRAINT "PK_OprExecutionAddendum" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprExecutionAddendum_OprExecutionRecord_ExecutionRecordId" FOREIGN KEY ("ExecutionRecordId") REFERENCES public."OprExecutionRecord" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE TABLE public."OprTeamMember" (
        "Id" uuid NOT NULL,
        "OprCaseId" uuid NOT NULL,
        "ScheduleId" uuid NOT NULL,
        "WorkforceId" uuid NOT NULL,
        "Role" integer NOT NULL,
        "IsLead" boolean NOT NULL,
        "CredentialCheckStatus" integer NOT NULL,
        "CredentialCheckedAt" timestamp with time zone,
        "IsCurrent" boolean NOT NULL,
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
        CONSTRAINT "PK_OprTeamMember" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OprTeamMember_MstWorkforceProfile_WorkforceId" FOREIGN KEY ("WorkforceId") REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OprTeamMember_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OprTeamMember_OprSchedule_ScheduleId" FOREIGN KEY ("ScheduleId") REFERENCES public."OprSchedule" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprAnesthesiaRecord_OprCaseId" ON public."OprAnesthesiaRecord" ("OprCaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprCase_CaseNumber" ON public."OprCase" ("CaseNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprCase_EncounterId_Status" ON public."OprCase" ("EncounterId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprCase_PatientId_RequestedAt" ON public."OprCase" ("PatientId", "RequestedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprCase_PrimarySurgeonId" ON public."OprCase" ("PrimarySurgeonId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprCase_RequesterDoctorId" ON public."OprCase" ("RequesterDoctorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprCaseProcedure_OprCaseId" ON public."OprCaseProcedure" ("OprCaseId") WHERE "IsPrimary" = TRUE AND "IsDelete" = FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprCaseProcedure_OprCaseId_Sequence" ON public."OprCaseProcedure" ("OprCaseId", "Sequence");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprCaseProcedure_PatientProcedureId" ON public."OprCaseProcedure" ("PatientProcedureId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprExecutionAddendum_ExecutionRecordId_AuthoredAt" ON public."OprExecutionAddendum" ("ExecutionRecordId", "AuthoredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprExecutionRecord_OprCaseId" ON public."OprExecutionRecord" ("OprCaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprHandover_DestinationUnitId_Status" ON public."OprHandover" ("DestinationUnitId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprHandover_OprCaseId_Revision" ON public."OprHandover" ("OprCaseId", "Revision");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprIntegrationDelivery_Destination_IdempotencyKey" ON public."OprIntegrationDelivery" ("Destination", "IdempotencyKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprIntegrationDelivery_OprCaseId" ON public."OprIntegrationDelivery" ("OprCaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprIntegrationDelivery_Status_RetryCount" ON public."OprIntegrationDelivery" ("Status", "RetryCount");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprMaterialUsage_BatchNumber_SerialNumber" ON public."OprMaterialUsage" ("BatchNumber", "SerialNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprMaterialUsage_OprCaseId_ExternalItemId" ON public."OprMaterialUsage" ("OprCaseId", "ExternalItemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprMaterialUsage_OprCaseId_Id_Revision" ON public."OprMaterialUsage" ("OprCaseId", "Id", "Revision");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprRecovery_OprCaseId" ON public."OprRecovery" ("OprCaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprSafetyChecklist_OprCaseId_Phase_Revision" ON public."OprSafetyChecklist" ("OprCaseId", "Phase", "Revision");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprSchedule_OprCaseId" ON public."OprSchedule" ("OprCaseId") WHERE "IsCurrent" = TRUE AND "IsDelete" = FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprSchedule_OprCaseId_Revision" ON public."OprSchedule" ("OprCaseId", "Revision");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprSchedule_RoomId_StartAt_EndAt_IsCurrent" ON public."OprSchedule" ("RoomId", "StartAt", "EndAt", "IsCurrent");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprStatusHistory_OprCaseId_OccurredAt" ON public."OprStatusHistory" ("OprCaseId", "OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprTeamMember_OprCaseId" ON public."OprTeamMember" ("OprCaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE UNIQUE INDEX "IX_OprTeamMember_ScheduleId_WorkforceId_Role" ON public."OprTeamMember" ("ScheduleId", "WorkforceId", "Role");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    CREATE INDEX "IX_OprTeamMember_WorkforceId_IsCurrent" ON public."OprTeamMember" ("WorkforceId", "IsCurrent");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260821060256_AddOperatingRoomFoundation', '9.0.18');
    END IF;
END $EF$;
COMMIT;

