# Laporan Perubahan Frontend — `FE-ACC-P2-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-006` |
| Judul | Layar Tutup Tahun |
| Slice | `P2-5` |
| Roadmap | `docs/module-blueprints/accounting/roadmap/frontend-roadmap-phase2.md` — kartu `FE-ACC-P2-006` |
| Trace | `ACC-DEC-053`, `ACC-DEC-054`; `FR-P2-030`, `031`, `032`, `033` |
| Contract version | `ACC-API-0.9` — `approved`, Rizki 10 September 2026 |
| Wewenang UI | Satu butir menu tingkat 2, satu layar, satu slice, satu hook, satu constants, satu CSS Module |
| Dependency | `BE-ACC-P2-010` — **`SEBAGIAN`**; route, DTO, dan seluruh pesan penolakan diverifikasi langsung ke `AccYearEndClosingService` |
| Klasifikasi | `MEDIUM` — dua tabel, satu pratinjau, satu aksi berkonfirmasi |
| Task mode | `CROSS-REPO` — source di frontend, laporan di backend |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`, dan laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a383e7622` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `3e2fb76` (branch `rizkiG`) |
| Tanggal | 10 September 2026 |
| Status | Selesai. UAT peramban diserahkan kepada Rizki atas permintaannya |

---

## 1. Keadaan yang ditemukan di awal

Layar ini belum ada. Backend-nya sudah berdiri lewat `BE-ACC-P2-010`, dan diperiksa langsung —
bukan dibaca dari dokumen:

| Yang diperiksa | Hasil |
| --- | --- |
| Route | `api/v1/corporate/accounting/year-end-closing` — cocok dengan kontrak |
| Hak akses | `YearEndClosing : Read` dan `: Generate` — cocok |
| Penolakan akun laba ditahan belum ditetapkan | **`422`** beserta kalimat yang menyebut layar tujuannya |
| Penolakan periode belum tertutup | **`409`**, dan pesannya **sudah memuat daftar periodenya** |
| Penolakan jurnal sudah pernah disusun | `409` beserta nomor jurnal dan statusnya |

Temuan yang paling menentukan bentuk layar: **pesan `409` periode belum tertutup sudah menyebut
periode mana saja**, lengkap dengan statusnya — misalnya *"Masih ada 3 periode tahun 2026 yang
belum ditutup: Januari 2026 (Terbuka), Februari 2026 (Terbuka), Maret 2026 (Menunggu
Persetujuan)."*

Artinya acceptance (3) dipenuhi dengan **menampilkan pesan backend apa adanya**, bukan dengan
merakit daftar sendiri di layar. Merakitnya sendiri justru akan membuang keterangan status yang
sudah ada.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya Manajer Akuntansi.** Layar dibuka lewat **Akuntansi › Tutup Tahun**.

Tutup tahun menolkan seluruh akun pendapatan dan beban satu tahun buku, lalu memindahkan
selisihnya — laba atau rugi — ke akun laba ditahan.

1. Petugas memilih **badan hukum** dan mengetik **tahun buku**. Nilai awalnya tahun lalu, karena
   tutup tahun lazimnya dijalankan di awal tahun berikutnya.
2. Petugas menekan **Pratinjau**. Layar menampilkan perhitungannya: kartu ringkasan berisi total
   pendapatan, total beban, laba atau rugi, dan keseimbangan jurnalnya; lalu dua tabel — akun
   pendapatan dan akun beban.
3. Bila hasilnya sesuai, petugas menekan **Susun Jurnal Penutup**. Kotak konfirmasi menyebut
   berapa baris yang akan dibuat dan berapa nilai laba/rugi yang dipindahkan, dan menyediakan
   isian keterangan opsional.
4. Jurnalnya dibuat **berstatus Draft**. Ia masih harus disahkan lewat layar Jurnal — layar ini
   tidak pernah mengesahkan apa pun.

### Kenapa jaminan "Pratinjau tidak membuat apa pun" ada di paling atas

Acceptance (1) menuntutnya, dan kartunya sendiri menyebut alasannya: *"Acceptance (1) bukan
hiasan — tanpanya orang enggan menekan Pratinjau dan justru menebak angkanya."*

Tutup tahun terasa menakutkan. Petugas yang ragu akan menghindari tombolnya dan menghitung
sendiri di luar sistem — dan **menebak angka tutup tahun jauh lebih berbahaya daripada menekan
tombol yang tidak membuat apa pun**.

Karena itu jaminannya dirender **di atas** tombol Pratinjau, bukan di bawahnya, dan menyebut
empat hal secara tegas: tidak ada jurnal yang dibuat, tidak ada saldo yang berubah, tidak ada
periode yang tertutup, dan jurnal pun baru lahir berstatus Draft saat tombol yang lain ditekan.
Ada uji yang memagari urutan itu.

### Jalur tidak normal

- **Akun laba ditahan belum ditetapkan (`422`).** Pesan aslinya ditampilkan, **disertai tombol
  "Buka Pengaturan Akuntansi"**. Ini satu-satunya penghalang yang disertai tautan, karena hanya
  keadaan ini yang dapat diperbaiki petugas sendiri, di layar lain, sekarang juga.
- **Masih ada periode belum tertutup (`409`).** Pesan aslinya ditampilkan apa adanya beserta
  daftar periodenya. Tidak ada tautan, sebab tidak ada satu layar tujuan yang berguna.
- **Pratinjau kosong.** Tombol Susun mati beserta keterangan bahwa tidak ada saldo yang perlu
  ditutup.
- **Tanpa hak akses.** Tombol Susun **dimatikan, bukan disembunyikan**, disertai keterangan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `NewQuilvianSystemBackend/Areas/.../Controllers/YearEndClosingController.cs` | Route dan hak akses sebenarnya |
| `NewQuilvianSystemBackend/Areas/.../DTOs/YearEndClosingDtos.cs` | Bentuk pratinjau, baris, dan permintaan |
| `NewQuilvianSystemBackend/Areas/.../Services/AccYearEndClosingService.cs` | Seluruh pesan penolakan beserta kode statusnya |
| `src/components/view/corporate/accounting/general-ledger/trial-balance-view.jsx` | Modul referensi visual: tabel dan format rupiah |
| `src/utils/Formatters` | `formatCurrencyIDR` |
| `src/components/features/base-features/confirm-modal.jsx` | Modal konfirmasi berisi isian |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/corporate/accounting/accounting-year-end-slice.jsx` | **Baru.** Dua thunk, tanpa cache, dan **kode status galat ikut disimpan** |
| `src/lib/constants/corporate/accounting/accounting-year-end-constants.jsx` | **Baru.** `resolveYearEndBlocker`, salinan teks, kolom tabel |
| `src/lib/hooks/corporate/accounting/year-end/use-year-end-closing.jsx` | **Baru.** Pratinjau atas permintaan, konfirmasi, penyusunan |
| `src/components/view/corporate/accounting/year-end/year-end-closing-view.jsx` | **Baru.** Layarnya |
| `src/style/corporate/accounting/year-end-closing-view.module.css` | **Baru.** Hanya tata letak dan perataan angka |
| `src/app/corporate/accounting/year-end-closing/page.jsx` beserta client-nya | **Baru.** Entry point dan metadata saja |
| `tests/unit/accounting-configuration-and-year-end.test.mjs` | **Baru.** Dibagi bersama `FE-ACC-P2-005` |
| `src/lib/state/store.jsx` | Mendaftarkan slice baru |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir menu tingkat 2 |

### 3.3 Kepatuhan arsitektur frontend

`UI GATE: 10 elemen — REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Header halaman | `Hero` | `base-features/hero.jsx` | `REUSE` |
| Gerbang tanpa hak akses | `AccessDeniedGate` | `base-features/access-denied-gate.jsx` | `REUSE` |
| Jaminan Pratinjau dan pesan penolakan | `InformationAlert` | `base-features/information-alert.jsx` | `REUSE` |
| Pemilih badan hukum | `AccountingLegalEntitySelect` | `view/corporate/accounting/shared/` | `REUSE` |
| Isian tahun buku | `BaseTextField` | `base-form-control.jsx:207` | `REUSE` |
| Kartu ringkasan angka | `SummaryGrid` | `base-features/summary-grid.jsx`; pola sama dipakai Neraca Saldo | `REUSE` |
| Tabel baris pendapatan dan beban | `DataTable` | `base-features/data-table.jsx` | `REUSE` |
| Tombol Pratinjau dan Susun | `BaseButton` | `base-features/base-button.jsx` | `REUSE` |
| Tautan ke Pengaturan Akuntansi | `BaseButton as={Link}` | pola yang sama dipakai 4 view lain | `REUSE` |
| Konfirmasi berisi isian keterangan | `ConfirmModal` dengan `children` | pola sama dipakai layar Periode Akuntansi | `REUSE` |

Dua catatan pelaksanaan:

- **`sortLatestFirst={false}` pada seluruh tabel.** Baris pratinjau berurut kode akun dari
  backend; mengurutkannya ulang membuat urutannya tidak lagi cocok dengan jurnal yang akan
  disusun. Ada uji yang memagarinya.
- **Kode status galat disimpan di slice, bukan hanya pesannya.** Layar harus membedakan `422`
  dari `409` untuk memutuskan apakah tautan ke Pengaturan Akuntansi ditampilkan. Membedakannya
  dengan mencocokkan teks pesan akan rapuh terhadap perubahan kalimat.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Menghitung pratinjau..." pada tabel; tombol Pratinjau menampilkan keadaan memuat |
| Kosong — belum pratinjau | "Belum ada pratinjau." disertai "Pilih badan hukum dan tahun buku, lalu tekan Pratinjau." |
| Kosong — tidak ada pendapatan | "Tidak ada saldo pendapatan yang perlu ditutup." |
| Kosong — tidak ada beban | "Tidak ada saldo beban yang perlu ditutup." |
| Gagal `422` | Spanduk kuning beserta pesan asli **dan tombol Buka Pengaturan Akuntansi** |
| Gagal `409` | Spanduk merah beserta pesan asli, termasuk daftar periode yang belum ditutup |
| Gagal lain | Spanduk merah beserta pesan asli; `403` ditangani `AccessDeniedGate` |
| Gagal menyusun | Toast merah beserta pesan asli backend |
| Tanpa hak akses | Tombol Susun mati beserta keterangan; Pratinjau tetap dapat dijalankan |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - Accounting - Year End Closing

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/year-end-closing/preview` | Menghitung dan menampilkan pratinjau; **tidak membuat apa pun** | `YearEndClosing : Read` |
| `POST` | `/v1/corporate/accounting/year-end-closing/generate` | Menyusun jurnal penutup berstatus `Draft` | `YearEndClosing : Generate` |

Pengesahan jurnalnya memakai endpoint yang sudah ada, `POST /journals/{id}/post`, lewat layar
Jurnal. Layar ini **tidak** memanggilnya.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada berkas yang disentuh | Tanpa keluaran, exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` | 0 error, 673 warning | `PASS` | Jumlah warning tidak bertambah — berkas baru nol warning |
| `npm run build` | `✓ Compiled successfully in 34.5s` | `PASS` | Route `○ /corporate/accounting/year-end-closing` terdaftar |
| `node --test tests/unit/` | 627 lulus, 0 gagal | `PASS` | 12 uji baru dibagi dengan `FE-ACC-P2-005` |
| Grep anti-regresi 1, 3, 4, 5, 6, 7 | Kosong seluruhnya | `PASS` | Keluaran perintah |
| Grep anti-regresi 2 — typography | Lima temuan, dipertahankan | `PASS` | Seluruhnya menyasar `<h2>`, `<p>`, dan `<span>` milik layar ini sendiri |
| `UAT-P2-19` sampai `UAT-P2-22` | Tidak dijalankan | `NOT RUN` | **Diserahkan kepada Rizki atas permintaannya**, 10 September 2026 |

Uji manual: `NOT RUN` — pemilik meminta pengujian layar dilakukan sendiri.

**Tidak dijalankan:** verifikasi terhadap API sungguhan. Backend lokal sudah dimatikan saat task
ini dikerjakan; bentuk respons dan seluruh kalimat penolakan diambil dari **source**, yang tetap
otoritatif.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Layar menyatakan tegas bahwa Pratinjau tidak membuat apa pun | Terpenuhi | `InformationAlert` dirender **sebelum** tombol Pratinjau; uji "jaminan 'Pratinjau tidak membuat apa pun' ada dan mendahului tombolnya" |
| (2) Tombol Susun mati bila pratinjau kosong atau akun laba ditahan belum ditetapkan, disertai tautan ke Pengaturan Akuntansi | Terpenuhi | `resolveYearEndBlocker` memisahkan `PRATINJAU_KOSONG` dari `KONFIGURASI`; tautan muncul **hanya** pada `422`. Uji "422 dikenali sebagai akun laba ditahan belum ditetapkan" dan "pratinjau tanpa baris menghalangi penyusunan" |
| (3) Penolakan karena periode belum tertutup menampilkan periode mana saja, bukan pesan umum | Terpenuhi | Pesan backend ditampilkan apa adanya, dan ia sudah memuat daftarnya. Uji "layar tidak merakit sendiri kalimat periode belum tertutup" memagari agar tidak diganti kalimat buatan layar |
| (4) Tidak memakai cache | Terpenuhi | Pratinjau hanya berjalan saat tombolnya ditekan, dan setiap penekanan memanggil backend lagi; pratinjau dibuang sesudah jurnal berhasil disusun |

**Definition of Done:** lint hijau, build hijau, dan laporan tracked ini ada — terpenuhi. Yang
**belum** terpenuhi: keempat UAT peramban, yang diserahkan kepada pemilik.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` yang berasal dari task ini |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `BE-ACC-P2-010` berstatus `SEBAGIAN` — enam acceptance-nya terpenuhi, tetapi bukti PostgreSQL belum lengkap. Tidak menghalangi layar ini; ia hanya berarti angka pratinjau belum diuji pada basis data sungguhan |
| Prasyarat pemakaian | **Akun laba ditahan wajib sudah ditetapkan** lewat `FE-ACC-P2-005`. Tanpa itu Pratinjau menolak `422` — dan layar ini memang menampilkan tautan ke sana untuk keadaan itu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tujuh berkas baru ditambah dua berkas diubah (`store.jsx`, `menu-items.jsx`), dibagi bersama `FE-ACC-P2-005`. Tidak ada stage, commit, maupun push |
| Langkah berikutnya | Tetapkan akun laba ditahan lebih dahulu, lalu jalankan `UAT-P2-19` sampai `UAT-P2-22` |
