# Laporan Perubahan Backend — `BE-HRD-346`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-346` |
| Judul | API daftar mesin absen |
| Slice | Amandemen `A52` atas `S-B1` Kehadiran — `EPIC HRD-112`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B86 |
| Trace | `FR-PK-014`; `HRD-DEC-212`; `INT-HR-103`; `02-backend-architecture.md` 35.4.3, 35.10; `api-contract.md` "Corporate / Human Resource / Master Data / Attendance Device"; `validation-matrix.md` `VR-PK-26`; `permission-audit-matrix.md` 30.1 (`AttendanceDevice : Read/Create/Update/Delete`); `AT-HRD-PK-30` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-345` ✅ (`ReleaseHeldLogsForDeviceAsync`) |
| Klasifikasi | `STANDARD` — skor 6: satu repository (0), sekitar 10 berkas diperiksa (1), 4 berkas source (1), endpoint baru dengan hak akses baru (2), pemanggilan pelepasan sesudah commit (1), tanpa schema (0), tanpa UI (0), ditambah 1 untuk validasi identitas lintas mesin |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/{Controllers/AttendanceDeviceController.cs,DTOs/AttendanceDeviceDtos.cs,Services/AttendanceDeviceService.cs}`, `Program.cs` (satu registrasi DI); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`) + perubahan `BE-HRD-344`/`345` yang belum di-commit |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — harness 99/99 (termasuk kontrol negatif); `AT-HRD-PK-30` lewat API devYoga; 403 akun non-superadmin |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `MasterData/AttendanceAndSchedule` |
| Pemilik / prefix registry | `Master / Reference / MasterData` / `Mst` (`ACTIVE`) — entity `MstAttendanceDevice` yang sudah ada; tidak ada entity baru |
| Keberlakuan | `NEW CODE` (controller, service, DTO baru) di atas entity `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-DEL-001`, `QBE-OPT-001` |
| Tidak berlaku | `QBE-CODE-*` — kode mesin diisi Petugas IT sesuai label fisik mesin, bukan nomor bisnis yang dialokasikan sistem; keunikannya dijaga indeks unik yang sudah ada |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| `MstAttendanceDevice` punya tabel tetapi tidak punya API (`HRD-TF-059`) | Petugas IT tidak dapat mendaftarkan mesin; scan dari mesin baru tertahan tanpa jalan keluar kecuali lewat basis data |
| Scan tertahan (`BE-HRD-345`) tidak terlihat di mana pun | IT tidak tahu mesin mana yang perlu didaftarkan |

---

## 2. Proses bisnis

**Melihat mesin tak dikenal.** `GET /unregistered` mengelompokkan scan "Menunggu Pendaftaran Mesin" per identitas
mesin: identitas persis seperti yang dikirim mesin, jumlah scan, rentang waktu scan, dan waktu terima terakhir. Bila
identitas itu milik mesin yang sudah terdaftar tetapi nonaktif, respons menunjuk mesin itu — cukup diaktifkan.

**Mendaftarkan, mengubah, mengaktifkan (`HRD-DEC-212` butir 1 dan 3).** Petugas IT mengisi kode, nama, jenis, lokasi,
rumah sakit, dan identitas teknis. Bila mesin tersimpan **aktif** — baru didaftarkan, identitasnya diubah, atau diaktifkan
kembali — scan tertahan yang cocok dilepas sesudah simpan, lalu kehadiran dihitung ulang (`BE-HRD-345`). Bila pelepasan
gagal, mesin tetap tersimpan; respons memuat pesan kegagalan dan scan tetap tertahan sampai mesin diaktifkan ulang.
Menonaktifkan mesin membuat scan berikutnya dari mesin itu ditahan.

**Identitas bentrok (`VR-PK-26`).** Scan dicocokkan lewat kode, nomor seri, dan ID eksternal sekaligus. Karena itu
ketiganya tidak boleh cocok dengan identitas mana pun milik mesin aktif lain → `409` "Identitas mesin sudah dipakai mesin
lain". Kode dan nomor seri juga unik terhadap mesin nonaktif (indeks unik basis data). Mesin nonaktif yang identitasnya
bentrok boleh disimpan tetapi tidak boleh diaktifkan.

**Menghapus.** Hapus lunak hanya untuk mesin yang belum punya log scan maupun catatan gangguan; selain itu `409` dengan
saran menonaktifkan. Kode mesin yang dihapus dapat dipakai lagi.

**Contoh.** Mesin GDB-03 mengirim scan Nadia 06.10 dan 14.40 sebelum terdaftar. `GET /unregistered` menampilkan
"UJI-A52-GDB-03 — 2 scan". IT mendaftarkannya; respons "Mesin absen berhasil didaftarkan. 2 scan tertahan dilepas.",
dan rekap Nadia berubah menjadi 06.10–14.40 tanpa scan ulang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `api-contract.md` Attendance Device; `validation-matrix.md` `VR-PK-26`; `permission-audit-matrix.md` 30.1 (log audit "Ya" untuk tulis); `02-backend-architecture.md` 35.4.3, 35.10; `03-frontend-architecture.md` (layar Mesin absen, `FE-HRD-312`)
- `MstAttendanceDevice.cs`, `MstAttendanceDeviceConfiguration.cs` (indeks unik kode dan nomor seri tersaring `IsDelete`), `MstAttendanceLocation.cs`, `MstHospitalSite.cs`, `HrdAttendanceDeviceIncident.cs`
- Pola: `ShiftController.cs` (legacy, `ApplicationDbContext` di controller), `LeaveAdjustmentReasonController.cs` + `LeaveAdjustmentReasonService.cs` (Controller → Service), `LoggerService.cs`
- `AttendanceRawLogService.ReleaseHeldLogsForDeviceAsync` (`BE-HRD-345`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/AttendanceDeviceController.cs` | **Baru.** 8 endpoint; `AccessController` `HUMAN_RESOURCE_MASTER_DATA` / `AttendanceDevice`; `AccessPermission` Read/Create/Update/Delete; log audit tulis lewat `LoggerService.AuditAsync` dengan isi di teks pesan (tanpa payload) |
| `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Services/AttendanceDeviceService.cs` | **Baru.** Daftar, mesin tak dikenal, detail, daftarkan, ubah, status, hapus; validasi masukan dan `VR-PK-26`; `DbUpdateException` indeks unik → 409; pelepasan sesudah commit dengan penanganan gagal; `AttendanceDeviceServiceResult<T>` |
| `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/DTOs/AttendanceDeviceDtos.cs` | **Baru.** Metadata, query, `AttendanceDeviceResponse` (termasuk `HeldScanCount`, `HeldScanRelease`, `HeldScanReleaseMessage`), `UnregisteredDeviceResponse`, request daftarkan/ubah/status |
| `Program.cs` | Satu baris `AddScoped<AttendanceDeviceService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 7 endpoint kontrak tersedia; ditambah `GET /filters/metadata` (pilihan jenis mesin dan urutan, untuk layar `FE-HRD-312`) |
| Database | Tanpa migration, tanpa schema baru |
| Keamanan/Auth | Resource baru `AttendanceDevice` (`Read`, `Create`, `Update`, `Delete`) terdaftar lewat `AccessController`; belum diberikan ke jabatan mana pun — pemberian izin ke Petugas IT dilakukan lewat Role Access |
| Audit | Setiap daftarkan/ubah/status/hapus menulis satu log `[AUD]` berisi kode, nama, id, identitas teknis, status aktif, dan hasil pelepasan; pelaku dari sesi |

### 3.4 Delta terhadap blueprint

| Butir blueprint | Implementasi | Alasan |
| --- | --- | --- |
| 35.4.3: `AttendanceDeviceController` memakai `ApplicationDbContext` untuk CRUD, mengikuti `ShiftController` | CRUD di `AttendanceDeviceService` baru | `QBE-SVC-001` (MUST / NEW CODE) mengalahkan pola legacy; cakupan kartu tidak menyebut service |
| Kontrak tidak menyebut `filters/metadata` | Ditambahkan | Pilihan jenis mesin dibutuhkan formulir; pola seluruh master data HR |
| `AttendanceDeviceResponse` tanpa rincian | Field mengikuti `MstAttendanceDevice` + `HeldScanCount`, `HeldScanRelease`, `HeldScanReleaseMessage` | "Daftar mesin beserta jumlah scan tertahan"; hasil pelepasan perlu terlihat petugas |
| Kartu: "mengaktifkan mesin memanggil pelepasan" | Pelepasan juga dipanggil saat mesin aktif didaftarkan atau diubah identitasnya | Identitas baru bisa sama dengan identitas yang selama ini ditahan |
| `PUT` tidak menyebut status aktif | `PUT` tidak mengubah status; status hanya lewat `PATCH /{id}/status` | Satu jalur untuk aktif/nonaktif dan pemeriksaan bentrok saat aktivasi |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/master-data/attendance-devices` — Tag `Corporate / Human Resource / Master Data / Attendance Device`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | `AttendanceDevice : Read` | — | `AttendanceDeviceFilterMetadataResponse` | `200`, `401`, `403` |
| `GET` | `/` | `AttendanceDevice : Read` | Query `search`, `deviceType`, `isActive`, `hospitalSiteId`, `attendanceLocationId`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `PagedResult<AttendanceDeviceResponse>` | `200`, `401`, `403` |
| `GET` | `/unregistered` | `AttendanceDevice : Read` | — | `List<UnregisteredDeviceResponse>` | `200`, `401`, `403` |
| `GET` | `/{id}` | `AttendanceDevice : Read` | — | `AttendanceDeviceResponse` | `200`, `401`, `403`, `404` |
| `POST` | `/` | `AttendanceDevice : Create` | `CreateAttendanceDeviceRequest` | `AttendanceDeviceResponse` + hasil pelepasan | `200`, `400`, `401`, `403`, `409` |
| `PUT` | `/{id}` | `AttendanceDevice : Update` | `UpdateAttendanceDeviceRequest` | `AttendanceDeviceResponse` + hasil pelepasan | `200`, `400`, `401`, `403`, `404`, `409` |
| `PATCH` | `/{id}/status` | `AttendanceDevice : Update` | `{ isActive }` | `AttendanceDeviceResponse` (+ pelepasan bila diaktifkan) | `200`, `401`, `403`, `404`, `409` |
| `DELETE` | `/{id}` | `AttendanceDevice : Delete` | — | — | `200`, `401`, `403`, `404`, `409` |

Validasi `400`: kode/nama wajib, jenis mesin dari metadata, port 1–65535, alamat IP sah, rumah sakit ada, lokasi aktif dan
berada di rumah sakit yang dipilih. `409`: `VR-PK-26`; hapus mesin yang punya log/gangguan.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap; tidak ada peringatan dari berkas baru) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (99 uji: 92 sebelumnya + 7 baru di `DeviceHoldTests.cs`) | 99/99 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\4222eeca-aa89-4652-b697-eb38df816c85\scratchpad\scopetest` |
| — `AT-HRD-PK-30` lewat pendaftaran: 50 scan GDB-01 tertahan, daftar mesin tak dikenal 1 baris/50; daftarkan GDB-01 | 50 `Matched` bermesin, 1 job (rekap yang ada), pesan "50 scan tertahan dilepas", daftar tak dikenal kosong, pemrosesan → `Present` | `PASS` | `Mendaftarkan_mesin_melepas_scan_tertahan` |
| — Daftarkan nonaktif → aktifkan → nonaktifkan | Nonaktif: tanpa pelepasan, `HeldScanCount` 50, daftar tak dikenal menunjuk mesin nonaktif. Aktif: 50 dilepas. Nonaktif lagi: scan baru ditahan | `PASS` | `Mengaktifkan_mesin_melepas_scan_tertahan` |
| — Ubah ID eksternal mesin aktif ke identitas tertahan | 50 dilepas | `PASS` | `Mengubah_identitas_mesin_melepas_scan_tertahan` |
| — `VR-PK-26`: kode sama, nomor seri sama, kode = ID eksternal aktif lain, ID eksternal = kode/nomor seri aktif lain, kode sama dengan mesin nonaktif | Enam kasus 409 dengan pesan kontrak; mesin nonaktif ber-ID eksternal bentrok boleh disimpan, aktivasinya 409 | `PASS` | `Identitas_mesin_bentrok_ditolak_409` |
| — Validasi masukan (kode kosong, jenis, port, IP, lokasi di rumah sakit lain) | Lima kasus 400 dengan pesan masing-masing; tidak ada baris tersimpan | `PASS` | `Validasi_masukan_mesin` |
| — Hapus | Mesin berlog 409; tanpa log → hapus lunak (`DeleteBy` = pelaku), detail 404, kode dipakai lagi; id asing 404 | `PASS` | `Hapus_mesin_ditolak_bila_sudah_punya_log` |
| — Jalur gagal: pelepasan galat saat menyimpan | Mesin tetap terdaftar, `HeldScanReleaseMessage` terisi, 50 scan tetap tertahan; nonaktifkan-aktifkan melepas 50 | `PASS` | `Pelepasan_gagal_tidak_membatalkan_pendaftaran` |
| Kontrol negatif: pelepasan sesudah simpan dan pemeriksaan `VR-PK-26` dimatikan, backend dibangun ulang | 6 dari 7 uji baru gagal (yang lulus: validasi masukan, tidak bergantung keduanya). Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 99/99 lagi | `PASS` | Keluaran `dotnet test` |
| Startup Development dengan resource hak akses baru | Backend menyala, Swagger 200, tanpa galat registri/DI | `PASS` | `be346.log` |
| API devYoga (16 November 2026) tanpa sesi | `401` | `PASS` | `at-log.jsonl` (entri "346 …") di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig` |
| — `filters/metadata` | `200`, enam jenis mesin | `PASS` | `at-log.jsonl` |
| — 2 scan Nadia dari `UJI-A52-GDB-03` (belum terdaftar) → `GET /unregistered` | Ditahan; `UJI-A52-GDB-03` 2 scan (beserta `UJI-A52-MESIN-99` 1 scan dari `BE-HRD-344`) | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-30`: `POST /attendance-devices` `UJI-A52-GDB-03` | `200` "Mesin absen berhasil didaftarkan. 2 scan tertahan dilepas."; 2 `Matched`, 1 job `ATT-DEVREL-<rekap>-v6` dieksekusi pekerja latar sampai `Completed`; rekap 15 Nov `Present` 06.10–14.40 dari 54 scan; log `Processed` bermesin; mesin hilang dari daftar tak dikenal | `PASS` | `at-log.jsonl` |
| — `VR-PK-26` (kode sama; ID eksternal = kode mesin aktif lain) | `409` "Identitas mesin sudah dipakai mesin lain" | `PASS` | `at-log.jsonl` |
| — Jenis mesin tidak valid | `400` | `PASS` | `at-log.jsonl` |
| — Nonaktifkan lalu aktifkan | `200`; aktivasi menjalankan pelepasan (0 tersisa) | `PASS` | `at-log.jsonl` |
| — Hapus mesin berlog / mesin tanpa log | `409` / `200`, detail sesudahnya `404` | `PASS` | `at-log.jsonl` |
| — Log audit | Lima baris `[AUD]` `AttendanceDevice.Create/UpdateStatus/Delete` berisi isi lengkap, pelaku `superadmin` | `PASS` | `be346.log` |
| — 403: akun non-superadmin `gilang.mahendra` (sandi dari pengguna, tidak disimpan) | Login `200`; `GET /`, `GET /unregistered`, `POST /`, `PATCH /{id}/status`, `DELETE /{id}` seluruhnya `403` | `PASS` | `at-log.jsonl` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Mengaktifkan mesin memanggil pelepasan scan sesudah commit | Terpenuhi (juga saat didaftarkan aktif dan saat identitas diubah) | Harness dan devYoga |
| Hapus ditolak bila mesin sudah punya log | Terpenuhi (juga bila punya catatan gangguan) | Harness dan devYoga |
| `AT-HRD-PK-30` | Terpenuhi lewat API | Bagian 5 |
| Uji 403 akun non-IT | Terpenuhi | Akun `gilang.mahendra` |
| DoD umum: build lulus, uji lulus, laporan ditulis, tanpa angka bisnis tertanam, tanpa kolom sensitif di log | Terpenuhi — batas port 1–65535 dan halaman 100 adalah batas teknis | Bagian 3–5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) Resource `AttendanceDevice` belum diberikan ke jabatan Petugas IT di Role Access — sampai itu dilakukan hanya superadmin yang dapat memakai API ini. (2) `MstAttendanceDevice` tidak punya penanda versi baris; dua perubahan bersamaan atas mesin yang sama: yang terakhir menang |
| Risiko tersisa | Identitas mesin dicocokkan peka huruf besar/kecil (sama dengan penerimaan scan); petugas perlu menyalin identitas persis dari daftar mesin tak dikenal |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 3 berkas source baru + `Program.cs`; laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-344`/`345` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-347` — API master hari libur |

Data uji devYoga baru (bertanda `UJI-A52`): mesin `UJI-A52-GDB-03` (aktif, didaftarkan lewat API) dengan 2 log Nadia
15 Nov 2026 (`UJI-A52-346-IN`/`-OUT`, kini `Processed`); mesin `UJI-A52-HAPUS` (terhapus lunak); job
`ATT-DEVREL-…-v6` `Completed`. Log `UJI-A52-RAW-HELD-344` (mesin `UJI-A52-MESIN-99`) tetap tertahan.
