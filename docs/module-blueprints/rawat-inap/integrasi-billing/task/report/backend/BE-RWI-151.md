# Laporan Perubahan Backend — `BE-RWI-151`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-151` |
| Judul | Outbox jujur |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-151` |
| Trace | `FR-RWF-014`, `016`; `INV-RWF-04`, `05`; `RWI-DEC-161`, `166`; integrasi 4.2; backend 9.6, 9.11; state 5.1 |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-150` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, diperiksa 1, diubah 2, logika 2, kontrak API 0, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/{Services,Models,Options,Workers}`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` (outbox), memanggil `BillingManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (outbox service, worker, model); `NEW CODE` (options) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-LOG-001`, `QBE-SEC-001` (data minimum dalam pesan) |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Worker lama menandai pesan `Published` tanpa ada penerima, sehingga status "terkirim" tidak
membuktikan apa pun. Isi pesan juga membawa data pasien dan penjamin yang tidak dibutuhkan Billing,
dan pesan yang tersangkut `Processing` tidak pernah diambil ulang.

## 2. Proses bisnis

1. Pendaftaran pesan (`EnqueueEventAsync`) hanya menerima empat jenis event dan tiga `SourceType`;
   isi pesan disusun dari delapan field daftar putih dan diperiksa ulang `IsWhitelisted`. Field lain
   → ditolak.
2. Kunci idempotensi `INPATIENT:<SourceType>:<SourceId>:<Version>` (`RWI-DEC-161` butir 4).
3. Worker mengambil `BatchSize` (50) pesan `Pending`, `Failed` yang jatuh tempo, atau `Processing`
   yang sewanya (300 detik) habis; menandainya `Processing` + `ProcessingStartedAtUtc`.
4. Worker memanggil `BillingInpatientEventReceiver` dalam proses yang sama. `Accepted = true` →
   `Published` dengan `AcknowledgedReceiptId`. Selain itu → `Failed`, coba ulang dengan backoff
   `min(2^n × 5 detik, 3600 detik)`; sesudah `MaxRetry` (10) kali → `DeadLetter`.
5. Contoh: Billing mati saat Budi diadmisi → pesan `Failed` (percobaan 1, coba lagi ±10 detik),
   … ; begitu Billing hidup, percobaan berikutnya `Published`. Bila 10 kali gagal → `DeadLetter`
   untuk ditangani putar ulang (`BE-RWI-157`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpIntegrationOutboxService.cs`, `IInpIntegrationOutboxService.cs`, `InpatientIntegrationOutboxWorker.cs`,
`InpIntegrationOutbox.cs`, `OutboxStatus.cs`, seluruh pemanggil `EnqueueEventAsync`
(`InpEpisodeService`, `InpBedOccupancyService`, `InpatientClearanceGateService`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Services/IInpIntegrationOutboxService.cs`, `InpIntegrationOutboxService.cs` | Tanda tangan baru berbasis penanda kejadian; `InpIntegrationOutboxPayload` (daftar putih, kunci idempotensi, `IsWhitelisted`) |
| `InPatientManagement/Models/InpIntegrationOutbox.cs` | `MarkPublished(receiptId)`, `RecordFailure(…, maxRetry)` |
| `InPatientManagement/Options/InpatientIntegrationOutboxOptions.cs` | Baru — `ProcessingLeaseSeconds = 300`, `BatchSize = 50`, `MaxRetry = 10` |
| `InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs` | Sewa pemrosesan, panggilan penerima Billing, `Published` hanya bila diterima, backoff, `DeadLetter` |
| `InPatientManagement/Services/InpEpisodeService.cs` | `ADMISSION_CONFIRMED` tanpa data penjamin/pasien |
| `InPatientManagement/Services/InpBedOccupancyService.cs` | Event tempat tidur memakai tanda tangan baru |
| `InPatientManagement/Services/InpatientClearanceGateService.cs` | Hanya penyesuaian tanda tangan outbox (service ini dihapus `BE-RWI-153`) |
| `Program.cs` | `Configure<InpatientIntegrationOutboxOptions>` dari bagian `InpatientIntegrationOutbox` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Memakai kolom `I1` (`ProcessingStartedAtUtc`, `AcknowledgedReceiptId`) |
| Keamanan/Auth | Data pasien dan penjamin tidak lagi tersalin ke pesan (`INV-RWF-05`) |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — pemantauan outbox ada pada `BE-RWI-157`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review pemanggil `EnqueueEventAsync` | Semua memakai tanda tangan baru | `PASS` | Review source |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Proses bisnis lima kriteria dengan worker berjalan | Belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Field di luar daftar putih ditolak | Terpenuhi (source) | `IsWhitelisted`, `AllowedFields` |
| 2. Tanpa tanda terima pesan tidak pernah `Published` | Terpenuhi (source) | `MarkPublished` hanya bila `receipt.Accepted` |
| 3. Billing mati → `Failed` dengan backoff | Terpenuhi (source) | `RecordFailure`, rumus backoff worker |
| 4. Gagal 10 kali → `DeadLetter` | Terpenuhi (source) | `RetryCount >= maxRetry` |
| 5. `Processing` yang sewanya habis diambil ulang | Terpenuhi (source) | Saringan `ProcessingStartedAtUtc < leaseExpiredBefore` |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `appsettings.json` tidak diubah; nilai bawaan opsi ada di kode dan dapat ditimpa bagian `InpatientIntegrationOutbox` |
| Masalah yang diketahui | Kunci idempotensi `BED_RELEASED` kini berbasis penempatan (sebelumnya episode) |
| Risiko tersisa | Pesan lama yang dulu `Published` tanpa terkirim ditangani putar ulang `BE-RWI-157` |
| Perubahan sampingan | Isi `ADMISSION_CONFIRMED` tidak lagi memuat penjamin, pasien, dan waktu admisi |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??` |
| Langkah berikutnya | Uji worker dengan Billing hidup/mati sesudah build dan migration `I1`/`I2` |
