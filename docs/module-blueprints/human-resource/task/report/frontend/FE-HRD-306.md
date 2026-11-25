# Laporan Perubahan Frontend — `FE-HRD-306`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-306` |
| Judul | Klien API dan state untuk endpoint `A52` |
| Slice | `A52` / `MVP-67` / `EPIC HRD-109` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-306` |
| Trace | `FR-PK-002`; `03-frontend-architecture.md` 31.2–31.3; `HRD-DEC-194` s.d. `HRD-DEC-217` |
| Contract version | `v26` — `api-contract.md` bagian 32, `approved` 12 November 2026 |
| Wewenang UI | Tidak ada layar yang dibuat atau diubah. Task ini hanya menyediakan lapisan service Axios dan state Redux; layar menjadi lingkup `FE-HRD-307` s.d. `FE-HRD-316` |
| Dependency | `BE-HRD-336` ✅; seluruh endpoint MVP sudah ada di backend (`BE-HRD-338`, `340`, `342`, `344`, `346`–`350` ✅) |
| Klasifikasi | `MEDIUM` — tiga service diperluas, tiga slice baru, satu slice diperluas, satu utilitas, store; tanpa layar |
| Task mode | `FRONTEND` (instruksi pengguna 23 November 2026: "lanjut sesuai saran") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/lib/services/hr`, `src/lib/state`, `src/utils/hr`, `tests/unit`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `52d3392d2` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `8a68ab67` (branch `yoga`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpenuhi |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Tidak ada satu pun pemanggilan endpoint `A52` di frontend | Pencarian `holidays`, `attendance-devices`, `revisions`, `my-schedule`, `attendance/reports` di `src/` (branch `YogaV2`) tidak menemukan apa pun | Seluruh layar `FE-HRD-307` s.d. `FE-HRD-315` belum punya jalur data |
| Ganti Petugas dan koreksi mundur memakai path alias `shift-assignments` | `roster-scheduling-service.js` baris 104–129 (sebelum task) | Path itu masih dilayani backend sebagai alias, tetapi kontrak `v26` hanya memuat `shift-replacements` |
| Ganti Petugas belum mengirim `expectedWorkforceProfileId` dan `warningReasons` | Badan permintaan hanya berisi tiga ruas | Perlindungan "diubah orang lain" (409) dan alasan peringatan W1–W7 belum bisa dipakai layar |
| Thunk roster hanya meneruskan teks pesan | `rejectWithValue(err?.response?.data?.message)` | Layar tidak dapat membedakan 403, 409, dan 422, dan tidak menerima rincian kode `B*`/`W*` |
| Jadwal Saya selalu gagal | `getMyScheduleAssignments` melempar "belum tersedia di server" (`employee-self-service.js:124`) | Endpoint `my-schedule` belum punya fungsi pemanggil |
| Setelan kehadiran dan pratinjau tutup sudah ada | `getSettings`/`updateSettings`, `getPeriodClosePreview` meneruskan badan dan respons apa adanya | Ruas baru `v26` (`ScanWindow*`, `ConversionCandidates`, `HeldDeviceScanCount`) otomatis ikut; tidak perlu diubah |

---

## 2. Proses bisnis dari sisi pengguna

Task ini tidak menambah layar. Yang berubah adalah apa yang **akan** diterima layar-layar berikutnya:

1. Kepala Unit menekan **Ganti Petugas** pada roster terbit (`FE-HRD-307`). Lapisan ini mengirim petugas yang terlihat di layar (`expectedWorkforceProfileId`) dan alasan tiap peringatan. Bila server menolak, layar menerima `{ message, statusCode, errors }`:
   - `422`: pesan server, misalnya "Pengganti melanggar aturan B3.", beserta daftar kode `B*`/`W*`;
   - `409`: "Shift ini baru saja diubah orang lain. Muat ulang roster.";
   - `403`: unit di luar cakupan atau periode sudah ditutup.
2. Kepala Unit menghapus satu shift dari roster terbit dengan alasan. Sesudah berhasil, detail roster dimuat ulang otomatis.
3. Riwayat Revisi (`FE-HRD-308`) memanggil daftar revisi berhalaman dengan filter tanggal, pegawai, dan jenis perubahan.
4. Petugas IT membuka layar mesin absen (`FE-HRD-312`): daftar, tambah, ubah, aktif/nonaktif, hapus, serta daftar mesin tak dikenal beserta jumlah scan tertahannya.
5. Admin HRD mengelola hari libur (`FE-HRD-313`): pilihan kalender dan jenis libur dari `filters/metadata`, daftar, tambah, ubah, aktif/nonaktif, hapus.
6. Pegawai membuka Jadwal Saya (`FE-HRD-314`): jadwal sendiri dan roster unit per bulan. Tidak ada parameter ID pegawai; server membaca identitas dari sesi.
7. Pengguna laporan (`FE-HRD-315`) memilih salah satu dari tujuh laporan, melihat baris, kolom, dan label periode (`Open`/`Closed`), lalu mengekspor `.xlsx` dengan filter yang sama.

**Jalur tidak normal.** Kode laporan di luar tujuh kode kontrak ditolak sebelum permintaan dikirim. Bila ekspor gagal, badan galat yang berbentuk berkas diurai lebih dulu, sehingga pesan server (misalnya 403 "Unit ini di luar cakupan laporan Anda.") tetap sampai ke layar, bukan "Request failed with status code 403". Permintaan yang dibatalkan karena pengguna berpindah halaman tidak dianggap galat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, `test-policy.md`, `REPORT_TEMPLATE.md`; `api-contract.md` bagian 32; `03-frontend-architecture.md` bagian 31; controller backend `RosterPeriodController`, `ShiftReplacementController`, `HolidayController`, `AttendanceDeviceController`, `AttendanceReportController`, `MyScheduleSelfServiceController` beserta DTO request/respons; slice `roster-scheduling-slice.js`, `attendance-slice.js`, `master-data-resource-slice-factory.jsx`, `master-data-membership-tier-slice.jsx`; modal `QuickShiftReplacementModal.jsx` dan `RetroactiveShiftCorrectionModal.jsx` (untuk memastikan tidak ada konsumen thunk yang rusak).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/roster-scheduling-service.js` | Ganti Petugas dan koreksi mundur pindah ke path kontrak `shift-replacements`. Ganti Petugas mengirim `expectedWorkforceProfileId` dan `warningReasons` bila diisi. Fungsi baru `removePublishedShift` dan `getRosterRevisions` |
| `src/lib/services/hr/attendance-management-service.js` | Fungsi baru `getAttendanceReport`, `exportAttendanceReport` (berkas `.xlsx` + nama berkas dari `Content-Disposition`), `getUnregisteredAttendanceDevices`. Konstanta `ATTENDANCE_REPORT_CODES` (tujuh kode) dengan penjaga kode; utilitas `readAttachmentFileName` |
| `src/lib/services/hr/employee-self-service.js` | Fungsi baru `getMySchedule` dan `getMyUnitRoster`. `getMyScheduleAssignments` lama dibiarkan; penggantiannya di view adalah lingkup `FE-HRD-314` |
| `src/utils/hr/hr-api-utils.js` | `toHrApiError` (galat terstruktur `{ message, statusCode, errors }`, pesan server lebih dulu) dan `withHrJsonErrorBody` (mengurai badan galat Blob pada unduhan) |
| `src/lib/state/slice/hr/scheduling/roster-scheduling-slice.js` | Thunk baru `removePublishedShiftThunk` dan `fetchRosterRevisions`, state `revisions`. `directReplaceShiftThunk` dan `retroactiveCorrectShiftThunk` kini menolak dengan galat terstruktur. Keduanya belum punya konsumen (modal memanggil service langsung) |
| `src/lib/state/slice/hr/master-data/master-data-holiday-slice.jsx` (baru) | Slice hari libur memakai `createMasterDataResourceSlice`; hanya thunk ber-URL kontrak yang diekspor (metadata, daftar, detail, tambah, ubah, status, hapus) |
| `src/lib/state/slice/hr/master-data/master-data-attendance-device-slice.jsx` (baru) | Slice mesin absen memakai factory yang sama; metadata filter tidak diekspor karena tidak ada di kontrak |
| `src/lib/state/slice/hr/attendance/attendance-report-slice.js` (baru) | State laporan: kode laporan, filter, baris, kolom, nama laporan, `periodState`, galat terstruktur |
| `src/lib/state/store.jsx` | Mendaftarkan `masterDataHoliday`, `masterDataAttendanceDevice`, `attendanceReport` |
| `tests/unit/hr-a52-api-client.test.mjs` (baru) | 12 uji: URL dan metode tiap fungsi, daftar putih URL kontrak `v26`, galat 403/409/422, ekspor dan galat Blob, pengisian state |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi tetap `state/service → InstanceAxios`; tidak ada instance Axios baru. Slice master data memakai factory baku `createMasterDataResourceSlice`, seperti slice tingkat membership. Slice laporan mengikuti bentuk `attendance-slice.js` (filter, thunk dengan `signal`, pembatalan bukan galat). Utilitas galat ditaruh di `src/utils/hr` (tanpa request, tanpa Redux). Helper Blob serupa sudah ada di modul Aset (`asset-record-utils.js`), tetapi sengaja tidak diimpor agar HR tidak bergantung pada utilitas modul lain. Tidak ada pola baru.

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` untuk tampilan — task ini tidak membuat layar. Yang disiapkan untuk layar:

| State | Yang disediakan lapisan ini |
| --- | --- |
| Memuat | `revisions.loading`, `attendanceReport.loading`, dan penanda memuat bawaan factory (`listLoading`, `actionLoading`) |
| Kosong | `items: []` dengan `totalData: 0` dari respons berhalaman |
| Gagal | `{ message, statusCode, errors }` dengan pesan server; tanpa pesan server dipakai peta kode HTTP berbahasa Indonesia |
| Tanpa hak akses | `statusCode: 403` beserta pesan server, sehingga layar dapat menampilkan "tidak berwenang" |

---

## 5. Endpoint yang dikonsumsi

#### HR Scheduling - Roster Period

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/shifts/{shiftAssignmentId}/remove` | Hapus satu shift dari roster terbit dengan alasan | `RosterPeriod : RemoveShift` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/revisions` | Riwayat Revisi berhalaman | `RosterPeriod : Read` |

#### HR Scheduling - Shift Assignment & Replacement

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/human-resource/scheduling-management/shift-replacements/{id}/replace` | Ganti Petugas (+ `expectedWorkforceProfileId`, `warningReasons`) | `ShiftReplacement : Update` |
| `POST` | `/v1/corporate/human-resource/scheduling-management/shift-replacements/{id}/retroactive-correct` | Koreksi mundur | `ShiftReplacement : Update` |

#### Corporate / Human Resource / Master Data / Holiday

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/holidays/filters/metadata` | Pilihan kalender kerja dan jenis libur | `Holiday : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/holidays` | Daftar | `Holiday : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/holidays/{id}` | Detail | `Holiday : Read` |
| `POST` | `/v1/corporate/human-resource/master-data/holidays` | Tambah | `Holiday : Create` |
| `PUT` | `/v1/corporate/human-resource/master-data/holidays/{id}` | Ubah | `Holiday : Update` |
| `PATCH` | `/v1/corporate/human-resource/master-data/holidays/{id}/status` | Aktif/nonaktif | `Holiday : Update` |
| `DELETE` | `/v1/corporate/human-resource/master-data/holidays/{id}` | Hapus | `Holiday : Delete` |

#### Corporate / Human Resource / Master Data / Attendance Device

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/attendance-devices` | Daftar mesin | `AttendanceDevice : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/attendance-devices/unregistered` | Mesin tak dikenal + jumlah scan tertahan | `AttendanceDevice : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/attendance-devices/{id}` | Detail | `AttendanceDevice : Read` |
| `POST` | `/v1/corporate/human-resource/master-data/attendance-devices` | Daftarkan mesin | `AttendanceDevice : Create` |
| `PUT` | `/v1/corporate/human-resource/master-data/attendance-devices/{id}` | Ubah | `AttendanceDevice : Update` |
| `PATCH` | `/v1/corporate/human-resource/master-data/attendance-devices/{id}/status` | Aktif/nonaktif | `AttendanceDevice : Update` |
| `DELETE` | `/v1/corporate/human-resource/master-data/attendance-devices/{id}` | Hapus | `AttendanceDevice : Delete` |

#### Corporate / Human Resource / Attendance Management / Attendance Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance/reports/{reportCode}` | Laporan di layar (`shift`, `attendance`, `leave`, `lateness`, `overtime`, `absence`, `unit-summary`) | `AttendanceReport : Read` |
| `GET` | `/v1/corporate/human-resource/attendance/reports/{reportCode}/export` | Unduh `.xlsx` dengan filter yang sama | `AttendanceReport : Export` |

#### Self Services / Human Resource / My Schedule

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/self-services/human-resource/my-schedule` | Jadwal sendiri (`fromDate`, `toDate`) | `MySchedule : Read` |
| `GET` | `/v1/self-services/human-resource/my-schedule/unit-roster` | Roster unit sendiri (`month`) | `MySchedule : Read` |

Setelan kehadiran (`GET`/`PUT .../attendance-management/settings`) dan pratinjau tutup (`GET .../attendance/periods/{id}/close-preview`) tetap memakai fungsi lama yang meneruskan ruas `v26` apa adanya.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hr-a52-api-client.test.mjs` | 12/12 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif: 7 mutasi (path lama `shift-assignments`; penjaga kode laporan dilepas; kode status tidak diteruskan; badan galat Blob tidak diurai; thunk kembali ke teks saja; `expectedWorkforceProfileId` hilang; path status mesin salah) | 7/7 tertangkap; source dipulihkan | `PASS` | `mut306.cjs` di scratchpad sesi |
| `npm run test:unit` (seluruh suite) | 5.226 lulus, 100 gagal dari 5.326 | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan ada di modul lain (Payroll, Rawat Inap, Bedah, Hemodialisis, Billing, menu). Dari berkas gagal yang mengimpor berkas terkait task ini: ESS-004 memeriksa isi view yang tidak disentuh; `workforce-core-cross-employee` gagal `ENOENT` pada berkas Payroll yang tidak ada; `workforce-core-target-state` memeriksa route `initial-placement-batches`. Tidak ada kegagalan dari perubahan task ini |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npx eslint` atas 10 berkas yang berubah | Tanpa error maupun peringatan | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil; 921 halaman; standalone siap | `PASS` | Keluaran perintah |

AUTOMATED TEST: `npm run test:unit` — PASS untuk berkas task (12/12); suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `NOT APPLICABLE` — task ini tidak mengubah kontrol interaktif mana pun. Verifikasi di layar berada pada `FE-HRD-307` s.d. `FE-HRD-316`.

**Tidak dijalankan:** uji API terhadap backend lokal; permintaan nyata dijalankan bersama layar di task berikutnya.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada URL yang tidak ada di kontrak `v26` | Terpenuhi | Uji `tidak ada URL di luar kontrak v26…` memanggil 26 fungsi/thunk dan mencocokkan setiap URL dengan daftar putih `api-contract.md` 32; kode laporan di luar tujuh kode ditolak tanpa request; Ganti Petugas/koreksi mundur pindah ke `shift-replacements`; metadata filter mesin absen sengaja tidak diekspor |
| Galat 403/409/422 diteruskan dengan pesan server | Terpenuhi | Uji `thunk roster dan laporan meneruskan galat 403/409/422…` (422 + kode `B3`, 409, 403 pada revisi dan laporan); uji ekspor gagal 403 dengan badan Blob; slice factory meneruskan `message` dan `statusCode` server |
| DoD: uji unit service (URL dan metode per fungsi) | Terpenuhi | 12 uji di `hr-a52-api-client.test.mjs` |
| DoD: laporan di `task/report/frontend/FE-HRD-306.md` | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Backend juga menyediakan `GET .../attendance-devices/filters/metadata` yang tidak tercantum di kontrak `v26`; lapisan ini tidak memakainya. Bila layar mesin absen membutuhkannya, kontrak perlu dimutakhirkan lebih dulu |
| Masalah yang diketahui | `getMyScheduleAssignments` lama masih ada dan masih dipakai `schedule-employee-view.jsx`; dihapus saat view disambungkan di `FE-HRD-314`. Komentar view `QuickShiftReplacementModal.jsx`/`RetroactiveShiftCorrectionModal.jsx` masih menyebut path `shift-assignments` (akan disentuh `FE-HRD-307`) |
| Dependency backend | `NONE` — seluruh endpoint MVP sudah tersedia |
| Perubahan sampingan | `NONE`. Mutasi kontrol negatif dipulihkan otomatis; `git diff` sesudahnya hanya berisi perubahan task |
| Interupsi | `NONE` |
| Status Git | ` M src/lib/services/hr/attendance-management-service.js`, ` M src/lib/services/hr/employee-self-service.js`, ` M src/lib/services/hr/roster-scheduling-service.js`, ` M src/lib/state/slice/hr/scheduling/roster-scheduling-slice.js`, ` M src/lib/state/store.jsx`, ` M src/utils/hr/hr-api-utils.js`, `?? src/lib/state/slice/hr/attendance/attendance-report-slice.js`, `?? src/lib/state/slice/hr/master-data/master-data-attendance-device-slice.jsx`, `?? src/lib/state/slice/hr/master-data/master-data-holiday-slice.jsx`, `?? tests/unit/hr-a52-api-client.test.mjs` |
| Langkah berikutnya | `FE-HRD-307` (matriks roster: Ganti Petugas, tambah dan hapus shift pada roster terbit); `FE-HRD-308` s.d. `FE-HRD-315` dapat menyusul karena seluruhnya hanya bergantung pada task ini |
