# Laporan Perubahan Frontend — `FE-HRD-308`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-308` |
| Judul | Layar Riwayat Revisi |
| Slice | `A52` / `MVP-67` / `EPIC HRD-110` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-308` |
| Trace | `FR-PK-004`; `HRD-DEC-195` (butir 3 dan 5), `HRD-DEC-217`; `03-frontend-architecture.md` 31.1 (rute usulan), 31.2 (baris Riwayat Revisi), 31.3 |
| Contract version | `v26` — `api-contract.md` bagian 32 (`GET roster-periods/{id}/revisions`), `approved` 12 November 2026 |
| Wewenang UI | Layar baru `/hr/scheduling/revisions` (rute `DEV_DISCRETION`), tombol **Riwayat Revisi** di Kisi Roster, satu butir menu di grup Penjadwalan. Tata letak dan urutan kolom `DEV_DISCRETION`; base component wajib |
| Keputusan tambahan | Pengguna, 23 November 2026: "catatan alasan hanya untuk pengguna berhak" = **semua pembaca roster** (pemegang `RosterPeriod : Read` dalam cakupan unit, diperiksa server). Di tabel hanya kategori; catatan dibuka per baris |
| Dependency | `FE-HRD-306` ✅, `BE-HRD-340` ✅ |
| Klasifikasi | `MEDIUM` — satu layar baru, satu utilitas, konstanta, menu, tautan dari kisi, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 23 November 2026: "lanjut sesuai saran") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/app/hr/scheduling/revisions`, `src/components/view/hr/scheduling`, `src/utils/hr/scheduling`, `src/lib/constants/hr/scheduling`, `src/utils/menu-sidebar/menu-items.jsx`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `52d3392d2` (branch `YogaV2`) + working tree `FE-HRD-306`/`307` |
| Commit backend yang dijadikan rujukan | `8a68ab67` (branch `yoga`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi di browser dengan API tiruan; UAT dengan data nyata devYoga belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Belum ada layar Riwayat Revisi | Tidak ada rute atau pemanggilan `.../revisions` di `src/`; klien `getRosterRevisions` baru tersedia sejak `FE-HRD-306` | Perubahan roster terbit (`HRD-DEC-195` butir 3) tidak bisa ditelusuri dari aplikasi |
| Backend mengirim catatan alasan ke setiap pembaca | `RosterPeriodService.GetRevisionsAsync` memeriksa cakupan baca unit lalu mengisi `ReasonNote` apa adanya; desain menandai `ReasonNote` sensitif (`02-backend-architecture.md` 9431) | Kartu meminta "catatan alasan hanya untuk pengguna berhak" tanpa definisi; ditanyakan ke pengguna (lihat Metadata) |
| Tidak ada `filters/metadata` untuk Riwayat Revisi | Kontrak §32 hanya memuat `GET .../revisions` | Pilihan jenis perubahan memakai empat kode dari dokumentasi DTO (`ShiftRevisionResponse.ChangeType`). Label di tabel tetap dari `ChangeTypeLabel` server |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: Kepala Unit, Kepala Bidang, dan Admin HRD yang memegang hak baca roster.

1. Pengguna membuka **Riwayat Revisi Roster** dari menu Penjadwalan, atau dari tombol **Riwayat Revisi** di Kisi Roster. Dari kisi, periode yang sama langsung terpilih (`?periodId=`).
2. Bila belum ada periode terpilih, layar meminta "Pilih periode roster untuk menampilkan Riwayat Revisi." dan belum memanggil riwayat apa pun.
3. Sesudah periode dipilih, tabel menampilkan satu baris per perubahan. Contoh sesuai `HRD-DEC-195`:

   | Tanggal Shift | Jenis Perubahan | Sebelum | Sesudah | Alasan | Diubah Oleh |
   | --- | --- | --- | --- | --- | --- |
   | 14 Feb 2027, Malam 21.00–07.00 | Ganti Petugas | Budi | Tono | Petugas sakit mendadak · **Lihat catatan** | Sari, 14 Feb 2027 19.04 |
   | 21 Feb 2027 | Hapus Shift | Tono · Siang | — | Alasan operasional lain | Sari |
   | 10 Feb 2027 | Koreksi Mundur | Malam 21.00–07.00 | Siang 14.00–21.00 | Salah catat · **Lihat catatan** | Sari |
   | 18 Feb 2027 | Tambah Shift | — | Budi · Pagi | Lonjakan pasien tak terduga · **Lihat catatan** | Sari |

   Jam ditampilkan dalam WIB.
4. Pengguna dapat menyaring berdasarkan tanggal awal–akhir, pegawai (dari daftar staf periode), dan jenis perubahan. Filter dapat digabung, dan tombol atur ulang mengosongkan semuanya. Setiap perubahan filter mengembalikan ke halaman 1.
5. **Lihat catatan** membuka jendela berisi tanggal shift, jenis perubahan, kategori, dan catatan alasan. Catatan tidak pernah tampil langsung di tabel, karena bisa memuat kondisi kesehatan pegawai.
6. Tidak ada tombol ubah atau hapus; riwayat hanya dapat dibaca (`HRD-DEC-195` butir 3).

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Tanggal akhir sebelum tanggal awal | "Tanggal akhir tidak boleh sebelum tanggal awal."; tidak ada permintaan ke server sampai rentang diperbaiki |
| Periode belum punya perubahan | "Belum ada perubahan pada roster ini." beserta penjelasan jenis perubahan yang tercatat |
| Unit periode di luar cakupan baca (403) | Pesan server "Anda tidak berwenang melihat data unit ini." dan tombol **Coba Lagi**; tabel tidak ditampilkan, sehingga tidak terbaca sebagai "belum ada perubahan" |
| Tanpa hak `RosterPeriod : Read` | "Anda tidak berwenang membuka Riwayat Revisi roster."; tidak ada permintaan riwayat; butir menu tidak tampil |
| Gagal memuat daftar periode atau detail periode | Pesan galat server di atas filter |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-308`; `api-contract.md` 32; `03-frontend-architecture.md` 31.1–31.3; `00-interview-decisions.md` `HRD-DEC-195`, `HRD-DEC-217`; `02-backend-architecture.md` (entitas `HrdShiftAssignmentRevision`, penanda sensitif `ReasonNote`); `RosterPeriodController.GetRevisions`, `RosterPeriodService.GetRevisionsAsync`, DTO `ShiftRevisionResponse`; `roster-scheduling-view.jsx` (pola daftar), `attendance-monitoring-view.jsx` (filter tanggal), `data-filter.jsx`, `filter-date-picker.jsx`, `status-badge.jsx`, `use-hr-remote-data.js`, `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/scheduling/revisions/page.jsx` (baru) | Rute `/hr/scheduling/revisions` dengan `Suspense` (layar membaca `?periodId=`) |
| `src/components/view/hr/scheduling/roster-revision-history-view.jsx` (baru) | Layar baca-saja: pilih periode, filter tanggal/pegawai/jenis, tabel berhalaman, jendela catatan, keadaan memuat/kosong/galat/tanpa hak, Coba Lagi |
| `src/utils/hr/scheduling/roster-revision-utils.js` (baru) | `buildRevisionQuery`, `validateRevisionDateRange`, `describeRevisionChange` (sisi sebelum/sesudah per jenis), `formatRevisionTime` (WIB), `revisionReasonLabel`, `buildRevisionStaffOptions` |
| `src/lib/constants/hr/scheduling/scheduling-constants.js` | Rute `revisions` dan `revisionsOfPeriod`; `ROSTER_REVISION_CHANGE_TYPES` (empat kode, label dan warna lencana) beserta opsi filternya |
| `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx` | Tombol **Riwayat Revisi** di kepala Kisi Roster (bila berhak baca) menuju periode yang sama |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir **Riwayat Revisi Roster** di grup Penjadwalan, `RosterPeriod : Read` |
| `tests/unit/hr-roster-revision-utils.test.mjs` (baru) | 7 uji utilitas |
| `tests/e2e/hr-roster-revision-history.spec.mjs` (baru) | 8 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Layar memakai pola daftar penjadwalan yang sudah ada (`useHrRemoteData` + `RosterSchedulingService`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `DataTable` + `RegionPagination`, `StatusBadge`, `ConfirmModal`, `HrInfoGrid`, `InformationAlert`, `BaseButton`), sama dengan `roster-scheduling-view.jsx`. Thunk `fetchRosterRevisions` dari `FE-HRD-306` sengaja tidak dipakai, agar modul penjadwalan tidak memakai dua pola state untuk layar sejenis; thunk itu tetap tersedia. `page.jsx` hanya merakit view. Logika murni ada di `src/utils/hr/scheduling`. Tidak ada gaya atau warna baru: lencana memakai kunci `StatusBadge` yang tersedia (`warning`, `info`, `active`, `danger`).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan "Memuat Riwayat Revisi..."; filter periode/pegawai nonaktif selama datanya dimuat |
| Kosong | Belum memilih periode: "Pilih periode roster untuk menampilkan Riwayat Revisi."; periode tanpa perubahan: "Belum ada perubahan pada roster ini." |
| Gagal | Pesan server; tabel disembunyikan; tombol **Coba Lagi** memuat ulang |
| Tanpa hak akses | "Anda tidak berwenang membuka Riwayat Revisi roster." (tanpa `RosterPeriod : Read`); 403 cakupan unit menampilkan pesan server |

---

## 5. Endpoint yang dikonsumsi

#### HR Scheduling - Roster Period

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/roster-periods` | Pilihan periode roster (100 terbaru) | `RosterPeriod : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/roster-periods/{id}` | Keterangan unit/rentang dan pilihan pegawai | `RosterPeriod : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/revisions` | Riwayat Revisi (`fromDate`, `toDate`, `workforceProfileId`, `changeType`, `pageNumber`, `pageSize`) | `RosterPeriod : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-roster-revision-history.spec.mjs` (build produksi, port 3710, API tiruan penuh, nol tulis) | 8/8 lulus | `PASS` | Keluaran Playwright |
| — Tanpa periode lalu pilih periode | Arahan tampil tanpa permintaan riwayat; memilih periode mengisi `?periodId=` dan memuat riwayat dengan `pageNumber=1&pageSize=20` | `PASS` | Skenario 1 |
| — Bagian riwayat `UAT-PK-03`/`UAT-PK-06` | Baris Ganti Petugas Budi → Tono "Malam 21.00–07.00", Hapus Shift "Tono · Siang", Koreksi Mundur, Tambah Shift; tidak ada tombol selain "Lihat catatan"; baris tanpa catatan tanpa tombol | `PASS` | Skenario 2 |
| — Catatan per baris | Catatan tidak ada di halaman sampai **Lihat catatan** diklik; jendela menampilkan catatan dan dapat ditutup | `PASS` | Skenario 3 |
| — Filter pegawai + jenis, atur ulang | Permintaan membawa `workforceProfileId`, lalu juga `changeType`; baris 4 → 2 → 1; atur ulang kembali ke 4 dan query bersih | `PASS` | Skenario 4 |
| — Filter tanggal | `fromDate`/`toDate` terkirim; rentang terbalik menampilkan pesan dan tidak mengirim permintaan; diperbaiki → terkirim dengan kedua tanggal | `PASS` | Skenario 5 |
| — 403 di luar cakupan | Pesan server dan Coba Lagi; tabel tidak dirender | `PASS` | Skenario 6 |
| — Tanpa `RosterPeriod : Read` | Pesan tidak berwenang; tanpa permintaan riwayat | `PASS` | Skenario 7 |
| — Tombol dari Kisi Roster | Membuka `/hr/scheduling/revisions?periodId=…` periode yang sama | `PASS` | Skenario 8 |
| Putaran pertama uji layar | 17/18 lulus. Skenario 403 gagal karena tabel tetap tampil dengan keadaan kosong "Belum ada perubahan pada roster ini." saat galat — menyesatkan. Layar diperbaiki: tabel tidak dirender saat galat | `NEW ERROR` → diperbaiki | Keluaran Playwright |
| Regresi uji layar `FE-HRD-307` (`hr-roster-published-change.spec.mjs`) sesudah tombol baru di kisi | 10/10 lulus (total 18/18 bersama spec baru) | `PASS` | Keluaran Playwright |
| `tests/unit/hr-roster-revision-utils.test.mjs` | 7/7 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif utilitas: 6 mutasi (filter kosong ikut dikirim; rentang terbalik diterima; jam bukan WIB; Hapus Shift tanpa sisi sebelum; kategori tanpa label; pegawai ganda) | 6/6 tertangkap sesudah uji pegawai ganda diperketat (putaran pertama 5/6: `Map` sudah membuang kunci ganda, sehingga mutasi hanya berbeda pada butir yang dipertahankan) | `PASS` | `mut308.cjs` di scratchpad sesi |
| `npm run test:unit` (seluruh suite) | 5.237 lulus, 100 gagal dari 5.337 | `EXISTING / ENVIRONMENT ISSUE` | Daftar 100 uji yang gagal identik per nama dengan putaran `FE-HRD-306`; seluruhnya di modul lain |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil | `PASS` | Keluaran perintah |

AUTOMATED TEST: `npm run test:unit` — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — seluruh kontrol baru (pilih periode, Tanggal Awal/Akhir, pegawai, jenis perubahan, atur ulang, paginasi via query, Lihat catatan, Tutup, Coba Lagi, Riwayat Revisi di kisi) dijalankan di peramban Chromium lewat Playwright terhadap build produksi.

**Tidak dijalankan:** riwayat dengan data roster nyata di devYoga (bergantung pada perubahan nyata dari `UAT-PK-03`/`-06`, yang belum dijalankan karena menulis ke basis data bersama). Bentuk respons yang ditirukan mengikuti `ShiftRevisionResponse` di source backend; perilaku backend dibuktikan `AT-HRD-PK-01`, `-07`, `-08` (laporan `BE-HRD-339`, `BE-HRD-340`). `npx prettier --check` memberi peringatan, sama seperti berkas tetangga di folder ini; repository tidak memiliki konfigurasi Prettier.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada tombol ubah/hapus | Terpenuhi | Skenario 2 (tombol tabel hanya "Lihat catatan"; tidak ada Ubah/Hapus/Edit di halaman) |
| Filter tanggal, pegawai, jenis perubahan | Terpenuhi | Skenario 4 dan 5 (query terkirim, filter digabung, atur ulang, rentang terbalik ditolak) |
| Catatan alasan hanya untuk pengguna berhak | Terpenuhi menurut keputusan pengguna | Berhak = pembaca roster dalam cakupan unit (diperiksa server; skenario 6 dan 7). Catatan tidak tampil di tabel, hanya dibuka per baris (skenario 3) |
| Outcome: Kepala Unit, Kepala Bidang, dan HRD menelusuri perubahan roster terbit | Terpenuhi | Menu `RosterPeriod : Read` dan tombol dari kisi (skenario 8) |
| DoD umum: base component, loading/kosong/galat/coba lagi, 403 "tidak berwenang", uji unit dan browser | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-03`, `UAT-PK-06` (bagian riwayat) | Terpenuhi dengan API tiruan; data nyata devYoga `NOT RUN` | Skenario 2 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pilihan periode memuat 100 periode terbaru dalam satu halaman; bila kelak lebih banyak, perlu pencarian di sisi server. Bila pengguna kelak harus dibedakan untuk catatan alasan, server perlu mengosongkan `ReasonNote` (layar sudah menyembunyikan catatan sampai dibuka) |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | Playwright mengubah `test-results/.last-run.json` (ter-track); dipulihkan dari `HEAD` dengan akhir baris CRLF |
| Interupsi | Sesi berakhir sesudah putaran uji layar pertama; dilanjutkan dari `git status` yang sama, tanpa mengulang langkah yang sudah terverifikasi |
| Status Git | `?? src/app/hr/scheduling/revisions/`, `?? src/components/view/hr/scheduling/roster-revision-history-view.jsx`, `?? src/utils/hr/scheduling/roster-revision-utils.js`, `?? tests/e2e/hr-roster-revision-history.spec.mjs`, `?? tests/unit/hr-roster-revision-utils.test.mjs`, ` M src/lib/constants/hr/scheduling/scheduling-constants.js`, ` M src/utils/menu-sidebar/menu-items.jsx`, ` M src/components/view/hr/scheduling/roster-matrix-detail-view.jsx`; ditambah berkas `FE-HRD-306`/`307` yang belum di-commit |
| Langkah berikutnya | `FE-HRD-309` (setelan kehadiran: jendela scan, tanpa pilihan konversi) |
