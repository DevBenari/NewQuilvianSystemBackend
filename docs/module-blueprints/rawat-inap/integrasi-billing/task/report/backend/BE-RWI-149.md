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
| Status | 🟡 Implementasi kode dan penerapan skema selesai; kriteria dua migration terpisah belum sesuai pengemasan gabungan. Pembaruan 5 Oktober 2026: build terintegrasi `PASS` dan migration `20261005033044_AddRawatInapFinishing` diterapkan berdasarkan output pengguna; API/alur bisnis dan rollback belum dijalankan |
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
| Database | kolom `InpEpisode`, `InpBedPlacement`, `InpIntegrationOutboxes`, `BilInvoice`, serta tabel `BilInpatientEventReceipt`. Perubahan `I1` + `I2` tercakup dalam `20261005033044_AddRawatInapFinishing`; pengguna melaporkan penerapan berhasil (`Done.`), bukti diterima 5 Oktober 2026. Nama database/lingkungan tidak disebut |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah atau mengubah endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pencocokan nama, tipe, nullability, bawaan, index, FK terhadap kamus 6.3–6.7 | Sesuai | `PASS` | Review source |
| Build project melalui `dotnet ef database update` | `Build succeeded.` | `PASS` | Output pengguna diterima 5 Oktober 2026; bukan eksekusi ulang oleh agent atau perintah `dotnet build` tersendiri; jumlah warning tidak disertakan |
| Migration `I1` + `I2` | Tercakup dalam `20261005033044_AddRawatInapFinishing`; `Up()`/`Down()` tersedia dan penerapan maju berhasil menurut output pengguna | `PASS` (penerapan maju); rollback `NOT RUN` | Output pengguna 5 Oktober 2026 dan source migration |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT APPLICABLE` — tanpa perubahan perilaku.

### 5.1 Pembaruan bukti 5 Oktober 2026

Build project saat `dotnet ef database update` **PASS** menurut output pengguna yang diterima 5 Oktober 2026 (`Build succeeded.`); migration `20261005033044_AddRawatInapFinishing` diterapkan sampai `Done.`. Uji API, regresi, alur klinis, dan rollback `Down()` tetap `NOT RUN`. Nama database dan lingkungan tidak tercantum pada output. Catatan pengecualian 2 Oktober 2026 adalah riwayat sesi implementasi, bukan status build/migration terkini.

Perubahan `I1` + `I2`: kolom `InpEpisode`, `InpBedPlacement`, `InpIntegrationOutboxes`, `BilInvoice`, serta tabel `BilInpatientEventReceipt`. Output lengkap, source migration, dan pemetaan lintas task ada pada [laporan BE-RWI-172](../../../../episode-rawat-inap/task/report/backend/BE-RWI-172.md#51-pembaruan-bukti-5-oktober-2026).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Bentuk data sama dengan kamus data 6.3–6.7 | Terpenuhi (source) | Daftar 3.2 |
| 2. FK rantai koreksi ke diri sendiri `Restrict` | Terpenuhi (source) | `InpBedPlacementConfiguration` |
| 3. Unique `IdempotencyKey` pada tanda terima | Terpenuhi (source) | `IX_BilInpatientEventReceipt_IdempotencyKey` |
| 4. Kedua migration punya `Down()` | Sebagian: perubahan `I1`/`I2` dan `Down()` ada dalam satu migration gabungan; tidak ada dua migration terpisah | `20261005033044_AddRawatInapFinishing` sudah diterapkan menurut output pengguna. Penyimpangan pengemasan dicatat, bukan perubahan otomatis acceptance criteria |
| 5. Tidak ada perubahan perilaku endpoint | Terpenuhi (source) | Hanya model/configuration |
| DoD build | Terpenuhi melalui build project terintegrasi pada perintah EF | Output pengguna: `Build succeeded.`; diterima 5 Oktober 2026 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `[Obsolete]` pada enam kolom lama `InpEpisode` memunculkan peringatan `CS0618` pada kode yang masih membacanya (`InpatientBillingQueryService`, `InpatientClearanceGateService`) sampai `BE-RWI-152`/`153` dikerjakan. Peringatan, bukan error |
| Masalah yang diketahui | Satu migration gabungan diterapkan, sedangkan kriteria 4 menyebut dua migration; penerapan berhasil tidak mengubah kriteria itu |
| Risiko tersisa | `Down()` gabungan membalik perubahan beberapa task sekaligus; eksekusi rollback belum diuji |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??` |
| Langkah berikutnya | Catat penyimpangan pengemasan dua migration `I1`/`I2` menjadi satu gabungan; eksekusi rollback belum diuji. Jangan membuat ulang perubahan yang sudah diterapkan. |
