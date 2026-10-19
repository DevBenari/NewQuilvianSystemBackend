-- =========================================================================================
-- BE-BKC-086 — Skrip Dry-Run Baca-Saja Backfill Tautan Transfer Lama
-- =========================================================================================
-- Modul: Billing & Kasir / Health Services
-- Dasar Keputusan: BKC-DEC-160, BKC-DEC-163, BKC-DES-088, BIL-INT-022, BIL-AT-214
-- Sifat: MURNI BACA-SAJA (READ-ONLY).
-- PERINGATAN: TIDAK ADA PERINTAH INSERT, UPDATE, DELETE, ALTER, ATAU DROP PADA BERKAS INI.
--
-- Tujuan:
-- Memberikan visibilitas terukur kepada pemilik modul Billing dan DBA mengenai dampak migrasi data
-- BE-BKC-087 sebelum otorisasi migrasi diberikan:
-- 1. Mengukur jumlah kandidat transfer IGD/Poli yang akan ditautkan ke invoice Rawat Inap.
-- 2. Mengidentifikasi jumlah transfer yang dilewati beserta klasifikasi alasannya.
-- 3. Mendeteksi anomali TYPE_MISMATCH pada data Registrasi.
-- 4. Memetakan status invoice rawat inap kandidat (OPEN vs FINAL/CLOSED).
-- 5. Memverifikasi ketiadaan kredit liar REFERRED_OUTPATIENT_ADMIN sebelum rilis (BKC-CQ-16).
-- =========================================================================================

-- -----------------------------------------------------------------------------------------
-- QUERY 1: Ringkasan Total Kandidat Siap Ditautkan per AdmissionTransferSourceType
-- -----------------------------------------------------------------------------------------
-- Kriteria identik dengan penulis runtime BE-BKC-082 (BKC-DES-077/079) dan migration BE-BKC-087.
-- -----------------------------------------------------------------------------------------

WITH first_receipt AS (
    SELECT
        t."Id" AS receipt_id,
        t."EncounterId" AS encounter_id,
        t."InvoiceId" AS invoice_id,
        t."ReceivedAtUtc" AS received_at_utc,
        ROW_NUMBER() OVER (
            PARTITION BY t."EncounterId", t."InvoiceId"
            ORDER BY t."ReceivedAtUtc" ASC, t."Id" ASC
        ) AS rn
    FROM "public"."BilInpatientEventReceipt" t
    WHERE t."EventType" = 'ADMISSION_CONFIRMED'
      AND t."Outcome" IN ('INVOICE_OPENED', 'INVOICE_ALREADY_OPEN')
      AND t."IsDelete" = false
)
SELECT
    e."AdmissionTransferSourceType" AS admission_transfer_source_type,
    COUNT(DISTINCT e."Id") AS total_kandidat_siap_ditautkan
FROM "public"."RegPatientEncounter" e
-- Syarat 1: Kunjungan rawat inap memiliki transfer asal yang sah dan tidak terhapus
JOIN "public"."RegPatientEncounter" s
    ON s."Id" = e."SourceEncounterId"
-- Syarat 2: Pasien sama, encounter asal tidak terhapus dan tidak dibatalkan
   AND s."PatientId" = e."PatientId"
   AND s."IsDelete" = false
   AND s."IsCancel" = false
-- Syarat 3: Ada invoice rawat inap aktif
JOIN "public"."BilInvoice" r
    ON r."EncounterId" = e."Id"
   AND r."ServiceType" = 'RANAP'
   AND r."IsDelete" = false
-- Syarat 4: Ada tanda terima ADMISSION_CONFIRMED yang valid
JOIN first_receipt fr
    ON fr.encounter_id = e."Id"
   AND fr.invoice_id = r."Id"
   AND fr.rn = 1
WHERE e."SourceEncounterId" IS NOT NULL
  AND e."AdmissionTransferSourceType" IN ('Emergency', 'Outpatient')
  AND e."IsDelete" = false
-- Syarat 5: Belum ada tautan aktif untuk pasangan (R, S)
  AND NOT EXISTS (
      SELECT 1
      FROM "public"."BilInvoiceEncounterLink" l
      WHERE l."RanapInvoiceId" = r."Id"
        AND l."LinkedEncounterId" = s."Id"
        AND l."IsDelete" = false
  )
GROUP BY e."AdmissionTransferSourceType";


-- -----------------------------------------------------------------------------------------
-- QUERY 2: Klasifikasi Seluruh Transfer Rawat Inap (Kandidat vs Alasan Dilewati)
-- -----------------------------------------------------------------------------------------
-- Menghitung seluruh kunjungan transfer E dan mengelompokkan mengapa dilewati:
-- - CANDIDATE_READY           : Memenuhi seluruh syarat 1-5, akan ditautkan
-- - ALREADY_LINKED            : Sudah memiliki tautan aktif
-- - PATIENT_MISMATCH          : Kunjungan asal milik pasien berbeda
-- - SOURCE_DELETED_OR_CANCEL  : Kunjungan asal telah dihapus / dibatalkan / tidak ditemukan
-- - NO_RANAP_INVOICE          : Belum / tidak memiliki invoice rawat inap RANAP aktif
-- - NO_ADMISSION_RECEIPT      : Tidak memiliki tanda terima ADMISSION_CONFIRMED yang sah
-- -----------------------------------------------------------------------------------------

WITH first_receipt AS (
    SELECT
        t."EncounterId" AS encounter_id,
        t."InvoiceId" AS invoice_id,
        MIN(t."ReceivedAtUtc") AS earliest_received_at_utc
    FROM "public"."BilInpatientEventReceipt" t
    WHERE t."EventType" = 'ADMISSION_CONFIRMED'
      AND t."Outcome" IN ('INVOICE_OPENED', 'INVOICE_ALREADY_OPEN')
      AND t."IsDelete" = false
    GROUP BY t."EncounterId", t."InvoiceId"
),
evaluated_transfers AS (
    SELECT
        e."Id" AS ranap_encounter_id,
        e."EncounterNumber" AS ranap_encounter_number,
        e."AdmissionTransferSourceType" AS source_type,
        CASE
            -- 1. Kunjungan asal tidak ditemukan sama sekali
            WHEN s."Id" IS NULL THEN 'SOURCE_DELETED_OR_CANCELLED'

            -- 2. Kunjungan asal dihapus atau dibatalkan
            WHEN s."IsDelete" = true OR s."IsCancel" = true THEN 'SOURCE_DELETED_OR_CANCELLED'

            -- 3. Pasien pada kunjungan asal tidak cocok dengan rawat inap
            WHEN s."PatientId" != e."PatientId" THEN 'PATIENT_MISMATCH'

            -- 4. Tidak ada invoice rawat inap aktif
            WHEN r."Id" IS NULL THEN 'NO_RANAP_INVOICE'

            -- 5. Tidak ada tanda terima konfirmasi admisi
            WHEN fr.encounter_id IS NULL THEN 'NO_ADMISSION_RECEIPT'

            -- 6. Sudah tertaut sebelumnya
            WHEN EXISTS (
                SELECT 1
                FROM "public"."BilInvoiceEncounterLink" l
                WHERE l."RanapInvoiceId" = r."Id"
                  AND l."LinkedEncounterId" = s."Id"
                  AND l."IsDelete" = false
            ) THEN 'ALREADY_LINKED'

            -- 7. Lolos seluruh syarat (Kandidat Bersih)
            ELSE 'CANDIDATE_READY'
        END AS evaluation_result
    FROM "public"."RegPatientEncounter" e
    LEFT JOIN "public"."RegPatientEncounter" s
        ON s."Id" = e."SourceEncounterId"
    LEFT JOIN "public"."BilInvoice" r
        ON r."EncounterId" = e."Id"
       AND r."ServiceType" = 'RANAP'
       AND r."IsDelete" = false
    LEFT JOIN first_receipt fr
        ON fr.encounter_id = e."Id"
       AND fr.invoice_id = r."Id"
    WHERE e."SourceEncounterId" IS NOT NULL
      AND e."AdmissionTransferSourceType" IN ('Emergency', 'Outpatient')
      AND e."IsDelete" = false
)
SELECT
    evaluation_result,
    COUNT(*) AS count_transfers
FROM evaluated_transfers
GROUP BY evaluation_result
ORDER BY
    CASE evaluation_result
        WHEN 'CANDIDATE_READY' THEN 1
        WHEN 'ALREADY_LINKED' THEN 2
        WHEN 'PATIENT_MISMATCH' THEN 3
        WHEN 'SOURCE_DELETED_OR_CANCELLED' THEN 4
        WHEN 'NO_RANAP_INVOICE' THEN 5
        WHEN 'NO_ADMISSION_RECEIPT' THEN 6
        ELSE 7
    END;


-- -----------------------------------------------------------------------------------------
-- QUERY 3: Deteksi Anomali TYPE_MISMATCH pada Kandidat yang Akan Ditautkan
-- -----------------------------------------------------------------------------------------
-- Keterangan:
-- Kandidat dengan TYPE_MISMATCH TETAP AKAN DITAUTKAN oleh backfill (selaras runtime BE-BKC-082),
-- namun dilaporkan agar pemilik dapat meninjau inkonsistensi data registrasi historis:
-- Misal: AdmissionTransferSourceType = 'Emergency' tetapi EncounterType asal = 1 (Outpatient).
-- -----------------------------------------------------------------------------------------

WITH first_receipt AS (
    SELECT
        t."EncounterId" AS encounter_id,
        t."InvoiceId" AS invoice_id
    FROM "public"."BilInpatientEventReceipt" t
    WHERE t."EventType" = 'ADMISSION_CONFIRMED'
      AND t."Outcome" IN ('INVOICE_OPENED', 'INVOICE_ALREADY_OPEN')
      AND t."IsDelete" = false
)
SELECT
    e."Id" AS ranap_encounter_id,
    e."EncounterNumber" AS ranap_encounter_number,
    e."AdmissionTransferSourceType" AS admission_transfer_source_type,
    s."Id" AS origin_encounter_id,
    s."EncounterNumber" AS origin_encounter_number,
    s."EncounterType" AS origin_encounter_type_raw,
    CASE s."EncounterType"
        WHEN 1 THEN 'Outpatient'
        WHEN 2 THEN 'Emergency'
        WHEN 3 THEN 'Inpatient'
        WHEN 4 THEN 'MedicalCheckup'
        WHEN 5 THEN 'Telemedicine'
        ELSE 'Unknown'
    END AS origin_encounter_type_label,
    CASE
        WHEN e."AdmissionTransferSourceType" = 'Emergency' AND s."EncounterType" != 2 THEN 'YES_TYPE_MISMATCH'
        WHEN e."AdmissionTransferSourceType" = 'Outpatient' AND s."EncounterType" != 1 THEN 'YES_TYPE_MISMATCH'
        ELSE 'MATCH'
    END AS mismatch_flag
FROM "public"."RegPatientEncounter" e
JOIN "public"."RegPatientEncounter" s
    ON s."Id" = e."SourceEncounterId"
   AND s."PatientId" = e."PatientId"
   AND s."IsDelete" = false
   AND s."IsCancel" = false
JOIN "public"."BilInvoice" r
    ON r."EncounterId" = e."Id"
   AND r."ServiceType" = 'RANAP'
   AND r."IsDelete" = false
JOIN first_receipt fr
    ON fr.encounter_id = e."Id"
   AND fr.invoice_id = r."Id"
WHERE e."SourceEncounterId" IS NOT NULL
  AND e."AdmissionTransferSourceType" IN ('Emergency', 'Outpatient')
  AND e."IsDelete" = false
  AND NOT EXISTS (
      SELECT 1
      FROM "public"."BilInvoiceEncounterLink" l
      WHERE l."RanapInvoiceId" = r."Id"
        AND l."LinkedEncounterId" = s."Id"
        AND l."IsDelete" = false
  )
  AND (
      (e."AdmissionTransferSourceType" = 'Emergency' AND s."EncounterType" != 2)
      OR
      (e."AdmissionTransferSourceType" = 'Outpatient' AND s."EncounterType" != 1)
  );


-- -----------------------------------------------------------------------------------------
-- QUERY 4: Sebaran Status Invoice Rawat Inap (R) dari Kandidat yang Akan Ditautkan
-- -----------------------------------------------------------------------------------------
-- Keterangan:
-- - Invoice berstatus OPEN: Akan langsung menerima efek penggantian / pengkreditan biaya
--   administrasi poliklinik pada saat rekalkulasi / pratinjau dibuka berikutnya (BE-BKC-084).
-- - Invoice berstatus FINAL / CLOSED / SETTLED_BY_WRITE_OFF: Tidak akan berubah statusnya,
--   namun rantai rujukan transfernya kini sah dan tidak lagi dianggap UNLINKED_TRANSFER_ORIGIN.
-- -----------------------------------------------------------------------------------------

WITH first_receipt AS (
    SELECT
        t."EncounterId" AS encounter_id,
        t."InvoiceId" AS invoice_id
    FROM "public"."BilInpatientEventReceipt" t
    WHERE t."EventType" = 'ADMISSION_CONFIRMED'
      AND t."Outcome" IN ('INVOICE_OPENED', 'INVOICE_ALREADY_OPEN')
      AND t."IsDelete" = false
)
SELECT
    r."Status" AS ranap_invoice_status,
    COUNT(*) AS count_invoices
FROM "public"."RegPatientEncounter" e
JOIN "public"."RegPatientEncounter" s
    ON s."Id" = e."SourceEncounterId"
   AND s."PatientId" = e."PatientId"
   AND s."IsDelete" = false
   AND s."IsCancel" = false
JOIN "public"."BilInvoice" r
    ON r."EncounterId" = e."Id"
   AND r."ServiceType" = 'RANAP'
   AND r."IsDelete" = false
JOIN first_receipt fr
    ON fr.encounter_id = e."Id"
   AND fr.invoice_id = r."Id"
WHERE e."SourceEncounterId" IS NOT NULL
  AND e."AdmissionTransferSourceType" IN ('Emergency', 'Outpatient')
  AND e."IsDelete" = false
  AND NOT EXISTS (
      SELECT 1
      FROM "public"."BilInvoiceEncounterLink" l
      WHERE l."RanapInvoiceId" = r."Id"
        AND l."LinkedEncounterId" = s."Id"
        AND l."IsDelete" = false
  )
GROUP BY r."Status"
ORDER BY r."Status";


-- -----------------------------------------------------------------------------------------
-- QUERY 5: Audit Ketiadaan Kredit REFERRED_OUTPATIENT_ADMIN Liar Sebelum Rilis (BKC-CQ-16)
-- -----------------------------------------------------------------------------------------
-- Memastikan tidak ada kredit refund bertipe REFERRED_OUTPATIENT_ADMIN yang tersimpan
-- sebelum migrasi dijalankan. Seharusnya mengembalikan nilai 0.
-- -----------------------------------------------------------------------------------------

SELECT
    COUNT(*) AS count_existing_referred_admin_credits,
    COALESCE(SUM("OriginalAmount"), 0) AS total_original_amount,
    COALESCE(SUM("AvailableAmount"), 0) AS total_available_amount
FROM "public"."BilRefundableCredit"
WHERE "SourceType" = 'REFERRED_OUTPATIENT_ADMIN'
  AND "IsDelete" = false;


-- -----------------------------------------------------------------------------------------
-- QUERY 6: Proyeksi Baris Tautan yang Akan Ditulis oleh Migration BE-BKC-087
-- -----------------------------------------------------------------------------------------
-- Menggunakan konstanta CreateBy resmi backfill: 7b1f0163-0000-4000-8000-000000000163.
-- LinkedAt diambil langsung dari ReceivedAtUtc tanda terima ADMISSION_CONFIRMED paling awal.
-- -----------------------------------------------------------------------------------------

WITH first_receipt AS (
    SELECT
        t."Id" AS receipt_id,
        t."EncounterId" AS encounter_id,
        t."InvoiceId" AS invoice_id,
        t."ReceivedAtUtc" AS received_at_utc,
        ROW_NUMBER() OVER (
            PARTITION BY t."EncounterId", t."InvoiceId"
            ORDER BY t."ReceivedAtUtc" ASC, t."Id" ASC
        ) AS rn
    FROM "public"."BilInpatientEventReceipt" t
    WHERE t."EventType" = 'ADMISSION_CONFIRMED'
      AND t."Outcome" IN ('INVOICE_OPENED', 'INVOICE_ALREADY_OPEN')
      AND t."IsDelete" = false
)
SELECT
    r."Id" AS projected_ranap_invoice_id,
    s."Id" AS projected_linked_encounter_id,
    CASE
        WHEN e."AdmissionTransferSourceType" = 'Emergency' THEN 'EMERGENCY_ORIGIN'
        WHEN e."AdmissionTransferSourceType" = 'Outpatient' THEN 'OUTPATIENT_ORIGIN'
        ELSE 'UNKNOWN_ORIGIN'
    END AS projected_link_reason,
    NULL::uuid AS projected_source_referral_id,
    fr.receipt_id AS projected_receipt_id,
    fr.received_at_utc AS projected_linked_at,
    '7b1f0163-0000-4000-8000-000000000163'::uuid AS projected_create_by,
    e."EncounterNumber" AS inpatient_encounter_number,
    r."InvoiceNumber" AS inpatient_invoice_number,
    r."Status" AS inpatient_invoice_status
FROM "public"."RegPatientEncounter" e
JOIN "public"."RegPatientEncounter" s
    ON s."Id" = e."SourceEncounterId"
   AND s."PatientId" = e."PatientId"
   AND s."IsDelete" = false
   AND s."IsCancel" = false
JOIN "public"."BilInvoice" r
    ON r."EncounterId" = e."Id"
   AND r."ServiceType" = 'RANAP'
   AND r."IsDelete" = false
JOIN first_receipt fr
    ON fr.encounter_id = e."Id"
   AND fr.invoice_id = r."Id"
   AND fr.rn = 1
WHERE e."SourceEncounterId" IS NOT NULL
  AND e."AdmissionTransferSourceType" IN ('Emergency', 'Outpatient')
  AND e."IsDelete" = false
  AND NOT EXISTS (
      SELECT 1
      FROM "public"."BilInvoiceEncounterLink" l
      WHERE l."RanapInvoiceId" = r."Id"
        AND l."LinkedEncounterId" = s."Id"
        AND l."IsDelete" = false
  )
ORDER BY fr.received_at_utc ASC;
