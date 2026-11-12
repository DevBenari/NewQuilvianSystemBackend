# Laporan Perubahan Backend — `BE-HRD-334`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-334` |
| Judul | Model, configuration, dan migration `AddSchedulingAttendanceAmendmentA52` |
| Slice | Amandemen `A52` atas `S-B4` Penjadwalan dan `S-B1` Kehadiran — `EPIC HRD-109`, gelombang `MVP-66` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B84 |
| Trace | `FR-PK-001`; `HRD-DEC-195`, `198`, `200`, `201`, `202`, `205`, `211`, `212`, `217`; `02-backend-architecture.md` 35.4.1, 35.6, 35.7; `data/data-dictionary.md` bagian 27 |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), sekitar 14 berkas diperiksa (1), 21 berkas diubah/dibuat (2), logika sederhana (0), tanpa perubahan API (0), dampak schema/migration (2), tanpa keamanan (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/**/Models`, `Repositories/Configurations/Corporate/HumanResource/**`, `Repositories/ApplicationDbContext.cs`, `Migrations/` (pembuatan berkas saja, atas konfirmasi terpisah pengguna) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a437ae42` (branch `yoga`) |
| Tanggal | 12 November 2026 |
| Status | 🟡 Sebagian — source dan berkas migration selesai serta tervalidasi; migration **belum diterapkan** ke database mana pun, sehingga uji kering dan satu kriteria terima masih menunggu |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement`, `AttendanceManagement`, `MasterData/AttendanceAndSchedule` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); `Master / Reference / MasterData` / `Mst` (`ACTIVE`) — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` untuk 6 model dan 6 configuration; `TOUCHED LEGACY` untuk `HrdAttendanceCorrectionRequest`, `HrdAttendanceSetting`, `MstOnCallType`, dan configuration-nya |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`; `QBE-CFG-002` untuk configuration lama |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/TASK_RULES.md`, `DATABASE_RULES.md`, `TASK_CLASSIFICATION.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`. Folder `.codex/` tidak ada dan memang sudah dicabut menurut `AGENTS.md` |

---

## 1. Masalah yang diperbaiki

Desain amandemen `A52` membutuhkan tempat menyimpan enam jenis data yang belum ada di sistem:
Riwayat Revisi roster terbit, cadangan siaga, catatan panggilan siaga, dispensasi, tugas
belajar, dan kejadian gangguan mesin absen. Desain itu juga membutuhkan tiga kolom tambahan:

- setelan kehadiran harus bisa menyimpan jendela scan, karena sekarang angkanya tertanam di kode;
- jenis siaga harus bisa menyimpan batas jawab telepon;
- koreksi presensi harus bisa merujuk kejadian gangguan mesin.

Tanpa tabel dan kolom ini, task `BE-HRD-335` s.d. `BE-HRD-358` tidak punya tempat menyimpan data.

---

## 2. Proses bisnis

Task ini hanya menyiapkan wadah data; belum ada perilaku yang berubah bagi pengguna. Contoh:
setelah migration nanti diterapkan, baris setelan kehadiran RSMMC otomatis berisi jendela scan
240 menit sebelum dan 480 menit sesudah shift. Angka itu sama dengan perilaku kode sekarang,
sehingga hasil hitung kehadiran tidak berubah sampai Admin HRD mengubah setelan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- `Areas/Corporate/HumanResource/SchedulingManagement/Models/HrdSchedulingSetting.cs` dan configuration-nya (pembanding kode baru terdekat)
- `Repositories/Configurations/Corporate/HumanResource/SchedulingManagement/TrxShiftAssignmentConfiguration.cs`, `TrxShiftReplacementConfiguration.cs`, `AttendanceManagement/HrdEarlyLeavePermissionConfiguration.cs` (tipe kolom waktu)
- `Repositories/ApplicationDbContext.cs`
- Model rujukan FK: `MstWorkforceProfile`, `MstShift`, `MstOrganizationUnit`, `MstAttendanceDevice`, `TrxShiftAssignment`, `TrxRosterPeriod`, `TrxShiftReplacement`, `TrxOnCallAssignment`
- `Migrations/20261111020000_AddHrdNotificationAndSuspensionAccess.cs`, `Migrations/20261111040000_ResolveHrdSchemaResidualDrift.cs` (penyebab drift, lihat 7)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Models/HrdShiftAssignmentRevision.cs` | Baru — Riwayat Revisi, hanya-tambah |
| `Areas/Corporate/HumanResource/SchedulingManagement/Models/HrdOnCallBackup.cs` | Baru — cadangan siaga berurutan |
| `Areas/Corporate/HumanResource/SchedulingManagement/Models/HrdOnCallCall.cs` | Baru — catatan panggilan siaga, `RowVersion` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceDispensation.cs` | Baru — dispensasi, `RowVersion` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdStudyAssignment.cs` | Baru — tugas belajar |
| `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceDeviceIncident.cs` | Baru — kejadian gangguan mesin |
| `Repositories/Configurations/Corporate/HumanResource/SchedulingManagement/HrdShiftAssignmentRevisionConfiguration.cs` | Baru — 7 FK `Restrict`, 2 index sesuai kamus data |
| `Repositories/Configurations/Corporate/HumanResource/SchedulingManagement/HrdOnCallBackupConfiguration.cs` | Baru — 2 unique index tersaring `IsDelete = false`; `CHECK BackupSequence >= 1` |
| `Repositories/Configurations/Corporate/HumanResource/SchedulingManagement/HrdOnCallCallConfiguration.cs` | Baru — token konkurensi; `CHECK BackupSequence >= 0` |
| `Repositories/Configurations/Corporate/HumanResource/AttendanceManagement/HrdAttendanceDispensationConfiguration.cs` | Baru — `CHECK EndAt > StartAt`; index pegawai+tanggal dan status |
| `Repositories/Configurations/Corporate/HumanResource/AttendanceManagement/HrdStudyAssignmentConfiguration.cs` | Baru — `CHECK EndDate >= StartDate` |
| `Repositories/Configurations/Corporate/HumanResource/AttendanceManagement/HrdAttendanceDeviceIncidentConfiguration.cs` | Baru — `CHECK EndAt > StartAt` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceCorrectionRequest.cs` | Tambah `DeviceIncidentId` + navigation |
| `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceSetting.cs` | Tambah `ScanWindowBeforeMinutes` (240), `ScanWindowAfterMinutes` (480); keterangan `AutoConvertIncompleteToAbsent` diubah menjadi "usang" (`HRD-DEC-216`) |
| `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstOnCallType.cs` | Tambah `AnswerTimeoutMinutes` (10) |
| `Repositories/Configurations/.../HrdAttendanceCorrectionRequestConfiguration.cs` | FK `Restrict` + index `DeviceIncidentId` |
| `Repositories/Configurations/.../HrdAttendanceSettingConfiguration.cs` | Nilai bawaan dua kolom baru |
| `Repositories/Configurations/.../MstOnCallTypeConfiguration.cs` | Nilai bawaan kolom baru |
| `Repositories/ApplicationDbContext.cs` | Enam `DbSet` baru |
| `Migrations/20261112043425_AddSchedulingAttendanceAmendmentA52.cs` + `.Designer.cs` | Baru — dipangkas menjadi operasi A52 saja (lihat 7) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui EF; ikut menyerap drift snapshot yang sudah ada (lihat 7) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada controller, DTO, atau endpoint yang berubah |
| Database | 6 tabel baru, 4 kolom baru, 19 index, 1 FK tambahan, 5 `CHECK`. Seluruhnya penambahan. Migration **dibuat, belum diterapkan** ke devYoga maupun database lain |
| Keamanan/Auth | `NOT APPLICABLE`. Kolom sensitif (`ReasonNote`, `CallReason`, `DeclineReason`, `Reason`) ditandai di komentar model sesuai kamus data bagian 27 |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (sesudah source) | 0 galat, 603 peringatan; nol peringatan dari berkas task ini | `PASS` | Keluaran build, 1 menit 29 detik |
| `dotnet build ... -p:RunAnalyzers=False` (sesudah migration dipangkas) | 0 galat | `PASS` | Keluaran build, 1 menit 31 detik |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | Snapshot sama dengan model; drift tidak akan terbawa migration berikutnya |
| `dotnet ef migrations script 20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix AddSchedulingAttendanceAmendmentA52` | 6 `CREATE TABLE`, 19 `CREATE INDEX`, 4 `ADD` kolom, 1 FK, 1 baris riwayat; tidak ada `DROP`/`UPDATE`/tabel lain | `PASS` | Skrip SQL di scratchpad sesi |
| Verifikasi drift baca-saja ke devYoga (`default_transaction_read_only=on`) | 147 operasi drift sudah sesuai skema devYoga; 0 perubahan nyata tertunda; kedua migration SDM 11 November tercatat di riwayat | `PASS` | Keluaran skrip `verify-drift.js` |
| Jumlah definisi tabel di snapshot | 995 → 1003 (6 A52 + 2 tabel drift yang sudah ada di devYoga) | `PASS` | Hitung `b.ToTable(` pada `HEAD` dan working tree |
| Pemindaian rahasia pada berkas yang berubah | Tidak ditemukan | `PASS` | Pencarian pola password/connection string/key |
| Uji kering migration naik → turun → `ROLLBACK` di devYoga | — | `NOT RUN` | Eksekusi database tidak diberi wewenang pada task ini |

Uji manual: `NOT APPLICABLE`.

**Tidak dijalankan:** penerapan dan uji kering migration ke devYoga — menunggu instruksi
terpisah pengguna, sesuai `CLAUDE.md` dan `DATABASE_RULES.md`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `Up()` hanya `CREATE TABLE`/`ADD COLUMN`/index/FK | Terpenuhi | Ringkasan skrip SQL (bagian 5); `CHECK` termasuk definisi `CREATE TABLE` |
| Seluruh FK `Restrict` | Terpenuhi | Configuration baru dan `HrdAttendanceCorrectionRequestConfiguration.cs`; skrip SQL `ON DELETE RESTRICT` |
| Unique index `HrdOnCallBackup` tersaring `IsDelete = false` | Terpenuhi | `HrdOnCallBackupConfiguration.cs`; skrip SQL `CREATE UNIQUE INDEX ... WHERE "IsDelete" = FALSE` |
| `Down()` membalik seluruhnya | Terpenuhi menurut pemeriksaan isi; belum dijalankan | `Down()` berisi lepas FK, hapus 6 tabel, hapus index, hapus 4 kolom |
| Pemrosesan kehadiran tanggal lama menghasilkan status yang sama sebelum dan sesudah migration | **Belum terpenuhi** | Butuh migration diterapkan; nilai bawaan 240/480 sama dengan konstanta kode sekarang (`AttendanceProcessingService.cs:20-21`) |
| DoD umum: build lulus, tanpa angka bisnis tertanam baru, kolom sensitif tidak masuk log | Terpenuhi | Bagian 5; nilai bawaan berasal dari desain 35.7/35.8 dan dapat diubah lewat setelan/master |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 603 peringatan build yang sudah ada (komentar XML di berkas lain); nol dari berkas task ini |
| Masalah yang diketahui | **Drift snapshot migration.** `20261111020000_AddHrdNotificationAndSuspensionAccess` dan `20261111040000_ResolveHrdSchemaResidualDrift` (branch SDM lain) ditulis tanpa memperbarui `ApplicationDbContextModelSnapshot.cs`. `migrations add` pertama menghasilkan 199 operasi `Up`, 169 di antaranya drift. Atas pilihan pengguna ("Pangkas ke A52 + verifikasi DB"), drift diverifikasi baca-saja sudah sesuai devYoga, lalu `Up()`/`Down()` dipangkas menjadi operasi A52, sedangkan snapshot dibiarkan menyerap drift. Berkas asli sebelum dipangkas disimpan di scratchpad sesi |
| Risiko tersisa | (1) Server dev bersama dan database lain belum diperiksa; bila ada yang belum menerapkan kedua migration SDM 11 November, perbedaannya tidak lagi terlihat oleh `migrations add`. (2) Selisih desain `QBE-SVC-001`: `02-backend-architecture.md` 35.4.3 menulis `HolidayController` dan `AttendanceDeviceController` memakai `ApplicationDbContext` langsung; untuk kode baru wajib lewat Module Service — dikoreksi saat `BE-HRD-346`/`347`. (3) Nama tabel `HrdSchedulingSettings` (jamak) pada kode lama tidak ditiru; tabel baru memakai nama tunggal sesuai kontrak engineering |
| Perubahan sampingan | `NONE` — perubahan snapshot adalah keluaran migration yang diizinkan pengguna |
| Interupsi | `NONE` |
| Status Git | 21 berkas source/migration task ini (8 diubah, 14 baru termasuk 2 berkas migration); 17 berkas blueprint dari pass sebelumnya. Tidak ada stage atau commit |
| Langkah berikutnya | (a) Bila diizinkan: uji kering migration di devYoga (naik → turun → `ROLLBACK`), lalu penerapan. (b) `BE-HRD-335` — konstanta nilai dan penanda hak akses |
