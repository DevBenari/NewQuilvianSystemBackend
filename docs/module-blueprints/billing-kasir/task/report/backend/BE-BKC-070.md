# BE-BKC-070 — Pemulihan Resep yang Terlanjur Tertahan

## Ringkasan untuk Pembaca Umum

Sebelum jalur pemberitahuan otomatis surat clearance dibangun pada modul Billing dan Farmasi (`BE-BKC-066`–`BE-BKC-069`), terdapat transaksi pasien yang tagihannya sudah lunas dibayar di kasir, namun resep obatnya masih berada dalam status tertahan (menunggu pembayaran) di modul Farmasi. Hal ini terjadi karena sistem lama belum memiliki mekanisme pengiriman surat lunas secara otomatis. Apabila jalur baru langsung diaktifkan tanpa pemulihan data masa lalu, pasien lama yang sudah membayar tagihan akan mendapati obatnya tidak pernah disiapkan oleh apoteker, sedangkan pasien baru yang baru membayar langsung dilayani dengan lancar. Perbedaan perilaku ini dapat membingungkan staf rumah sakit dan merugikan pasien.

Sesuai keputusan arsitektur dan bisnis rumah sakit (**`BKC-DEC-111`**), modul Billing menyediakan pekerjaan pemulihan sekali jalan (*one-off idempotent recovery job*) melalui layanan `BilPrescriptionClearanceRecoveryService`:
1. **Membaca Kebenaran dari Billing Tanpa Bypass Data**: Sistem tidak menggunakan skrip pembaruan langsung ke basis data (*no raw SQL update bypass*). Sistem menelusuri seluruh tagihan yang memuat item obat dan membaca status pelunasan resminya.
2. **Hanya Memulihkan Resep yang Memang Sudah Lunas**: Tagihan yang statusnya sudah ditutup lunas (`CLOSED`) atau diselesaikan lewat pemutihan piutang resmi (`SETTLED_BY_WRITE_OFF`) akan diproses. Tagihan yang belum lunas (sisa tagihan masih ada, status `DRAFT`, `PENDING`, atau `FINAL` belum berbayar) **tidak akan** memperoleh surat clearance.
3. **Pemeriksaan Status Terlebih Dahulu (`BE-BKC-068`)**: Untuk setiap resep pada tagihan lunas, sistem terlebih dahulu menanyakan statusnya melalui `ReadPrescriptionClearanceAsync`. Jika statusnya belum dikenal (`UNKNOWN`), sistem menerbitkan surat clearance pertama (`CLEARED`, versi 1).
4. **Idempotensi Penuh (Anti-Surat Ganda)**: Bila pekerjaan ini dijalankan dua kali atau lebih, resep yang sudah memiliki surat clearance sah akan otomatis dilewati (*skipped*). Hal ini menjamin tidak ada nomor versi ganda, tidak ada surat duplikat, dan tidak ada lonjakan beban sistem.

---

## Spesifikasi Teknis Task

- TASK ID: BE-BKC-070
- TASK TYPE: Fitur / Recovery Runner (Pekerjaan pemulihan idempoten untuk resep yang tagihannya sudah lunas sebelum jalur handoff berdiri)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 3 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 3 → 1 + logika pemulihan rekonsiliasi & transaksi database → 1 + integrasi domain in-process → 1; total score 3)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/00-interview-decisions.md` (`BKC-DEC-111`, baris 2013–2024)
  - `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` (`BE-BKC-070`, baris 2429–2444)
  - `docs/module-blueprints/billing-kasir/contracts/integration-contract.md` (`BIL-INTEGRATION-1.1`, `BIL-INT-014`)
  - `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceItem.cs`
- FILES CHANGED:
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingConsumerHandoffDtos.cs` (menambahkan record `PrescriptionClearanceRecoveryResult`)
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Services/BilPrescriptionClearanceRecoveryService.cs` (layanan pemulihan idempoten)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI untuk `BilPrescriptionClearanceRecoveryService`)
- IMPLEMENTATION:
  1. **DTO Hasil Pemulihan (`PrescriptionClearanceRecoveryResult`)**:
     Menyajikan metrik rinci: `TotalInvoicesEvaluated`, `TotalPrescriptionsEvaluated`, `TotalPrescriptionsRecovered`, `TotalPrescriptionsSkippedAlreadyCleared`, `TotalPrescriptionsSkippedUnpaid`, `RecoveredPrescriptionIds`, dan `SummaryNotes`.
  2. **Layanan Pemulihan `BilPrescriptionClearanceRecoveryService`**:
     - Membaca seluruh `BilInvoiceItem` dengan `SourceDomain == "PHARMACY"`, mengabaikan item berstatus `Voided` atau `IsDelete`.
     - Mengelompokkan item berdasarkan `PrescriptionId` (dari `SourceDetailId`) dan tagihan induknya (`BilInvoice`).
     - **Acceptance Criteria 2 (Resep Belum Lunas Tidak Berubah)**: Memeriksa `invoice.Status`. Jika bukan `CLOSED` dan bukan `SETTLED_BY_WRITE_OFF`, resep dilewati (*skipped unpaid*) tanpa menerbitkan surat clearance apa pun.
     - **Pemeriksaan Permukaan `BE-BKC-068`**: Memanggil `_handoffService.ReadPrescriptionClearanceAsync(prescriptionId)`.
     - **Acceptance Criteria 3 (Idempotensi)**: Jika hasil pemeriksaan mengembalikan `IsKnown == true` (resep sudah memiliki riwayat surat clearance), resep langsung dilewati (*skipped already cleared*), sehingga pemanggilan berulang kali tidak pernah melahirkan surat ganda.
     - **Acceptance Criteria 1 (Penerbitan Surat Pertama untuk Resep Lunas)**: Jika `IsKnown == false`, sistem memanggil `_handoffService.PublishForClearanceChangeAsync` di dalam transaksi resmi dengan `specificPrescriptionId` dan kode sebab yang sesuai (`INVOICE_SETTLED` atau `INVOICE_WRITTEN_OFF`). Waktu kejadian diambil dari `invoice.ClosedAt` (atau waktu pembaruan tagihan).
     - **Acceptance Criteria 4 (Nol Bypass Data Langsung)**: Seluruh pemulihan membaca model EF Core resmi dan menerbitkan entitas aggregate `BilPrescriptionClearanceHandoff` melalui domain service yang dilindungi PostgreSQL advisory lock `BIL_INVOICE_LEDGER_{InvoiceId:N}`. Tidak ada pernyataan `UPDATE ... SET` mentah yang dijalankan.
     - Mencatat ringkasan audit log `BillingPrescriptionClearanceRecovery.Completed` ke `LoggerService`.
  3. **Pendaftaran Dependency Injection**:
     Menambahkan `services.AddScoped<BilPrescriptionClearanceRecoveryService>();` pada `BillingManagementServiceCollectionExtensions.cs`.
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE`
  - Keberlakuan: `NEW CODE` untuk `BilPrescriptionClearanceRecoveryService.cs`; `TOUCHED LEGACY` untuk `BillingConsumerHandoffDtos.cs` dan `BillingManagementServiceCollectionExtensions.cs`.
  - QBE Compliance: Mematuhi `QBE-SVC-001`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-002`, `QBE-MOD-003`, dan `QBE-DB-001`.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: **Nol endpoint HTTP baru**. Pemulihan berbentuk in-process service/runner yang dapat diorkestrasi oleh job runner internal atau pengujian otomatis/manual.
- DATABASE IMPACT: Nol migrasi database dan nol manipulasi data langsung (tidak memerlukan otorisasi pemutakhiran data terpisah sesuai klausul `BKC-DEC-111`).
- SECURITY & AUDIT IMPACT: Menjaga integritas data finansial dan keselamatan klinis. Setiap surat yang lahir dicatat secara transaksional dengan audit event `BillingPrescriptionClearanceHandoff.Created` dan `BillingPrescriptionClearanceRecovery.Completed`.
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `Invoke-QbeConformanceCheck.ps1` (3 berkas) | `PASS` | VERIFIED | Evaluasi QBE lulus, 0 violations, 0 review, 0 info |
  | `dotnet build` | **Menunggu eksekusi manual pengguna** | NOT VERIFIED | Sesuai instruksi pengguna untuk kompilasi mandiri |
  | Review Diff dan Scope | Selesai | VERIFIED | Memenuhi 4 kriteria penerimaan `BE-BKC-070` dan batasan `BKC-DEC-111` |
  | `AC-1` (resep lunas memperoleh surat pertama) | Terpenuhi pada source | VERIFIED (Logic) | Invoice `CLOSED` memicu `PublishForClearanceChangeAsync` dengan `INVOICE_SETTLED` (`CLEARED`, versi 1) |
  | `AC-2` (resep belum lunas tidak memperoleh apa pun) | Terpenuhi pada source | VERIFIED (Logic) | Invoice non-`CLOSED` dilewati (`skippedUnpaid++`) tanpa ada penulisan baris |
  | `AC-3` (idempotensi 2x run tanpa duplikasi) | Terpenuhi pada source | VERIFIED (Logic) | `ReadPrescriptionClearanceAsync` mengembalikan `IsKnown == true` pada run kedua, langsung di-skip (`skippedAlreadyCleared++`) |
  | `AC-4` (nol update data langsung) | Terpenuhi pada source | VERIFIED (Logic) | Nol SQL update; seluruh penulisan melewati domain service resmi `BilConsumerHandoffService` |
- WARNINGS: Tidak ada.
- KNOWN ISSUES: Tidak ada issue baru pada source yang ditulis.
- NEXT TASKS: Gelombang backend `BIL-CASH-001` (`BE-BKC-066`–`BE-BKC-070`) telah lengkap seluruhnya. Task frontend pendukung pada gelombang berikutnya: `FE-BKC-040` (Layar `BIL-SCR-41` Surat ke Modul Konsumen).
