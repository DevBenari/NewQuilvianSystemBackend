-- =====================================================================================
-- BE-FIN-041, BE-FIN-038, BE-FIN-043 — Skrip Migrasi Skema Database Fisik (DBeaver)
--
-- Migration IDs yang tercakup secara sekuensial:
--   1. 20260928120000_AddDepositAppliedAmountToFinPayment (BE-FIN-041)
--   2. 20260929120000_AddArInvoiceBatchAndReceiptDeduction (BE-FIN-038)
--   3. 20260929130000_AddPPNAmountToFinSupplierReturn (BE-FIN-043)
--
-- Sifat: 100% Idempotent, dibungkus START TRANSACTION ... COMMIT.
-- Kompatibel penuh dengan DBeaver dan psql.
-- =====================================================================================

START TRANSACTION;

-- -------------------------------------------------------------------------------------
-- 1. BE-FIN-041: 20260928120000_AddDepositAppliedAmountToFinPayment
-- -------------------------------------------------------------------------------------
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260928120000_AddDepositAppliedAmountToFinPayment') THEN
        -- Tambah kolom DepositAppliedAmount jika belum ada
        IF NOT EXISTS(
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' AND table_name = 'FinPayment' AND column_name = 'DepositAppliedAmount'
        ) THEN
            ALTER TABLE public."FinPayment" ADD "DepositAppliedAmount" numeric(18,2) NOT NULL DEFAULT 0.00;
        END IF;

        -- Perbarui check constraint NetTransferAmount
        ALTER TABLE public."FinPayment" DROP CONSTRAINT IF EXISTS "CK_FinPayment_NetTransfer";
        ALTER TABLE public."FinPayment" ADD CONSTRAINT "CK_FinPayment_NetTransfer" 
            CHECK ("NetTransferAmount" = "TotalAmount" - "DeductionAmount" + "AdditionAmount" - "DepositAppliedAmount");

        -- Tambah check constraint DepositAppliedAmount non-negatif
        ALTER TABLE public."FinPayment" DROP CONSTRAINT IF EXISTS "CK_FinPayment_DepositApplied";
        ALTER TABLE public."FinPayment" ADD CONSTRAINT "CK_FinPayment_DepositApplied" 
            CHECK ("DepositAppliedAmount" >= 0);

        -- Catat ke __EFMigrationsHistory
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260928120000_AddDepositAppliedAmountToFinPayment', '9.0.18');
    END IF;
END $EF$;

-- -------------------------------------------------------------------------------------
-- 2. BE-FIN-038: 20260929120000_AddArInvoiceBatchAndReceiptDeduction
-- -------------------------------------------------------------------------------------
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260929120000_AddArInvoiceBatchAndReceiptDeduction') THEN
        
        -- Tabel 1: FinReceivableInvoiceBatch
        CREATE TABLE IF NOT EXISTS public."FinReceivableInvoiceBatch" (
            "Id" uuid NOT NULL,
            "BatchNumber" character varying(50) NOT NULL,
            "DebtorType" character varying(30) NOT NULL DEFAULT 'PAYER',
            "DebtorReferenceId" uuid NOT NULL,
            "PeriodStart" date NOT NULL,
            "PeriodEnd" date NOT NULL,
            "TotalAmount" numeric(18,2) NOT NULL,
            "Status" character varying(20) NOT NULL DEFAULT 'DRAFT',
            "IssuedAt" timestamp with time zone NULL,
            "RowVersion" uuid NOT NULL,
            "CreateDateTime" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
            "CreateBy" uuid NOT NULL,
            "UpdateDateTime" timestamp with time zone NULL,
            "UpdateBy" uuid NOT NULL,
            "DeleteDateTime" timestamp with time zone NULL,
            "DeleteBy" uuid NOT NULL,
            "CancelDateTime" timestamp with time zone NULL,
            "CancelBy" uuid NOT NULL,
            "IsCancel" boolean NOT NULL DEFAULT false,
            "IsDelete" boolean NOT NULL DEFAULT false,
            CONSTRAINT "PK_FinReceivableInvoiceBatch" PRIMARY KEY ("Id"),
            CONSTRAINT "CK_FinReceivableInvoiceBatch_DebtorType" CHECK ("DebtorType" = 'PAYER'),
            CONSTRAINT "CK_FinReceivableInvoiceBatch_Status" CHECK ("Status" IN ('DRAFT','ISSUED','PARTIALLY_PAID','PAID','CANCELLED'))
        );

        -- Tabel 2: FinReceivableInvoiceBatchItem
        CREATE TABLE IF NOT EXISTS public."FinReceivableInvoiceBatchItem" (
            "Id" uuid NOT NULL,
            "BatchId" uuid NOT NULL,
            "ReceivableId" uuid NOT NULL,
            "CreateDateTime" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
            "CreateBy" uuid NOT NULL,
            "UpdateDateTime" timestamp with time zone NULL,
            "UpdateBy" uuid NOT NULL,
            "DeleteDateTime" timestamp with time zone NULL,
            "DeleteBy" uuid NOT NULL,
            "CancelDateTime" timestamp with time zone NULL,
            "CancelBy" uuid NOT NULL,
            "IsCancel" boolean NOT NULL DEFAULT false,
            "IsDelete" boolean NOT NULL DEFAULT false,
            CONSTRAINT "PK_FinReceivableInvoiceBatchItem" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_FinReceivableInvoiceBatchItem_FinReceivableInvoiceBatch_BatchId" 
                FOREIGN KEY ("BatchId") REFERENCES public."FinReceivableInvoiceBatch" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_FinReceivableInvoiceBatchItem_FinReceivable_ReceivableId" 
                FOREIGN KEY ("ReceivableId") REFERENCES public."FinReceivable" ("Id") ON DELETE RESTRICT
        );

        -- Tabel 3: FinReceiptDeduction
        CREATE TABLE IF NOT EXISTS public."FinReceiptDeduction" (
            "Id" uuid NOT NULL,
            "DeductionNumber" character varying(50) NOT NULL,
            "ReceiptId" uuid NOT NULL,
            "ReceiptAllocationId" uuid NOT NULL,
            "DeductionType" character varying(30) NOT NULL,
            "Amount" numeric(18,2) NOT NULL,
            "Reason" character varying(500) NULL,
            "ReferenceNumber" character varying(100) NULL,
            "IsReversal" boolean NOT NULL DEFAULT false,
            "ReversalOfDeductionId" uuid NULL,
            "RowVersion" uuid NOT NULL,
            "CreateDateTime" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
            "CreateBy" uuid NOT NULL,
            "UpdateDateTime" timestamp with time zone NULL,
            "UpdateBy" uuid NOT NULL,
            "DeleteDateTime" timestamp with time zone NULL,
            "DeleteBy" uuid NOT NULL,
            "CancelDateTime" timestamp with time zone NULL,
            "CancelBy" uuid NOT NULL,
            "IsCancel" boolean NOT NULL DEFAULT false,
            "IsDelete" boolean NOT NULL DEFAULT false,
            CONSTRAINT "PK_FinReceiptDeduction" PRIMARY KEY ("Id"),
            CONSTRAINT "CK_FinReceiptDeduction_Type" CHECK ("DeductionType" IN ('PPH23','BANK_ADMIN_FEE','OTHER')),
            CONSTRAINT "CK_FinReceiptDeduction_Amount" CHECK ("Amount" > 0),
            CONSTRAINT "CK_FinReceiptDeduction_OtherReason" CHECK ("DeductionType" <> 'OTHER' OR "Reason" IS NOT NULL),
            CONSTRAINT "CK_FinReceiptDeduction_Reversal" CHECK ("IsReversal" = ("ReversalOfDeductionId" IS NOT NULL)),
            CONSTRAINT "FK_FinReceiptDeduction_FinReceipt_ReceiptId" 
                FOREIGN KEY ("ReceiptId") REFERENCES public."FinReceipt" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_FinReceiptDeduction_FinReceiptAllocation_ReceiptAllocationId" 
                FOREIGN KEY ("ReceiptAllocationId") REFERENCES public."FinReceiptAllocation" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_FinReceiptDeduction_FinReceiptDeduction_ReversalOfDeductionId" 
                FOREIGN KEY ("ReversalOfDeductionId") REFERENCES public."FinReceiptDeduction" ("Id") ON DELETE RESTRICT
        );

        -- Index FinReceivableInvoiceBatch
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinReceivableInvoiceBatch_BatchNumber" 
            ON public."FinReceivableInvoiceBatch" ("BatchNumber") WHERE "IsDelete" = false;
        CREATE INDEX IF NOT EXISTS "IX_FinReceivableInvoiceBatch_DebtorType" 
            ON public."FinReceivableInvoiceBatch" ("DebtorType");
        CREATE INDEX IF NOT EXISTS "IX_FinReceivableInvoiceBatch_DebtorReferenceId" 
            ON public."FinReceivableInvoiceBatch" ("DebtorReferenceId");
        CREATE INDEX IF NOT EXISTS "IX_FinReceivableInvoiceBatch_PeriodStart" 
            ON public."FinReceivableInvoiceBatch" ("PeriodStart");
        CREATE INDEX IF NOT EXISTS "IX_FinReceivableInvoiceBatch_Status" 
            ON public."FinReceivableInvoiceBatch" ("Status");

        -- Index FinReceivableInvoiceBatchItem
        CREATE INDEX IF NOT EXISTS "IX_FinReceivableInvoiceBatchItem_BatchId" 
            ON public."FinReceivableInvoiceBatchItem" ("BatchId");
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinReceivableInvoiceBatchItem_ActiveReceivable" 
            ON public."FinReceivableInvoiceBatchItem" ("ReceivableId") WHERE "IsDelete" = false;

        -- Index FinReceiptDeduction
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinReceiptDeduction_DeductionNumber" 
            ON public."FinReceiptDeduction" ("DeductionNumber") WHERE "IsDelete" = false;
        CREATE INDEX IF NOT EXISTS "IX_FinReceiptDeduction_ReceiptId" 
            ON public."FinReceiptDeduction" ("ReceiptId");
        CREATE INDEX IF NOT EXISTS "IX_FinReceiptDeduction_DeductionType" 
            ON public."FinReceiptDeduction" ("DeductionType");
        CREATE INDEX IF NOT EXISTS "IX_FinReceiptDeduction_Allocation" 
            ON public."FinReceiptDeduction" ("ReceiptAllocationId");
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinReceiptDeduction_ReversalOnce" 
            ON public."FinReceiptDeduction" ("ReversalOfDeductionId") 
            WHERE "ReversalOfDeductionId" IS NOT NULL AND "IsDelete" = false;

        -- Catat ke __EFMigrationsHistory
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260929120000_AddArInvoiceBatchAndReceiptDeduction', '9.0.18');
    END IF;
END $EF$;

-- -------------------------------------------------------------------------------------
-- 3. BE-FIN-043: 20260929130000_AddPPNAmountToFinSupplierReturn
-- -------------------------------------------------------------------------------------
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260929130000_AddPPNAmountToFinSupplierReturn') THEN
        -- Tambah kolom PPNAmount jika belum ada
        IF NOT EXISTS(
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' AND table_name = 'FinSupplierReturn' AND column_name = 'PPNAmount'
        ) THEN
            ALTER TABLE public."FinSupplierReturn" ADD "PPNAmount" numeric(18,2) NOT NULL DEFAULT 0.00;
        END IF;

        -- Tambah check constraint PPNAmount non-negatif
        ALTER TABLE public."FinSupplierReturn" DROP CONSTRAINT IF EXISTS "CK_FinSupplierReturn_PPNAmount";
        ALTER TABLE public."FinSupplierReturn" ADD CONSTRAINT "CK_FinSupplierReturn_PPNAmount" 
            CHECK ("PPNAmount" >= 0);

        -- Catat ke __EFMigrationsHistory
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260929130000_AddPPNAmountToFinSupplierReturn', '9.0.18');
    END IF;
END $EF$;

COMMIT;
