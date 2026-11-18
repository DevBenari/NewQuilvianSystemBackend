# Laporan Perubahan Backend — `BE-HRD-343`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-343` |
| Judul | Status harian `NotYetPresent`/`NoScan`, hari libur hanya jadwal tetap, scan di luar jendela |
| Slice | Amandemen `A52` atas `S-B1` Kehadiran — `EPIC HRD-111`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B86 |
| Trace | `FR-PK-008`, `FR-PK-009`, `FR-PK-011`; `HRD-DEC-207`, `HRD-DEC-210`, `HRD-DEC-211` (dengan `HRD-DEC-104`, `HRD-DEC-216`); `02-backend-architecture.md` 35.9.4; `state-transition-matrix.md` 30.6; `AT-HRD-PK-19`, `-20`, `-21`, `-26`, `-28` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-342` ✅ (jendela dan toleransi dari setelan) |
| Klasifikasi | `HEAVY` — skor 8: satu repository (0), sekitar 14 berkas diperiksa (1), 2 berkas source (0), logika status inti + pekerjaan latar (3), perubahan perilaku pemrosesan (2), tanpa schema (0), tanpa keamanan (0), tanpa UI (0), ditambah 2 untuk ketergantungan waktu dan konkurensi penjadwal |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/Services/{AttendanceProcessingService,AttendanceSchedulerService}.cs`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ff1b96ca` (branch `yoga`) + merge `origin/QuilvianIntegrationBackend` (`cf79bd62`) yang belum di-commit |
| Tanggal | 13 November 2026 |
| Status | ✅ Selesai — harness 78/78 (termasuk kontrol negatif); perilaku diperiksa lewat API devYoga (NoScan, pengecualian di luar jendela, scan tertunda dan job hitung ulang otomatis) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — dua service yang sudah ada |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Pegawai terjadwal tanpa scan langsung `Absent` begitu diproses, bahkan saat shift masih berjalan, beserta pengecualian `ABSENT` yang memblokir payroll | Alpa muncul sebelum periode ditutup (melanggar `HRD-DEC-207`, `HRD-DEC-216`); perawat shift malam yang diproses pukul 02.15 tercatat Alpa padahal shiftnya baru selesai 07.00 |
| Hari libur menjadikan setiap pegawai tanpa scan `Holiday`, termasuk pegawai roster | Perawat ICU yang dijadwalkan pada 17 Agustus dan tidak datang tercatat "Libur" (melanggar `HRD-DEC-210`) |
| Scan di luar jendela tidak diambil sama sekali dan tidak tercatat di mana pun | Scan 04.30 milik pegawai yang datang lebih awal hilang dari pandangan HRD (melanggar `HRD-DEC-211` butir 2) |
| Rekap yang sudah `Processed` tidak pernah diproses ulang oleh pekerjaan latar (job harian per tanggal hanya sekali, tanpa paksaan) | Status sementara tidak pernah berubah: "Belum Hadir" shift malam tertahan, scan mesin yang tertunda tidak mengubah "Tidak Ada Scan" (risiko kartu) |

---

## 2. Proses bisnis

**Tanpa scan (`HRD-DEC-207`).** Budi Pagi 07.00–14.00 tanpa scan. Diproses pukul 09.00: "Belum Hadir"
(`NotYetPresent`), tanpa pengecualian scan hilang. Diproses sesudah 14.00: "Tidak Ada Scan" (`NoScan`)
dengan pengecualian `MissingCheckIn`/`MissingCheckOut`. Keduanya bukan Alpa; `Absent` hanya diberikan
saat periode ditutup (`BE-HRD-344`). Shift malam Andi 21.00–07.00 yang diproses pukul 02.00 tetap
"Belum Hadir" karena jam selesainya di hari berikutnya (`HRD-DEC-104`).

**Hari libur (`HRD-DEC-210`).** Status `Holiday` hanya diberikan bila sumber jadwal `FixedWorkSchedule`.
Pegawai roster pada hari libur tetap diharapkan hadir; tanda `IsHoliday` pada rekap tetap tercatat untuk
ditampilkan.

**Scan di luar jendela (`HRD-DEC-211`).** Scan pegawai pada hari kerja itu (00.00–24.00 WIB) yang tidak
masuk jendela shift menjadi satu pengecualian `OutsideScanWindow` per scan (kode `OUTSIDE_WINDOW_<id log>`,
tidak memblokir payroll), dengan pesan "Scan 03:00 di luar jendela scan shift (11-11 10:00–12-11 05:00);
tidak dipasangkan. Ajukan koreksi presensi bila scan ini sah." Log mesin tidak diubah. Scan yang masuk
jendela shift berlaku pada hari sebelum atau sesudahnya (misalnya pulang shift malam) bukan pengecualian.

**Hitung ulang otomatis (state matrix 30.6).** Setiap siklus pekerjaan latar (30 detik), penjadwal mencari
rekap dalam rentang `MaximumCatchUpDays` di periode yang belum ditutup:

- `NotYetPresent` yang jam selesai shiftnya sudah lewat;
- `NoScan` atau `IncompletePunch` yang punya scan baru di dalam jendela dan belum terpasang.

Untuk masing-masing, satu job pemrosesan ulang per pegawai-tanggal dijadwalkan dengan kunci
`ATT-RECHECK-<id rekap>-v<versi>`, sehingga sapuan berikutnya tidak menggandakannya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-207`, `HRD-DEC-210`, `HRD-DEC-211`; `02-backend-architecture.md` 35.9.4; `state-transition-matrix.md` 30.6; `acceptance-test-matrix.md` `AT-HRD-PK-19`–`28`
- `AttendanceProcessingService.cs` (`ProcessOneCoreAsync`, `ApplyPunchResult`, `BuildExceptions`, `SynchronizeExceptions`), `AttendanceScheduleResolverService.cs` (jadwal tetap, hari libur, status shift yang diterima)
- `AttendanceSchedulerService.cs`, `AttendanceSchedulerHostedService.cs`, `AttendanceSchedulerOptions.cs`, `appsettings.json` (`AttendanceScheduler`)
- `AttendancePeriodService.cs` (pratinjau tutup menghitung rekap `Processed` — alasan `NoScan` tetap `Processed`)
- `AttendanceValueConstants.cs` (`NotYetPresent`, `NoScan`, `OutsideScanWindow` dari `BE-HRD-335`), `HrdAttendanceException.cs` (`ExceptionCode` maks. 50)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceProcessingService.cs` | `ApplyPunchResult` menerima waktu pemrosesan: tanpa scan → `NotYetPresent`/`NoScan`, tidak lagi `Absent`; `Holiday` hanya jadwal tetap. `BuildExceptions`: pengecualian scan hilang tidak dibuat selama "Belum Hadir" dan tetap dibuat pada hari libur untuk pegawai roster; pengecualian `ABSENT` tidak muncul lagi. Baru: `BuildOutsideScanWindowExceptionsAsync`, `IsFixedScheduleHoliday`, `IsShiftStillRunning` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceSchedulerService.cs` | Baru: `EnsureProvisionalStatusRecheckJobsAsync`, dipanggil dari `EnsureAutomaticDailyJobsAsync` di setiap siklus (sebelum batas jam harian). Konstruktor menerima `AttendanceSettingService` (jendela untuk mencocokkan scan tertunda) |

`AttendanceScheduleResolverService.cs` tidak perlu diubah: resolver tetap menandai `IsHoliday` untuk semua
sumber (dibutuhkan untuk tanda libur pada roster dan Jadwal Saya); keputusan status ada di pemrosesan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. Nilai `AttendanceStatus` baru `NotYetPresent`/`NoScan` dan jenis pengecualian `OutsideScanWindow` muncul pada respons rekap (konstanta dan labelnya sudah ada sejak `BE-HRD-335`) |
| Database | Tanpa migration. Pengecualian `OutsideScanWindow` ditulis ke `HrdAttendanceException`; job hitung ulang ke `HrdAttendanceSchedulerJob` |
| Keamanan/Auth | Tidak berubah |
| Delta cakupan | `AttendanceSchedulerService.cs` tidak tercantum di cakupan kartu, tetapi diubah untuk menutup Risiko kartu ("pastikan jadwal pekerjaan latar mencakupnya"); resolver tercantum tetapi tidak diubah |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru atau yang berubah bentuknya. Hasil `POST attendance/processing/process`,
`process-range`, dan job penjadwal kini memakai urutan status 35.9.4.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori, pemrosesan penuh (78 uji: 72 sebelumnya + 6 baru) | 78/78 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\scopetest` |
| — `AT-HRD-PK-19`: shift berjalan tanpa scan → `NotYetPresent`, tanpa pengecualian; shift kemarin → `NoScan`, `IsAbsent` salah, tanpa pengecualian `Absent` | Sesuai | `PASS` | `Tanpa_scan_Belum_Hadir…` |
| — `AT-HRD-PK-20`: `NoScan`, lalu scan 06.55 dan 14.02 tiba → sapuan membuat tepat satu job paksa untuk pegawai itu (panggilan kedua 0), lalu `Present` | Sesuai | `PASS` | `Scan_tertunda…` |
| — `AT-HRD-PK-21`: shift malam bertanggal kemarin, jam selesai 2 jam lagi → `NotYetPresent`; sapuan 0 job sekarang, 1 job sesudah jam selesai | Sesuai | `PASS` | `Shift_malam…` |
| — `AT-HRD-PK-26`: scan 02.30 untuk shift 07.00 (jendela 240) → `NoScan`, satu pengecualian `OutsideScanWindow` (tidak memblokir payroll); log tetap `Matched`, tidak terpasang, aktif; proses ulang tidak menggandakan | Sesuai | `PASS` | `Scan_di_luar_jendela…` |
| — Scan pulang shift malam kemarin (07.05) tidak menjadi pengecualian shift Siang hari ini | Sesuai | `PASS` | `Scan_milik_shift_malam…` |
| — `AT-HRD-PK-28`: 17 Agustus libur nasional — Andi (jadwal tetap) `Holiday`; Sari (roster) `NoScan` dengan tanda `IsHoliday` | Sesuai | `PASS` | `Hari_libur_hanya_untuk_jadwal_tetap` (tanggal 2026 agar shift sudah lewat) |
| Kontrol negatif: syarat hari libur dikembalikan ke semua sumber, backend dibangun ulang | Tepat uji `AT-HRD-PK-28` yang gagal. Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 78/78 lagi | `PASS` | Keluaran `dotnet test` |
| API devYoga (13 November 2026), Nadia Putri, roster Siang 11 November 2026 (`ROST-202611-0004`): tanpa scan | `NoScan` (sebelumnya `Absent`), `IsAbsent` salah | `PASS` | `at-log.jsonl` |
| — Scan uji 03.00 (di luar jendela 10.00–05.00) | `NoScan` + pengecualian `OutsideScanWindow` dengan pesan di bagian 2; log tetap `Matched`, tidak terpasang | `PASS` | `at-log.jsonl` |
| — Scan uji 13.58 dan 21.03 disisipkan | Dalam satu siklus, penjadwal lokal membuat job `ATT-RECHECK-…-v10` (paksa, untuk Nadia saja; hanya satu job di seluruh devYoga). Pemrosesan setara job → `Present` 13.58–21.03, `MissingCheckIn/Out` tertutup, pengecualian 03.00 tetap | `PASS` | `at-log.jsonl`, `be.log` |
| — Eksekusi job oleh pekerja latar | Job berstatus `RetryScheduled`: "`HumanResource:AttendanceScheduler:SystemActorUserId` belum dikonfigurasi" — konfigurasi lingkungan yang sudah gagal untuk job harian sebelum task ini | `BLOCKED` (konfigurasi) | `HrdAttendanceSchedulerJob` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada `Absent` sebelum tutup periode | Terpenuhi | Harness dan devYoga |
| Shift malam dihitung sampai jam selesai hari berikutnya | Terpenuhi | `AT-HRD-PK-21` |
| Scan di luar jendela menjadi pengecualian `OutsideScanWindow` dan log tetap utuh | Terpenuhi | `AT-HRD-PK-26` dan devYoga |
| `AT-HRD-PK-19`, `-20`, `-21`, `-26`, `-28` | Terpenuhi (unit service) | Bagian 5 |
| Risiko kartu: pekerjaan latar mencakup perubahan `NotYetPresent` → `NoScan` | Terpenuhi di kode; eksekusi di lingkungan menunggu konfigurasi `SystemActorUserId` | Bagian 5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru |
| Masalah yang diketahui | (1) **`AttendanceScheduler:SystemActorUserId` belum dikonfigurasi** di lingkungan lokal/devYoga, sehingga seluruh job penjadwal (harian maupun hitung ulang) gagal dan dijadwalkan ulang. Perlu diisi akun sistem di konfigurasi lingkungan. (2) Pegawai yang punya satu scan saat shift masih berjalan tetap tercatat `IncompletePunch` (35.9.4 butir 6); scan pulang yang masuk kemudian akan menghitung ulang lewat sapuan. (3) Tanggal tanpa jadwal tidak diperiksa untuk scan di luar jendela |
| Risiko tersisa | (1) Scan pada hari kerja yang termasuk jendela shift hari tetangga yang belum terbit tidak dikecualikan dan dapat tercatat sebagai pengecualian hari ini. (2) Sapuan dibatasi 100 rekap per siklus; antrean besar diselesaikan bertahap tiap 30 detik |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 2 berkas source task ini (diubah) di atas merge yang belum di-commit; laporan ini; 2 dokumen roadmap/traceability. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-344` — pratinjau tutup berisi daftar dan konversi tanpa syarat setelan |

Data uji devYoga baru (bertanda `UJI-A52`): tiga log mesin Nadia Putri 11 November 2026
(`UJI-A52-RAW-0300`, `-1358`, `-2103`); satu job `ATT-RECHECK-…-v10` berstatus `RetryScheduled`.
