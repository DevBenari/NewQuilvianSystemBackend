# Laporan Perubahan Backend — `BE-RWI-149`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-149` |
| Judul | Bentuk data integrasi `1.1.0` (`I1`, `I2`) |
| Slice | `MVP-0` / `RWF-W0` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-149` |
| Trace | `FR-RWF-006`, `007`, `014`, `019`; `RWI-DEC-166`, `192`; kamus data 6.3–6.8; backend 9.7, 9.9, 9.10 |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, diperiksa 1, diubah 2, logika 0, kontrak API 0, database 2, keamanan 0, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | Model, enum, dan configuration `InPatientManagement` dan `BillingManagement/Billing`; `ApplicationDbContext.cs` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` serta pembuatan dan penerapan migration `I1`/`I2` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` (`I1`) dan `BillingManagement` / `Billing` (`I2`) |
| Prefix registry | `Inp`, `Bil` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (`BilInpatientEventReceipt`, `InpClearanceObservation`); `TOUCHED LEGACY` (model lain) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-DB-001` |
| Wewenang | Source: ya. Pembuatan migration: **tidak** (keputusan pengguna). Eksekusi database: tidak |

---

## 1. Masalah yang diperbaiki

Kontrak `1.1.0` membutuhkan kolom jejak status kasir, rantai koreksi penempatan, sewa pemrosesan
outbox, tanda "perlu diperiksa" pada invoice, dan tabel tanda terima Billing. Bentuk data itu
belum ada.

## 2. Proses bisnis

Task ini hanya menyiapkan bentuk data; perilaku endpoint tidak berubah. Pemakaiannya ada pada
`BE-RWI-150` (tanda terima), `151` (sewa pemrosesan), `153` (jejak status kasir), `154` (rantai
koreksi), dan `155` (tanda "perlu diperiksa").

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 6.3–6.8, model dan configuration `InpEpisode`, `InpBedPlacement`, `InpIntegrationOutbox`,
`BilInvoice`, enum `OutboxStatus`, `BillingClearanceStatus`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Enums/InpClearanceObservation.cs` | Baru — `Cleared`, `Pending`, `Blocked`, `Revoked`, `Unreadable = 9` |
| `InPatientManagement/Models/InpEpisode.cs` + configuration | `DepartureClearanceObserved`, `DepartureClearanceObservedAt`, `DepartureClearanceWarningAcknowledged` (bawaan `false`), `ClosureClearanceObserved`; index `IX_InpEpisode_DepartureClearanceObserved`. Enam kolom lama ditandai `[Obsolete]`, **tidak dihapus** |
| `InPatientManagement/Models/InpBedPlacement.cs` + configuration | `SupersededAtUtc`, `CorrectsPlacementId`, `SupersededByCorrectionId`; dua FK ke diri sendiri `Restrict` dan dua index |
| `InPatientManagement/Models/InpIntegrationOutbox.cs` + configuration | `ProcessingStartedAtUtc`, `AcknowledgedReceiptId`, `ReplayBatchId`, `ReplayedAtUtc`; `IX_InpIntegrationOutbox_Status_ProcessingStartedAtUtc`, `IX_InpIntegrationOutbox_ReplayBatchId` |
| `BillingManagement/Billing/Models/BilInvoice.cs` + configuration | `RequiresReview` (bawaan `false`), `ReviewReasonCode`, `ReviewFlaggedAt`, `ReviewResolvedAt`, `ReviewResolvedByUserId` (FK `Restrict`), `ReviewResolutionNote`; index terfilter `IX_BilInvoice_RequiresReview` |
| `BillingManagement/Billing/Models/BilInpatientEventReceipt.cs` + `BilInpatientEventReceiptConfiguration.cs` | Baru — unique `IX_BilInpatientEventReceipt_IdempotencyKey`, index `EncounterId`/`EpisodeId`, FK invoice `Restrict` |
| `Repositories/ApplicationDbContext.cs` | `DbSet` `BilInpatientEventReceipts` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Kolom dan tabel baru. **Migration `I1` dan `I2` belum dibuat** — dibuat pemilik |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah atau mengubah endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pencocokan nama, tipe, nullability, bawaan, index, FK terhadap kamus 6.3–6.7 | Sesuai | `PASS` | Review source |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Migration `I1`/`I2` (`Up()`/`Down()`) | Tidak dibuat | `NOT RUN` | Keputusan pengguna |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT APPLICABLE` — tanpa perubahan perilaku.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Bentuk data sama dengan kamus data 6.3–6.7 | Terpenuhi (source) | Daftar 3.2 |
| 2. FK rantai koreksi ke diri sendiri `Restrict` | Terpenuhi (source) | `InpBedPlacementConfiguration` |
| 3. Unique `IdempotencyKey` pada tanda terima | Terpenuhi (source) | `IX_BilInpatientEventReceipt_IdempotencyKey` |
| 4. Kedua migration punya `Down()` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | Migration dibuat pemilik |
| 5. Tidak ada perubahan perilaku endpoint | Terpenuhi (source) | Hanya model/configuration |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `[Obsolete]` pada enam kolom lama `InpEpisode` memunculkan peringatan `CS0618` pada kode yang masih membacanya (`InpatientBillingQueryService`, `InpatientClearanceGateService`) sampai `BE-RWI-152`/`153` dikerjakan. Peringatan, bukan error |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Migration `I1`/`I2` memakai satu snapshot bersama dengan migration sub-modul lain; urutan pada `02-module-map.md` 7.4 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??` |
| Langkah berikutnya | Pemilik membuat migration `I1`/`I2` dan memeriksa `Down()` |
