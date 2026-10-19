# Laporan Perubahan Backend — `BE-BKC-086`

## Ringkasan untuk Pembaca Umum

Sebelum pembaruan sistem pencatatan tautan transfer otomatis (`BE-BKC-082`) dipasang, pasien rawat jalan atau IGD yang dialihkan ke rawat inap belum memiliki catatan tautan rujukan resmi di tabel penagihan rumah sakit (`BilInvoiceEncounterLink`). Untuk memastikan pasien transfer lama tersebut tetap mendapatkan perlakuan adil (penggantian biaya administrasi rawat jalan saat rawat inap) dan tidak memicu peringatan *"asal transfer belum tertaut"* pada audit kepatuhan penagihan (`MVP-37`), sistem membutuhkan langkah pemulihan data historis (*data backfill*).

Namun, melakukan perubahan data secara massal pada database operasional rumah sakit memiliki risiko tinggi. Oleh karena itu, arsitektur tata kelola Quilvian (`BKC-DEC-160`, `BKC-DES-088`) mewajibkan adanya **tahap uji coba kering baca-saja (*read-only dry-run*)**:
1. Menghitung secara pasti berapa banyak data transfer lama yang memenuhi kriteria sah untuk ditautkan.
2. Menghitung data mana saja yang sengaja dilewati beserta alasannya (misalnya: rujukan dibatalkan, pasien tidak cocok, atau invoice rawat inap belum ada).
3. Mengetahui berapa tagihan rawat inap yang saat ini masih terbuka (*OPEN*) sehingga akan otomatis terpotong biaya administrasinya saat tagihan dibuka kembali.
4. Menjamin bahwa skrip evaluasi sama sekali tidak melakukan perubahan, penulisan, atau penghapusan data apa pun di database (*zero write effect*).

Hasil dari task `BE-BKC-086` ini menyediakan instrumen skrip SQL analitik dan dokumen panduan eksekusi (*runbook*) resmi agar pemilik sistem dan tim DBA dapat meninjau angka dampak sebelum memberikan izin migrasi data (`BE-BKC-087`).

---

### Hasil Implementasi `BE-BKC-086`

1. **Penerbitan Skrip SQL Dry-Run Baca-Saja ([`dry-run-backfill-transfer-origin-links.sql`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/billing-kasir/evidence/dry-run-backfill-transfer-origin-links.sql)):**
   - Skrip disusun murni menggunakan perintah `SELECT` PostgreSQL dengan klausa `JOIN` dan CTE (*Common Table Expression*) berkinerja tinggi.
   - Menggunakan kriteria yang **identik 100%** dengan logika runtime pada `BillingInpatientEventReceiver.cs` (`BE-BKC-082`) dan migration data `BE-BKC-087`.
   - Terdiri dari 6 query komprehensif:
     - **Query 1:** Menghitung total kandidat siap ditautkan per tipe sumber transfer (`Emergency` vs `Outpatient`).
     - **Query 2:** Mengklasifikasikan seluruh riwayat transfer rawat inap menjadi kelompok `CANDIDATE_READY`, `ALREADY_LINKED`, `PATIENT_MISMATCH`, `SOURCE_DELETED_OR_CANCELLED`, `NO_RANAP_INVOICE`, dan `NO_ADMISSION_RECEIPT`.
     - **Query 3:** Mendeteksi anomali `TYPE_MISMATCH` pada data registrasi historis.
     - **Query 4:** Menguraikan sebaran status tagihan rawat inap kandidat (`OPEN` vs `FINAL`/`CLOSED`).
     - **Query 5:** Memeriksa audit ketiadaan kredit refund liar `REFERRED_OUTPATIENT_ADMIN` sebelum rilis (`BKC-CQ-16`).
     - **Query 6:** Menampilkan proyeksi baris presisi yang akan ditulis oleh migration (termasuk nilai `ReceiptId`, `LinkedAt` dari tanda terima admisi terawal, dan `CreateBy` resmi).
2. **Penerbitan Runbook Operasional ([`runbook-dry-run-backfill-transfer-origin-links.md`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/billing-kasir/evidence/runbook-dry-run-backfill-transfer-origin-links.md)):**
   - Panduan praktis bagi DBA dan Backend Engineer untuk mengeksekusi skrip di lingkungan Development, Staging, dan Production.
   - Menjelaskan cara interpretasi hasil per query dan pemenuhan kriteria acceptance `BIL-AT-214`.
   - Mengunci gerbang otorisasi `{OTORISASI-MIG-2}` sebelum pelaksanaan migrasi `BE-BKC-087`.

---

## Lembar Metadata Task

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-086` |
| Judul | Skrip dry-run baca-saja backfill tautan transfer lama |
| Slice / Gelombang | Gelombang Eksekusi 2 (Revisi Blueprint 2.2 / 2.3) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-086` |
| Trace | `FR-BKC-283`; `BKC-DEC-160`, `BKC-DEC-163`; `BKC-DES-088`; `BIL-INT-022` |
| Kontrak | `BIL-INTEGRATION-1.8` `BIL-INT-022`; `02-backend-architecture.md` amendment `2.3` Bagian 8.1–8.2 |
| Acceptance Criteria | `BIL-AT-214` (Skrip hanya membaca; kriteria identik dengan runtime; hasil hitungan per alasan sesuai; nol baris tertulis) |
| Dependency | `BE-BKC-082` (selesai) |
| Klasifikasi | Sedang / Analisis Data & Keselamatan Migrasi Finansial |
| Task mode | `BACKEND` (Instrumen Skrip & Evidence Blueprint) |
| Target berkas | `docs/module-blueprints/billing-kasir/evidence/dry-run-backfill-transfer-origin-links.sql`<br/>`docs/module-blueprints/billing-kasir/evidence/runbook-dry-run-backfill-transfer-origin-links.md` |
| Tanggal Implementasi | 17 Oktober 2026 |
| Status | ✅ **SELESAI (Skrip dan Runbook tersedia, siap dieksekusi mandiri oleh DBA/Pemilik)** |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Billing` |
| Entity / Model Terkait | `BilInvoiceEncounterLink`, `BilInpatientEventReceipt`, `BilInvoice`, `RegPatientEncounter` |
| Prefix Registry | `Bil` — status `ACTIVE` pada registry arsitektur Quilvian |
| Applicability | `TOUCHED LEGACY` (Penyusunan instrumen analitik verifikasi backfill data historis) |
| QBE Rules Berlaku | `QBE-DB-001` (keselamatan eksekusi database, operasi baca-saja terpisah dari wewenang migrasi tulis), `QBE-SEC-001` (audit integritas data finansial tanpa kebocoran data rahasia medis) |
| Database Migration | **NOL MIGRATION.** Task ini murni instrumen baca-saja (dry-run). Sesuai aturan keselamatan repository, eksekusi query terhadap database operasional diserahkan kepada DBA/pemilik basis data. |

---

## 1. Rincian Instrumen Teknis yang Dihasilkan

### 1.1 Berkas Skrip SQL: [`dry-run-backfill-transfer-origin-links.sql`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/billing-kasir/evidence/dry-run-backfill-transfer-origin-links.sql)

Skrip dirancang dengan 5 invariant kriteria yang identik dengan runtime `BE-BKC-082`:
1. `E.SourceEncounterId IS NOT NULL` dan `E.AdmissionTransferSourceType IN ('Emergency', 'Outpatient')` dan `E.IsDelete = false`.
2. Kunjungan asal `S`: `S.PatientId = E.PatientId`, `S.IsDelete = false`, dan `S.IsCancel = false`.
3. Ada invoice rawat inap aktif `R`: `R.EncounterId = E.Id`, `R.ServiceType = 'RANAP'`, dan `R.IsDelete = false`.
4. Ada tanda terima `T` (`BilInpatientEventReceipt`) bertipe `ADMISSION_CONFIRMED` dengan `Outcome IN ('INVOICE_OPENED', 'INVOICE_ALREADY_OPEN')`. Jika ada lebih dari satu, urutan `ReceivedAtUtc` terawal yang dipilih (`ORDER BY ReceivedAtUtc ASC, Id ASC LIMIT 1`).
5. Belum ada baris aktif pada `BilInvoiceEncounterLink` untuk kombinasi `(R.Id, S.Id)`.

Skrip juga memastikan kolom audit backfill:
- `ReceiptId` = `T.Id`
- `LinkedAt` = `T.ReceivedAtUtc` (waktu tanda terima asli, **bukan** waktu eksekusi skrip/migrasi)
- `CreateBy` = `'7b1f0163-0000-4000-8000-000000000163'` (penanda sistem backfill resmi)

### 1.2 Berkas Runbook: [`runbook-dry-run-backfill-transfer-origin-links.md`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/billing-kasir/evidence/runbook-dry-run-backfill-transfer-origin-links.md)

Memuat panduan rinci mencakup:
- Lingkungan eksekusi yang disarankan.
- Format pembacaan tabel keluaran query 1 hingga 6.
- Langkah validasi data boundary untuk acceptance test `BIL-AT-214`.
- Hubungan syarat gerbang dengan `{OTORISASI-MIG-2}` sebelum tugas `BE-BKC-087` dijalankan.

---

## 2. Matriks Pemenuhan Acceptance Criteria

| ID Pengujian | Deskripsi Skenario | Bukti Pemenuhan pada Instrumen | Status |
| :--- | :--- | :--- | :---: |
| `BIL-AT-214` (Kriteria 1) | Skrip hanya membaca (read-only) | Skrip hanya memuat klausa `SELECT` dan CTE (`WITH ... AS (...)`). Tidak ada perintah `INSERT`, `UPDATE`, `DELETE`, `DROP`, atau `ALTER`. | ✅ Terpenuhi |
| `BIL-AT-214` (Kriteria 2) | Kriteria sama persis dengan penulis runtime `BE-BKC-082` dan migration `BE-BKC-087` | Memeriksa 5 invariant yang sama persis: `SourceEncounterId`, `PatientId` match, `IsCancel = false`, invoice `RANAP` aktif, tanda terima `ADMISSION_CONFIRMED`, dan keunikan pasangan `(RanapInvoiceId, LinkedEncounterId)`. | ✅ Terpenuhi |
| `BIL-AT-214` (Kriteria 3) | Klasifikasi alasan dilewati sesuai spesifikasi | Query 2 mengelompokkan secara terukur: `CANDIDATE_READY`, `ALREADY_LINKED`, `PATIENT_MISMATCH`, `SOURCE_DELETED_OR_CANCELLED`, `NO_RANAP_INVOICE`, dan `NO_ADMISSION_RECEIPT`. | ✅ Terpenuhi |
| `BIL-AT-214` (Kriteria 4) | Pemeriksaan anomali integritas data (`TYPE_MISMATCH` & `BKC-CQ-16`) | Query 3 mendeteksi inkonsistensi tipe encounter asal vs transfer; Query 5 memverifikasi ketiadaan kredit liar `REFERRED_OUTPATIENT_ADMIN`. | ✅ Terpenuhi |

---

## 3. Langkah Selanjutnya (Roadmap & Delivery)

1. **Eksekusi oleh DBA / Pemilik Basis Data:** Tim DBA atau pemilik modul menjalankan berkas `dry-run-backfill-transfer-origin-links.sql` pada database target sesuai panduan runbook.
2. **Persetujuan `{OTORISASI-MIG-2}`:** Setelah angka hasil dry-run ditinjau dan disetujui, pemilik memberikan otorisasi untuk rilis migrasi data.
3. **Pelaksanaan Task `BE-BKC-087`:** Menghasilkan migration EF Core `<timestamp>_BackfillTransferOriginEncounterLinks` yang mengaplikasikan penulisan data backfill ke database.
