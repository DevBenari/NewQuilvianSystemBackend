# Laporan Perubahan Frontend — `FE-HRD-310`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-310` |
| Judul | Monitoring harian: status baru dan pengecualian jendela scan |
| Slice | `A52` / `MVP-67` / `EPIC HRD-111` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-310` |
| Trace | `FR-PK-008`, `FR-PK-009`, `FR-PK-011`; `HRD-DEC-197`, `207`, `210`, `211`, `216`; `02-backend-architecture.md` 35.9.4; `03-frontend-architecture.md` 31.3 |
| Contract version | `v26` — `api-contract.md` bagian 32 (`attendance/dailies`, status baru lewat `filters/metadata`), `approved` 12 November 2026 |
| Wewenang UI | Layar yang sudah ada `/hr/attendance` (`attendance-monitoring-view.jsx`) dan laci `RawPunchAuditDrawer.jsx`. Tata letak `DEV_DISCRETION`; base component wajib |
| Dependency | `FE-HRD-306` ✅, `BE-HRD-343` ✅ |
| Klasifikasi | `MEDIUM` — satu view, satu laci, dua fungsi service, satu utilitas, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 23 November 2026: "lanjutkan") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/components/view/hr/attendance`, `src/lib/services/hr`, `src/utils/hr/attendance`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `52d3392d2` (branch `YogaV2`) + working tree `FE-HRD-306` s.d. `309` |
| Commit backend yang dijadikan rujukan | `8a68ab67` (branch `yoga`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — acceptance criteria terpenuhi di browser dengan API tiruan; `UAT-PK-07`/`UAT-PK-11` dengan data nyata belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Label status ditentukan dari flag, bukan status | `getAttendanceBadge` memeriksa `hasMissingPunch` lebih dulu → "1 Punch - Perlu Koreksi", lalu `isAbsent` → "Alfa" | Untuk hari tanpa scan sama sekali, backend mengisi `HasMissingPunch = true` (`AttendanceProcessingService` baris 1224–1226: masuk **dan** pulang kosong). Akibatnya Budi yang belum datang pukul 09.00 tampil "1 Punch - Perlu Koreksi", bukan "Belum Hadir" |
| Label status baru belum dipakai | Layar tidak memanggil `GET dailies/filters/metadata`; label ditulis di kode | Bertentangan dengan `03-frontend-architecture.md` 31.3 butir 1 |
| Tidak ada filter status | `filters.attendanceStatus` ada di slice, tetapi tidak ada kontrolnya | Pengguna tidak bisa menyaring "Tidak Ada Scan" atau "Belum Lengkap" |
| Pengecualian scan di luar jendela tidak terlihat | Laci hanya memuat log mesin; `GET dailies/{id}/exceptions` tidak dipanggil | Scan yang tidak dipasangkan (`HRD-DEC-211`) tidak dapat ditelusuri dari layar |
| Kartu ringkasan menyesatkan | "Incomplete Punch" (Inggris) menghitung `missingPunchCount`, yang juga mencakup hari tanpa scan; "Alfa" tanpa keterangan | Pengguna bisa mengira ada Alfa sebelum periode ditutup |
| **Celah backend**: metadata tidak memuat `IncompletePunch` | `AttendanceDailyQueryService.GetMetadata` hanya memuat `Incomplete` ("Data belum lengkap"), `NotYetPresent`, `NoScan`; status `IncompletePunch` yang dipakai pemrosesan (`HRD-DEC-106`) tidak ada | Tanpa penanganan, label "Belum Lengkap" tidak bisa diambil dari server |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: HRD dan Kepala Unit yang memantau presensi harian (`AttendanceDaily : Read`).

**Contoh UAT-PK-07.** Budi berjadwal 07.00–14.00 dan belum scan.

| Saat dibuka | Yang tampil di baris Budi |
| --- | --- |
| Pukul 09.00 (shift masih berjalan) | **Belum Hadir** |
| Pukul 14.05 (shift selesai, tetap tanpa scan) | **Tidak Ada Scan** |
| Sesudah periode ditutup | **Tidak hadir** (label server untuk `Absent`) — hanya diberikan penutupan periode |

Selama periode masih terbuka tidak ada label Alfa/Tidak hadir pada baris mana pun. Rina yang hanya scan masuk tampil **Belum Lengkap**.

**Contoh UAT-PK-11.** 17 Agustus libur nasional. Andi (jadwal kantor tetap) tidak scan → **Hari libur**. Sari (roster) tidak scan → **Tidak Ada Scan**, karena hari libur tidak membebaskan pegawai roster (`HRD-DEC-210`). Statusnya dihitung server; layar hanya menampilkannya.

**Alur pemakaian.**

1. Pengguna membuka **Monitoring & Rekapitulasi Presensi HR**. Di atas tabel ada penjelasan: "Selama periode masih terbuka tidak ada status Alfa: pegawai tanpa scan tercatat Belum Hadir selama shift berjalan, lalu Tidak Ada Scan sesudah shift selesai. Satu scan saja tercatat Belum Lengkap. Scan di luar jendela tidak dipasangkan dan muncul sebagai pengecualian."
2. Kartu ringkasan kini: Hadir Tepat Waktu, Terlambat, Izin Pulang Cepat, Dinas Luar, **Scan Belum Lengkap / Tidak Ada**, **Pengecualian Terbuka**, **Alfa (Periode Ditutup)**.
3. Filter **status** memakai label server dan dapat digabung dengan tanggal, unit, dan nama.
4. Baris yang punya pengecualian terbuka menampilkan "1 pengecualian terbuka" di bawah lencana status.
5. Tombol **Log & Pengecualian** membuka laci berisi dua bagian:
   - **Pengecualian hari ini**: jenis (misalnya "Scan di luar jendela"), waktu scan, keterangan dari server, dan status. Contoh keterangan: "Scan 02.30 di luar jendela scan shift (03.00–22.00); tidak dipasangkan. Ajukan koreksi presensi bila scan ini sah." Pengecualian scan di luar jendela tampil paling atas.
   - **Data mentah mesin**: log tap seperti sebelumnya.

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Metadata status gagal dimuat | Peringatan dengan pesan server; status tampil sebagai kode aslinya (misalnya `NoScan`), kecuali "Belum Lengkap"; tetap tanpa Alfa |
| Tidak ada pengecualian | Laci menampilkan "Tidak ada pengecualian pada tanggal dinas ini." |
| Gagal memuat daftar, log, atau pengecualian | Pesan galat server (perilaku lama dipertahankan) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-310`; `api-contract.md` 32; `02-backend-architecture.md` 35.9.4; `04-prd-to-mvp.md` `UAT-PK-07`, `UAT-PK-11`; `AttendanceDailyController.cs`, `AttendanceDailyQueryService.cs` (metadata, ringkasan), `AttendanceProcessingService.cs` (penentuan status, `BuildOutsideScanWindowExceptionsAsync`), `AttendanceValueConstants.cs`, `AttendanceDailyQueryDtos.cs`; `attendance-monitoring-view.jsx`, `RawPunchAuditDrawer.jsx`, `attendance-slice.js`, `use-hr-remote-data.js`, `status-badge.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/attendance/attendance-monitoring-view.jsx` | Lencana status dari `attendanceStatus` + label metadata; filter status; penanda jumlah pengecualian per baris; kartu ringkasan diganti namanya dan kartu Pengecualian Terbuka ditambahkan; penjelasan status; peringatan bila metadata gagal; tombol "Log & Pengecualian". `getAttendanceBadge` berbasis flag dihapus |
| `src/components/view/hr/attendance/RawPunchAuditDrawer.jsx` | Memuat pengecualian dan log bersamaan; bagian **Pengecualian hari ini** di atas data mentah mesin; judul "Log Mesin & Pengecualian" |
| `src/lib/services/hr/attendance-management-service.js` | `getDailyFilterMetadata` (`GET dailies/filters/metadata`), `getDailyExceptions` (`GET dailies/{id}/exceptions`) |
| `src/utils/hr/attendance/attendance-daily-status-utils.js` (baru) | `buildAttendanceStatusLabels`, `buildAttendanceStatusOptions` (dengan cadangan "Belum Lengkap"), `resolveDailyStatusBadge`, `EXCEPTION_TYPE_LABELS`, `sortDailyExceptions` |
| `tests/unit/hr-attendance-daily-status-utils.test.mjs` (baru) | 7 uji |
| `tests/e2e/hr-attendance-daily-status.spec.mjs` (baru) | 4 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Daftar tetap memakai slice `attendance` yang sudah ada (Redux). Metadata dimuat lewat `useHrRemoteData`, sama seperti layar HR lain. Label status dari server; frontend hanya memetakan warna lencana ke kunci `StatusBadge` yang tersedia. Satu-satunya label tertanam adalah cadangan "Belum Lengkap", yang diambil dari `03-frontend-architecture.md` 31.3 butir 1 dan hanya dipakai selama metadata server belum memuatnya. Base component dipakai seluruhnya (`FilterSelect`, `StatusBadge`, `DataTable`, `InformationAlert`, `ConfirmModal`). Peringatan ESLint `react-hooks/set-state-in-effect` pada `RawPunchAuditDrawer.jsx` sudah ada sebelum task (baris `setLoading(true)` di efek berasal dari `FE-HRD-091`) dan tidak ditambah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel dan laci menampilkan teks memuat (perilaku lama) |
| Kosong | Daftar: "Belum ada data presensi untuk kriteria ini."; laci: "Tidak ada pengecualian pada tanggal dinas ini." / "Tidak ada log mesin pada tanggal dinas ini." |
| Gagal | Pesan server; metadata gagal → peringatan, status tampil sebagai kode |
| Tanpa hak akses | Perilaku lama: 403 dari server tampil sebagai pesan galat daftar |

---

## 5. Endpoint yang dikonsumsi

Nilai `[Tags]` controller: `AttendanceDailyController`.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance/dailies/filters/metadata` | Label status (`attendanceStatusOptions`) dan opsi filter | `AttendanceDaily : Read` |
| `GET` | `/v1/corporate/human-resource/attendance/dailies` | Daftar harian; kini juga `attendanceStatus` | `AttendanceDaily : Read` |
| `GET` | `/v1/corporate/human-resource/attendance/dailies/summary` | Kartu ringkasan, termasuk `openExceptionCount` | `AttendanceDaily : Read` |
| `GET` | `/v1/corporate/human-resource/attendance/dailies/{id}/exceptions` | Pengecualian hari itu, termasuk `OutsideScanWindow` | `AttendanceDaily : Read` |
| `GET` | `/v1/corporate/human-resource/attendance/dailies/{id}/raw-logs` | Log mesin mentah (tidak berubah) | `AttendanceDaily : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-attendance-daily-status.spec.mjs` (build produksi, port 3710, API tiruan penuh, nol tulis) | 4/4 lulus | `PASS` | Keluaran Playwright |
| — `UAT-PK-07`/`UAT-PK-11` | Budi "Belum Hadir", Sari "Tidak Ada Scan", Rina "Belum Lengkap", Andi "Hari libur", Tono "Hadir"; isi tabel tanpa "Alfa/Alpa/Tidak hadir/Perlu Koreksi" walau baris berflag `hasMissingPunch`; kartu dan penjelasan baru tampil; "Incomplete Punch" hilang | `PASS` | Skenario 1 |
| — Pengecualian scan di luar jendela | "1 pengecualian terbuka" hanya pada baris Sari; laci menampilkan "Scan di luar jendela", pesan server, dan log mesin; baris tanpa pengecualian menampilkan keadaan kosong | `PASS` | Skenario 2 |
| — Filter status | Opsi berlabel server; "Tidak Ada Scan" mengirim `attendanceStatus=NoScan`; "Belum Lengkap" mengirim `IncompletePunch` | `PASS` | Skenario 3 |
| — Metadata gagal | Peringatan tampil; `NoScan` tampil sebagai kode; tetap tanpa Alfa | `PASS` | Skenario 4 |
| Putaran awal uji layar | Sesi terputus saat uji pertama berjalan; diulang dari build yang sama (server 3710 sisa dihentikan lebih dulu). Satu skenario gagal karena pemilih "Pengecualian Terbuka" juga cocok dengan "1 pengecualian terbuka" (pencocokan tanpa huruf besar); pemilih dibuat persis, source tidak berubah | Uji diperbaiki | Keluaran Playwright |
| `tests/unit/hr-attendance-daily-status-utils.test.mjs` | 7/7 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif: 6 mutasi (label dari `hasMissingPunch`; `isAbsent` jadi Alfa; tanpa cadangan Belum Lengkap; label server diabaikan; opsi cadangan tidak ditambahkan; urutan pengecualian) | 6/6 tertangkap; source dipulihkan | `PASS` | `mut310.cjs` di scratchpad sesi |
| `npm run test:unit` (seluruh suite) | 5.250 lulus, 100 gagal dari 5.350 | `EXISTING / ENVIRONMENT ISSUE` | Daftar 100 uji yang gagal identik per nama dengan putaran `FE-HRD-306`; seluruhnya di modul lain |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil | `PASS` | Keluaran perintah |

AUTOMATED TEST: `npm run test:unit` — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — kontrol yang diubah (filter status, tombol Log & Pengecualian, Tutup) dijalankan di peramban Chromium lewat Playwright terhadap build produksi.

**Tidak dijalankan:** `UAT-PK-07` dan `UAT-PK-11` dengan data nyata. Keduanya bergantung pada jam saat dibuka (09.00 dan 14.05) dan data libur nasional di devYoga. Status dihitung server dan sudah dibuktikan `AT-HRD-PK-19`–`21`, `-26`, `-28` (laporan `BE-HRD-343`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada label "Alpa" untuk hari yang belum ditutup | Terpenuhi | Label hanya dari `attendanceStatus` (uji unit 1, 4; mutasi M1/M2); skenario 1 dan 4 memeriksa seluruh isi tabel |
| Outcome: rekap harian menampilkan "Belum Hadir", "Tidak Ada Scan", "Belum Lengkap", dan pengecualian scan di luar jendela | Terpenuhi | Skenario 1 dan 2 |
| DoD umum: base component, memuat/kosong/galat, uji unit dan browser | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-07`, `UAT-PK-11` | Bagian layar terpenuhi dengan API tiruan; data nyata devYoga `NOT RUN` | Skenario 1 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kartu "Scan Belum Lengkap / Tidak Ada" memakai `missingPunchCount` server, yang juga menghitung pegawai "Belum Hadir" selama shift berjalan. Ringkasan server belum memisahkan ketiga status itu |
| Masalah yang diketahui | **Temuan backend**: `GET dailies/filters/metadata` belum memuat `IncompletePunch` ("Belum Lengkap"). Layar memakai label cadangan dari arsitektur 31.3 dan opsi filter tambahan, yang otomatis tergantikan begitu server mengirim labelnya. Disarankan satu task backend kecil untuk menambah opsi itu ke metadata |
| Dependency backend | `NONE` untuk task ini (lihat temuan di atas) |
| Perubahan sampingan | Playwright menghapus `test-results/.last-run.json` (ter-track) ketika sesi terputus; dipulihkan dari `HEAD` dengan akhir baris CRLF. Server uji 3710 sisa putaran terputus dihentikan sebelum uji diulang |
| Interupsi | Sesi berakhir saat build/uji layar pertama; dilanjutkan dari `git status` yang sama, build yang sudah selesai dipakai ulang |
| Status Git | ` M src/components/view/hr/attendance/attendance-monitoring-view.jsx`, ` M src/components/view/hr/attendance/RawPunchAuditDrawer.jsx`, ` M src/lib/services/hr/attendance-management-service.js`, `?? src/utils/hr/attendance/attendance-daily-status-utils.js`, `?? tests/unit/hr-attendance-daily-status-utils.test.mjs`, `?? tests/e2e/hr-attendance-daily-status.spec.mjs`; ditambah berkas `FE-HRD-306` s.d. `309` yang belum di-commit |
| Langkah berikutnya | `FE-HRD-311` (tutup periode: pratinjau berisi daftar dan jumlah scan tertahan) |
