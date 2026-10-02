-- =====================================================================================
-- Verifikasi Skema Database BE-FIN-041, BE-FIN-038, dan BE-FIN-043
-- Seluruh perintah di berkas ini HANYA MEMBACA (read-only).
-- Kompatibel dengan DBeaver dan psql.
-- =====================================================================================

-- 1. Periksa pencatatan di __EFMigrationsHistory (harus 3 baris)
SELECT "MigrationId", "ProductVersion"
FROM "__EFMigrationsHistory"
WHERE "MigrationId" IN (
    '20260928120000_AddDepositAppliedAmountToFinPayment',
    '20260929120000_AddArInvoiceBatchAndReceiptDeduction',
    '20260929130000_AddPPNAmountToFinSupplierReturn'
)
ORDER BY "MigrationId";

-- 2. Periksa kolom baru pada FinPayment (harus ada DepositAppliedAmount numeric(18,2))
SELECT table_name, column_name, data_type, numeric_precision, numeric_scale, column_default
FROM information_schema.columns
WHERE table_schema = 'public' 
  AND table_name = 'FinPayment' 
  AND column_name = 'DepositAppliedAmount';

-- 3. Periksa check constraint pada FinPayment (CK_FinPayment_NetTransfer dan CK_FinPayment_DepositApplied)
SELECT conname, pg_get_constraintdef(c.oid)
FROM pg_constraint c
JOIN pg_class t ON c.conrelid = t.oid
JOIN pg_namespace n ON t.relnamespace = n.oid
WHERE n.nspname = 'public' 
  AND t.relname = 'FinPayment'
  AND conname IN ('CK_FinPayment_NetTransfer', 'CK_FinPayment_DepositApplied');

-- 4. Periksa keberadaan 3 tabel baru dari BE-FIN-038
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN (
    'FinReceivableInvoiceBatch',
    'FinReceivableInvoiceBatchItem',
    'FinReceiptDeduction'
  )
ORDER BY table_name;

-- 5. Periksa kolom baru pada FinSupplierReturn (harus ada PPNAmount numeric(18,2))
SELECT table_name, column_name, data_type, numeric_precision, numeric_scale, column_default
FROM information_schema.columns
WHERE table_schema = 'public' 
  AND table_name = 'FinSupplierReturn' 
  AND column_name = 'PPNAmount';

-- 6. Periksa check constraint pada FinSupplierReturn (CK_FinSupplierReturn_PPNAmount)
SELECT conname, pg_get_constraintdef(c.oid)
FROM pg_constraint c
JOIN pg_class t ON c.conrelid = t.oid
JOIN pg_namespace n ON t.relnamespace = n.oid
WHERE n.nspname = 'public' 
  AND t.relname = 'FinSupplierReturn'
  AND conname = 'CK_FinSupplierReturn_PPNAmount';
