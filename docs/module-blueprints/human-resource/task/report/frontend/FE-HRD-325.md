# Laporan Perubahan Frontend — `FE-HRD-325`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-325` |
| Judul | Halaman daftar Data Penggajian Pegawai (`/hr/workforce-core/payroll-data`) |
| Slice | `S-A8` / `MVP-69` — gelombang 1 |
| Roadmap | `roadmap/frontend-roadmap.md` bagian Slice S-A8, kartu `FE-HRD-325` |
| Trace | `HRD-DEC-219` (daftar tanpa nominal, nomor disamarkan), `HRD-DEC-222` (jenis Perawat), `HRD-DEC-223` (impor HR dihentikan); API Contract §33; pasangan backend `BE-HRD-360` |
| Contract version | `v27` bagian 33 — approved 16 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Halaman daftar beserta kartu ringkasan, filter, tabel, dan butir menu sesuai kartu roadmap. Formulir ubah milik `FE-HRD-326` dan tidak diubah di sini |
| Dependency | `BE-HRD-360` ✅ (18 November 2026); `BE-HRD-363` ✅ |
| Klasifikasi | `LIGHT` — source sudah ada; 2 berkas diubah teks saja; verifikasi di browser dengan data asli |
| Task mode | `FRONTEND` (instruksi pengguna 18 November 2026 "lanjut FE-HRD-325") |
| Target tulis | `QuilvianSystemFrontendDev`: `src/app/hr/workforce-core/payroll-data/page.jsx`, `src/lib/hooks/hr/workforce-core/use-workforce-payroll-data.js`; laporan ini, roadmap, dan traceability di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `baad7dd62` (branch `YogaV2`); source slice S-A8 sudah ada di commit itu |
| Commit backend yang dijadikan rujukan | `a17a2bc3` (branch `yoga`) beserta perubahan `BE-HRD-360` s.d. `363` yang belum di-commit |
| Tanggal | 18 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terbukti di browser dengan data asli devYoga; `npm run build` lulus; ESLint 0 error; uji unit 21/21; pemeriksa CSS 0 temuan |

---

## 1. Keadaan yang ditemukan di awal

Seluruh source halaman sudah ada sejak 16 November 2026 (route, view, kolom tabel, hook, service, konstanta, utilitas, butir menu), sudah lulus lint, uji unit, dan pemeriksa CSS, tetapi **belum pernah di-build dan belum pernah dibuka di browser**. Pemeriksaan source menemukan dua sisa dari versi lama yang masih menyebut impor CSV, padahal impor di halaman HR sudah dihentikan (`HRD-DEC-223`):

| Berkas | Sisa |
| --- | --- |
| `page.jsx` | Deskripsi metadata halaman: "…pegawai dan dokter, serta impor data awalnya." |
| `use-workforce-payroll-data.js` | Komentar: ringkasan "hanya dimuat ulang setelah impor" |

Perilaku lain sudah sesuai kontrak v27 §33: filter dan paginasi dikerjakan backend, ringkasan diambil terpisah, kolom Aksi hanya untuk pemegang `WorkforcePayrollData : Update`, dan galat `403` ditangani `AccessDeniedGate`.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas HR membuka **Sumber Daya Manusia › Administrasi Kepegawaian › Data Penggajian Pegawai**. Butir menu hanya tampil bagi pemegang `WorkforcePayrollData : Read`.
2. Di bagian atas tampil empat kartu ringkasan, misalnya pada devYoga: **Total Pegawai dan Dokter 45**, **Gaji Utama Disetujui 1**, **Rekening Terverifikasi 1**, **Data Lengkap 1**.
3. Tabel memuat semua karyawan, perawat, dan dokter aktif, termasuk yang belum punya data. Contoh baris lengkap: *Dewi Anggraeni · RSMMC-20260303001 · Karyawan* — penetapan **Disetujui** "Struktur UJI-362 · mulai 01/11/2026", rekening **Terverifikasi** "Bank Central Asia · ****2003", kelengkapan **Lengkap**. Gaji pokok dan nomor rekening lengkap tidak pernah tampil di daftar.
4. Petugas mempersempit daftar:
   - **Jenis Penerima**: Karyawan, Perawat, atau Dokter. Contoh: memilih Dokter menampilkan 17 dokter.
   - **Kelengkapan Data**: Lengkap atau Belum lengkap.
   - **Pencarian** nama atau nomor. Contoh: "dewi" menampilkan dua orang yang namanya memuat Dewi.
   - **Jumlah baris**: 10, 25, 50, atau 100 per halaman, dengan navigasi halaman di bawah tabel.
   - Tombol **↻ Atur ulang filter** mengembalikan semuanya ke keadaan awal.
5. Bagi orang yang belum lengkap, kolom Kelengkapan menyebut kekurangannya, misalnya "Belum ada penetapan gaji utama." dan "Rekening payroll belum terverifikasi."
6. Pemegang hak ubah melihat tombol **Ubah** pada setiap baris (alur formulir: `FE-HRD-326`).

Jalur tidak normal:

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Filter tanpa hasil, misalnya Perawat pada devYoga (belum ada karyawan berprofesi Perawat) | "Data karyawan, perawat, dan dokter tidak ditemukan." — "Coba gunakan filter lain." |
| Server gagal | Kotak merah "Server mengalami kesalahan saat memproses permintaan. Coba lagi beberapa saat atau hubungi IT Helpdesk."; filter dan tombol atur ulang tetap dapat dipakai |
| Tanpa hak akses | Halaman "Ups! Akses Ditolak — Silahkan hubungi IT Helpdesk untuk meminta hak akses untuk halaman atau fitur ini" beserta pesan backend; tabel tidak tampil |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`src/app/hr/workforce-core/payroll-data/page.jsx`; `src/components/view/hr/workforce-core/workforce-payroll-data-view.jsx`, `workforce-payroll-data-table-columns.jsx`; `src/lib/hooks/hr/workforce-core/use-workforce-payroll-data.js`; `src/lib/services/hr/workforce-payroll-data-service.js`; `src/lib/constants/hr/workforce-core/workforce-payroll-data-constants.js`; `src/utils/hr/workforce-core/workforce-payroll-data-utils.js`; `src/utils/menu-sidebar/menu-items.jsx` (butir `corporateWorkforcePayrollData`); base component `data-filter.jsx` (penundaan pencarian, tombol atur ulang), `filter-select.jsx`, `information-alert.jsx`, `access-denied-gate.jsx`, `status-badge.jsx`, `pagination.jsx`; `tests/unit/workforce-payroll-data.test.mjs`; API Contract v27 §33.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/workforce-core/payroll-data/page.jsx` | Deskripsi metadata menjadi "Kelengkapan penetapan gaji utama dan rekening payroll karyawan, perawat, dan dokter aktif." (sebutan impor dihapus, `HRD-DEC-223`) |
| `src/lib/hooks/hr/workforce-core/use-workforce-payroll-data.js` | Komentar "setelah impor" diganti "setelah simpan"; tanpa perubahan perilaku |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi sesuai `rules/frontend/frontend-architecture.md`: route `app/…/page.jsx` → view → hook → service Axios (`InstanceAxios`) → konstanta/utilitas murni. Tidak ada Redux slice baru; state daftar lokal di hook, sama dengan halaman HR lain. Seluruh elemen memakai base component yang ada: `Hero`, `SummaryCards`, `InformationAlert`, `DataFilter`, `FilterSelect`, `DataTable`, `Pagination`, `StatusBadge`, `BaseButton`, `AccessDeniedGate`, `ToastStack`. Gaya memakai CSS Module region yang sudah ada dan utilitas Bootstrap (`d-flex`, `text-break`); tidak ada CSS baru. Tidak ada pola baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Teks "Mengambil data penggajian..." di area tabel; kartu ringkasan menampilkan kerangka memuat |
| Kosong | "Data karyawan, perawat, dan dokter tidak ditemukan." — "Coba gunakan filter lain." |
| Gagal | Kotak merah berisi pesan galat; daftar dikosongkan; filter tetap aktif sehingga pengguna dapat mencoba lagi |
| Tanpa hak akses | "Ups! Akses Ditolak" beserta pesan backend "Anda tidak memiliki akses ke menu atau fitur ini."; butir menu sendiri tidak tampil tanpa `WorkforcePayrollData : Read` |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Core / Payroll Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-core/payroll-data` | Tabel: `search`, `personType`, `completeness`, `pageNumber`, `pageSize` | `WorkforcePayrollData : Read` |
| `GET` | `/v1/corporate/human-resource/workforce-core/payroll-data/summary` | Empat kartu ringkasan | `WorkforcePayrollData : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 9 berkas slice | 0 error, 2 peringatan `setState` di dalam effect pada hook daftar (pola yang sama dengan view HR lain) | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/workforce-payroll-data.test.mjs` | 21/21 lulus | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` atas 3 view slice | "OK — 3 berkas diperiksa, 0 temuan." | `PASS` | Keluaran perintah |
| `npm run build` (server lokal dimatikan lebih dulu) | "Compiled successfully in 85s", 816 halaman dibangkitkan termasuk `○ /hr/workforce-core/payroll-data`; standalone siap; tidak ada berkas tracked lain yang berubah | `PASS` | `fe-build-325.log` |
| Browser — muat normal (`next dev` 3000 → backend lokal 7184, devYoga, superadmin; semua permintaan tulis diblokir) | Kartu 45/1/1/1; 25 baris; kolom No., Nama / Nomor, Penetapan Gaji Utama, Rekening Payroll, Kelengkapan, Aksi; permintaan `?pageNumber=1&pageSize=25` | `PASS` | `fe-325.mjs`, `325-desktop.png` |
| — Opsi Jenis Penerima | "Jenis Penerima", Karyawan, Perawat, Dokter | `PASS` (AC2) | idem |
| — Jenis = Dokter | 17 baris, semuanya "· Dokter"; permintaan `personType=Dokter` | `PASS` (AC2) | idem |
| — Jenis = Perawat | 0 baris, pesan kosong tampil; permintaan `personType=Perawat` | `PASS` (AC1, AC2) | `325-kosong.png` |
| — Atur ulang filter | Kembali 25 baris, permintaan tanpa filter; kotak cari kosong | `PASS` (AC2) | `fe-325.mjs` |
| — Kelengkapan = Lengkap | 1 baris Dewi Anggraeni; opsi "Kelengkapan Data", Lengkap, Belum lengkap | `PASS` (AC2) | idem |
| — Kerahasiaan pada halaman | `****2003` tampil; nomor lengkap `9900362003` dan gaji pokok (13.000.000 dalam dua format) **tidak** ada di teks halaman | `PASS` (AC3) | idem |
| — Cari "dewi" | 2 baris, keduanya memuat "dewi"; permintaan `search=dewi` | `PASS` (AC2) | idem |
| — 10 baris per halaman + halaman berikutnya | Halaman 1: 10 baris, halaman 2: 10 baris lain (tanpa irisan); permintaan `pageNumber=2&pageSize=10` | `PASS` (AC2) | idem |
| — Lebar badge | 75 badge; paling lebar 62% dan rata-rata 41% dari lebar selnya | `PASS` (AC4) | idem |
| — Teks panjang (respons disunting di browser: nama 115 karakter, unit 77 huruf tanpa spasi, catatan panjang) | Nama membungkus 4 baris; isi sel tidak terpotong (`scrollWidth` ≤ `clientWidth`); tabel tidak melebihi wadahnya | `PASS` (AC4) | `325-teks-panjang.png` |
| — Memuat (respons daftar ditunda 4 detik) | "Mengambil data penggajian..." tampil | `PASS` (AC1) | `325-memuat.png` |
| — Gagal (respons `500` tanpa pesan) | Pesan galat server tampil, 0 baris, tombol atur ulang tetap ada | `PASS` (AC1) | `325-gagal.png` |
| — Tanpa hak akses (respons `403`) | "Ups! Akses Ditolak" + pesan backend; tabel tidak tampil | `PASS` (AC1) | `325-tanpa-akses.png` |
| — Ponsel 390 px | Dokumen 375 px (tidak melebihi layar); tabel bergulir mendatar di dalam wadahnya (`overflow-x: auto`); kartu dan filter bertumpuk satu kolom | `PASS` | `325-ponsel.png` |

Uji manual: `PASS`.

**Tidak dijalankan:** menu tanpa hak `WorkforcePayrollData : Read` dengan akun non-superadmin dan alur tombol **Ubah** — masing-masing cakupan `FE-HRD-327` dan `FE-HRD-326`. Uji unit seluruh repository tidak dijalankan; yang dijalankan hanya berkas uji slice.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Seluruh state tampil: memuat, kosong, gagal, dan tanpa hak akses | Terpenuhi | Bagian 6: memuat, Perawat (kosong), `500`, `403` |
| 2. Filter Jenis memuat Karyawan, Perawat, dan Dokter; filter, pencarian, reset, dan paginasi mengubah hasil yang tampil | Terpenuhi | Bagian 6: opsi, Dokter 17, Perawat 0, Lengkap 1, cari 2, reset, halaman 2 |
| 3. Gaji pokok tidak tampil di daftar; nomor rekening hanya empat karakter terakhir | Terpenuhi | `****2003`; nomor lengkap dan gaji tidak ada di halaman |
| 4. Badge status tidak melebar memenuhi sel; teks panjang membungkus tanpa memotong tabel | Terpenuhi | Badge maksimal 62% lebar sel; uji teks panjang |
| `npm run build` | Lulus | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 2 peringatan ESLint `react-hooks` (`setState` di dalam effect) pada `use-workforce-payroll-data.js`; sama dengan pola view HR lain dan tidak diubah |
| Masalah yang diketahui | Filter Perawat kosong di devYoga karena 27 dari 28 karyawan aktif belum diisi profesinya (data, bukan kode; G3 ditutup pemilik). Footer global aplikasi melayang di atas isi pada tangkapan halaman penuh; ini tata letak global, bukan halaman ini |
| Dependency backend | `BE-HRD-360` s.d. `BE-HRD-363` ✅; tidak ada yang tertunda |
| Perubahan sampingan | Lingkungan: folder cache `.next/dev/cache/turbopack` (32 GB, ruang disk bebas tinggal 25,8 GB) dihapus sesudah `next dev` dipastikan mati; dibuat ulang otomatis; ruang bebas menjadi 56,8 GB. Tidak ada berkas tracked yang tersentuh |
| Interupsi | `NONE` |
| Status Git | ` M src/app/hr/workforce-core/payroll-data/page.jsx` · ` M src/lib/hooks/hr/workforce-core/use-workforce-payroll-data.js` |
| Langkah berikutnya | `FE-HRD-326` — formulir ubah per orang (jalur berhasil, ditolak, dan tanpa hak di browser) |
