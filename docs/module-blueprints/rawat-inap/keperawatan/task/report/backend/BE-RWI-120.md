# Laporan Perubahan Backend — `BE-RWI-120`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-120` |
| Judul | Balance cairan per shift dan 24 jam |
| Slice | Gelombang 4 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-120` |
| Trace | `FR-KEP-059`, `FR-KEP-060`; `AC-KEP-093`; `VAL-KEP-26a`; kamus data 11.11; api-contract 7.5, 7.9 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-119` ✅ 17 September 2026 |
| Klasifikasi | `MEDIUM` — perhitungan baca dan konfigurasi |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — lima kriteria terpetakan; `dotnet build` lolos; kontrak API runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY`; tabel `CliNursingShift` dari K5 |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-SVC-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-API-001`; `CliNursingShift` **tanpa** `SortOrder` tersimpan (urutan dari jam mulai) |
| Hak akses baru | `NursingShift : Read`, `: Update` |
| Database | Nol migration tambahan |

## 1. Masalah yang diperbaiki

Balance cairan dibaca per shift dan per 24 jam, tetapi jam shift berbeda antarunit. Menanam jam shift di kode membuat
balance salah untuk unit yang berbeda, dan unit tanpa shift terkonfigurasi tidak boleh diberi shift buatan.

## 2. Proses bisnis

1. Admin mengatur shift bawaan: `PUT nursing-shifts` `{ serviceUnitId: null, shifts: [Pagi 07:00–14:00, Siang 14:00–21:00, Malam 21:00–07:00] }`.
   ICU menimpa dengan tiga shift delapan jam untuk unitnya; daftar kosong menghapus shift unit sehingga unit memakai bawaan.
2. Ringkasan Budi 17 September → hari dimulai pada jam mulai shift paling awal: 07.00 17 September sampai 07.00 18
   September (waktu rumah sakit). Balance per shift dan per 24 jam dijumlah dari entri **aktif**; entri `Cancelled`
   tidak ikut. Pukul 03.00 tanpa `date` masih termasuk hari shift 16 September.
3. Unit dan bawaan tanpa shift → hanya baris 24 jam 00.00–24.00, `ShiftConfigurationMissing = true`, nol shift buatan.
4. **Jalur tidak normal:** Pagi 07–14, Siang 14–20, Malam 21–07 → `400` "Jam shift harus menutup 24 jam tanpa celah dan
   tanpa tumpang tindih."; kode shift ganda → `400`; jam tidak sah → `400`.
5. Jam shift tidak dibaca penjaga tulis mana pun: kewenangan tetap penempatan unit (`AC-KEP-093`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 11.11, api-contract 7.5 dan 7.9, validation matrix 6.3.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/DailyMonitoringService.cs` | `GetFluidTotalsAsync`, `GetShiftSetAsync`, `ReplaceShiftSetAsync`, `BuildDayWindow`, `CoversFullDay` |
| `Areas/HealthServices/ClinicalManagement/Services/HospitalTimeZone.cs` | Batas hari pada jam dinding rumah sakit |
| `Areas/HealthServices/ClinicalManagement/Controllers/NursingShiftController.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Controllers/FluidBalanceController.cs` | `GET episodes/{episodeId}/totals` |
| `Areas/HealthServices/ClinicalManagement/Controllers/DailyObservationController.cs` | Ringkasan `daily-monitoring/episodes/{episodeId}/summary` memuat `FluidTotals` |
| `Areas/HealthServices/ClinicalManagement/DTOs/DailyMonitoringDtos.cs` | `FluidTotalsResponse`, `FluidTotalLine`, DTO shift |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup Nursing Shift sesuai 7.9; balance dalam ringkasan 7.5. Delta: endpoint baca terpisah `GET fluid-balance-entries/episodes/{episodeId}/totals`; `SortOrder` pada request diterima tetapi tidak disimpan |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Jam shift tidak mempengaruhi kewenangan |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Nursing Shift — `nursing-shifts`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Shift unit, atau bawaan dengan `IsDefault = true` | `NursingShift : Read` |
| `PUT` | `/` | Mengganti seluruh shift satu unit atau bawaan | `NursingShift : Update` |

#### Health Services / Clinical Management / Fluid Balance

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}/totals` | Balance per shift dan 24 jam satu hari | `FluidBalance : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran algoritma `CoversFullDay` pada contoh kontrak | Pagi 07–14, Siang 14–20, Malam 21–07: akhir Siang 20.00 ≠ mulai Malam 21.00 → tolak | `PASS` | Review source |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Verifikasi kontrak API runtime 7.5 | Tidak dijalankan | `NOT RUN` | Build lolos; belum dijalankan pada putaran ini |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Hanya entri aktif | Terpenuhi | Filter `EntryStatus == Active` |
| 2. Per shift dan per 24 jam | Terpenuhi | `FluidTotalsResponse.Shifts` dan `.Day` |
| 3. Tanpa shift hanya 24 jam | Terpenuhi | `ShiftConfigurationMissing`, `Shifts` kosong |
| 4. Shift per unit atau bawaan | Terpenuhi | `ResolveShiftsAsync`, `ReplaceShiftSetAsync` |
| 5. Shift tidak mempengaruhi kewenangan | Terpenuhi | Nol pembacaan shift pada `NursingEpisodeWriteGuard` |
| DoD: `dotnet build`, verifikasi kontrak | `dotnet build` lolos; **verifikasi kontrak dikecualikan atas keputusan pemilik 17 September 2026** | `PASS` sebagian; sisanya `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | "Shift pertama" dibaca sebagai shift berjam mulai paling awal, karena urutan tidak disimpan |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` |
| Langkah berikutnya | Frontend Pengawasan Harian |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |
