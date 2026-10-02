# BE-BKC-064 — Dry-run baca-saja: mengukur dampak backfill

## Ringkasan untuk pembaca umum

Task ini menjawab satu pertanyaan dengan angka: berapa banyak tagihan lama yang akan ikut
berpindah ke "Closed" begitu migration perbaikan data dijalankan, dan berapa di antaranya sudah
terlanjur menerima koreksi tanpa koreksi piutangnya tercatat. Query disiapkan sesi ini; **eksekusi
sebenarnya dijalankan pengguna sendiri** di database dev/lokal miliknya (bukan oleh agent — sesuai
wewenang baca database yang tetap terpisah), lalu hasilnya dilaporkan kembali dan dicatat di sini.

**Hasil: hanya 1 invoice jadi kandidat, dan 0 di antaranya kehilangan koreksi AR.** `BKC-OQ-100`
tertutup — tidak dibutuhkan koreksi piutang susulan pada database yang diukur.

---

- TASK ID: BE-BKC-064
- TASK TYPE: Query baca-saja (dry-run), bukan implementasi source
- COMPLEXITY: LIGHT (nol source diubah)
- CLASSIFICATION SCORE: NOT APPLICABLE — task ini tidak menyentuh source aplikasi
- MODEL: Claude Sonnet 5
- TASK MODE: Query baca-saja dijalankan **pengguna sendiri** di luar sesi agent (bukan `BACKEND MODE` — tidak ada source yang ditulis)
- WRITE TARGET: NOT APPLICABLE (task ini seharusnya read-only terhadap database, nol tulisan ke source maupun blueprint di luar laporan ini)
- FILES INSPECTED: `appsettings.Development.json` (struktur key saja, **bukan** nilai — `ConnectionStrings:DefaultConnection` dikonfirmasi ADA tanpa membaca isinya); model `BilInvoice.cs`, `BilCalculationVersion`, `BilPaymentAllocation.cs`, `BilRefundableCredit.cs`, `BilWriteOffCase.cs`, `BilAdjustment.cs`, `BilHandoffAdjustment.cs`, `BilArHandoff.cs` (nama tabel/kolom/nilai konstanta, untuk menyusun query yang kriterianya identik dengan `BillingInvoiceClosureService.CalculateOutstandingAsync`)
- FILES CHANGED: **NONE** — task ini tidak berwenang menulis source maupun migration
- IMPLEMENTATION: **Nol source diubah.** Query SQL baca-saja (`SELECT` murni, nol `UPDATE`/`INSERT`/`DELETE`, dilampirkan di bawah) disiapkan agent, dipecah tiga statement terpisah atas permintaan pengguna (tool query pengguna hanya menampilkan hasil statement terakhir pada percobaan pertama), dijalankan pengguna sendiri di database dev/lokalnya, hasilnya dilaporkan kembali dan dicatat di sini serta di `00-interview-decisions.md` (`BKC-OQ-100`).
- **Backend Governance Preflight**: NOT APPLICABLE — task ini tidak menyentuh Area/Module/entity manapun.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: NONE
- DATABASE IMPACT: **NONE.** Query yang dijalankan murni `SELECT`, tidak mengubah satu baris pun. Dikonfirmasi dari bentuk query itu sendiri (nol `UPDATE`/`INSERT`/`DELETE`) — bukan dari observasi setelah eksekusi.
- SECURITY IMPACT: NONE
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/check | Result | Classification | Evidence/note |
  | --- | --- | --- | --- |
  | Query 1 — `candidates_final_zero_outstanding` | **1** | VERIFIED (dijalankan pengguna) | Screenshot hasil query dari tool database pengguna, dilaporkan dalam percakapan |
  | Query 2 — `candidates_missing_ar_correction` | **0** | VERIFIED (dijalankan pengguna) | Sama |
  | Query 3 — `legacy_closed_missing_closed_at` | **0** | VERIFIED (dijalankan pengguna) | Sama |
- WARNINGS: Ketiga angka berasal dari **satu** database (tempat pengguna menjalankan query — kemungkinan dev/lokal, database persisnya tidak dikonfirmasi eksplisit dalam percakapan). Bila `BE-BKC-065` kelak dijalankan ke database **lain** (staging/production), dry-run ini **MUST diulang** pada database tujuan — angka `1`/`0`/`0` tidak otomatis berlaku untuk database lain. Lihat catatan yang sama pada `00-interview-decisions.md` § Penutupan `BKC-OQ-100`.
- KNOWN ISSUES:
  1. `BKC-OQ-100` **DITUTUP** — tidak dibutuhkan koreksi AR susulan pada database yang diukur (0 kandidat kehilangan koreksi).
  2. Identitas database yang diukur (nama/environment persis) **tidak tercatat eksplisit** — hanya diketahui pengguna menjalankannya di tool database miliknya sendiri. Bila kelak dipertanyakan angka mana berasal dari database mana, ini MUST diklarifikasi ulang.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT APPLICABLE
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: Percobaan pertama (satu query `UNION ALL` tiga bagian) hanya menampilkan 1 dari 3 baris pada tool pengguna — dipecah menjadi tiga statement terpisah pada percobaan kedua, ketiganya berhasil dijalankan dan dilaporkan
- GIT STATUS: Tidak berubah dari task sebelumnya (`BE-BKC-060`–`063`); task ini menambah nol perubahan pada `git status`
- NEXT RECOMMENDED STEP: `BE-BKC-065` (migration backfill, **satu baris** sesuai angka di atas) siap dirancang detail eksekusinya, tetapi pembuatan dan eksekusi migration tetap menuntut otorisasi eksplisit terpisah sesuai `AGENTS.md` bagian Keselamatan Database — belum diberikan pada pass ini. Bila migration akan dijalankan ke database selain yang diukur di sini, ulangi dry-run pada database tujuan lebih dulu.

## Query dry-run (siap jalan, belum dieksekusi)

Kriteria pada query ini **MUST** tetap identik dengan yang dipakai migration `BackfillClosedInvoicesFromFullySettledFinal` (`BE-BKC-065`) — keduanya menghitung sisa tagihan dengan rumus yang sama persis dengan `BillingInvoiceClosureService.CalculateOutstandingAsync`. Bila query ini direvisi saat dijalankan, revisinya **MUST** ikut diterapkan ke migration `BE-BKC-065`, bukan hanya ke sini.

```sql
-- BE-BKC-064 — Dry-run baca-saja untuk gap FINAL->CLOSED (BKC-OQ-100)
-- READ-ONLY. Tidak ada UPDATE/INSERT/DELETE di berkas ini.

WITH outstanding AS (
    SELECT
        inv."Id" AS invoice_id,
        inv."Status" AS invoice_status,
        GREATEST(
            calc."PatientAmount"
            - COALESCE(paid.amount, 0)
            + COALESCE(excess.amount, 0)
            - COALESCE(wo.amount, 0)
            - COALESCE(adj.amount, 0),
            0
        ) AS outstanding_amount
    FROM "public"."BilInvoice" inv
    JOIN "public"."BilCalculationVersion" calc
        ON calc."InvoiceId" = inv."Id"
        AND calc."VersionNo" = inv."CurrentCalculationVersion"
        AND calc."IsDelete" = false
    LEFT JOIN (
        SELECT "TargetId" AS invoice_id,
               SUM(CASE WHEN "ReversesAllocationId" IS NOT NULL THEN -"Amount" ELSE "Amount" END) AS amount
        FROM "public"."BilPaymentAllocation"
        WHERE "TargetType" = 'INVOICE' AND "IsDelete" = false
        GROUP BY "TargetId"
    ) paid ON paid.invoice_id = inv."Id"
    LEFT JOIN (
        SELECT "InvoiceId" AS invoice_id, SUM("AvailableAmount") AS amount
        FROM "public"."BilRefundableCredit"
        WHERE "SourceType" = 'ALLOCATION_EXCESS' AND "IsDelete" = false
        GROUP BY "InvoiceId"
    ) excess ON excess.invoice_id = inv."Id"
    LEFT JOIN (
        SELECT "InvoiceId" AS invoice_id, SUM("Amount") AS amount
        FROM "public"."BilWriteOffCase"
        WHERE "Status" = 'POSTED' AND "Category" = 'PATIENT_AR' AND "IsDelete" = false
        GROUP BY "InvoiceId"
    ) wo ON wo.invoice_id = inv."Id"
    LEFT JOIN (
        SELECT a."InvoiceId" AS invoice_id,
               SUM(CASE WHEN a."Direction" = 'CREDIT' THEN a."Amount" ELSE -a."Amount" END) AS amount
        FROM "public"."BilAdjustment" a
        WHERE a."Status" = 'POSTED' AND a."IsDelete" = false
            AND (a."ReversesWriteOffCaseId" IS NULL
                 OR a."ReversesWriteOffCaseId" NOT IN (
                     SELECT "Id" FROM "public"."BilWriteOffCase"
                     WHERE "Category" = 'NON_BILLABLE_RESIDUAL'
                 ))
        GROUP BY a."InvoiceId"
    ) adj ON adj.invoice_id = inv."Id"
    WHERE inv."IsDelete" = false
)

-- (1) Jumlah invoice FINAL yang sisa tagihannya sudah nol — kandidat backfill BE-BKC-065.
SELECT 'candidates_final_zero_outstanding' AS metric, COUNT(*) AS value
FROM outstanding
WHERE invoice_status = 'FINAL' AND outstanding_amount <= 0

UNION ALL

-- (2) Di antara kandidat itu, berapa yang punya adjustment/write-off Posted TANPA
-- baris BilHandoffAdjustment pasangannya — inilah jawaban BKC-OQ-100.
SELECT 'candidates_missing_ar_correction' AS metric, COUNT(DISTINCT o.invoice_id) AS value
FROM outstanding o
WHERE o.invoice_status = 'FINAL' AND o.outstanding_amount <= 0
    AND (
        EXISTS (
            SELECT 1 FROM "public"."BilAdjustment" a
            WHERE a."InvoiceId" = o.invoice_id AND a."Status" = 'POSTED' AND a."IsDelete" = false
                AND NOT EXISTS (
                    SELECT 1 FROM "public"."BilHandoffAdjustment" ha
                    WHERE ha."SourceAdjustmentId" = a."Id"
                )
        )
        OR EXISTS (
            SELECT 1 FROM "public"."BilWriteOffCase" w
            WHERE w."InvoiceId" = o.invoice_id AND w."Status" = 'POSTED'
                AND w."Category" = 'PATIENT_AR' AND w."IsDelete" = false
                AND NOT EXISTS (
                    SELECT 1 FROM "public"."BilHandoffAdjustment" ha
                    WHERE ha."SourceWriteOffCaseId" = w."Id"
                )
        )
    )

UNION ALL

-- (3) Invoice CLOSED warisan (dari era sebelum kontrak ini) yang ClosedAt-nya masih kosong.
SELECT 'legacy_closed_missing_closed_at' AS metric, COUNT(*) AS value
FROM "public"."BilInvoice"
WHERE "Status" = 'CLOSED' AND "IsDelete" = false AND "ClosedAt" IS NULL;
```

### Penjelasan tiap baris hasil

| `metric` | Artinya | Dipakai untuk |
| --- | --- | --- |
| `candidates_final_zero_outstanding` | Jumlah invoice yang akan berpindah `FINAL` → `CLOSED` bila `BE-BKC-065` dijalankan | Ukuran nyata dampak migration |
| `candidates_missing_ar_correction` | Dari jumlah di atas, berapa yang **sudah** menerima penyesuaian/write-off tanpa koreksi piutang tercatat | Jawaban `BKC-OQ-100` — menentukan apakah dibutuhkan koreksi AR susulan, bukan sekadar pemindahan status |
| `legacy_closed_missing_closed_at` | Invoice `CLOSED` dari era sebelum kontrak ini yang `ClosedAt`-nya kosong | Ukuran langkah 3 migration `BE-BKC-065` |
