# Laporan Perubahan Backend — `BE-HRD-349`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-349` |
| Judul | Laporan kehadiran (tujuh laporan) dengan cakupan unit |
| Slice | Amandemen `A52` — `EPIC HRD-114`, gelombang `MVP-68` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B87 |
| Trace | `FR-PK-017`; `HRD-DEC-206`, `HRD-ASM-79` (dengan `HRD-DEC-197`); `02-backend-architecture.md` 35.4.2; `api-contract.md` "Attendance Report"; `validation-matrix.md` `VR-PK-27`, `VR-PK-28`; `permission-audit-matrix.md` 30.1 (`AttendanceReport : Read`, penanda `ReadAllUnits`); `AT-HRD-PK-39` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-336` 🟡 (`HrdUnitScopeService` — bagian yang dipakai sudah ada), `BE-HRD-344` ✅ |
| Klasifikasi | `HEAVY` — skor 8: satu repository (0), sekitar 20 berkas diperiksa (2), 4 berkas source baru + `Program.cs` (1), baca lintas lima sumber data (2), hak akses dan cakupan unit (2), privasi data sakit (1), tanpa schema (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services/AttendanceReportService.cs,Controllers/AttendanceReportController.cs,DTOs/AttendanceReportDtos.cs,AttendanceExplicitPermissions.cs}`, `Program.cs` (satu registrasi DI); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`) + perubahan `BE-HRD-344` s.d. `348` yang belum di-commit |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — harness 111/111 (termasuk kontrol negatif); tujuh laporan, `AT-HRD-PK-39`, `VR-PK-27`, `VR-PK-28` lulus lewat API devYoga |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru |
| Keberlakuan | `NEW CODE` (service, controller, DTO, penanda baru), hanya baca |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001` |
| Tidak berlaku | `QBE-LOG-001` untuk tampilan (matriks 30.1: GET laporan tidak dicatat; log ekspor milik `BE-HRD-350`), `QBE-CODE-*`, `QBE-TXN-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Tidak ada endpoint laporan kehadiran M12 (`HRD-TF-061`) | Kepala Unit, Kepala Bidang, dan Admin HRD tidak dapat melihat tujuh laporan |
| Penanda `AttendanceReport : ReadAllUnits` belum terdaftar (dipindah dari `BE-HRD-335`) | Cakupan baca semua unit hanya berlaku bagi superadmin |

---

## 2. Proses bisnis

**Cakupan (`HRD-DEC-206` butir 2, `VR-PK-27`).** Data mengikuti cakupan baca pengguna dari `HrdUnitScopeService`:
Kepala Unit melihat unitnya, Kepala Bidang melihat unit di bawahnya, pemegang `AttendanceReport : ReadAllUnits` (Admin HRD,
dan peran `HRD-ASM-79` bila diberi) melihat semua unit. Tanpa filter unit, laporan memuat seluruh unit dalam cakupan.
Filter unit di luar cakupan, atau pengguna berizin tanpa cakupan sama sekali, ditolak `403` "Anda tidak berwenang melihat
data unit ini". Pegawai biasa tidak memegang `AttendanceReport : Read`, sehingga sudah ditolak di tingkat hak akses.

**Data sakit (`HRD-DEC-206` butir 5).** Cuti berkategori `Sick` hanya ditulis "Sakit" — nama jenis cutinya pun tidak.
`AttendanceReportRow` tidak punya kolom alasan, lampiran, atau diagnosis.

**Status periode (butir 6).** `PeriodState = Closed` bila setiap tanggal rentang tercakup periode kehadiran `Closed`
yang berlaku (periode tanpa unit berlaku untuk semua; dengan filter unit, periode unit itu juga dihitung); selain itu
`Open`, yang berarti data masih dapat berubah (termasuk "Belum Lengkap").

**Kolom per laporan.** Blueprint hanya menyebut nama ketujuh laporan dan `AttendanceReportRow`. Kolom berikut disusun dari
data yang ada dan dikirim dalam `Columns` setiap respons, sehingga layar dan ekspor `BE-HRD-350` memakai definisi yang sama:

| `reportCode` | Sumber | Satu baris = | Kolom (selain Kode Pegawai, Nama, Unit) | `status` menyaring |
| --- | --- | --- | --- | --- |
| `shift` | Shift pada roster `Published`/`Closed` | Satu shift | Tanggal, Shift, Mulai, Selesai, Roster, Status | Status shift |
| `attendance` | `HrdAttendanceDaily` | Satu rekap harian | Tanggal, Shift, Masuk, Pulang, Status, Terlambat, Pulang Cepat, Kerja (menit) | Status harian |
| `leave` | Cuti `Approved`/`Taken`/`Completed` | Satu permohonan | Mulai, Selesai, Hari, Jenis (sakit → "Sakit"), Status | Status cuti |
| `lateness` | Rekap harian dengan menit terlambat > 0 | Satu rekap | Tanggal, Shift, Jadwal Masuk, Masuk, Terlambat | Status harian |
| `overtime` | Lembur yang sudah disetujui (`ApprovedForWork` s.d. `PostedToPayroll`) | Satu permohonan | Tanggal, Rencana Mulai/Selesai, Diajukan, Disetujui (menit), Status | Status lembur |
| `absence` | Rekap `Absent`/`NoScan`/`IncompletePunch`/`Incomplete` + hari cuti | Satu pegawai per tanggal | Tanggal, Keterangan (Alpa, Tidak Ada Scan, Belum Lengkap, Sakit, Cuti) | `Absent`, `NoScan`, `IncompletePunch`, `Sick`, `Leave` |
| `unit-summary` | Gabungan rekap, cuti, lembur | Satu unit | Unit, Jumlah Pegawai, Hadir, Terlambat, Alpa, Tidak Ada Scan, Belum Lengkap, Cuti, Sakit, Lembur (menit) | — |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-206`, `HRD-ASM-79`; `api-contract.md` Attendance Report; `validation-matrix.md` `VR-PK-27`, `-28`; `permission-audit-matrix.md` 30.1; `acceptance-test-matrix.md` `AT-HRD-PK-37`–`39`; `03-frontend-architecture.md` M12
- `HrdUnitScopeService.cs` (`GetReadScopeAsync`, konstanta penanda), `SchedulingExplicitPermissions.cs` (pola penanda dan catatan pemindahan `ReadAllUnits`)
- Model `HrdAttendanceDaily`, `HrdAttendancePeriod`, `WfpLeaveRequest`, `MstLeaveType`, `WfpOvertimeRequest`, `TrxShiftAssignment`/`TrxRosterAssignment`/`TrxRosterPeriod`, `MstShift`, `MstOrganizationUnit`, `MstWorkforceProfile`; `LeaveRequestValueConstants`, status lembur di `WfpOvertimeRequest`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceReportService.cs` | **Baru.** `GetReportAsync` dan `BuildAsync(paging)`; tujuh pembangun laporan terproyeksi (paginasi di basis data untuk shift/attendance/lateness/leave/overtime; di memori untuk absence dan unit-summary yang menggabungkan sumber); cakupan, `VR-PK-27`, `VR-PK-28`, `PeriodState`; `AttendanceReportServiceResult` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Controllers/AttendanceReportController.cs` | **Baru.** `GET /{reportCode}`; `AccessController` `HUMAN_RESOURCE_ATTENDANCE` / `AttendanceReport`; `AccessPermission("AttendanceReport", "Read")` |
| `Areas/Corporate/HumanResource/AttendanceManagement/DTOs/AttendanceReportDtos.cs` | **Baru.** Query, `AttendanceReportResponse` (+ `Columns`, `PeriodState`, paginasi), `AttendanceReportRow` |
| `Areas/Corporate/HumanResource/AttendanceManagement/AttendanceExplicitPermissions.cs` | **Baru.** Satu-satunya deklarasi penanda `AttendanceReport : ReadAllUnits` |
| `Program.cs` | Satu baris `AddScoped<AttendanceReportService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /{reportCode}` sesuai kontrak; respons `PagedResult` diperluas dengan `PeriodState` (kontrak) serta `ReportCode`, `ReportName`, rentang, dan `Columns` |
| Database | Tanpa migration; hanya baca |
| Keamanan/Auth | Resource baru `AttendanceReport : Read` dan penanda `ReadAllUnits` (terdaftar saat startup, keduanya belum diberikan ke jabatan mana pun) |
| Privasi | Tidak ada kolom alasan, lampiran, diagnosis; nama jenis cuti sakit pun diganti "Sakit" |

### 3.4 Delta dan keputusan desain

| Butir | Implementasi | Alasan |
| --- | --- | --- |
| Kolom tiap laporan tidak dirinci di blueprint | Kolom pada bagian 2 | Disusun dari data yang ada; dikirim dalam `Columns` agar layar dan ekspor identik. **Perlu dikonfirmasi pemilik modul** bila ada kolom wajib PRD M12 yang belum tercakup |
| Filter `status` | Arti per laporan (bagian 2); diabaikan di `unit-summary` | Kontrak menyebut `status` tanpa rincian |
| `PeriodState` tanpa filter unit | Hanya periode tanpa unit yang dihitung | Tanpa unit tertentu, periode per unit tidak dapat mewakili seluruh laporan |
| Label status shift, cuti, lembur | Nilai mentah (`Published`, `Approved`, `Realized`); label Indonesia hanya untuk status harian dan kategori ketidakhadiran | Belum ada sumber label resmi untuk ketiganya; layar dapat memetakan |
| `SchedulingExplicitPermissions.cs` menyebut penanda `ReadAllUnits` "belum ditulis" | Tidak diubah | Kalimatnya tetap benar (penanda tidak ditulis di berkas itu) dan menunjuk berkas modul Attendance |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/attendance/reports` — Tag `Corporate / Human Resource / Attendance Management / Attendance Report`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{reportCode}` | `AttendanceReport : Read` | Query `organizationUnitId`, `fromDate`, `toDate` (default bulan berjalan), `status`, `pageNumber`, `pageSize` (maks. 100) | `AttendanceReportResponse` | `200`, `400` (`VR-PK-28`, tanggal terbalik), `401`, `403` (izin; `VR-PK-27`), `404` (kode tak dikenal) |

`GET /{reportCode}/export` dikerjakan `BE-HRD-350`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap; tidak ada peringatan dari berkas baru) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (111 uji: 107 sebelumnya + 4 baru di `AttendanceReportTests.cs`; hierarki RS → Bidang → {ICU, HCU}, Farmasi) | 111/111 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\4222eeca-aa89-4652-b697-eb38df816c85\scratchpad\scopetest` |
| — `AT-HRD-PK-39` (contoh `HRD-DEC-206`): Rina sakit 5 Feb (jenis "Cuti Sakit Dengan Surat Dokter", alasan berisi diagnosis) | `absence`: "Rina — 5 — Sakit"; `leave`: jenis "Sakit"; respons tidak memuat diagnosis, alasan, atau nama jenis cuti sakit; `AttendanceReportRow` tanpa properti alasan/lampiran/diagnosis | `PASS` | `Ketidakhadiran_dan_cuti_menulis_sakit_saja` |
| — Cakupan | Sari (Kepala Unit ICU) hanya ICU; Dodi (Bidang) ICU+HCU; Admin HRD (penanda `ReadAllUnits`) dan superadmin semua unit; Sari→HCU, Sari→Farmasi, Dodi→Farmasi, Budi (tanpa jabatan) dengan/tanpa unit → 403 pesan kontrak tanpa data | `PASS` | `Cakupan_unit_mengikuti_pengguna` |
| — Isi tujuh laporan | Shift hanya roster terbit; attendance 4 baris dan saring status; lateness 15 menit dengan jam masuk; overtime hanya yang disetujui, tanpa alasan; unit-summary ICU (2 pegawai, 2 hadir, 1 terlambat, 1 tidak ada scan, 1 belum lengkap, 2 hari cuti, 1 sakit, 120 menit lembur); paginasi dan `BuildAsync` tanpa paging | `PASS` | `Isi_tujuh_laporan` |
| — `VR-PK-28`, kode, `PeriodState` | 92 hari sah, 93 hari 400 pesan kontrak; tanggal terbalik 400; kode tak dikenal 404; default bulan berjalan; `Open` → `Closed` sesudah periode Februari ditutup; rentang melewati Maret `Open` | `PASS` | `Rentang_kode_dan_status_periode` |
| Kontrol negatif: cuti sakit memakai nama jenisnya, dan pemeriksaan cakupan dimatikan, backend dibangun ulang | Tiga uji yang menjaganya gagal. Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 111/111 lagi | `PASS` | Keluaran `dotnet test` |
| Startup Development | Backend menyala tanpa galat registri; resource `AttendanceReport` terdaftar dengan aksi `Read` dan `ReadAllUnits` | `PASS` | `be349.log`, `SysActionAccess` |
| API devYoga (16 November 2026) — cuti sakit uji disisip: jenis `UJI-A52-CS` "UJI-A52 Cuti Sakit Dengan Surat Dokter" (`Sick`), permohonan `UJI-A52-LV-349` Raka Firmansyah ICU 2 Okt 2026 `Approved` dengan alasan "UJI-A52 diagnosis palsu Z99 demam" | Tersimpan | — | `at-349.mjs` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig` |
| — Tanpa sesi | `401` | `PASS` | `at-log.jsonl` (entri "349 …") |
| — Superadmin, ICU 1–7 Okt 2026 (periode `ATP-202610-0001` ditutup) | `absence`: Bagas 2 Okt Alpa, **Raka 2 Okt Sakit**, Raka 3 Okt Alpa, Arif 4 Okt Alpa; `leave`: Raka "Sakit"; `attendance` 4 baris; `unit-summary` ICU 4 pegawai, 1 hadir, 3 alpa, 1 sakit; `lateness` dan `overtime` kosong; semuanya `PeriodState = Closed` | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-39` | Respons `absence` dan `leave` tidak memuat "Z99", "diagnosis", "demam", atau "Surat Dokter" | `PASS` | `at-log.jsonl` |
| — `shift` ICU Nov 2026 | 6 shift dari `ROST-202611-0004`, `-0005`, `UJI-A52-MYSCH`; roster Draft `ROST-202611-0003` tidak tampil; `Open` | `PASS` | `at-log.jsonl` |
| — `VR-PK-28` (93 hari) / kode `payroll` | `400` pesan kontrak / `404` | `PASS` | `at-log.jsonl` |
| — `gilang.mahendra` tanpa izin | `403` | `PASS` | `at-log.jsonl` |
| — `VR-PK-27`: izin `AttendanceReport : Read` diberikan **sementara** ke jabatan gilang (atas persetujuan pengguna); gilang tanpa penempatan berjabatan meminta ICU / tanpa filter | Keduanya `403` "Anda tidak berwenang melihat data unit ini" | `PASS` | `at-log.jsonl` |
| — Izin dikembalikan | Set 32 butir dikirim ulang, dibaca identik; gilang kembali `403` | `PASS` | `at-log.jsonl`; salinan `at-349-policy-backup.json` |
| Uji lintas unit dengan Kepala Unit sungguhan (ICU boleh, HCU 403) lewat API | Tidak dijalankan: butuh akun Kepala Unit berizin beserta sandinya; perilaku ini terbukti di harness | `NOT RUN` | — |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kolom sakit hanya "Sakit" | Terpenuhi | `AT-HRD-PK-39` harness dan devYoga |
| Tidak ada lampiran/diagnosis | Terpenuhi | DTO tanpa kolom; pencarian teks respons |
| Rentang > 92 hari 400 | Terpenuhi | `VR-PK-28` harness dan devYoga |
| Uji 403 lintas unit | Terpenuhi di harness (Kepala Unit/Bidang); devYoga membuktikan 403 `VR-PK-27` untuk cakupan kosong | Bagian 5 |
| Risiko kartu: kueri terproyeksi dan berpaginasi | Terpenuhi untuk lima laporan; `absence` dan `unit-summary` menggabungkan di memori (dibatasi 92 hari dan cakupan) | Bagian 3.2 |
| DoD umum | Terpenuhi — 92 hari adalah batas teknis yang tercatat di matriks validasi | Bagian 3–5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) `AttendanceReport : Read` dan penanda `ReadAllUnits` belum diberikan ke jabatan mana pun; Kepala Unit/Bidang perlu `Read`, Admin HRD perlu `Read` + `ReadAllUnits` lewat Role Access. (2) **Temuan data devYoga:** rekap harian Nadia Putri dan Dewi Anggraeni tidak punya `OrganizationUnitId`, sehingga hanya terlihat oleh pembaca semua unit, bukan oleh Kepala Unit mereka. Dugaan (belum ditelusuri): unit rekap diambil dari penempatan saat pemrosesan, dan penempatan utama keduanya tanpa unit. (3) Kolom per laporan perlu dikonfirmasi pemilik modul (bagian 3.4) |
| Risiko tersisa | `absence` dan `unit-summary` dihitung di memori; untuk cakupan semua unit dengan 92 hari, ukurannya sebanding dengan jumlah rekap ketidakhadiran dan cuti di rentang itu |
| Perubahan sampingan | `NONE`. Izin Role Access devYoga yang diubah untuk uji sudah dikembalikan identik |
| Interupsi | `NONE` |
| Status Git | 4 berkas source baru + `Program.cs`; laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-344` s.d. `348` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-350` — ekspor `.xlsx` dan log ekspor (memakai `BuildAsync(paging:false)` dan `Columns`) |

Data uji devYoga baru (bertanda `UJI-A52`, disisip ke basis data): jenis cuti `UJI-A52-CS` (kategori `Sick`) dan
permohonan cuti `UJI-A52-LV-349` (Raka Firmansyah, ICU, 2 Okt 2026, `Approved`, alasan berisi diagnosis palsu).
