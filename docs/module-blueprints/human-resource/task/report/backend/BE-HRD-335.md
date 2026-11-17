# Laporan Perubahan Backend — `BE-HRD-335`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-335` |
| Judul | Konstanta nilai dan penanda hak akses tanpa endpoint |
| Slice | Amandemen `A52` atas `S-B4` Penjadwalan dan `S-B1` Kehadiran — `EPIC HRD-109`, gelombang `MVP-66` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B84 |
| Trace | `FR-PK-002`; `HRD-DEC-194`, `206`, `208`; `02-backend-architecture.md` 35.4.3–35.4.4; `contracts/permission-audit-matrix.md` 30.1 |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-334` 🟡 — source sudah di-commit pengguna (`9cc8097c`); migration belum diterapkan. Task ini tidak membutuhkan tabel baru |
| Klasifikasi | `SMALL` — skor 2: satu repository (0), sekitar 10 berkas diperiksa (1), 4 berkas source (1), logika sederhana (0), tanpa perubahan bentuk API (0), tanpa schema (0), hak akses berupa penanda baru yang tidak otomatis diberikan (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/SchedulingManagement/**`, `Areas/Corporate/HumanResource/AttendanceManagement/{Constants,Services}/**`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `9cc8097c` (branch `yoga`) |
| Tanggal | 12 November 2026 |
| Status | ✅ Selesai — dengan satu penanda dipindah ke `BE-HRD-349` (lihat 7) dan startup Development menunggu migration `BE-HRD-334` diterapkan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement`, `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` untuk `SchedulingExplicitPermissions.cs`; `TOUCHED LEGACY` untuk dua kelas konstanta dan `AttendanceDailyQueryService` |
| QBE yang berlaku | `QBE-NAM-001` (nama kelas/konstanta); tidak ada entity, configuration, atau controller baru sehingga `QBE-ENT-*`, `QBE-CFG-*`, `QBE-SVC-001` tidak berlaku |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` (folder `.codex/` sudah dicabut menurut `AGENTS.md`) |

---

## 1. Masalah yang diperbaiki

Task-task `A52` berikutnya membutuhkan nilai status dan kode aturan yang sama di banyak tempat.
Contohnya, status harian "Belum Hadir" dan "Tidak Ada Scan", status dispensasi, jenis perubahan
pada Riwayat Revisi, serta kode aturan roster `B3`/`B8`/`W6`/`W7`. Bila setiap service menulis
teksnya sendiri, satu salah ketik (`"NoScans"`) sudah cukup membuat filter dan laporan diam-diam
kosong.

Cakupan unit (`HRD-DEC-194`/`206`) juga membutuhkan cara memberi seseorang hak "kelola roster
semua unit". Hak ini tidak terikat pada endpoint mana pun, sehingga harus dideklarasikan sebagai
**penanda** agar dapat dicentang pada layar Akses Role.

---

## 2. Proses bisnis

Contoh pemakaian penanda sesudah `BE-HRD-336` selesai:

| Pengguna | Hak yang dicentang di Akses Role | Roster yang boleh diubah |
| --- | --- | --- |
| Sari, Kepala Ruang ICU | `RosterPeriod : Update` | Hanya ICU (unit penempatan berjabatannya) |
| Petugas HR Scheduling | `RosterPeriod : Update` + `RosterPeriod : ManageAllUnits` | Semua unit |

Penanda tidak otomatis diberikan ke siapa pun. Admin HRD harus mencentangnya untuk jabatan yang
tepat. Sampai `BE-HRD-336` dikerjakan, penanda ini belum dibaca oleh kode mana pun, sehingga
mencentangnya belum mengubah apa-apa.

Label Bahasa Indonesia dikirim backend. Contoh: filter status di `GET /filters/metadata`
Kehadiran Harian kini juga menawarkan "Belum Hadir", "Tidak Ada Scan", dan "Tugas Belajar".
Sampai `BE-HRD-342` mengubah pemrosesan harian, filter tersebut memang mengembalikan daftar
kosong, karena belum ada baris berstatus itu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `02-backend-architecture.md` 35.4.3–35.4.4, kartu `BE-HRD-335` dan `BE-HRD-349`
- `Areas/HealthServices/RadiologyManagement/RadiologyExplicitPermissions.cs`, `Areas/Corporate/HumanResource/BenefitManagement/HrdInstallmentPlanApprovalExplicitPermissions.cs` (pola penanda)
- `Attributes/AccessExplicitPermissionAttribute.cs`, `Services/Security/PermissionRegistryDescriptor.cs` (aturan registri)
- `Areas/Corporate/HumanResource/SchedulingManagement/Controllers/RosterPeriodController.cs` (kode modul dan `SortOrder` action)
- `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceDailyQueryService.cs`, `AttendanceProcessingService.cs`, `AttendanceRawLogService.cs` (daftar label yang sudah ada)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/SchedulingExplicitPermissions.cs` | Baru — penanda `RosterPeriod : ManageAllUnits` (`HUMAN_RESOURCE_SCHEDULING_MANAGEMENT`, `AccessTypes.Update`, `SortOrder = 10`) |
| `Areas/Corporate/HumanResource/AttendanceManagement/Constants/AttendanceValueConstants.cs` | `AttendanceStatus` + `NotYetPresent`, `NoScan`, `StudyAssignment`; `AttendanceExceptionType` + `OutsideScanWindow`; `RawLogProcessingStatus` + `AwaitingDeviceRegistration`; kelas baru `DispensationType`, `DispensationStatus`, `StudyAssignmentStatus`, `DeviceIncidentStatus` |
| `Areas/Corporate/HumanResource/SchedulingManagement/Constants/SchedulingRequestValueConstants.cs` | Kelas baru `ShiftRevisionChangeType`, `OnCallCallStatus`, `RosterRuleCode` (`B3`, `B8`, `W2`, `W3`, `W6`, `W7`) |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceDailyQueryService.cs` | Tiga pilihan status baru pada metadata filter beserta label Bahasa Indonesia |

Nama konstanta `RosterRuleCode` diambil dari arti aturan pada `02-backend-architecture.md` 19.9.2
dan 35.9.2. Contohnya, `W3` = `ConsecutiveNightShifts` (shift malam berturut-turut), bukan jam
kerja mingguan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons tidak berubah. `GET .../attendance-daily/filters/metadata` mendapat tiga butir tambahan pada `attendanceStatusOptions` |
| Database | `NOT APPLICABLE` — tanpa migration, tanpa eksekusi database |
| Keamanan/Auth | Satu action penanda baru di registri. Tidak ada `SysAccessPolicy` yang dibuat; tidak ada endpoint yang dilonggarkan |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Perubahan isi pada endpoint yang sudah ada:

| Method | Path | Hak akses | Perubahan |
| --- | --- | --- | --- |
| `GET` | `/api/.../attendance-daily/filters/metadata` | `AttendanceDaily : Read` | `attendanceStatusOptions` + `{ "value": "NotYetPresent", "label": "Belum Hadir" }`, `{ "value": "NoScan", "label": "Tidak Ada Scan" }`, `{ "value": "StudyAssignment", "label": "Tugas Belajar" }` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (sama dengan sebelumnya); nol peringatan dari berkas task ini | `PASS` | Keluaran build, 1 menit 45 detik |
| `PermissionRegistryDescriptor.BuildFromAssembly` atas DLL hasil build (konsol scratchpad lewat refleksi) | Tidak melempar galat; `HUMAN_RESOURCE_SCHEDULING_MANAGEMENT \| RosterPeriod:ManageAllUnits \| Manage Roster For All Units \| Update \| route=` (kosong); total 2.411 action | `PASS` | Keluaran konsol `regcheck` |
| Kontrol positif pada pembacaan yang sama | `RadReport:ActAsRadiologist` ikut terbaca, jadi pembaca penanda memang bekerja | `PASS` | Keluaran konsol yang sama |
| Resource `AttendanceReport` di registri | Tidak ada; memastikan `ReadAllUnits` memang belum boleh dideklarasikan | `PASS` | Keluaran konsol yang sama |
| Startup `ASPNETCORE_ENVIRONMENT=Development` | — | `NOT RUN` | Migration `A52` belum diterapkan, padahal model sudah memuat kolom baru (`HrdAttendanceSetting.ScanWindow*`, `MstOnCallType.AnswerTimeoutMinutes`). `AttendanceSchedulerHostedService` membaca `HrdAttendanceSettings`, sehingga berisiko galat `42703` yang bukan berasal dari task ini. Startup juga menulis baris `SysActionAccess` penanda baru ke devYoga lewat `AccessMenuSeeder`. Karena itu startup dijalankan bersama penerapan migration. Registri yang dibangun startup memakai `BuildCore` yang sama dengan `BuildFromAssembly` |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Registri hak akses memuat `RosterPeriod : ManageAllUnits` | Terpenuhi | Bagian 5 |
| Registri hak akses memuat `AttendanceReport : ReadAllUnits` | **Dipindah ke `BE-HRD-349`** sesuai baris Risiko pada kartu | Resource `AttendanceReport` baru lahir dari `AttendanceReportController`; registri melempar `InvalidOperationException` untuk penanda tanpa resource |
| Penanda menunjuk resource yang sudah terdaftar | Terpenuhi | `RosterPeriod` terdaftar dari `RosterPeriodController` pada modul yang sama |
| Startup tanpa galat registri | Terpenuhi lewat `BuildFromAssembly`; startup penuh menunggu migration | Bagian 5 |
| Konstanta 35.4.4 tersedia sebagai satu sumber | Terpenuhi | Bagian 3.2 |
| DoD umum: build lulus, tanpa angka bisnis tertanam, kolom sensitif tidak masuk log | Terpenuhi | Tidak ada angka atau log baru |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 603 peringatan build yang sudah ada; nol dari berkas task ini |
| Masalah yang diketahui | Konstanta kode aturan `B1`, `B2`, `B4`–`B7`, `W1`, `W4`, `W5` masih literal di `RosterValidationService`; desain memang hanya meminta enam kode baru. `AttendanceProcessingService` punya daftar label status sendiri yang berbeda kata ("Pulang awal" vs "Pulang lebih awal"); tidak disentuh karena di luar cakupan |
| Risiko tersisa | (1) `AttendanceReport : ReadAllUnits` wajib diingat di `BE-HRD-349`; kartunya sudah diberi catatan. (2) Sesudah penanda ini terdaftar, startup berikutnya akan menambah satu baris `SysActionAccess` lewat `AccessMenuSeeder`. Ini perilaku normal penanda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Saat mulai, ditemukan pengguna sudah meng-commit `BE-HRD-334` (`9cc8097c`); tidak ada yang dikerjakan ulang |
| Status Git | 4 berkas source (3 diubah, 1 baru); 2 dokumen roadmap/traceability diubah; laporan ini baru. Tidak ada stage atau commit |
| Langkah berikutnya | `BE-HRD-336` — `HrdUnitScopeService` dan penerapan cakupan pada endpoint roster (butuh data penempatan berjabatan dan akun non-superadmin untuk uji 403). Penerapan migration `BE-HRD-334` tetap menunggu instruksi pengguna |
