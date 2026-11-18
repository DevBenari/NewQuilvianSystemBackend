# Laporan Perubahan Backend — `BE-HRD-347`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-347` |
| Judul | API master hari libur |
| Slice | Amandemen `A52` atas `S-B1` Kehadiran — `EPIC HRD-112`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B86 |
| Trace | `FR-PK-015`; `HRD-DEC-210`; `02-backend-architecture.md` 35.4.3, 35.9.4; `api-contract.md` "Corporate / Human Resource / Master Data / Holiday"; `validation-matrix.md` `VR-PK-25`; `permission-audit-matrix.md` 30.1 (`Holiday : Read/Create/Update/Delete`, log audit untuk tulis); `AT-HRD-PK-28` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-334` ✅ |
| Klasifikasi | `STANDARD` — skor 5: satu repository (0), sekitar 10 berkas diperiksa (1), 4 berkas source (1), endpoint baru dengan hak akses baru (2), validasi rentang kalender (1), tanpa schema (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/{Controllers/HolidayController.cs,DTOs/HolidayDtos.cs,Services/HolidayService.cs}`, `Program.cs` (satu registrasi DI); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`) + perubahan `BE-HRD-344` s.d. `346` yang belum di-commit |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — harness 102/102 (termasuk kontrol negatif); `AT-HRD-PK-28` dengan hari libur dari API lulus di devYoga; 403 akun non-superadmin |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `MasterData/AttendanceAndSchedule` |
| Pemilik / prefix registry | `Master / Reference / MasterData` / `Mst` (`ACTIVE`) — entity `MstHoliday` dan `MstWorkCalendar` yang sudah ada; tidak ada entity baru |
| Keberlakuan | `NEW CODE` (controller, service, DTO baru) di atas entity `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-DEL-001`, `QBE-OPT-001` |
| Tidak berlaku | `QBE-CODE-*` — kode hari libur diisi Admin HRD sesuai kontrak (`CreateHolidayRequest.HolidayCode`), bukan nomor yang dialokasikan sistem; keunikannya per kalender dijaga indeks unik yang sudah ada |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| `MstHoliday` punya tabel dan dibaca resolver jadwal, tetapi tidak punya API (`HRD-TF-059`) | Admin HRD tidak dapat mencatat hari libur; pegawai jadwal tetap tercatat "Tidak Ada Scan" pada hari libur nasional (devYoga: 0 hari libur) |

---

## 2. Proses bisnis

**Mengelola hari libur (`HRD-DEC-210` butir 1).** Admin HRD memilih kalender kerja, lalu mengisi kode, nama, tanggal
mulai dan selesai, jenis (libur nasional, cuti bersama, libur internal rumah sakit, hari raya keagamaan, libur daerah,
lainnya), dan tanda libur nasional. Tanggal wajib berada di dalam rentang kalender yang dipilih (`VR-PK-25`). Hari libur
dapat diubah, dinonaktifkan, atau dihapus lunak. Setiap perubahan menulis log audit berisi pelaku dan isi perubahan.

**Dampak pada kehadiran (butir 2–3, `BE-HRD-343`).** Pegawai jadwal tetap pada hari libur berstatus `Holiday`, bukan
"Tidak Ada Scan". Pegawai roster tetap diharapkan hadir sesuai jadwal; hari libur hanya tanda.

**Roster terbit tidak berubah (butir 5).** API ini hanya menulis `MstHoliday`; tidak ada pemanggilan ke roster.

**Contoh devYoga.** Dewi Anggraeni (jadwal tetap) pada Jumat 13 Nov 2026 berstatus "Tidak Ada Scan". Admin menambah
"Libur uji" 13–15 Nov; diproses ulang, Dewi menjadi `Holiday`. Nadia Putri (roster ICU 15 Nov) tetap `Present`. Sesudah
hari libur dihapus dan diproses ulang, Dewi kembali "Tidak Ada Scan".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-210`; `api-contract.md` Holiday; `validation-matrix.md` `VR-PK-25`; `permission-audit-matrix.md` 30.1
- `MstHoliday.cs`, `MstHolidayConfiguration.cs` (kolom `date`, indeks unik `(WorkCalendarId, HolidayCode)` tersaring `IsDelete`), `MstWorkCalendar.cs` dan configuration-nya
- `AttendanceScheduleResolverService.cs` (`ResolveCalendarAndHolidayAsync`, `IsHolidayOnDate`), `WorkCalendarController.cs` (hapus kalender ditolak bila punya hari libur)
- Pola dari `BE-HRD-346`: `AttendanceDeviceController`/`AttendanceDeviceService`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/HolidayController.cs` | **Baru.** 7 endpoint; `AccessController` `HUMAN_RESOURCE_MASTER_DATA` / `Holiday`; `AccessPermission` Read/Create/Update/Delete; log audit tulis lewat `LoggerService.AuditAsync` dengan isi di teks pesan |
| `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Services/HolidayService.cs` | **Baru.** Metadata, daftar, detail, tambah, ubah, status, hapus lunak; validasi `VR-PK-25`, jenis, kalender aktif, kode unik per kalender (409, termasuk penangkapan `DbUpdateException`); `HolidayServiceResult` |
| `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/DTOs/HolidayDtos.cs` | **Baru.** Metadata (opsi kalender beserta rentang), query, `HolidayResponse`, `CreateHolidayRequest`/`UpdateHolidayRequest` (tanggal `DateOnly`), status |
| `Program.cs` | Satu baris `AddScoped<HolidayService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 7 endpoint sesuai kontrak; request mengikuti field kontrak ditambah `Description` opsional (kolom yang sudah ada) |
| Database | Tanpa migration, tanpa schema baru. `IsPaidHoliday` dan `IsRecurringAnnually` tidak diubah lewat API (tidak ada di kontrak; aturan uang hari libur milik lembur/payroll, `HRD-DEC-210` butir 4) dan tetap ditampilkan pada respons |
| Keamanan/Auth | Resource baru `Holiday` (`Read`, `Create`, `Update`, `Delete`); belum diberikan ke jabatan mana pun — pemberian izin ke Admin HRD lewat Role Access |
| Audit | Satu log `[AUD]` per tambah/ubah/status/hapus berisi kode, nama, id, kalender, tanggal, jenis, tanda nasional, status aktif; pelaku dari sesi |

### 3.4 Delta terhadap blueprint

| Butir blueprint | Implementasi | Alasan |
| --- | --- | --- |
| 35.4.3: `HolidayController` "tanpa service — CRUD sederhana memakai `ApplicationDbContext` langsung, mengikuti `ShiftController`" | CRUD di `HolidayService` baru | `QBE-SVC-001` (MUST / NEW CODE) mengalahkan pola legacy |
| Cakupan kartu hanya controller dan DTO | Ditambah service dan satu registrasi DI | Konsekuensi baris di atas |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/master-data/holidays` — Tag `Corporate / Human Resource / Master Data / Holiday`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | `Holiday : Read` | — | `HolidayFilterMetadataResponse` | `200`, `401`, `403` |
| `GET` | `/` | `Holiday : Read` | Query `workCalendarId`, `year`, `holidayType`, `isActive`, `search`, `pageNumber`, `pageSize` | `PagedResult<HolidayResponse>` | `200`, `401`, `403` |
| `GET` | `/{id}` | `Holiday : Read` | — | `HolidayResponse` | `200`, `401`, `403`, `404` |
| `POST` | `/` | `Holiday : Create` | `CreateHolidayRequest` | `HolidayResponse` | `200`, `400`, `401`, `403`, `409` |
| `PUT` | `/{id}` | `Holiday : Update` | `UpdateHolidayRequest` | `HolidayResponse` | `200`, `400`, `401`, `403`, `404`, `409` |
| `PATCH` | `/{id}/status` | `Holiday : Update` | `{ isActive }` | `HolidayResponse` | `200`, `401`, `403`, `404` |
| `DELETE` | `/{id}` | `Holiday : Delete` | — | — | `200`, `401`, `403`, `404` |

`400`: kode/nama wajib, jenis dari metadata, tanggal wajib, kalender aktif, `VR-PK-25` "Tanggal libur harus berada di
dalam kalender kerja yang dipilih". `409`: kode sudah dipakai pada kalender yang sama.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap; tidak ada peringatan dari berkas baru) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (102 uji: 99 sebelumnya + 3 baru di `HolidayTests.cs`) | 102/102 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\4222eeca-aa89-4652-b697-eb38df816c85\scratchpad\scopetest` |
| — `AT-HRD-PK-28` dengan hari libur dari service: 17 Agt 2026 | Sebelum: jadwal tetap `NoScan`. Sesudah tambah: jadwal tetap `Holiday`; roster `NoScan` dengan `IsHoliday`; dinonaktifkan → jadwal tetap `NoScan` lagi. Sidik roster (periode + seluruh shift) identik di setiap langkah | `PASS` | `Hari_libur_dari_api_berlaku_dan_roster_tidak_berubah` |
| — `VR-PK-25` | Empat kasus 400 dengan pesan kontrak; tanggal tepat di batas kalender sah; kalender nonaktif/asing, jenis tidak valid, kode kosong 400; ubah ke luar kalender 400 dan data tidak berubah | `PASS` | `Tanggal_di_luar_kalender_ditolak` |
| — Kode unik, ubah, hapus, daftar | Kode ganda 409; ubah tanggal (`UpdateBy` = pelaku); daftar per kalender/tahun urut tanggal; hapus lunak (`DeleteBy`), kode dipakai lagi; id asing 404; metadata memuat kalender dan label jenis | `PASS` | `Kode_unik_ubah_hapus_dan_daftar` |
| Kontrol negatif: pemeriksaan rentang kalender dimatikan, backend dibangun ulang | Tepat uji `VR-PK-25` yang gagal. Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 102/102 lagi | `PASS` | Keluaran `dotnet test` |
| Startup Development dengan resource hak akses baru | Backend menyala, Swagger 200, tanpa galat registri/DI | `PASS` | `be347.log` |
| API devYoga (16 November 2026) tanpa sesi | `401` | `PASS` | `at-log.jsonl` (entri "347 …") di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig` |
| — `filters/metadata` | Kalender `WCL-RSMMC-00002` 2026-01-01..2027-01-01; enam jenis | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-28`: `POST` hari libur uji 13–15 Nov 2026 (`Hospital`) | `200`. Dewi (jadwal tetap) 13 Nov: `NoScan` → `Holiday`. Nadia (roster) 15 Nov: tetap `Present` | `PASS` | `at-log.jsonl` |
| — Roster terbit tidak berubah | Sidik md5 5 periode roster beserta seluruh shift identik sebelum, sesudah tambah + proses, dan di akhir | `PASS` | `at-log.jsonl` |
| — `VR-PK-25` (Jan 2027; selesai < mulai) dan kode ganda | `400` dengan pesan kontrak; `409` | `PASS` | `at-log.jsonl` |
| — `PUT`, daftar 2026, `PATCH` nonaktif, `DELETE` | `200` (nama dan tanggal berubah, `updateBy` = superadmin); daftar 1 baris; nonaktif; hapus, detail sesudahnya `404` | `PASS` | `at-log.jsonl` |
| — Sesudah hari libur dihapus | Dewi kembali `NoScan`, Nadia `Present` | `PASS` | `at-log.jsonl` |
| — Log audit | Empat baris `[AUD]` `Holiday.Create/Update/UpdateStatus/Delete` berisi isi lengkap, pelaku `superadmin` | `PASS` | `be347.log` |
| — 403: akun non-superadmin `gilang.mahendra` | Login `200`; enam aksi (metadata, daftar, tambah, ubah, status, hapus) seluruhnya `403` | `PASS` | `at-log.jsonl` |
| — Tanda libur pada rekap roster Nadia | `IsHoliday` salah: rekap Nadia tanpa rumah sakit, sehingga resolver tidak menemukan kalender kerja berlingkup rumah sakit ("Work calendar tidak ditemukan") | `NOT VERIFIED` di devYoga (data); terbukti di harness | `ScheduleResolutionJson` rekap |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tanggal di luar rentang kalender ditolak | Terpenuhi | `VR-PK-25` harness dan devYoga |
| Perubahan tercatat logger | Terpenuhi | Empat log `[AUD]` dengan pelaku |
| Roster terbit tidak berubah | Terpenuhi | Sidik roster identik (harness dan devYoga) |
| `AT-HRD-PK-28` (bersama `BE-HRD-343`) | Terpenuhi untuk status; tanda libur roster terbukti di harness | Bagian 5 |
| DoD umum | Terpenuhi | Bagian 3–5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) Resource `Holiday` belum diberikan ke jabatan Admin HRD di Role Access — sampai itu dilakukan hanya superadmin yang dapat memakai API ini. (2) **Temuan data devYoga:** rekap pegawai yang penempatannya tanpa rumah sakit (contoh Nadia Putri) tidak menemukan kalender kerja, karena satu-satunya kalender (`WCL-RSMMC-00002`) berlingkup rumah sakit; akibatnya hari libur tidak terbaca untuk pegawai itu, termasuk bila ia berjadwal tetap. Perbaikannya di data penempatan atau dengan kalender tanpa rumah sakit, bukan di task ini |
| Risiko tersisa | Menambah atau menghapus hari libur tidak memicu hitung ulang rekap yang sudah ada; rekap jadwal tetap pada tanggal itu baru berubah saat diproses ulang (job harian, sapuan, atau proses manual). Tidak diminta kontrak; bisa menjadi task lanjutan bila dibutuhkan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 3 berkas source baru + `Program.cs`; laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-344` s.d. `346` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-348` — Jadwal Saya: jadwal sendiri dan roster unit |

Data uji devYoga: hari libur `UJI-A52-LIBUR` dibuat lalu dihapus lunak (tetap tersimpan sebagai baris terhapus); rekap
Dewi Anggraeni 13 Nov 2026 kini ada (`NoScan`, jadwal tetap) karena diproses selama uji; rekap Nadia 15 Nov tetap
`Present`.
