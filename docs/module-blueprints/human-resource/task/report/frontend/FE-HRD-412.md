# Laporan Perubahan Frontend — `FE-HRD-412`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-412` |
| Judul | Tutup periode menyebut pengecualian yang diselesaikan |
| Slice | `A52-B` / `MVP-78` · `EPIC HRD-132` (`MUST HAVE`) |
| Roadmap | `roadmap/frontend-roadmap.md`, kartu `FE-HRD-412` (amandemen `A52-B`) |
| Trace | `FR-PK-034`; `HRD-DEC-249`; `VR-PK-31`; `api-contract.md` 37.3; desain `03` 36.3 |
| Contract version | `v31` — approved 24 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Kalimat dan letaknya mengikuti desain 36.3 ("{n} pengecualian scan akan diselesaikan oleh penutupan"). Layar yang diubah: `/hr/attendance/periods` (`F-T2`); route tidak berubah. Komponen dasar: `ConfirmModal`, `InformationAlert`, `ToastStack` |
| Dependency | `FE-HRD-410` ✅; `BE-HRD-464` ✅ (ruas `conversionExceptionCount`, `closedConversionExceptionCount`); bukti API data asli ada di `BE-HRD-468` |
| Klasifikasi | `SMALL` — satu utilitas, satu dialog, satu view diubah; satu uji unit dan satu spec E2E baru; tanpa service, slice, atau route baru |
| Task mode | `FRONTEND` (instruksi pengguna 27 November 2026: "kerjakan FE-HRD-412") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/utils/hr/attendance`, `src/components/view/hr/attendance`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`) + perubahan `FE-HRD-410`/`411` yang belum di-commit + perubahan Lab milik sesi lain (tidak disentuh) |
| Commit backend yang dijadikan rujukan | `cce1dbf5` (branch `yoga`) + perubahan `BE-HRD-467`/`468` yang belum di-commit |
| Tanggal | 27 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Dialog tutup (`FE-HRD-311`) tidak membaca `conversionExceptionCount` | `summarizeClosePreview` di `attendance-period-close-utils.js` | Admin HRD tidak tahu bahwa pengecualian lupa scan pada hari yang dikonversi ikut ditutup |
| Pesan berhasil hanya menyebut hari yang dikonversi | `attendance-periods-view.jsx`, `submitClose` | Jumlah pengecualian yang diselesaikan (`closedConversionExceptionCount`) tidak terlihat |
| `409` saat tutup hanya membawa pesan isu pemblokir pertama | `AttendancePeriodService.CloseAsync`: `Fail(409, firstIssue)` tanpa daftar | Bila pemblokir muncul di antara pratinjau dan tutup, dialog hanya menampilkan satu kalimat; daftar pemblokir dan jumlahnya tidak tampil, dan tombol tutup tetap aktif karena pratinjau lama masih "dapat ditutup" |
| Periode presensi devYoga | `GET attendance/periods` (baca-saja): `ATP-202611-0001` dan `ATP-202610-0001`, keduanya `Closed` | Tidak ada periode Open untuk uji layar data asli tanpa menulis data |

---

## 2. Proses bisnis dari sisi pengguna

Contoh dari `BE-HRD-464`. Rina lupa scan pulang pada 10 Februari 2027, sehingga hari itu Belum Lengkap dan pemrosesan presensi membuat pengecualian "lupa scan pulang" yang dulu memblokir tutup.

1. Admin HRD membuka **Periode Presensi** dan menekan **Tutup** pada `ATP-202702-…`.
2. Dialog menampilkan: "Saat periode ditutup, 1 hari Belum Lengkap atau Tidak Ada Scan akan menjadi Tidak hadir. **1 pengecualian scan akan diselesaikan oleh penutupan.** Tindakan ini tidak dapat dibatalkan kecuali lewat Buka Darurat." Daftar hari (Rina, 10 Feb) tetap tampil seperti sebelumnya.
3. Admin HRD mengisi alasan lalu menekan **Tutup & Konversi 1 Hari**. Pesan berhasil berbunyi: "Periode … ditutup. 1 hari dikonversi menjadi Tidak hadir. **1 pengecualian scan diselesaikan oleh penutupan.**"

Jalur lain:

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Tidak ada pengecualian scan (`0`) atau server lama tanpa ruas | Kalimat dan pesan sama persis seperti sebelum task ini |
| Periode terblokir pemblokir lain (mis. jadwal bentrok, koreksi aktif) | "Periode belum dapat ditutup…", daftar pemblokir dengan jumlahnya, dan bila ada catatan "2 pengecualian scan akan diselesaikan oleh penutupan." Tombol tutup terkunci |
| Pemblokir muncul sesudah pratinjau dibuka (`409` saat tutup) | Pesan server tetap tampil. Pratinjau dimuat ulang sehingga daftar pemblokir tampil dan tombol tutup terkunci. Alasan yang sudah diketik tidak hilang, dan daftar periode tidak dimuat ulang (status tetap) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; kartu `FE-HRD-412`; kontrak `api-contract.md` 37.3 dan `VR-PK-31`; desain `03` 36.3; laporan `BE-HRD-464`, `BE-HRD-468`; `AttendancePeriodService.CloseAsync` (bentuk `409` dan respons); `attendance-periods-view.jsx`, `attendance-period-close-dialog.jsx`, `attendance-period-close-utils.js`; `attendance-management-service.js` (`getPeriodClosePreview`, `closePeriod`); spec `hr-attendance-period-close.spec.mjs` dan uji unit `hr-attendance-period-close-utils.test.mjs` (`FE-HRD-311`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/hr/attendance/attendance-period-close-utils.js` | `summarizeClosePreview` membaca `conversionExceptionCount` (bilangan bulat ≥ 0; tidak ada/tidak sah = 0). `buildConversionExceptionNote` menghasilkan "{n} pengecualian scan akan diselesaikan oleh penutupan." hanya bila > 0. `buildCloseConfirmationText` menyisipkan kalimat itu sebelum "Tindakan ini tidak dapat dibatalkan…". `buildCloseSuccessMessage` membentuk pesan berhasil dan menambah "{m} pengecualian scan diselesaikan oleh penutupan." bila `closedConversionExceptionCount` > 0 |
| `src/components/view/hr/attendance/attendance-period-close-dialog.jsx` | Saat periode terblokir (kalimat konfirmasi tidak tampil), catatan jumlah pengecualian scan ditampilkan tersendiri (`InformationAlert` info) di bawah peringatan terblokir |
| `src/components/view/hr/attendance/attendance-periods-view.jsx` | `refreshPreview(periodId)` memuat pratinjau tanpa menghapus alasan dan pesan galat aksi; dipakai `openClose` dan sesudah `409` pada `submitClose`. Pesan berhasil memakai `buildCloseSuccessMessage` |
| `tests/unit/hr-attendance-period-close-exceptions.test.mjs` (baru) | 5 uji: AC1 (> 0; 0/tidak ada/tidak sah; terblokir), AC2 (disebut; 0/respons lama) |
| `tests/e2e/hr-attendance-period-close-exceptions.spec.mjs` (baru) | 4 uji Playwright API tiruan: AC1+AC2, kalimat tidak tampil untuk 0/ruas tidak ada, terblokir, `409` |

### 3.3 Kepatuhan arsitektur frontend

- Tidak ada service, slice, atau hook baru. Respons diperluas dibaca dari fungsi service yang ada (`FE-HRD-410` menyatakan tutup periode tidak butuh fungsi baru).
- Teks dibentuk di utilitas murni yang sudah menjadi sumber kalimat dialog (`FE-HRD-311`), sehingga dapat diuji `node:test`.
- Pesan galat tetap dari server (`getHrApiErrorMessage`). Layar tidak menulis pesan pemblokir sendiri; daftar pemblokir berasal dari pratinjau server.

### 3.4 Delta terhadap kartu dan kontrak

| Hal | Kartu/kontrak | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| "Pratinjau dan konfirmasi" | Dua tempat | Di layar ini pratinjau dan konfirmasi berada di satu dialog. Kalimat masuk ke teks konfirmasi saat periode dapat ditutup, dan tampil sebagai catatan tersendiri saat terblokir | Kalimat tidak tampil dua kali di dialog yang sama |
| AC3 "`409` pemblokir lain tetap menampilkan daftar isu" | — | Sesudah `409` saat tutup, pratinjau dimuat ulang | `409` dari server hanya membawa pesan isu pertama, tanpa daftar. Daftar diambil dari pratinjau server; layar tidak mengarang isi daftar |
| Pesan berhasil bila jumlah `0` | AC2 "menyebut jumlah yang diselesaikan" | Disebut hanya bila > 0 | Sama dengan aturan AC1 (jumlah 0 → kalimat tidak tampil); respons server lama tanpa ruas tetap menghasilkan pesan lama |
| Pesan berhasil server | `BE-HRD-464` menambah jumlah ke pesan server | Layar membentuk pesan dari ruas angka, bukan dari teks server | Mengikuti pola `FE-HRD-311` (label status dari metadata); ruas angka adalah sumber resmi menurut kontrak |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah: "Memuat pratinjau penutupan…", tombol tutup terkunci. Juga berlaku saat pratinjau dimuat ulang sesudah `409` |
| Kosong | Tidak berubah: tanpa hari calon konversi → kalimat "Tidak ada hari Belum Lengkap…" (tanpa kalimat pengecualian, karena pengecualian konversi selalu berada pada hari calon konversi) |
| Gagal | Tidak berubah: pesan server di dialog. Bila pratinjau yang dimuat ulang gagal, pesan pratinjau tampil dan tombol tutup terkunci |
| Tanpa hak akses | Tidak berubah: tombol Tutup hanya untuk `AttendancePeriod : Close`; `403` server tampil sebagai pesan |
| Basi / bentrok (`409`) | Pesan server + daftar pemblokir dari pratinjau terbaru; tombol tutup terkunci bila server menyatakan belum dapat ditutup |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Attendance / Period Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance/periods/{id}/close-preview` | Pratinjau; ruas baru `conversionExceptionCount`. Dipanggil saat dialog dibuka dan sekali lagi sesudah `409` | `AttendancePeriod : Read` |
| `POST` | `/v1/corporate/human-resource/attendance/periods/{id}/close` | Tutup `{ reason }` (tidak berubah); ruas baru `closedConversionExceptionCount` | `AttendancePeriod : Close` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hr-attendance-period-close-exceptions.test.mjs tests/unit/hr-attendance-period-close-utils.test.mjs` | 10/10 lulus (5 baru + 5 `FE-HRD-311`) | `PASS` | Keluaran perintah |
| Kontrol negatif unit: 7 mutasi (jumlah tidak dibaca; jumlah tanpa pembulatan/penyaring; kalimat walau 0; konfirmasi tanpa kalimat; pesan berhasil tanpa jumlah; pesan berhasil menyebut walau 0; pesan memakai jumlah hari, bukan pengecualian) | 7/7 tertangkap; source pulih (hash `76269b59e32d` sama) | `PASS` | `mut412.cjs`, `mut412-hasil.log` di scratchpad sesi |
| E2E API tiruan `hr-attendance-period-close-exceptions.spec.mjs` | 4/4 lulus | `PASS` | Keluaran Playwright |
| Kontrol negatif E2E: 4 mutasi pada dialog/view (catatan saat terblokir dibuang; `409` tanpa muat ulang pratinjau; pesan berhasil versi lama; muat ulang pratinjau menghapus pesan galat) | 4/4 tertangkap; berkas pulih (hash sama). Dua mutasi view sempat lolos pada jeda HMR 5 detik, dan mutasi ke-4 sekali lagi pada jeda 12/25 detik, karena server dev belum memuat ulang berkas. Dengan jeda lebih panjang ketiganya tertangkap, dan mutasi ke-4 tertangkap baik lewat spec debug sementara (dihapus) maupun skrip | `PASS` | `mut412e2e.cjs`, `mut412e2e-4.cjs`, `mut412e2e-hasil.log` |
| Regresi E2E: `hr-attendance-period-close.spec.mjs` (`FE-HRD-311`, 5 uji) + spec baru | 9/9 lulus sesudah mutasi dipulihkan | `PASS` | `e2e412-akhir.log` |
| `npx eslint` atas lima berkas yang diubah/baru | 0 galat, 0 peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` (seluruh repository) | Tanpa galat | `PASS` | `lint412.log` |
| `npm run build` | "Compiled successfully in 102s", `prepare-standalone` siap | `PASS` | `build412.log` |
| Suite unit penuh | 5.458 lulus, 152 gagal dari 5.610. Jumlah gagal sama dengan baseline `FE-HRD-410`/`411`; tidak satu pun dari berkas task ini | `EXISTING / ENVIRONMENT ISSUE` | `unit412.log` |
| Data asli devYoga (baca-saja, backend lokal 7184) | Kedua periode (`ATP-202611-0001`, `ATP-202610-0001`) `Closed`; tidak ada periode Open untuk pratinjau | `NOT RUN` (layar data asli) | `cek412.mjs` |

**Uji manual:** dijalankan lewat Playwright pada server `next dev` dengan API tiruan: buka dialog Tutup, baca kalimat, isi alasan, kirim, baca pesan berhasil; periode terblokir; `409` dengan pratinjau kedua yang berbeda. Server dev yang dipakai adalah server 3000 yang sudah berjalan di folder yang sama (Next menolak `next dev` kedua). Server itu tidak dihentikan oleh task ini.

**Tidak dijalankan:** uji layar dengan data asli devYoga. Uji itu butuh periode presensi Open berisi hari Belum Lengkap ber-pengecualian, sedangkan membuat periode atau membuka ulang periode adalah tulis data (tidak ada wewenang eksekusi database). Bukti API data asli untuk ruas yang sama sudah ada di `BE-HRD-468` (`ATP-202611-0001`: `conversionExceptionCount = 1`, tutup `200`, `closedConversionExceptionCount = 1`). Respons tiruan di spec meniru bentuk respons itu.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Jumlah > 0 tampil di pratinjau dan konfirmasi; jumlah 0 atau ruas tidak ada → kalimat tidak tampil | Terpenuhi | Uji unit AC1 (tiga uji); E2E: kalimat di teks konfirmasi (sekali), catatan saat terblokir, tidak tampil untuk 0 dan untuk respons tanpa ruas |
| 2. Pesan berhasil menyebut jumlah yang diselesaikan | Terpenuhi | Uji unit AC2; E2E: toast "… 1 pengecualian scan diselesaikan oleh penutupan."; `0`/respons lama → pesan lama |
| 3. `409` pemblokir lain tetap menampilkan daftar isu | Terpenuhi | E2E terblokir (daftar pemblokir + tombol terkunci) dan E2E `409` (pesan server + pratinjau dimuat ulang + daftar pemblokir + tombol terkunci + alasan tetap + daftar periode tidak dimuat ulang); 5 uji `FE-HRD-311` tetap lulus |
| DoD: AC terbukti, laporan `FE-HRD-412.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Suite unit penuh tetap memuat 152 kegagalan yang sudah ada di modul lain |
| Masalah yang diketahui | Ulang `UAT-PK-09`/`UAT-PK-17` di layar dengan data asli menunggu periode presensi Open di devYoga (lihat "Tidak dijalankan"). Bila Pengguna memberi wewenang membuat periode uji, langkahnya dapat dijalankan di `FE-HRD-417` |
| Dependency backend | Tidak ada yang tertunda. Status kontrak API 37.3 masih "Rencana (belum tersedia)"; penyegarannya dicatat untuk pass blueprint (artefak terkunci hash) |
| Perubahan sampingan | `NONE`. Spec debug sementara `tests/e2e/zz-tmp-412-debug.spec.mjs` dibuat lalu dihapus. Playwright dijalankan dengan `--output` ke scratchpad; `test-results/` tidak berubah |
| Interupsi | `next dev` 3711 ditolak karena ada `next dev` lain di port 3000 (bukan milik task ini); E2E dijalankan terhadap server itu dengan rahasia sesi uji yang sama. Saat build, server itu sudah tidak berjalan. Backend 7184 yang dinyalakan untuk pemeriksaan baca-saja dihentikan sebelum build |
| Status Git | Berkas task ini: `M src/utils/hr/attendance/attendance-period-close-utils.js`, `M src/components/view/hr/attendance/attendance-period-close-dialog.jsx`, `M src/components/view/hr/attendance/attendance-periods-view.jsx`, `?? tests/unit/hr-attendance-period-close-exceptions.test.mjs`, `?? tests/e2e/hr-attendance-period-close-exceptions.spec.mjs`. Perubahan `FE-HRD-410`/`411` dan Lab tidak disentuh. Tidak ada stage, commit, atau push |
| Langkah berikutnya | Gelombang 3 `MVP-78`: `FE-HRD-413` (layar HR Perubahan Shift), `FE-HRD-414` (Tukar Dinas), `FE-HRD-415` (Penugasan Tambahan), `FE-HRD-100` (ESS) |
