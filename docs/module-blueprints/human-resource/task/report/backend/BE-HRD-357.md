# Laporan Perubahan Backend — `BE-HRD-357`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-357` |
| Judul | Dashboard kehadiran (`POST-MVP`) |
| Slice | Amandemen `A52` — `EPIC HRD-119`, gelombang `POST-MVP` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B88 |
| Trace | `FR-PK-030`, `FR-PK-031`; `HRD-DEC-208` (dengan `HRD-DEC-197`, `HRD-DEC-207`, `HRD-DEC-099`/`100` untuk `W6`/`W7`); `02-backend-architecture.md` 35.3.4, 35.4.2, 35.4.3; `api-contract.md` "Attendance Dashboard"; `validation-matrix.md` `VR-PK-27`; `permission-audit-matrix.md` 30.1, 30.2; `AT-HRD-PK-40`, `AT-HRD-PK-41` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-336` ✅ (`HrdUnitScopeService`) |
| Klasifikasi | `HEAVY` — skor 8: satu repository (0), sekitar 25 berkas diperiksa (2), 3 berkas source baru + `Program.cs` (1), baca lintas enam sumber data (2), hak akses dan cakupan unit (2), privasi data sakit (1), tanpa schema (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services/AttendanceDashboardService.cs,Controllers/AttendanceDashboardController.cs,DTOs/AttendanceDashboardDtos.cs}`, `Program.cs` (satu registrasi DI); proyek uji HR (`AttendanceDashboardTests.cs` baru, `A52PermissionRegistryTests.cs`, `AcceptanceTestIndex.md`); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`), working tree bersih sebelum task |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — proyek uji HR 186/186 (termasuk kontrol negatif); `AT-HRD-PK-40` dan `AT-HRD-PK-41` lulus lewat API devYoga dengan superadmin dan akun non-superadmin |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru |
| Keberlakuan | `NEW CODE` (service, controller, DTO), hanya baca |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001` |
| Tidak berlaku | `QBE-LOG-001` (matriks 30.1: `GET attendance/dashboard*` tidak dicatat), `QBE-CODE-*`, `QBE-TXN-001`, `QBE-DB-*` (tanpa schema) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Tidak ada dashboard kehadiran gabungan (`HRD-TF-058`: ringkasan hanya per modul) | Kepala Unit, Kepala Bidang, dan Admin HRD tidak dapat melihat kehadiran hari ini, pegawai bertugas, keterlambatan, ketidakhadiran, dan jadwal yang belum lengkap di satu tempat |
| Resource `AttendanceDashboard : Read` belum ada | Hak akses dashboard tidak dapat diberikan lewat Akses Role |

---

## 2. Proses bisnis

**Pengguna dan cakupan (`HRD-DEC-208` butir 1, 3; `VR-PK-27`).** Cakupan sama dengan laporan: Kepala Unit membaca unitnya,
Kepala Bidang membaca unit di bawahnya, pemegang penanda `AttendanceReport : ReadAllUnits` (Admin HRD) membaca semua unit.
Unit di luar cakupan, atau pengguna berizin tanpa cakupan sama sekali, ditolak `403` "Anda tidak berwenang melihat data unit
ini" — untuk kartu maupun rinciannya. Pegawai biasa tidak memegang `AttendanceDashboard : Read`, sehingga sudah ditolak di tingkat
hak akses.

**Dihitung saat diminta (butir 2).** Tidak ada snapshot, cache, atau pekerjaan latar. Setiap permintaan menghitung dari rekap harian,
cuti, roster, dan siaga saat itu dan mengembalikan `CalculatedAt` (jam hitung). Tombol **Muat Ulang** cukup memanggil ulang.

**Setiap angka dapat ditelusuri (`FR-PK-031`).** Kartu dan rincian memakai **satu** fungsi perhitungan; angka kartu adalah jumlah
baris daftar yang sama, sehingga untuk tanggal, unit, dan jam hitung yang sama `Count` = `TotalData` rincian.

**Isi lima kartu (butir 4).** Satu kategori per pegawai per tanggal, urutan: tugas belajar → cuti/sakit (bila tidak hadir) → dinas luar
→ izin → status harian. Hari libur/off tidak dihitung terjadwal.

| `cardCode` | Angka (`metric`, angka pertama = angka utama) | Sumber |
| --- | --- | --- |
| `attendance-today` | Terjadwal (`scheduled`), Hadir, Terlambat, Belum Hadir, Tidak Ada Scan, Cuti, Sakit, Izin, Dinas Luar, Tugas Belajar | Rekap harian tanggal itu + cuti `Approved`/`Taken`/`Completed` yang mencakup tanggal itu |
| `on-duty` | Bertugas (`total`), Shift Berjalan (`shift`), Siaga Utama (`onCall`) | Shift roster `Published`/`Closed` yang berjalan pada jam acuan (pengganti Ganti Petugas ditandai "Petugas Pengganti"); siaga `Primary` yang jendelanya mencakup jam acuan |
| `lateness` | Terlambat (`late`) | Rekap dengan menit terlambat > 0 (sudah sesudah toleransi; dispensasi menyusul `BE-HRD-354`), terlama di atas |
| `absence` | Tidak Hadir (`total`), Tidak Ada Scan, Belum Lengkap, Cuti, Sakit, Izin, Dinas Luar, Tugas Belajar, Alpa | Sama dengan kartu pertama; Alpa hanya bila tanggal tercakup periode kehadiran `Closed` |
| `incomplete-schedule` | Belum Lengkap (`total`), Roster Belum Terbit, Peringatan Kuota (`W6`), Peringatan Kualifikasi (`W7`) | Roster belum terbit periode berjalan/berikutnya; `W6`/`W7` dari ringkasan pemeriksaan roster terbit |

**Data sakit (butir 5).** Sakit hanya "Sakit"; `DashboardDetailRow` tidak punya kolom alasan, lampiran, atau diagnosis, dan nama jenis
cuti tidak dikirim.

**Contoh (uji harness).** Pukul 09.00 10 Februari 2027 Sari membuka dashboard ICU: Terjadwal 13, Hadir 3, Terlambat 1 (Tono 12 menit),
Belum Hadir 1, Tidak Ada Scan 2, Cuti 1, Sakit 1, Izin 1, Dinas Luar 1, Tugas Belajar 1; Pegawai Bertugas: Ani (Shift), Bayu (Petugas
Pengganti), dokter siaga utama. Dodi (Kepala Bidang) melihat juga HCU; Sari meminta HCU → 403.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-208`, `HRD-DEC-197`, `HRD-DEC-099`; `04-prd-to-mvp.md` `EPIC HRD-119`; `02-backend-architecture.md` 35.3.4, 35.4.2–35.4.3, 35.9.4; `api-contract.md` Attendance Dashboard; `permission-audit-matrix.md` 30.1–30.2; `validation-matrix.md` `VR-PK-27`; `acceptance-test-matrix.md` `AT-HRD-PK-40`, `-41`; `03-frontend-architecture.md` M01
- `AttendanceReportService.cs`/`Controller`/DTO (pola cakupan, `ApiResponse`, DTO berpaginasi), `HrdUnitScopeService.cs`, `AttendanceExplicitPermissions.cs`
- `AttendanceProcessingService.cs` (penentuan status harian), `AttendanceValueConstants.cs`, `EarlyLeavePermissionService.cs` (kapan rekap ditautkan ke izin), `RosterValidationService.cs`/`RosterPeriodService.cs` (penyimpanan `W6`/`W7` di `ValidationSummaryJson`), `ShiftReplacementService.cs` (pengganti menulis langsung ke shift), `OnCallAssignmentService.cs`, `MyScheduleSelfService.cs` (status siaga yang tidak aktif)
- Model `HrdAttendanceDaily`, `HrdAttendancePeriod`, `HrdEarlyLeavePermission`, `HrdStudyAssignment`, `WfpLeaveRequest`, `TrxShiftAssignment`, `TrxRosterPeriod`, `TrxOnCallAssignment`, `MstOnCallType`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceDashboardService.cs` | **Baru.** `GetDashboardAsync`, `GetCardDetailAsync`; satu `ComputeAsync` untuk kartu dan rincian; kategori per pegawai; cakupan `VR-PK-27`; saringan unit di basis data; `AttendanceDashboardServiceResult<T>` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Controllers/AttendanceDashboardController.cs` | **Baru.** `GET /`, `GET /cards/{cardCode}/details`; `AccessController` `HUMAN_RESOURCE_ATTENDANCE` / `AttendanceDashboard`; `AccessPermission("AttendanceDashboard", "Read")` |
| `Areas/Corporate/HumanResource/AttendanceManagement/DTOs/AttendanceDashboardDtos.cs` | **Baru.** Query, `AttendanceDashboardResponse` (kartu + angka + `CalculatedAt` + `OnDutyReferenceAt`), `AttendanceDashboardDetailResponse : PagedResult<DashboardDetailRow>`, `DashboardDetailRow` |
| `Program.cs` | Satu baris `AddScoped<AttendanceDashboardService>()` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AttendanceDashboardTests.cs` | **Baru.** 6 uji (`AT-HRD-PK-40`, `-41`, isi kartu, pegawai bertugas, jadwal belum lengkap, kode/metric/paginasi) |
| `Tests/QuilvianSystemBackend.HumanResourceTests/A52PermissionRegistryTests.cs` | `AttendanceDashboardController` masuk daftar validator registri dan uji 403; `AttendanceDashboard : Read` masuk daftar resource |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AcceptanceTestIndex.md` | `AT-HRD-PK-40`, `-41` dipetakan ke uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint sesuai kontrak `v26`. Rincian memakai bentuk `PagedResult` yang diperluas dengan kode kartu, metric, label, tanggal, dan jam hitung |
| Database | Tanpa migration; hanya baca |
| Keamanan/Auth | Resource baru `AttendanceDashboard : Read` (terdaftar saat startup, belum diberikan ke jabatan mana pun); cakupan dari `HrdUnitScopeService` |
| Privasi | Tidak ada kolom alasan, lampiran, diagnosis, atau nama jenis cuti; sakit hanya "Sakit" |

### 3.4 Delta dan keputusan desain

Blueprint menyebut isi kartu tetapi tidak merinci aturan hitungnya. Keputusan berikut diambil dari data yang ada dan **perlu
dikonfirmasi pemilik modul** bila berbeda dengan maksud `HRD-DEC-208`:

| Butir | Implementasi | Alasan |
| --- | --- | --- |
| Nama `metric` dan isi rincian | Tabel bagian 2; `metric` kosong = angka utama; metric tak dikenal `400` dengan daftar pilihan | Kontrak hanya menyebut "Query sama + `metric`" |
| "Izin" | Rekap yang tertaut izin pulang cepat berstatus `Approved` | Satu-satunya izin di data. Rekap sudah tertaut sejak izin **diajukan**, sehingga status izin diperiksa |
| "Dinas Luar" | Status harian `BusinessTrip` dan `OnDuty` (surat tugas dinas luar) | Dua status yang sudah dihasilkan pemrosesan |
| "Tugas Belajar" | Status harian `StudyAssignment` | Status ini dihasilkan pemrosesan sesudah `BE-HRD-355`; sampai itu angkanya 0 |
| Cuti/Sakit pada kartu pertama | Cuti dan Sakit dipisah (kartu pertama juga memuat "Sakit") | Keputusan menyebut "Cuti" di kartu pertama dan memisahkan "Sakit" di kartu Ketidakhadiran; pemisahan menjaga satu kategori per pegawai |
| Cuti tanpa rekap harian | Tetap dihitung terjadwal | Pemrosesan tidak menghasilkan status `Leave`; contoh keputusan menghitung cuti di dalam "terjadwal" |
| `Absent` sebelum periode ditutup (data lama sebelum `BE-HRD-343`) | Ditampilkan "Tidak Ada Scan" | `HRD-DEC-208`: Alpa hanya dari periode yang sudah ditutup |
| Belum Lengkap, Alpa, Belum diproses pada kartu pertama | Hanya masuk "Terjadwal"; rinciannya di kartu Ketidakhadiran | Tidak termasuk daftar isi kartu pertama |
| Keterlambatan vs "Terlambat" kartu pertama | Kartu Keterlambatan memuat setiap rekap dengan menit terlambat > 0 (termasuk "Belum Lengkap" yang terlambat); kartu pertama memuat status `Late` | Kartu pertama satu kategori per pegawai; kartu Keterlambatan memuat menitnya |
| Pegawai Bertugas untuk tanggal selain hari ini | Jam acuan = jam dinding yang sama pada tanggal itu (`OnDutyReferenceAt`) | Kartu ini bergantung jam; tanggal lain tetap memberi gambaran yang konsisten |
| Shift siaga (`IsOnCall`) dan siaga cadangan | Tidak dihitung shift; siaga dari `TrxOnCallAssignment` `Primary` saja, status selain `Cancelled`/`Rejected`/`Completed`, pada roster terbit | Menghindari hitung ganda; keputusan menyebut "siaga utama" |
| "Periode berjalan atau berikutnya" | Roster yang berakhir pada/sesudah tanggal dan mulai paling lambat akhir bulan berikutnya; `W6`/`W7` untuk shift dari tanggal itu sampai batas yang sama | Periode roster bulanan |
| Unit tanpa roster sama sekali | Tidak dihitung "Roster Belum Terbit" | Unit jam kantor memakai jadwal tetap |
| `W6`/`W7` | Dibaca dari `TrxRosterPeriod.ValidationSummaryJson` (hasil pemeriksaan saat terbit); ringkasan rusak dilewati | Satu-satunya tempat peringatan slot disimpan |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/attendance/dashboard` — Tag `Corporate / Human Resource / Attendance Management / Attendance Dashboard`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | `AttendanceDashboard : Read` | Query `date` (default hari ini WIB), `organizationUnitId` (opsional) | `AttendanceDashboardResponse` | `200`, `401`, `403` (izin; `VR-PK-27`) |
| `GET` | `/cards/{cardCode}/details` | `AttendanceDashboard : Read` | Query sama + `metric`, `pageNumber`, `pageSize` (maks. 100) | `AttendanceDashboardDetailResponse` (`PagedResult<DashboardDetailRow>`) | `200`, `400` (metric tak dikenal), `401`, `403`, `404` (kartu tak dikenal) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat; tidak ada peringatan dari berkas baru | `PASS` | Keluaran build |
| Proyek uji HR (`dotnet test`, SQLite di memori) | 186/186 lulus (177 sebelumnya + 6 uji dashboard + 3 kasus baru di uji registri) | `PASS` | Keluaran `dotnet test` |
| — `AT-HRD-PK-40` / uji (a)(c): ICU pukul 09.00 | 27 angka pada lima kartu masing-masing sama dengan `TotalData` dan jumlah baris rinciannya; `CalculatedAt` = jam hitung | `PASS` | `AT_HRD_PK_40_angka_kartu_sama_dengan_rincian` |
| — Isi kartu pertama dan Ketidakhadiran | Angka bagian 2; izin yang masih diajukan tetap "Hadir"; pegawai jadwal tetap yang sakit masuk "Sakit"; libur tidak terjadwal; Alpa 0 sebelum tutup, 1 sesudah periode ICU ditutup | `PASS` | `Kehadiran_hari_ini_dan_ketidakhadiran` |
| — Privasi | Respons dashboard dan rincian tidak memuat diagnosis, alasan cuti/izin, atau nama jenis cuti sakit; DTO tanpa kolom alasan/lampiran/diagnosis | `PASS` | sama |
| — Uji (d): Pegawai Bertugas | 09.00: Ani, Bayu (Petugas Pengganti), dokter siaga utama; shift dihapus, roster Draft, shift siaga, siaga cadangan dan dibatalkan tidak ikut. 02.00 11 Feb: shift malam Yudi. 10 Feb dibuka pukul 02.00 11 Feb: jam acuan 10 Feb 02.00 → malam Tono | `PASS` | `Pegawai_bertugas` |
| — Jadwal Belum Lengkap | Roster Draft Feb dan Maret ICU (April, Januari, dan roster terbit tidak); `W6` 12 Feb dan `W7` 15 Feb dengan nama shift; `W6` lampau dan `W1` tidak; Kepala Bidang melihat roster HCU juga | `PASS` | `Jadwal_belum_lengkap` |
| — Uji (b)(e), `AT-HRD-PK-41`, `VR-PK-27` | Sari → HCU/Farmasi/Bidang 403 tanpa data (kartu dan rincian); perawat tanpa jabatan 403 dengan/tanpa unit; Sari hanya ICU; Dodi ICU+HCU; Admin HRD (`ReadAllUnits`) dan superadmin semua unit | `PASS` | `AT_HRD_PK_41_cakupan_unit` |
| — Lapis hak akses | Kedua endpoint lolos `PermissionRegistryValidator`; `AttendanceDashboard : Read` terdaftar; pegawai tanpa kebijakan Akses Role ditolak, berkebijakan dan superadmin lolos | `PASS` | `A52PermissionRegistryTests` |
| — Kode, metric, paginasi | Kartu tak dikenal 404; metric tak dikenal 400 dengan pilihan; halaman 2 ukuran 5 dari 13 baris; tanggal default WIB | `PASS` | `Kode_kartu_metrik_dan_paginasi` |
| Kontrol negatif: (1) pemeriksaan cakupan dimatikan, (2) shift dihapus ikut dihitung, (3) Alpa tanpa periode ditutup, (4) angka kartu "Hadir" ditambah 1 | Empat uji yang menjaganya gagal (`AT_HRD_PK_41`, `Pegawai_bertugas`, `Kehadiran_hari_ini…`, `AT_HRD_PK_40`). Kode dipulihkan (dibandingkan byte demi byte), seluruh proyek uji 186/186 | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2444 identitas kanonik"; resource `AttendanceDashboard : Read` tersimpan di `SysActionAccess` | `PASS` | `be357.log` di scratchpad sesi; log uji |
| API devYoga (17 November 2026) — tanpa sesi | `401` | `PASS` | `at-log.jsonl` (entri "357 …") |
| — `AT-HRD-PK-40` superadmin ICU 2 Okt 2026 (periode `ATP-202610-0001` ditutup) | Terjadwal 2, Sakit 1 (Raka), Alpa 1 (Bagas) — sama dengan laporan `BE-HRD-349`; Roster Belum Terbit 1 (`ROST-202611-0003`); **semua angka = `TotalData` rincian** | `PASS` | `at-357.mjs`, `at-log.jsonl` |
| — `AT-HRD-PK-40` superadmin ICU dan semua unit hari ini (09.52 WIB) | Bertugas 2 (Gilang, Raka — Pagi UJI-A52 07.00–14.00); semua angka = rincian; jam hitung tampil | `PASS` | `at-log.jsonl` |
| — Privasi data sakit | Rincian Sakit ICU 2 Okt: "Raka Firmansyah — Sakit"; tidak memuat "Z99", "diagnosis", "demam", "Surat Dokter" | `PASS` | `at-log.jsonl` |
| — Kartu/metric tak dikenal | `404` / `400` dengan daftar pilihan | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-41`: `gilang.mahendra` (non-superadmin, tanpa izin) | Login 200; dashboard dan rincian `403` | `PASS` | `at-log.jsonl` |
| — `VR-PK-27`: izin `AttendanceDashboard : Read` diberikan **sementara** ke jabatan gilang (atas persetujuan pengguna), tanpa penempatan berjabatan | `403` "Anda tidak berwenang melihat data unit ini", dengan dan tanpa unit | `PASS` | `at-log.jsonl` |
| — Penempatan berjabatan uji "Kepala Bidang" Instalasi Rawat Inap (induk ICU) | ICU 2 Okt: angka sama dengan superadmin dan = rincian; IGD `403` (kartu dan rincian); tanpa filter hanya ICU | `PASS` | `at-log.jsonl` |
| — Pemulihan | Izin dikirim ulang (32 butir) dan dibaca identik; penempatan uji `IsActive=false`, `IsDelete=true`; gilang kembali `403` | `PASS` | `at-log.jsonl`; salinan `at-357-policy-backup.json` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Angka kartu = jumlah baris rincian (`FR-PK-031`, `AT-HRD-PK-40`) | Terpenuhi | Harness (27 angka) dan devYoga (superadmin + Kepala Bidang uji) |
| Lima kartu per cakupan, dihitung saat diminta, jam hitung tampil (`FR-PK-030`) | Terpenuhi | Bagian 2, 5 |
| Pegawai biasa 403 (`AT-HRD-PK-41`); Kepala Unit tidak dapat memilih unit lain | Terpenuhi | Harness dan devYoga |
| Shift malam pukul 02.00 masuk Pegawai Bertugas | Terpenuhi | `Pegawai_bertugas` |
| Sakit hanya "Sakit" | Terpenuhi | Harness dan devYoga |
| Risiko kinerja: dibatasi satu tanggal | Terpenuhi — semua kueri tersaring satu tanggal (atau jam acuan / jendela roster dua bulan) dan cakupan unit di basis data | Bagian 3.2 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) `AttendanceDashboard : Read` belum diberikan ke jabatan mana pun; Kepala Unit/Bidang dan Admin HRD perlu diberi lewat Role Access (Admin HRD juga `AttendanceReport : ReadAllUnits`). (2) Aturan hitung bagian 3.4 perlu dikonfirmasi pemilik modul. (3) Kartu dan rincian dihitung pada dua permintaan berbeda; pada pergantian shift atau saat job pemrosesan berjalan di antara keduanya, angka kartu dan rincian yang dibuka belakangan bisa berbeda. Bila perlu, kontrak dapat ditambah parameter jam hitung pada rincian (penyegaran kontrak, bukan task ini). (4) "Tugas Belajar" bernilai 0 sampai `BE-HRD-355` menghasilkan statusnya; pengurangan menit terlambat oleh dispensasi menyusul `BE-HRD-354` |
| Risiko tersisa | Rekap harian tanpa `OrganizationUnitId` (temuan `BE-HRD-349`: Nadia Putri, Dewi Anggraeni) hanya terlihat oleh pembaca semua unit |
| Perubahan sampingan | `NONE`. Izin Role Access dan penempatan uji di devYoga sudah dipulihkan |
| Interupsi | `NONE` |
| Status Git | 3 berkas source baru + `Program.cs`; 1 uji baru + 2 berkas uji diubah; laporan ini (baru); 2 dokumen roadmap/traceability. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-353`, `BE-HRD-355`, `BE-HRD-356` siap dikerjakan: `BE-HRD-343` sudah ✅ (tanda ⬜ di Grafik B88 basi dan dibetulkan bersama task ini); `BE-HRD-354` menunggu `BE-HRD-353`; `BE-HRD-358` menunggu `BE-HRD-353` s.d. `356`. Frontend: `FE-HRD-321` (bergantung `FE-HRD-306`) |
