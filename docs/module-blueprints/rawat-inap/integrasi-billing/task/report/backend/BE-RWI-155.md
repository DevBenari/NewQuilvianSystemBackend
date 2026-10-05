# Laporan Perubahan Backend — `BE-RWI-155`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-155` |
| Judul | Jembatan layanan klinis `RANAP`, biaya admin, dan finalisasi |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-155` |
| Trace | `FR-RWF-011`, `012`, `015`; `RWI-DEC-192`, `195`; `INV-RWF-08`; `VAL-RWF-15` s.d. `17`; `AC-RWF-011`, `013`, `090`, `091`; API 3.9; backend 9.6; state 5.5 |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-150` ✅ |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 2, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BillingManagement/Billing/**` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Prefix registry | `Bil` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (jembatan, kalkulasi, finalisasi, invoice service, controller) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Jembatan layanan klinis Billing hanya menagih kunjungan rawat jalan, sehingga tindakan, lab,
radiologi, obat, dan konsultasi pasien rawat inap harus diinput kasir. Biaya admin rawat inap
belum memperhitungkan tarif kamar, dan invoice dapat difinalkan walau ada biaya kamar dobel atau
layanan bertarif kosong.

## 2. Proses bisnis

1. Jembatan menerima kunjungan `Outpatient` **dan** `Inpatient` dengan titik tagih yang sama;
   tujuan tagihan adalah invoice kunjungan itu sendiri (`RANAP` untuk rawat inap).
2. Contoh `AC-RWF-011`: tindakan "Pasang infus" `Completed` dan lab "Darah Lengkap" pasien rawat
   inap masuk invoice `RANAP` dengan harga master tarif. Obat masuk saat `DISPENSED`; MAR bukan
   domain sumber tagihan (`AC-RWF-090`, `091`).
3. Kalkulasi invoice `RANAP` menghitung tarif kamar lebih dulu, lalu biaya admin dengan dasar
   jasa non-farmasi **termasuk** tarif kamar (`AC-RWF-013`).
4. Invoice `RANAP` yang memuat biaya kamar manual bersamaan dengan tarif kamar otomatis ditandai
   `RequiresReview` (`ReviewReasonCode` biaya kamar manual + otomatis).
5. Finalisasi ditolak bila `RequiresReview` (`BIL-FIN-020`) atau masih ada layanan "tarif belum ada"
   (`BIL-FIN-021`, termasuk segmen tarif kamar tanpa tarif) — juga lewat jalur departure exception.
6. Kasir membuka antrean "perlu diperiksa", membatalkan baris kamar yang dobel, lalu menyelesaikan
   pemeriksaan. Bila manual dan otomatis masih sama-sama aktif → 422 `BIL-REV-001`.
7. Pencatatan manual layanan klinis lewat `from-source` kini juga ditolak untuk kunjungan rawat
   inap, karena domainnya sudah dimiliki jembatan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingClinicalChargeBridgeService.cs`, `BillingSourceTariffResolver.cs`, `BillingChargeSourceAdapter.cs`,
`BillingCalculationService.cs`, `AdministrationFeeCalculationService.cs`, `BillingFinalizationService.cs`,
`BillingInvoiceService.cs`, `BillingInvoicesController.cs`; kontrak API 3.9, backend 9.6.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Services/BillingClinicalChargeBridgeService.cs` | Menerima `EncounterType.Inpatient` |
| `Billing/Services/BillingCalculationService.cs` | Tarif kamar sebelum biaya admin; dasar biaya admin + tarif kamar untuk `RANAP`; penandaan `RequiresReview`; `IsManualRoomChargeItem` |
| `Billing/Services/BillingFinalizationService.cs`, `Dtos/BillingFinalizationDtos.cs` | `BIL-FIN-020`, `BIL-FIN-021`; `BlockingCodes` pada kesiapan finalisasi |
| `Billing/Services/BillingInvoiceService.cs` | `GetReviewQueueAsync`, `ResolveReviewAsync` (`BIL-REV-001`); penjaga `from-source` meliputi rawat inap |
| `Billing/Dtos/BillingInvoiceDtos.cs` | `InvoiceReviewQueueQuery`, `InvoiceReviewItemResponse`, `ResolveInvoiceReviewRequest` |
| `Billing/Controllers/BillingInvoicesController.cs` | Dua endpoint API 3.9 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint API 3.9; kesiapan finalisasi bertambah `BlockingCodes` (aditif) |
| Database | Memakai kolom `I2` pada `BilInvoice` |
| Keamanan/Auth | Memakai `BillingInvoice : Read` dan `: Update` yang sudah ada, sesuai kontrak |

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/review-queue` | Antrean invoice "perlu diperiksa" | `BillingInvoice : Read` |
| `POST` | `/{id}/review-resolution` | Menyelesaikan pemeriksaan; 422 `BIL-REV-001` | `BillingInvoice : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review jalur rawat jalan (regresi) | Kondisi hanya diperluas; logika titik tagih tidak berubah | `PASS` | Review diff |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Proses bisnis per jenis layanan dengan contoh berangka | Belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tindakan dan lab rawat inap masuk invoice `RANAP` dengan harga master | Terpenuhi (source) | Jembatan menerima `Inpatient`; `BillingSourceTariffResolver` |
| 2. Obat saat diserahkan; MAR bukan sumber | Terpenuhi (source) | Kebijakan `PHARMACY` `DISPENSED`; tidak ada domain MAR |
| 3. Biaya admin untuk invoice bertarif kamar | Terpenuhi (source) | `CalculateAdministrationFeeAsync` |
| 4. Finalisasi ditolak `BIL-FIN-020`/`021` | Terpenuhi (source) | `BillingFinalizationService` |
| 5. Penyelesaian ditolak `BIL-REV-001` | Terpenuhi (source) | `ResolveReviewAsync` |
| 6. Regresi rawat jalan tidak berubah | Terpenuhi (source, review diff) | Belum dibuktikan runtime |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pesan penolakan `from-source` berubah karena kini meliputi rawat inap |
| Masalah yang diketahui | `NONE`. Kedua endpoint memakai kunci `Read`/`Update` yang sudah ada, mengikuti pola controller ini (banyak action berbagi kunci yang sama) |
| Risiko tersisa | Titik tagih rawat inap disamakan dengan rawat jalan; regresi rawat jalan perlu diuji runtime |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??`. `BillingInvoiceService.cs` kini juga diubah `BE-RWI-179` (belum di-commit) |
| Langkah berikutnya | Uji per jenis layanan dan regresi rawat jalan sesudah build |
