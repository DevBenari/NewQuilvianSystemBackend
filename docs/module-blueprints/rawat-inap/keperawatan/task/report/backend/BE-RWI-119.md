# Laporan Perubahan Backend — `BE-RWI-119`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-119` |
| Judul | Cairan, gula darah, dan observasi harian (migration K5) |
| Slice | Gelombang 3 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-119` |
| Trace | `FR-KEP-057`, `FR-KEP-061`, `FR-KEP-062`; `RWI-DEC-148`, `RWI-DEC-150` b (`G-25`); `VAL-KEP-24a`–`c`, `24h`, `25a`–`c`, `26b`; state matrix 5.4; kamus data 11.8–11.11; api-contract 7.6–7.8 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-114` ✅ 17 September 2026 |
| Klasifikasi | `HEAVY` — tujuh tabel baru, satuan GDS keselamatan insulin |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement`, `Repositories/`, `Migrations/`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — tujuh kriteria terpetakan; migration K5 **ditulis, tidak dijalankan**; `dotnet build` **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY`; entity baru `CliFluidBalanceEntry`, `CliFluidBalanceEntryRevision`, `CliBloodGlucoseReading`, `CliBloodGlucoseReadingRevision`, `CliDailyObservation`, `CliDailyObservationRevision`, `CliNursingShift` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-ENT-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-SVC-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DEL-001`, `QBE-LOG-001`, `QBE-TXN-001` |
| Hak akses baru | `FluidBalance`, `BloodGlucose`, `DailyObservation` : `Read`/`Create`/`Update` |
| Nomor bisnis | `NOT APPLICABLE` — entri terukur tidak bernomor pada kamus data 11.8–11.10 |
| Database | `20260917103000_AddDailyMonitoring` (K5); `Down` menolak bila tabel berisi data pasien; belum diterapkan |

## 1. Masalah yang diperbaiki

Pengawasan Harian tidak punya tempat terstruktur: cairan tanpa sumber dan volume tidak dapat dijumlah, GDS tanpa satuan
eksplisit dapat dibaca berlipat delapan belas, dan koreksi menimpa nilai lama tanpa jejak.

## 2. Proses bisnis

1. **Cairan.** Infus RL 500 ml pukul 10.00 → `POST fluid-balance-entries` (arah `Intake`, sumber `Infusion`, 500 ml). Server
   mengambil pelaksana dari akun login dan menegakkan episode berjalan serta penempatan unit.
   Koreksi 14.30 menjadi 450 ml "sisa 50 ml di kantong" → `PUT /{id}/correct` dengan `ExpectedRevisionNumber = 0` →
   `RevisionNumber = 1`, revisi menyimpan 500 ml.
2. **GDS bangsal.** `POST blood-glucose-readings` `{ glucoseValue: 280, glucoseUnit: 1 }` — satuan wajib tanpa bawaan.
3. **Observasi.** Diet 75%, mobilisasi `AssistedWalking`, lingkar perut 92 cm, agitasi kosong (= belum dinilai) →
   `POST daily-observations`.
4. **Jalur tidak normal:**
   - Volume 0 atau > 10.000 ml → `400`; Urin sebagai cairan masuk → `400` "Sumber Urin bukan cairan masuk.";
     waktu > 5 menit di masa depan atau sebelum pasien masuk → `400`.
   - GDS tanpa satuan → `400` "Pilih satuan gula darah: mg/dL atau mmol/L."; 280 mmol/L → `400` "Nilai gula darah 280
     mmol/L tidak mungkin. Periksa angka dan satuan."
   - Membatalkan GDS yang sudah dipakai pelaksanaan sliding scale → `409`; koreksinya diizinkan dan pelaksanaan itu
     ditandai `ReadingCorrectedAfterExecution`.
   - Diet di luar 0–100 atau lingkar perut di luar 20–250 cm → `400`; observasi tanpa satu isian pun → `400`.
   - Koreksi/pembatalan tanpa alasan → `400`; versi basi → `409` "Data sudah diubah pengguna lain. Muat ulang."
   - Kiriman ulang berkunci sama → `200` entri yang sama.
5. Koreksi dan pembatalan mengunci baris (`FOR UPDATE`) di dalam transaksi. Entri tidak pernah dihapus.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 11.8–11.11 dan 11.17, validation matrix 6.3, state matrix 5.4, api-contract 7.6–7.9.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/FluidDirection.cs`, `FluidSourceCategory.cs`, `ClinicalMeasurementStatus.cs`, `BloodGlucoseMethod.cs`, `MobilizationLevel.cs` | Baru |
| Tujuh model `Cli*` Pengawasan Harian | Baru |
| `Repositories/Configurations/HealthServices/ClinicalManagement/DailyMonitoringConfigurations.cs` | Baru — check constraint volume, tautan obat, satuan, batas observasi; unique parsial |
| `Repositories/ApplicationDbContext.cs`, `Migrations/20260917103000_AddDailyMonitoring.cs`, snapshot | DbSet, K5, snapshot |
| `Areas/HealthServices/ClinicalManagement/DTOs/DailyMonitoringDtos.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Services/DailyMonitoringService.cs`, `.Fluid.cs`, `.Glucose.cs`, `.Observation.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Controllers/FluidBalanceController.cs`, `BloodGlucoseReadingController.cs`, `DailyObservationController.cs` | Baru |
| `Program.cs` | Registrasi `DailyMonitoringService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga grup baru sesuai 7.6–7.8. Delta: koreksi entri cairan tidak dapat memindah sumber dari/ke Obat (`400`) — tautan dosis hanya lahir saat pencatatan; `GET` daftar memakai rentang bawaan 24 jam, paling panjang 31 hari |
| Database | Tujuh tabel baru `ClinicalManagement` |
| Keamanan/Auth | `CancelReason`, `CorrectionReason`, `DietNote`, `Note` tidak masuk payload logger |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Fluid Balance — `fluid-balance-entries`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Entri satu rentang | `FluidBalance : Read` |
| `POST` | `/` | Mencatat entri | `FluidBalance : Create` |
| `PUT` | `/{id}/correct` | Koreksi beralasan | `FluidBalance : Update` |
| `PATCH` | `/{id}/cancel` | Membatalkan beralasan | `FluidBalance : Update` |
| `GET` | `/{id}/revisions` | Riwayat koreksi | `FluidBalance : Read` |

#### Health Services / Clinical Management / Blood Glucose Reading — `blood-glucose-readings`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | GDS satu rentang beserta `UsedBySlidingScaleExecutionId` | `BloodGlucose : Read` |
| `POST` | `/` | Mencatat GDS | `BloodGlucose : Create` |
| `PUT` | `/{id}/correct` | Koreksi; pelaksanaan pemakainya ditandai | `BloodGlucose : Update` |
| `PATCH` | `/{id}/cancel` | Membatalkan GDS yang belum dipakai | `BloodGlucose : Update` |
| `GET` | `/{id}/revisions` | Riwayat koreksi | `BloodGlucose : Read` |

#### Health Services / Clinical Management / Daily Observation — `daily-observations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Observasi satu rentang | `DailyObservation : Read` |
| `POST` | `/` | Mencatat observasi | `DailyObservation : Create` |
| `PUT` | `/{id}/correct` | Koreksi; isian lama disimpan JSON | `DailyObservation : Update` |
| `PATCH` | `/{id}/cancel` | Membatalkan | `DailyObservation : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Verifikasi kontrak API 7.6–7.8 terhadap controller dan DTO | Seluruh endpoint dan isian ada | `PASS` | Bagian 4 |
| Nama check constraint dan index migration–konfigurasi–snapshot | Sama | `PASS` | Generator sesi ini |
| `dotnet build`, verifikasi skema | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tabel cairan, gula darah, observasi, shift beserta revisi di `ClinicalManagement` | Terpenuhi | Tujuh model, konfigurasi, K5 |
| 2. Cairan bersumber, ml, berwaktu, berpelaksana | Terpenuhi | `RecordFluidAsync`, `ValidateFluidValues` |
| 3. Pembatalan dan koreksi beralasan, nilai lama tersimpan | Terpenuhi | Tiga tabel revisi; `CancelReason` |
| 4. GDS satu tempat, satuan wajib tanpa bawaan | Terpenuhi | `GlucoseUnit` nullable pada request → `400`; check constraint `CK_CliBloodGlucoseReading_Unit` |
| 5. Diet, mobilisasi, lingkar perut, agitasi terstruktur | Terpenuhi | `CliDailyObservation` + `VAL-KEP-26b` |
| 6. `Active`/`Cancelled` beserta nomor revisi | Terpenuhi | `ClinicalMeasurementStatus`, `RevisionNumber` |
| 7. Mundur dilarang tanpa ekspor bila berisi data | Terpenuhi | `Down` K5 menolak bila tabel berisi baris |
| DoD: `dotnet build`, verifikasi skema dan kontrak | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Batas nilai GDS 10–1000 mg/dL dan 0,6–55,5 mmol/L mengikuti `VAL-KEP-25b` apa adanya |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Build dan migration belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??`; `M` `ApplicationDbContext.cs`, snapshot, `Program.cs` |
| Langkah berikutnya | `BE-RWI-120`, `BE-RWI-122`, `BE-RWI-123` |
