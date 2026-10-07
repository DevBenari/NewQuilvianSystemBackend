# Laporan Perubahan Backend — `BE-RWI-165`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-165` |
| Judul | Observasi WSD per selang (`K9` bagian WSD) |
| Slice | `MVP-1` / `RWF-W2` — Catatan Keperawatan V1 |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-165` |
| Trace | `FR-RWF-054`, `FR-RWF-058`; `RWI-DEC-200`; `INV-RWF-10`, `11`, `12`; `AC-RWF-052`, `057`, `058`; `UAT-RWF-09`, `UAT-RWF-22` |
| Contract version | `keperawatan` `0.6.0` **`approved`** (`RWI-DEC-221`, 2 Oktober 2026): API 8.4, kamus data 12.5–12.6, backend 12.6.2 |
| Dependency | — |
| Klasifikasi | `HEAVY` — skor 11: repository 0, berkas diperiksa 1, berkas diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/**`, `Repositories/**`, `Program.cs`, `Migrations/**`, laporan dan bukti roadmap sub-modul `keperawatan` |
| Model | Codex (implementasi awal, terhenti karena batas penggunaan); Claude Opus 5.5 (review, perbaikan, build akhir, migration, database update, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), seluruh perubahan masih berupa working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Ketujuh acceptance criteria terpetakan ke source; build akhir `PASS`; migration diterapkan ke database development |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` (pemilik data cairan dan WSD) |
| Prefix registry | `Cli` — `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` (dua model, satu enum, service, controller, DTO); `TOUCHED LEGACY` (`DailyMonitoringService.Fluid.cs`, `DailyMonitoringConfigurations.cs`) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`/`002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |
| Wewenang | Source: ya. Migration: ya (preferensi pemilik di `AGENTS.md` 5 Oktober 2026). Database development: ya, instruksi eksplisit pengguna 5 Oktober 2026 ("anda yang melakukan build dan database update"). Deployment: tidak |
| Branch | `MHamzah`, upstream `origin/MHamzah`; tidak ada switch, stage, commit, atau push |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/` (TASK_RULES, TASK_CLASSIFICATION, API_RULES, DATABASE_RULES, REVIEW_RULES, REPORT_TEMPLATE, TEST_POLICY, BACKEND_ENGINEERING_CONTRACT, MODULE_OWNERSHIP_PREFIX_REGISTRY); `rules/rule-output/`; standar endpoint transaksi dan aturan hak akses |

## 1. Masalah yang diperbaiki

Sebelum task ini, cairan yang keluar dari selang WSD (*water sealed drainage*) dicatat perawat sebagai entri cairan biasa, dengan volume dihitung sendiri di kertas. Sistem tidak tahu berapa sisa cairan di botol pada shift sebelumnya, tidak membedakan dua selang pada pasien yang sama, dan tidak mencegah salah hitung.

Contoh akibatnya: sisa botol shift lalu 200 ml, perawat membuang 300 ml, sisa sekarang 150 ml. Cairan yang benar-benar keluar adalah 250 ml, tetapi perawat yang lupa sisa lama bisa menulis 300 ml, sehingga balance cairan 24 jam keliru 50 ml.

## 2. Proses bisnis

**Tujuan.** Volume WSD dihitung server dari bacaan botol, per selang dan per shift, lalu menjadi tepat satu entri cairan yang sama yang dibaca Spooling Cairan dan Pengawasan Harian.

**Pelaku.** Perawat bangsal yang berwenang menulis pada episode itu (penjaga `NursingEpisodeWriteGuard`), dengan hak akses `FluidBalance` yang sudah ada (gate G-15).

**Pemicu.** Selang WSD dipasang, lalu tiap akhir shift perawat membaca botol.

**Prasyarat.** Episode `Admitted` atau `DischargePending` dan pasien belum keluar ruangan.

**Langkah utama.**

1. Perawat mendaftarkan selang: label (misalnya "WSD kanan"), lokasi, waktu pasang, dan sisa awal di botol bila ada.
2. Tiap shift perawat mengisi periode, sisa botol sekarang, dan volume yang dibuang.
3. Server mengambil sisa sebelumnya: sisa pada pembacaan aktif terakhir selang itu, atau sisa awal saat pemasangan, atau 0 ml.
4. Server menghitung **bertambah = sisa sekarang + dibuang − sisa sebelumnya**.
5. Server menyimpan pembacaan dan satu entri cairan keluar bersumber `DrainOrWsd` dengan volume hasil hitung, dalam satu transaksi dan di bawah kunci baris selang.
6. Saat selang dicabut, perawat mencatat waktu pelepasan; pembacaan berikutnya ditolak.

**Contoh berangka (`UAT-RWF-09`).** Sisa lalu 200 ml, dibuang 300 ml, sisa sekarang 150 ml. Perhitungan: 150 + 300 − 200 = **250 ml**. Server menyimpan satu entri cairan keluar 250 ml. Pembacaan pertama tanpa sisa awal memakai 0 ml: sisa sekarang 100 ml, dibuang 0 → 100 ml.

**Contoh dua selang (`UAT-RWF-22`).** WSD kanan (sisa lalu 200) dan WSD kiri (sisa lalu 0) dibaca pada shift yang sama. Masing-masing memakai sisa lalu selangnya sendiri, sehingga hasilnya dua pembacaan dan dua entri cairan yang terpisah.

**Aturan bisnis.**

| Aturan | Akibat bila dilanggar |
| --- | --- |
| Hasil bertambah tidak boleh negatif | `422` `CLI-WSD-001` |
| Selang yang sudah dilepas, atau pasien yang sudah keluar ruangan, tidak menerima pembacaan | `422` `CLI-WSD-002` |
| Hanya pembacaan aktif terakhir yang boleh dikoreksi atau dibatalkan | `422` `CLI-WSD-003` |
| Periode pembacaan tidak boleh tumpang tindih dengan pembacaan sebelumnya | `422` |
| Label selang aktif unik per episode | `409` |
| Entri cairan milik WSD tidak dapat dikoreksi dari layar cairan biasa | `409` `WSD_MANAGED_ENTRY` |

**Perubahan status selang.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Daftarkan | `Active` | Perawat ber-`FluidBalance : Create` | Waktu pasang tidak sebelum admisi dan tidak di masa depan |
| `Active` | Lepas | `Removed` | Perawat ber-`FluidBalance : Update` | Waktu lepas tidak mendahului pembacaan terakhir |

**Jalur tidak normal.** Koreksi pembacaan terakhir menghitung ulang bertambah dari sisa sebelumnya yang tersimpan, lalu merevisi entri cairan lewat mesin revisi cairan yang sudah ada (`CorrectFluidAsync`), sehingga riwayat revisinya tetap satu. Pembatalan membatalkan pembacaan dan entri cairannya bersama. Dua perawat yang menyimpan bersamaan diantre oleh kunci baris selang, jadi keduanya tidak memakai sisa lama yang sama.

**Hasil akhir.** Spooling Cairan dan Pengawasan Harian menampilkan output WSD dari entri cairan yang sama; tidak ada tabel volume kedua.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task dan kontrak (`contracts/api-contract.md` 8.4, `data/data-dictionary.md` 12.5–12.6, `02-backend-architecture.md` 12.6.2), `CliFluidBalanceEntry` beserta mesin revisinya, `NursingEpisodeWriteGuard`, `NursingResult`, `FluidBalanceController` (pembanding hak akses), `AccessMenuSeeder` dan `PermissionRegistryDescriptor` (perilaku resource bersama), serta log sesi Codex untuk menelusuri suntingan yang terputus.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/CliWsdDrain.cs` | Baru. Selang WSD per episode |
| `Areas/HealthServices/ClinicalManagement/Models/CliWsdReading.cs` | Baru. Pembacaan per shift dengan FK unik ke satu entri cairan |
| `Areas/HealthServices/ClinicalManagement/Enums/CliWsdDrainStatus.cs` | Baru. `Active`, `Removed`, `Cancelled` |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliWsdDrainConfigurations.cs` | Baru. Key, presisi `numeric(8,1)`, FK `Restrict`, unique label aktif per episode, unique `FluidBalanceEntryId` |
| `Areas/HealthServices/ClinicalManagement/DTOs/WsdObservationDtos.cs` | Baru. Request dan response API 8.4 |
| `Areas/HealthServices/ClinicalManagement/Services/CliWsdObservationService.cs` | Baru. Daftar, daftar selang, koreksi selang, lepas, riwayat, catat, koreksi, dan batal pembacaan |
| `Areas/HealthServices/ClinicalManagement/Controllers/WsdObservationController.cs` | Baru. Delapan endpoint; atribut `[AccessController]` disamakan dengan `FluidBalanceController` pada review Claude |
| `Areas/HealthServices/ClinicalManagement/Services/DailyMonitoringService.Fluid.cs` | Koreksi dan pembatalan cairan menerima transaksi pemanggil dan penanda `fromWsd`; entri milik WSD ditolak dari jalur cairan biasa |
| `Repositories/Configurations/HealthServices/ClinicalManagement/DailyMonitoringConfigurations.cs` | Check constraint volume diperluas: sumber `DrainOrWsd` (14) boleh `≥ 0` ml |
| `Repositories/ApplicationDbContext.cs` | DbSet `CliWsdDrains`, `CliWsdReadings` (nama jamak dirapikan pada review) |
| `Program.cs` | Registrasi `CliWsdObservationService` |
| `Migrations/20261005071042_AddRawatInapKeperawatanFinishing*.cs`, `ApplicationDbContextModelSnapshot.cs` | Migration gabungan Finishing keperawatan (lihat 3.3) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Delapan endpoint baru sesuai API 8.4. Delta: `remove` dan `cancel` menerima `PATCH` sesuai kontrak **dan** alias `POST` sesuai standar endpoint transaksi; respons pembacaan menambah `RevisionNumber` |
| Database | Tabel baru `CliWsdDrain`, `CliWsdReading`; check constraint `CK_CliFluidBalanceEntry_Volume` diganti versi yang lebih longgar untuk sumber 14. Dikemas dalam migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` bersama bagian `K9` task lain, `K10`, `K11`, dan kolom `InpEpisode.Version`. **Diterapkan** ke database development 5 Oktober 2026 |
| Keamanan/Auth | Memakai Resource `FluidBalance` yang sudah ada (`Read`, `Create`, `Update`); tidak ada kemampuan baru di layar Akses Role. Penulisan tetap dijaga penugasan perawat pada episode |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / WSD Observation

Base URL: `api/v1/health-services/clinical-management/wsd-drains`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Selang per episode beserta pembacaan terakhir | `FluidBalance : Read` | Query `episodeId`, `includeRemoved` | `List<WsdDrainResponse>` |
| `POST` | `/` | Mendaftarkan selang | `FluidBalance : Create` | `RegisterWsdDrainRequest` | `WsdDrainResponse` (`201`) |
| `PUT` | `/{drainId}` | Koreksi label, lokasi, atau waktu pasang dengan alasan dan versi | `FluidBalance : Update` | `CorrectWsdDrainRequest` | `WsdDrainResponse` |
| `PATCH` / `POST` | `/{drainId}/remove` | Melepas selang | `FluidBalance : Update` | `RemoveWsdDrainRequest` | `WsdDrainResponse` |
| `GET` | `/{drainId}/readings` | Riwayat pembacaan | `FluidBalance : Read` | — | `List<WsdReadingResponse>` |
| `POST` | `/{drainId}/readings` | Mencatat pembacaan shift; server menghitung bertambah dan membuat entri cairan | `FluidBalance : Create` | `RecordWsdReadingRequest` | `WsdReadingResponse` (`201`) |
| `PUT` | `/readings/{readingId}/correction` | Koreksi pembacaan terakhir | `FluidBalance : Update` | `CorrectWsdReadingRequest` | `WsdReadingResponse` |
| `PATCH` / `POST` | `/readings/{readingId}/cancel` | Membatalkan pembacaan terakhir | `FluidBalance : Update` | `CancelClinicalMeasurementRequest` | `WsdReadingResponse` |

Kode status: `400` isian tidak sah (misalnya alasan koreksi kosong); `403` perawat tidak berwenang pada episode; `404` selang atau pembacaan tidak ditemukan; `409` label aktif ganda atau versi berubah; `422` `CLI-WSD-001` hasil negatif, `CLI-WSD-002` selang sudah dilepas atau pasien keluar, `CLI-WSD-003` bukan pembacaan terakhir.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)`, 3 menit 6 detik | `PASS` | Log build akhir. Nol warning pada berkas baru; warning di berkas yang diubah berada di baris lama yang tidak disentuh |
| Build tahap penyelesaian sebelum akhir | Build Codex: 3 lalu 4 error (argumen salah tempel, `!.Value` pada `Guid`, nama field dokter). Diperbaiki seluruhnya; satu percobaan gagal menyalin berkas log karena disk C: penuh sesaat | `NEW ERROR` (sudah diperbaiki); `EXISTING / ENVIRONMENT ISSUE` (disk) | Log build Codex dan Claude |
| `dotnet ef migrations add AddRawatInapKeperawatanFinishing --no-build` | Migration `20261005071042` terbentuk: 11 `CreateTable`, 49 `CreateIndex`, 4 `AddColumn`, 1 `AddForeignKey`, 1 ganti check constraint; `Down()` simetris; nol operasi pada tabel di luar cakupan | `PASS` | Isi migration ditinjau baris per baris |
| `dotnet ef database update --no-build` | `Applying migration '20261005050735_AddNutritionAndBloodInstructionVerification'`, `'20261005071042_AddRawatInapKeperawatanFinishing'`, `Done.` | `PASS` | Keluaran perintah |
| `dotnet ef migrations list --no-build` | Nol migration `Pending` | `PASS` | Keluaran perintah |
| Pemeriksaan statis atribut hak akses (66 endpoint baru dan tersentuh) | Nol ketidakcocokan `[AccessPermission]` ↔ `ControllerName` ↔ `[AccessAction]`; `AccessType` hanya Read/Create/Update/Delete | `PASS` | Skrip pemeriksa atribut |
| Verifikasi proses bisnis — contoh 200/300/150 | `ComputeIncreaseMl(200, 150, 300)` = 150 + 300 − 200 = 250; entri cairan `Output`/`DrainOrWsd` 250 ml dibuat di transaksi yang sama | `PASS` | `CliWsdObservationService.cs:16-17`, `:97-106` |
| Verifikasi proses bisnis — dua selang, negatif, selang dilepas, bukan terakhir | Sisa lalu diambil per `WsdDrainId`; negatif → `CLI-WSD-001`; `Removed`/keluar → `CLI-WSD-002`; bukan terakhir → `CLI-WSD-003` | `PASS` | `CliWsdObservationService.cs:91-99`, `:121`, `:147` |
| Uji API HTTP terautentikasi dan UAT klinis `UAT-RWF-09`/`22` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini; aplikasi tidak dijalankan karena `AGENTS.md` melarang menjalankan aplikasi dan hosted service tanpa wewenang runtime.

**Tidak dijalankan:** uji HTTP lewat Swagger, UAT bersama perawat, rollback `Down()` terhadap database.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Satu selang: sisa lalu 200 ml, dibuang 300 ml, sisa sekarang 150 ml → bertambah 250 ml dan satu entri cairan `DrainOrWsd` 250 ml | Terpenuhi | `CliWsdObservationService.cs:97-106`; unique `IX_CliWsdReading_FluidBalanceEntryId` |
| 2. Dua selang dihitung terpisah per selang | Terpenuhi | `LatestAsync` memfilter `WsdDrainId` (`:147`) |
| 3. Hasil negatif → 422 `CLI-WSD-001` | Terpenuhi | `:99`, `:129` |
| 4. Pembacaan sesudah selang dilepas → 422 `CLI-WSD-002` | Terpenuhi | `:91` |
| 5. Koreksi selain pembacaan terakhir → 422 `CLI-WSD-003`; koreksi terakhir merevisi entri cairan lewat mesin revisi yang ada | Terpenuhi | `:121`, `:130-133`; `DailyMonitoringService.Fluid.cs` `CorrectFluidAsync(..., fromWsd: true)` |
| 6. Pembacaan pertama memakai sisa awal saat pemasangan, atau 0 ml | Terpenuhi | `:97` (`latest?.CurrentResidualMl ?? drain.InitialResidualMl`), `InitialResidualMl ?? 0` (`:36`) |
| 7. Tidak ada tabel volume WSD kedua (`INV-RWF-10`) | Terpenuhi | Volume balance hanya di `CliFluidBalanceEntry`; `CliWsdReading` mengikuti kamus data 12.6 |
| DoD: `dotnet build` tanpa error | Terpenuhi | Build akhir `0 Error(s)` |
| DoD: laporan mencatat status penerapan migration apa adanya | Terpenuhi | Bagian 3.3 dan 5 |
| DoD: uji runtime/UAT | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** ("setelah itu update tandai sebagai selesai semua") | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Resource `FluidBalance` dipakai dua controller. Resource yang terbaca lebih dulu menentukan label di layar Akses Role, jadi metadata `WsdObservationController` disamakan dengan `FluidBalanceController` |
| Masalah yang diketahui | Entri cairan WSD boleh bernilai 0 ml (pembacaan tanpa cairan baru tetap tercatat), sesuai check constraint baru |
| Risiko tersisa | Belum diuji lewat HTTP dan bersama perawat. Rollback `Down()` akan gagal menambah ulang constraint lama bila sudah ada entri WSD 0 ml atau di atas 10.000 ml |
| Perubahan sampingan | Review Claude: atribut akses WSD dilengkapi; DbSet `*Records` dinamai ulang menjadi bentuk jamak sesuai QBE. Tidak ada perubahan di luar scope |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude dari keadaan Git dan log sesi Codex. Satu pemanggilan build terputus saat sesi tersambung ulang, lalu diverifikasi dari log bahwa build itu selesai sukses |
| Status Git | `git status --short`: 151 entri (63 `M`, 3 `D`, 85 `??`), mencakup pekerjaan sesi sebelumnya yang dipertahankan. Berkas task ini tercantum di 3.2. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `FE-RWI-183` (layar WSD); UAT `UAT-RWF-09`/`22` bersama perawat |
