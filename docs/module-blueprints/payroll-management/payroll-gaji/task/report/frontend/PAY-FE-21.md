# Laporan Perubahan Frontend — `PAY-FE-21`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-21` |
| Judul | Kelengkapan Data Payroll |
| Slice | Belum terdaftar sebagai EPIC (di luar `EPIC PGJ-01`–`07`); didaftarkan setelah source ditulis, lihat bagian 7. Layar tidak ada di `03-frontend-architecture.md` (`FE-PGJ-01`–`10`) |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-21` |
| Trace | Perintah pemilik 2026-11-16 (Payroll hanya membaca data HRD; yang ditulis Payroll hanya profil payroll). **Belum ada FR-PGJ dan PRL-DEC bernomor** — tidak dikarang |
| Contract version | **Tidak tercakup `PGJ-C1`**. Endpoint dibaca langsung dari source backend (`PAY-BE-24`); kontrak amandemen belum ada — gerbang `G1` terbuka |
| Wewenang UI | Tidak ada skema layar yang disetujui di blueprint. Isi layar, kolom, dan susunan ditentukan saat penulisan source dari pola layar Payroll yang ada; tidak ada keputusan UI pemilik yang tercatat |
| Dependency | `PAY-BE-24` 🟡 — source di-commit pemilik dan `dotnet build` berhasil menurut pemilik, tetapi uji API belum |
| Klasifikasi | `MEDIUM` — satu halaman daftar baca dengan ringkasan dan filter, satu modal impor dua tahap, satu butir menu, sembilan berkas source baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repository backend |
| Model | Source ditulis 2026-11-16 (model tidak dicatat saat itu); pendaftaran roadmap dan laporan ini: Claude Sonnet 5.5 |
| Commit frontend | `9bb9916a5` dan `925a63be2` (branch `yasmina`, 2026-11-16) memuat halaman dan utilitas bersama; HEAD saat laporan ditulis `925a63be2` |
| Commit backend yang dijadikan rujukan | `dc723f0f` (`PAY-BE-24`) |
| Tanggal | 2026-11-16 (revisi 2026-11-17) |
| Revisi | 2026-11-17 — atas perintah pemilik: tombol, modal, hook, dan seluruh kode impor profil dihapus; deskripsi halaman diganti; status profil otomatis dan tetapan PPh/iuran dari Backend ditampilkan. Lihat bagian 8 |
| Status | 🟡 2026-11-17 — revisi source selesai; ESLint 0 galat (2 peringatan `set-state-in-effect`); 23 dari 23 uji unit lulus (17 uji readiness + 6 penjaga modal, termasuk pencocokan ke source backend revisi); suite penuh 4.989 uji, himpunan kegagalan identik dengan sebelum revisi (0 baru); `check-css-usage.mjs` dan 10 grep anti-regresi 0 temuan; **`npm run build` NOT RUN**; layar belum dibuka di peramban dan belum diuji manual (`VISUAL CHECK` dan `MANUAL TEST: NOT FEASIBLE`); prasyarat `PAY-BE-24` revisi belum dibangun maupun diuji API. Build 2026-11-16 (laporan lisan pemilik, bukan bukti agen) mencakup versi sebelum revisi |

> **Revisi 2026-11-17.** Bagian 1 (butir Backend), 2 (langkah 4–5), 3.1, 3.2, 4 (tombol impor dan catatan), 5 (langkah uji
> 4–9), dan 6 (kriteria 3 dan 4) menggambarkan desain 2026-11-16 dan **digantikan bagian 8** untuk perilaku sekarang.
> Bagian lama dipertahankan sebagai catatan desain.

---

## 1. Keadaan yang ditemukan di awal

- Layar Profil Penggajian (`/payroll/workforce-compensations`) bersumber dari profil payroll, sehingga pegawai atau
  dokter yang belum punya profil tidak tampil dan tidak ada tempat membuat profil banyak orang.
- Backend (`PAY-BE-24`) menyediakan `GET /`, `GET /summary`, dan `POST /import` di
  `api/v1/corporate/payroll-management/payroll-data-readiness` dengan hak `PayrollDataReadiness : Read` dan `Import`.
- Perintah pemilik 2026-11-16: Payroll hanya membaca data HRD; satu-satunya yang ditulis adalah profil payroll.

## 2. Proses bisnis

**Tujuan.** Petugas Payroll melihat siapa yang belum siap digaji beserta alasannya, lalu mengimpor profil payroll untuk
orang yang data HRD-nya sudah lengkap.

**Langkah:**

1. Petugas membuka **Payroll > Penghasilan Pegawai > Kelengkapan Data**.
2. Layar menampilkan lima kartu ringkasan (total, punya profil, penetapan gaji disetujui, rekening terverifikasi,
   lengkap) dan tabel pegawai/dokter aktif dengan status tiap kelengkapan dan daftar alasan.
3. Petugas menyaring menurut Jenis Penerima, Kelengkapan Data, dan kata kunci.
4. Petugas dengan hak `Import` menekan tombol impor, memilih berkas CSV (kolom wajib Nomor Pegawai/Dokter; opsional
   Kode Cost Center, PPh Bulanan, BPJS Bulanan), menekan **Periksa Data** (uji kering, belum menyimpan), lalu
   **Simpan Data**.
5. Bila ada baris yang ditolak, tidak ada yang tersimpan dan modal menampilkan nomor baris beserta sebabnya.

**Jalur tidak normal:**

| Keadaan | Hasil di layar |
| --- | --- |
| 403 saat memuat daftar | Layar akses ditolak (`AccessDeniedGate`) |
| Gagal memuat | Peringatan galat di atas tabel |
| Tidak ada hasil | Keadaan kosong tabel |
| Isian berkas tidak terbaca (PPh bukan angka, nomor notasi ilmiah, judul kolom wajib hilang) | Ditahan di peramban per baris, tidak dikirim |
| Impor ditolak server (400) | Daftar baris gagal dengan nomor baris dan sebab; tidak ada data tersimpan |
| Pengguna tanpa hak `Import` | Tombol impor tidak tampil |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/app/payroll/data-readiness/page.jsx` | Baru | Route `/payroll/data-readiness` |
| `src/components/view/payroll/data-readiness/payroll-data-readiness-view.jsx` | Baru | Hero, kartu ringkasan, catatan, filter, tabel |
| `src/components/view/payroll/data-readiness/payroll-data-readiness-table-columns.jsx` | Baru | Kolom tabel dan lencana status |
| `src/components/view/payroll/data-readiness/payroll-data-import-modal.jsx` | Baru | Modal impor dua tahap |
| `src/lib/constants/payroll/payroll-data-readiness-constants.js` | Baru | Endpoint, teks, opsi filter |
| `src/lib/hooks/payroll/use-payroll-data-readiness.js` | Baru | Daftar, ringkasan, filter, `canImport` |
| `src/lib/hooks/payroll/use-payroll-data-import.js` | Baru | Baca CSV, uji kering, simpan |
| `src/lib/services/payroll/payroll-data-readiness-service.js` | Baru | `list`, `summary`, `importRows` |
| `src/utils/payroll/payroll-data-readiness-utils.js` | Baru | Normalizer, parser CSV, peringatan impor |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah | Butir "Kelengkapan Data" di grup "Penghasilan Pegawai", dijaga `PayrollDataReadiness : Read` |
| `tests/unit/payroll-data-readiness.test.mjs` | Baru | 16 uji, termasuk pencocokan ke source backend |

Utilitas generik dipakai dari `src/utils/shared/api-payload-utils.js` (dipindah untuk memutus kopling dengan halaman HR).

### 3.2 Tabel keputusan base component

| Elemen | Status | Dasar |
| --- | --- | --- |
| Hero, ringkasan, filter, tabel, paginasi, peringatan, toast, penjaga akses, modal konfirmasi, tombol | `REUSE` | Komponen `base-features` yang sama dengan layar Payroll lain |
| Modal impor | `COMPOSE` dari `ConfirmModal` dan field dasar | Pola impor dan `confirmProps.disabled` dikunci uji |

`UI GATE`: tidak ada elemen `NEW`. Tabel ini disusun dari tinjauan source untuk laporan ini, bukan dari tabel keputusan
yang dibuat sebelum JSX ditulis (task didaftarkan setelah source ada).

## 4. Dokumentasi layar

| Wilayah | Isi | Sumber data | Hak akses |
| --- | --- | --- | --- |
| Kepala | Judul "Kelengkapan Data Payroll" dan deskripsi | Teks tetap | `PayrollDataReadiness : Read` (butir menu) |
| Kartu ringkasan | Lima hitungan | `GET /summary` | `Read` |
| Catatan | Payroll hanya membaca data HRD; yang dibuat lewat impor hanya profil payroll | Teks tetap | — |
| Filter | Kata kunci, Jenis Penerima, Kelengkapan Data, jumlah baris | Opsi tetap | `Read` |
| Tabel | Kolom No.; Pegawai / Dokter (nama, nomor, jenis); Profil Payroll; Penetapan Gaji Utama (HRD); Rekening Payroll (HRD); Kelengkapan (lencana dan daftar alasan). Tanpa nominal gaji dan nomor rekening | `GET /` | `Read` |
| Tombol impor | Membuka modal impor | — | `PayrollDataReadiness : Import` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint pada 10 berkas feature | 0 galat, 2 peringatan `react-hooks/set-state-in-effect` pada hook baru (pola sama dengan hook Payroll lain) | `PASS` | Dijalankan agen 2026-11-16 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-data-readiness.test.mjs` | 16 uji, 16 lulus, 0 gagal | `PASS` | Dijalankan agen 2026-11-16 |
| `check-css-usage.mjs` pada tiga berkas view | `OK — 3 berkas diperiksa, 0 temuan` | `PASS` | Dijalankan agen 2026-11-16 |
| Pencocokan ke source backend | Uji membaca controller dan service backend: kolom impor, resource RBAC, rute, batas baris, batas kepemilikan, dan log tanpa nominal | `PASS` | Uji nomor 182, 207, 233 |
| `npm run build` | Berhasil menurut pemilik, 2026-11-16 | `PASS (dilaporkan pemilik)` | Laporan lisan; tanpa keluaran tersimpan; bukan bukti agen |
| Pemeriksaan visual dan screenshot | Layar belum dibuka | `NOT FEASIBLE` oleh agen | — |
| Uji manual | Belum dijalankan | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: node --test tests/unit/payroll-data-readiness.test.mjs — PASS (16/16)`

Suite penuh tidak dijalankan pada pendaftaran ini; baseline 51 gagal di modul lain (16 November 2026) tidak diukur ulang.

**Langkah uji untuk pemilik** (login dengan hak `PayrollDataReadiness : Read` dan `Import`):

1. Buka **Payroll > Penghasilan Pegawai > Kelengkapan Data**. Harapan: lima kartu terisi; tabel menampilkan pegawai dan dokter aktif, termasuk yang belum punya profil.
2. Saring Jenis Penerima = Dokter, lalu Kelengkapan = Belum lengkap, lalu kata kunci sebagian nama. Harapan: hasil menyempit sesuai; tombol atur ulang mengembalikan.
3. Ubah jumlah baris dan pindah halaman. Harapan: halaman benar, tidak ada baris ganda.
4. Buka impor, unduh/lihat templat, pilih berkas satu baris orang yang lengkap di HRD, tekan Periksa Data. Harapan: hasil uji kering; tidak ada data tersimpan.
5. Tekan Simpan Data. Harapan: toast sukses; daftar dimuat ulang; orang itu `hasPayrollProfile`.
6. Impor ulang berkas yang sama. Harapan: baris "Sudah ada (dilewati)" dengan peringatan.
7. Berkas dua baris: satu valid, satu orang tanpa rekening di HRD. Harapan: daftar baris gagal; baris valid tidak tersimpan.
8. Berkas dengan PPh "abc". Harapan: ditahan di layar sebelum dikirim.
9. Akun tanpa `Import`. Harapan: tombol impor tidak tampil. Akun tanpa `Read`: butir menu tidak tampil; membuka alamat langsung menampilkan akses ditolak.

**Tidak dijalankan:** pemeriksaan visual, uji manual langkah 1–9, suite penuh.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar dan ringkasan memuat seluruh pegawai dan dokter aktif dengan filter dan paginasi | Terpetakan ke source; belum diuji manual | Bagian 3, 5 |
| 2. Status kelengkapan dan alasan tampil per orang | Terpetakan ke source; belum diuji manual | Bagian 3, 5 |
| 3. Impor dua tahap (uji kering lalu simpan); baris gagal tampil dengan nomor dan sebab | Terpetakan ke source; unit lulus; belum diuji manual | Bagian 5 |
| 4. Butir menu dijaga `Read`; tombol impor dijaga `Import` | Terpetakan ke source dan uji kontrak | Bagian 5 |
| 5. Empat keadaan (memuat, kosong, gagal, berisi) | Terpetakan ke source; belum dilihat | Bagian 5 |
| DoD: ESLint 0 galat | Terpenuhi | Bagian 5 |
| DoD: build | Terpenuhi menurut pemilik; tanpa bukti keluaran | Bagian 5 |
| DoD: pemeriksaan visual dan uji manual | **Belum terpenuhi** | `NOT FEASIBLE` / `NOT RUN` |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

Kriteria di atas **disusun dari perilaku source**, bukan dari kontrak atau skema layar yang disetujui, karena keduanya belum ada.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Pendaftaran terlambat | Source ditulis dan di-commit pada 2026-11-16 sebelum task ini didaftarkan; ID `PAY-FE-21` dan statusnya diberikan setelahnya atas permintaan pemilik. Gerbang keputusan base component dan skema layar tidak dijalankan sebelum penulisan |
| Gerbang `G1` | Kontrak `PGJ-C1` dan `03-frontend-architecture.md` tidak memuat layar ini. Perlu amandemen kontrak dan skema layar, lalu persetujuan pemilik (`design-business-module`, model minimum Claude Opus 5) |
| Requirement dan keputusan | Belum ada FR-PGJ dan PRL-DEC bernomor; tidak dikarang |
| Urutan kerja | HR melengkapi dan menyetujui data penggajian (S-A8) → Payroll impor profil di layar ini → Simulasi. Per 2026-11-16 data produksi belum memiliki penetapan gaji dan rekening terverifikasi, sehingga impor ditolak sampai HR mengisinya |
| Hak akses baru | Admin perlu mencentang `PayrollDataReadiness : Read` dan `Import` di Pengaturan > Manajemen Role > Akses Role; daftar izin dimuat sekali per sesi (login ulang) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pemilik. Tidak ada yang di-stage atau di-commit oleh pembuat laporan ini |
| Langkah berikutnya | Digantikan bagian 8.6 (revisi 2026-11-17): bangun Backend revisi, `npm run build`, uji manual delapan langkah di bagian 8.4, putuskan sumber tetapan PPh dan iuran, dan amandemen kontrak dan skema layar. Langkah uji 1–9 di bagian 5 menggambarkan layar impor yang sudah dihapus |

---

## 8. Revisi 2026-11-17 — tanpa impor profil; status profil otomatis dari Backend

Bagian ini **menggantikan** bagian 1 (butir Backend), 2 (langkah 4–5 dan baris jalur tidak normal tentang impor), 3.1,
3.2, 4 (tombol impor dan catatan), 5 (langkah uji 4–9), dan 6 (kriteria 3 dan 4) untuk perilaku sekarang. Bagian itu
dipertahankan sebagai catatan desain 2026-11-16. Perubahan mengikuti revisi Backend
([`PAY-BE-24`](../backend/PAY-BE-24.md) bagian 8), perintah pemilik 2026-11-17.

### 8.1 Proses bisnis dari sisi pengguna

**Pengguna.** Petugas Payroll dan HRD yang melihat siapa yang belum siap digaji. Halaman ini **hanya membaca**: tidak ada
impor, unggah berkas, maupun tombol sinkronisasi manual.

**Langkah:**

1. Petugas membuka **Payroll > Penghasilan Pegawai > Kelengkapan Data**. Judul dan deskripsi: "Data pegawai, penetapan
   gaji utama, dan rekening payroll bersumber dari HRD. Payroll menggunakan data tersebut untuk menyiapkan proses
   penggajian tanpa impor profil manual."
2. Lima kartu ringkasan dan satu catatan tetap (HRD yang menyelesaikan kelengkapan, persetujuan gaji, dan verifikasi
   rekening). Di bawahnya, **kalimat ringkasan dari jumlah Backend** bila ada: berapa orang akan disiapkan profilnya
   otomatis, berapa profil perlu perhatian, dan berapa yang tetapan PPh dan iurannya belum ditetapkan.
3. Tabel: kolom **Profil Payroll** kini menampilkan status dari Backend (`Siap`, `Akan Disiapkan Otomatis`, `Menunggu
   Data HRD`, `Perlu Perhatian`) dan, bila ada, baris "PPh dan iuran: Belum ditetapkan". Kolom Penetapan Gaji Utama
   (HRD), Rekening Payroll (HRD), dan Kelengkapan (lencana dan penghalang) mengikuti `isComplete` dari Backend.
4. Saringan Jenis Penerima, Kelengkapan Data, kata kunci, jumlah baris, dan paginasi tidak berubah. Header saringan tidak
   lagi memuat tombol aksi.

**Jalur tidak normal:**

| Keadaan | Hasil di layar |
| --- | --- |
| 403 saat memuat daftar | Layar akses ditolak (`AccessDeniedGate`) |
| Gagal memuat daftar | Peringatan galat di atas tabel |
| Gagal memuat ringkasan | Toast galat ("Gagal memuat ringkasan kelengkapan data payroll."); kartu tampil kosong, bukan angka karangan; daftar tetap tampil |
| Tidak ada hasil | Keadaan kosong tabel |
| Backend lama tanpa status profil | Lencana "Belum Ada" (tidak ditebak) |

### 8.2 Berkas

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/payroll/data-readiness/payroll-data-readiness-view.jsx` | Tombol impor, handler, modal, dan `usePayrollDataImport` dihapus; ditambah `PayrollNoticeList` untuk kalimat ringkasan |
| `src/components/view/payroll/data-readiness/payroll-data-readiness-table-columns.jsx` | Kolom Profil Payroll memakai status dan tetapan dari Backend |
| `src/components/view/payroll/data-readiness/payroll-data-import-modal.jsx` | **Dihapus** (tanpa consumer lain) |
| `src/lib/hooks/payroll/use-payroll-data-import.js` | **Dihapus** (tanpa consumer lain) |
| `src/lib/hooks/payroll/use-payroll-data-readiness.js` | `canImport`, izin `Import`, dan `reload` (hanya dipakai impor) dihapus; `summaryNotices` ditambah; toast untuk kegagalan ringkasan |
| `src/lib/services/payroll/payroll-data-readiness-service.js` | `importRows` dihapus; hanya `list` dan `summary` |
| `src/lib/constants/payroll/payroll-data-readiness-constants.js` | Endpoint dan izin `import`, batas dan kolom berkas, dan seluruh teks impor dihapus; teks baku baru, nilai status Backend, kunci lencana |
| `src/utils/payroll/payroll-data-readiness-utils.js` | Parser CSV, templat, dan normalizer impor dihapus; normalizer status baru; `buildReadinessNotices` |
| `tests/unit/payroll-data-readiness.test.mjs` | Ditulis ulang: 17 uji |
| `tests/unit/confirm-modal-cancel-guard.test.mjs` | Entri modal impor yang dihapus dikeluarkan |

Yang **tidak** dihapus karena dipakai modul lain: `parseCsvText`, `maskIds`, dan `readImportFailedRows` (impor penghasilan
variabel, tetap di `payroll-variable-input-utils.js`). Impor `import-hrd` di detail Payroll Run tidak tersentuh.

### 8.3 Tabel keputusan base component

| Elemen | Status | Dasar |
| --- | --- | --- |
| Hero, ringkasan, catatan, filter, tabel, paginasi, lencana status, toast, penjaga akses | `REUSE` | Komponen `base-features` yang sama dengan layar Payroll lain; tanpa perubahan props bawaan |
| Kalimat ringkasan | `REUSE` | `PayrollNoticeList` (komponen bersama modul Payroll) |
| Lencana status profil baru | `REUSE` | `StatusBadge` dengan konfigurasi kunci (`region-status-info`, `-inactive`, `-warning` yang sudah ada) |

`UI GATE: PASS — 10 elemen: REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`. Tidak ada stylesheet baru.

Hasil pemeriksa dan grep anti-regresi (keadaan akhir berkas):

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    .../data-readiness/payroll-data-readiness-view.jsx .../data-readiness/payroll-data-readiness-table-columns.jsx
OK — 2 berkas diperiksa, 0 temuan.

$ grep (10 pola checklist: warna literal, font-*, <button/btn, <table, fw-/fs-, !important, position fixed,
        <input/select/textarea/UUID/GUID, style={{, variant tidak sah)  pada kedua berkas JSX      0 0 0 0 0 0 0 0 0 0
```

Tata letak header: slot `actions` `DataFilter` dikosongkan; header tetap memuat judul dan deskripsi, sehingga tidak ada ruang
kosong tambahan di sisi kanan (belum dilihat di peramban).

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/payroll/data-readiness` | `VISUAL CHECK: NOT FEASIBLE` | Dev server, build, dan skrip peramban tidak dijalankan (instruksi pemilik), dan agen tidak memegang sesi login. Yang paling perlu dilihat: lebar kolom Profil Payroll dengan lencana "Akan Disiapkan Otomatis" dan susunan header filter tanpa tombol |

### 8.4 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint pada 6 berkas feature dan 2 berkas uji | 0 galat; 2 peringatan `react-hooks/set-state-in-effect` pada hook (pola sama dengan sebelumnya) | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-data-readiness.test.mjs tests/unit/confirm-modal-cancel-guard.test.mjs` | 23 uji (17 + 6), 23 lulus, 0 dilewati (source backend ada, jadi pencocokan ikut berjalan) | `PASS` | Keluaran perintah |
| Pencocokan ke source backend | Nama field DTO baris dan ringkasan, nilai status profil dan pajak, resource RBAC, rute, 410 tanpa jalur tulis, readiness murni baca, penyiapan hanya membuat, urutan penyiapan sebelum seed, `import-hrd` ada, DI, indeks unik | `PASS` | Uji nomor 9–14 |
| Penjagaan layar | Tidak ada tombol, modal, hook, panggilan POST, izin `Import`, atau kalimat "silakan impor"; modal dan hook sudah tidak ada | `PASS` | Uji nomor 15–17 |
| `check-css-usage.mjs` dan grep anti-regresi | 0 temuan | `PASS` | Bagian 8.3 |
| Pemeriksa statis simbol impor | `OK — 5 berkas, semua impor terpetakan.`; tidak ada sisa referensi simbol impor profil di `src` | `PASS` | Keluaran skrip |
| Suite unit penuh | 4.989 uji, 4.937 lulus, 52 gagal; himpunan kegagalan identik dengan run sebelum revisi (0 baru). Satu dari 52 (`ESS-004`, layanan mandiri) tidak terkait | `EXISTING / ENVIRONMENT ISSUE` (52), `PASS` (tanpa kegagalan baru) | Selisih himpunan nama uji |
| `npm run build` | Tidak dijalankan (instruksi pemilik) | `NOT RUN` | — |
| Pemeriksaan visual dan uji manual | Tidak dilakukan | `NOT FEASIBLE` | Bagian 8.3 |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/payroll-data-readiness.test.mjs — PASS (17/17)`

**Langkah uji untuk pemilik** (setelah Backend revisi dibangun; akun `PayrollDataReadiness : Read`):

1. Buka **Kelengkapan Data**. Harapan: tidak ada tombol "Impor Profil Payroll"; header filter rapi tanpa ruang kosong di kanan; deskripsi sesuai kalimat pemilik.
2. Periksa kolom Profil Payroll. Harapan: lencana mengikuti keadaan nyata (mis. "Akan Disiapkan Otomatis" bagi yang gaji dan rekeningnya sudah ada di HRD, "Menunggu Data HRD" bagi yang belum); tidak ada pesan yang meminta impor.
3. Orang dengan gaji belum disetujui atau rekening belum terverifikasi. Harapan: "Belum Lengkap" beserta penghalang yang menyebut HRD.
4. Kalimat di bawah catatan. Harapan: jumlahnya sama dengan jumlah baris berlencana yang bersangkutan (akan disiapkan, perlu perhatian, tetapan belum ditetapkan); hilang bila tidak ada.
5. Setelah HR melengkapi data satu orang, muat ulang halaman. Harapan: keadaan orang itu berubah sesuai data HRD terbaru.
6. Setelah payroll run dibuka untuk periode berlaku. Harapan: orang yang memenuhi syarat berlencana "Siap" dan muncul di Profil Penggajian.
7. Matikan backend lalu muat ulang. Harapan: peringatan galat daftar dan toast galat ringkasan.
8. Akun tanpa `Read`. Harapan: butir menu tidak tampil; membuka alamat langsung menampilkan akses ditolak.

**Tidak dijalankan:** `npm run build`, `npm run dev`, pemeriksaan visual, uji manual.

### 8.5 Acceptance criteria roadmap yang tidak lagi berlaku (delta)

Kartu `PAY-FE-21` di roadmap masih memuat desain impor dan **tidak diubah** (wewenang roadmap hanya baris status).

| Kriteria kartu | Keadaan sekarang |
| --- | --- |
| 1. Daftar dan ringkasan dengan saringan dan paginasi dari server | Tetap |
| 2. Status profil, penetapan gaji, rekening, dan kelengkapan beserta alasan | Tetap; status profil kini dari Backend (empat nilai) dan tetapan PPh dan iuran ditampilkan terpisah |
| 3. Impor dua tahap (Periksa Data lalu Simpan Data) | **Dihapus** atas perintah pemilik 2026-11-17 |
| 4. Tombol impor hanya tampil dengan `Import` | **Dihapus**; butir menu tetap dijaga `Read` |
| 5. Empat keadaan dan akses ditolak | Tetap; kegagalan ringkasan kini juga memberi toast |

Amandemen kontrak dan skema layar (gerbang `G1`) harus memuat perubahan ini sebelum task dapat naik ke ✅.

### 8.6 Catatan penutup revisi

| Hal | Isi |
| --- | --- |
| Dependency | `PAY-BE-24` 🟡 (revisi belum dibangun maupun diuji API). Layar bergantung pada field baru Backend; terhadap Backend lama lencana profil tampil "Belum Ada" |
| Blocker khusus | Tetapan PPh dan iuran tidak punya sumber sah di HRD dan tidak ada jalur massal setelah impor dihapus ([`PAY-BE-24`](../backend/PAY-BE-24.md) 8.8). Layar hanya menandainya ("Belum ditetapkan"); tidak mengisinya |
| Masalah yang diketahui | (1) Layar belum pernah dilihat di peramban. (2) Kolom Profil Payroll lebih lebar (lencana "Akan Disiapkan Otomatis"). (3) `Notes` dari Backend tidak ditampilkan per baris: jumlahnya sudah dirangkum di kalimat ringkasan dan lencana, supaya tabel tidak memuat kalimat yang sama berulang |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan revisi belum di-stage dan belum di-commit |
| Langkah berikutnya | Pemilik: bangun Backend revisi dan lulus uji API `PAY-BE-24`; `npm run build`; uji manual delapan langkah di atas; putuskan sumber tetapan PPh dan iuran; amandemen kontrak dan skema layar (`G1`). Status tetap 🟡 |
