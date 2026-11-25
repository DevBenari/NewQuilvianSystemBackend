# Laporan Perubahan Frontend — `FE-ACC-FIN-008-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-FIN-008-02` |
| Judul | Layar laba rugi dan menunya |
| Slice | Gelombang pengiriman `FIN-1` |
| Roadmap | `roadmap/frontend-roadmap-finishing.md` revisi 1, kartu `FE-ACC-FIN-008-02` |
| Trace | ACC-FIN-008 Income Statement (ClickUp `z8m132yv84`); `FR-FIN-010`..`015`, `040`; `ACC-DEC-142`..`147`, `149`, `152`; `03-frontend-architecture.md` bagian 15.2, 15.6, 15.9 |
| Contract version | `ACC-API-0.17` (`GET /general-ledger/income-statement`, `/income-statement/export`) — approved Rizki 24 November 2026 (`GATE-DESAIN-1124`) |
| Dependency | `BE-ACC-FIN-008-02` ✅ (commit backend `0d616989`), `FE-ACC-FIN-005-01` ✅ (pengunduh, `buildGeneralLedgerHref`) |
| Task mode | `FRONTEND` — perintah Rizki 25 November 2026 "lanjut FE-ACC-FIN-008-02" |
| Target tulis | Repository `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `c72037029` |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-11 terbukti 25 November 2026: uji layar Playwright agent terhadap `next dev` yang dinyalakan Rizki, 16/16; ESLint 0/0; `npm run build` `PASS` (Rizki, 10:24:55 WIB). UAT belum dijalankan — milik tim UAT |

## 1. Masalah yang diperbaiki

Accounting belum punya layar laba rugi. Laporan laba rugi dari backend (`BE-ACC-FIN-008-02`) tidak dapat dibaca petugas tanpa Swagger, tidak dapat diunduh dari layar, dan tidak dapat dicetak.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Membaca pendapatan, beban, dan laba bersih satu periode beserta tahun berjalan dan pembanding tahun lalu, lalu menelusuri angka ke buku besar |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | Menu Akuntansi → Laba Rugi |
| Langkah | 1) Pilih periode; bila perlu pilih unit biaya dan tampilkan akun bersaldo nol. 2) Ringkasan tiga angka tahun berjalan dan tabel bertingkat enam kolom tampil. 3) Klik angka kolom periode atau tahun berjalan pada baris akun → Buku Besar akun itu dengan rentang kolom tersebut. 4) Unduh CSV atau Cetak |
| Jalur tidak normal | Pembanding tidak ada → kolom `—` dan catatan alasan. Gagal memuat → "Laba rugi gagal dimuat." beserta tombol Coba lagi. Gagal unduh → "Berkas gagal diunduh.". Tanpa hak → `AccessDeniedGate` |
| Hasil | Laporan di layar, berkas `laba-rugi_{periode}[_{unit}].csv`, atau cetakan tanpa menu, saringan, dan tombol |

**Contoh.** Petugas membuka Desember 2020. Kolom pembanding berjudul "Desember 2019" dan "Jan–Des 2019", dengan laba bersih pembanding Rp 300.000 dan YTD pembanding Rp 270.000. Selisihnya tampil "(Rp 300.000)", dan pembaca layar membacanya "minus Rp 300.000". Akun Pendapatan Rawat Inap yang bersaldo debit tampil "(Rp 30.000)" dan mengurangi subtotal induknya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/corporate/accounting/income-statement/page.jsx`, `income-statement-client.jsx` | **Baru.** Rute tipis, pola Neraca Saldo |
| `src/components/view/corporate/accounting/general-ledger/income-statement-view.jsx` | **Baru.** Hero, `DataFilter` (periode, unit biaya, akun nol; Unduh CSV, Cetak, Muat Ulang), `SummaryGrid` tiga angka YTD, `InformationAlert` (galat + Coba lagi, gagal unduh, catatan unit, alasan tanpa pembanding), tabel laporan |
| `src/components/view/corporate/accounting/general-ledger/financial-statement-table.jsx` | **Baru — dipakai ulang `FE-ACC-FIN-008-03`.** Rangkaian `DataTable` dengan baris judul bagian, akun, induk (subtotal + penanda "Subtotal"), total, dan jumlah akhir; angka akun di kolom yang punya rentang menjadi `BaseButton ghost` ke Buku Besar |
| `src/lib/hooks/corporate/accounting/general-ledger/use-income-statement.jsx` | **Baru.** Saringan, permintaan dengan pembatalan, pembersihan saat badan hukum/saringan berganti, label kolom dan rentang dari respons, unduhan blob, cetak, tautan Buku Besar bertoken |
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | + thunk `getIncomeStatement` (pola `requestId`), state, `clearIncomeStatement`, tiga selector |
| `src/lib/constants/corporate/accounting/general-ledger/income-statement-constants.jsx` | **Baru.** Config, keadaan kosong, salinan teks, pilihan akun nol |
| `src/lib/constants/corporate/accounting/general-ledger/financial-statement-constants.jsx` | **Baru — dipakai ulang `008-03`.** Jenis baris, jenis akun, kunci nilai, salinan aksesibilitas, nama bulan singkat |
| `src/utils/corporate/accounting/general-ledger/financial-statement-utils.js` | **Baru — fungsi murni, dipakai ulang `008-03`.** Pendataran bagian ke baris tabel, format angka (negatif dalam kurung + teks "minus", `null` → `—`), label rentang bulan, tanggal |
| `src/style/corporate/accounting/financial-statement-table.module.css` | **Baru.** Indentasi lewat variabel `--statement-depth`, penanda subtotal, kelas `srOnly`; hanya token |
| `src/style/corporate/accounting/income-statement-view.module.css` | **Baru.** Pembungkus saringan; `@media print` yang hanya berlaku saat layar ini tampil (`body:has(.printRoot)`) |
| `src/utils/menu-sidebar/menu-items.jsx` | + butir **Laba Rugi** sesudah Neraca Saldo (`RiLineChartLine`), tanpa `requiredPermission` seperti butir Accounting lain |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | + kartu beranda Laba Rugi |

Nol baris komentar baru. **Delta lingkup:**

- **Berkas yang tidak tercantum di kartu.** Komponen tabel, util, dan konstanta laporan dipisah supaya `008-03` memakainya ulang. Kartu memberi letak dan bentuk komponen tabel sebagai `DEV_DISCRETION`.
- **CSS tabel.** CSS tabel menjadi berkas tersendiri.
- **Saringan akun nol.** Sketsa memakai kotak centang, tetapi layar memakai `FilterSelect` dua opsi. Alasannya ada di bagian 4.

### 3.2 Dampak kontrak, keamanan, privasi

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Persis `ACC-API-0.17`; nol delta |
| Penyusunan pohon | Tidak dilakukan frontend; urutan, kedalaman, subtotal, total, dan laba bersih dibaca apa adanya dari respons (`03-frontend-architecture.md` bagian 15.9) |
| Nama berkas | Disusun frontend dengan pola backend, karena CORS tidak mengekspos `Content-Disposition` |
| Tautan | Akun dan unit biaya di alamat Buku Besar memakai token rute privat, bukan Guid (diuji L8) |
| Basi | Mengganti periode, unit, akun nol, atau badan hukum membersihkan laporan; respons lama dibuang lewat `requestId` |

## 4. Gerbang keputusan base component

`UI GATE: 9 elemen — REUSE 8, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Hero dan badan hukum | `Hero` + `AccountingHeroLegalEntity` | Neraca Saldo, Buku Besar | REUSE | — |
| Saringan periode | `DataFilter` + `FilterSelect` | Neraca Saldo | REUSE | — |
| Saringan unit biaya | `ResourceFilterSelect` + `useSelectResource("costCenters")` | Buku Besar | REUSE | `onlyActive: false` (`VAL-GL-01`) |
| Akun bersaldo nol | `FilterSelect` dua opsi | Pola saringan di seluruh `DataFilter` | REUSE | Lihat pilihan A |
| Unduh, Cetak, Muat Ulang, Coba lagi | `BaseButton` | Neraca Saldo, Buku Besar | REUSE | — |
| Ringkasan tiga angka | `SummaryGrid` | Buku Besar | REUSE | — |
| Catatan unit, alasan, galat | `InformationAlert` | Buku Besar | REUSE | Galat memakai `title` + `message` berisi tombol |
| Tabel bertingkat | `DataTable` | Semua layar list | COMPOSE | Lihat pilihan B |
| Tautan angka ke Buku Besar | `BaseButton ghost size sm` | Nomor jurnal di Buku Besar | REUSE | — |

**Pilihan A — kontrol akun bersaldo nol** (diputuskan agent, mengikuti konvensi "pakai ulang, jangan bikin tandingan").

1. **`FilterSelect` dua opsi ("Sembunyikan/Tampilkan akun bersaldo nol") — direkomendasikan dan dipakai.** Bentuk dan tinggi kontrol sama dengan saringan lain. Risiko regresi nol. Biaya nol.
2. `BaseCheckboxCard` compact di dalam `DataFilter`. Bentuknya berupa kartu, sehingga tingginya berbeda dari kontrol saringan dan merusak keselarasan baris.
3. `<input type="checkbox">` mentah seperti layar Finance. Ini melanggar checklist UI dan menyalin pola legacy.

**Pilihan B — tabel bertingkat.**

1. **COMPOSE `DataTable` dengan baris yang didatarkan beserta jenisnya — direkomendasikan dan dipakai.** Nol perubahan base component. Indentasi lewat variabel CSS. Subtotal dan total memakai `<strong>`, ditambah penanda "Subtotal".
2. EXTEND `DataTable` dengan prop `getRowClassName`. Latar baris total bisa dibedakan, tetapi berkas bersama yang dipakai ratusan layar ikut tersentuh.
3. Tabel mentah `data-flat-table`. Kendalinya penuh, tetapi menduplikasi base component.

## 5. Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Modul referensi visual | Buku Besar (`FE-ACC-FIN-005-01`); Neraca Saldo untuk saringan periode |
| Urutan halaman | Hero → DataFilter → SummaryGrid → InformationAlert → tabel, sesuai skema `FE-L3` dan Buku Besar (ringkasan bergantung pada periode terpilih) |
| Kolom di berkas `*-table-columns.jsx` | Tidak. Kolom disusun di komponen tabel laporan, karena judulnya diturunkan dari respons |
| Grep: warna literal / typography / `<button>` / `<table>` / `fw-` `fs-` / `!important` | 0 / 0 / 0 / 0 / 0 / 0 |
| Inline style | Satu: `style={{ "--statement-depth": row.depth }}`. Nilainya dinamis per baris, sesuai aturan |
| `globals.css` | Tidak disentuh. Aturan cetak memakai `:global` di module CSS dan hanya berlaku saat layar ini tampil |
| Aksesibilitas | Angka negatif punya teks tersembunyi "minus …", nilai kosong dibaca "tidak tersedia", induk punya penanda "Subtotal", dan tombol angka punya `aria-label` "Buka buku besar {akun} untuk {rentang}" |
| `DataTable` `sortLatestFirst` | `false`, supaya urutan pra-order dari backend tidak diurutkan ulang |

## 6. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` 11 berkas JS/JSX tersentuh | Kode keluar `0`, 0 galat, 0 peringatan; dua peringatan React Compiler atas dependensi `useMemo` diperbaiki sebelumnya | `PASS` |
| Pemindaian baris komentar baru | Nol | `PASS` |
| `npm run build` | Dijalankan Rizki 25 November 2026; `.next/BUILD_ID` 10:24:55 WIB, sesudah suntingan source terakhir 10:16:28; keluaran memuat rute `corporate/accounting/income-statement` | `PASS` |
| Kompilasi `next dev` | Rute `/`, `/trial-balance`, `/income-statement`, `/general-ledger` → `200` | `PASS` |
| Uji layar `07-uji-layar.mjs` L1–L16, run 3 (resmi, 10.17–10.18 WIB) | 16/16 | `PASS` |
| Regresi Neraca Saldo (`09-regresi-neraca-saldo.mjs`, salinan skrip `008-01`) | 10/11. V4 gagal karena skrip mengunci "5 baris", sedangkan data uji 2019 (`BE-ACC-FIN-008-02`) kini menambah 4 baris bersaldo pembuka. API menunjukkan 9 baris, total 2.900.000, seimbang. Bukan regresi kode | `PASS` dengan catatan |

**Riwayat run uji layar** (JSON tiap run disimpan):

- **Run 1 (10.10 WIB):** 12/14. Hasilnya L10 gagal dan skrip berhenti.
- **Run 2 (10.12 WIB):** hasil yang sama, sesudah perbaikan cetak pertama.
- **Run 3:** semua lulus, sesudah perbaikan cetak kedua dan perbaikan skrip.

Penyebab dan perbaikannya ada di bagian 9.

`MANUAL TEST: PASS`. Playwright (Edge, headless) dijalankan terhadap `next dev` yang dinyalakan Rizki, dengan layar dibuka lewat klik menu. Bukti JSON, PNG, dan CSV ada di `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-008-02-20261125/`.

Simulasi yang dipakai:

- L2 menunda respons asli selama 2,5 detik.
- L14 dan L15 memutus jaringan (`route.abort`).
- L10 memakai emulasi media cetak (`page.emulateMedia`). Dialog cetak sungguhan tidak dibuka.

L16 memakai pengguna nyata tanpa hak.

`AUTOMATED TEST: SKIPPED (opsional) — unit test tidak ditulis atas kesepakatan kerja Rizki.`

## 7. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 Menu Laba Rugi sesudah Neraca Saldo membuka layar | **Terpenuhi** | L1: urutan menu Jurnal … Buku Besar, Neraca Saldo, **Laba Rugi**, Rekonsiliasi Control Account, Tutup Tahun (PNG `L01-menu`) |
| AC-2 Enam kolom angka dan tiga ringkasan YTD | **Terpenuhi** | L2: kolom "September 2026, Pembanding, Selisih, Jan–Sep 2026, YTD Pembanding, Selisih YTD"; ringkasan "Tahun berjalan Jan–Sep 2026" (PNG `L02-september`) |
| AC-3 Indentasi `Depth`; induk sebagai subtotal | **Terpenuhi** | L4: indentasi 0/20/40 px; penanda "Subtotal"; urutan dari respons |
| AC-4 Negatif dalam kurung (`F8`) | **Terpenuhi** | L11: 4-1002 "(Rp 30.000)" dibaca "minus Rp 30.000"; subtotal 4-1000 Rp 470.000 (PNG `L06-akun-kontra`); L12 selisih "(Rp 300.000)" |
| AC-5 Pembanding tidak ada → `—` + alasan (`F7`) | **Terpenuhi** | L3 "Periode September 2025 belum ada di sistem.", kolom `—`; L12 jalur pembanding ada (PNG `L07-pembanding`) |
| AC-6 Unit biaya → catatan unit (`F11`) | **Terpenuhi** | L6 (PNG `L03-unit-biaya`) |
| AC-7 Akun nol memuat ulang dengan `IncludeZeroAccounts` (`F9`) | **Terpenuhi** | L5: permintaan `includeZeroAccounts=true` → 19 baris; `false` → 8 baris |
| AC-8 Klik baris akun → Buku Besar dengan rentang kolom | **Terpenuhi** | L8: klik angka YTD 4001 membuka Buku Besar dengan `dateFrom=2026-01-01`, `dateTo=2026-09-30`, dan unit terpilih, lewat alamat bertoken (PNG `L04-buku-besar`) |
| AC-9 Unduh CSV; Cetak tanpa menu, saringan, tombol | **Terpenuhi** | L7 `laba-rugi_2026-09_CC-MMC-00001.csv`; L9 `laba-rugi_2026-09.csv`, kode sama dengan tabel; L10 cetak (PNG `L05-cetak`) |
| AC-10 Keadaan belum pilih, kosong, gagal, memuat | **Terpenuhi** | L1, L13 (PNG `L08-kosong`), L14 (PNG `L09-galat`), L2 (PNG `L02-memuat`) |
| AC-11 Lint nol galat | **Terpenuhi** | ESLint kode keluar `0` |
| Negatif: tanpa hak; gagal unduh | **Terpenuhi** | L16 (PNG `L10-tanpa-hak`); L15 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan, tidak diubah | (1) Tautan dari akun nonaktif atau akun tanpa izin posting membuka Buku Besar dengan pemilih akun yang tidak menampilkan labelnya, karena daftar pilihan Buku Besar hanya berisi akun aktif yang menerima transaksi. Datanya tetap dimuat. (2) Footer tata letak global menutupi baris bawah tabel pada tangkapan layar layar; ini perilaku semua layar. (3) **ESLint proyek tidak menangkap `export` yang salah tempat** (diuji lewat stdin: kode keluar `0`). Lint lulus bukan bukti sintaks benar; bukti yang sah adalah kompilasi `next dev`/`next build` |
| Risiko | Rendah. Aturan cetak bergantung pada `#app-sidebar`, `.iq-top-navbar`, `footer`, dan variabel `--mmc-sidebar-width` dari tata letak global. Bila tata letak diganti, cetak perlu diperiksa ulang |
| Status Git frontend | `M` slice buku besar, `menu-items.jsx`, `accounting-constants.jsx`; `??` 10 berkas baru (bagian 3.1). Belum di-commit |
| Langkah berikutnya | `BE-ACC-FIN-008-03` (neraca), lalu `FE-ACC-FIN-008-03` yang memakai ulang tabel, util, dan konstanta laporan |

## 9. Cacat yang ditemukan saat pengerjaan dan diperbaiki

| Hal | Isi |
| --- | --- |
| Frontend 500 sementara | Sisipan thunk `getIncomeStatement` pertama mendarat di tengah thunk `getTrialBalance`, sehingga `next dev` menolak slice dan **semua halaman** menjawab `500` sejak sisipan itu sampai diperbaiki, beberapa menit kemudian dalam sesi ini, sebelum uji layar pertama. ESLint tidak menangkapnya (lihat temuan 3). Thunk dipindah ke tempat yang benar; keempat rute kembali `200` |
| Cetak menampilkan sidebar dan footer | Pemilih `:global(...)` tanpa kelas lokal dibuang oleh CSS Module. Diganti dengan `body:has(.printRoot)`. Sesudah itu masih gagal karena tata letak global memasang `.app-sidebar { display: flex !important }` dan `margin-left: var(--mmc-sidebar-width) !important`. Diselesaikan tanpa `!important`: sidebar dan footer yang `position: fixed` diberi `visibility: hidden`, dan `--mmc-sidebar-width` diisi `0px` hanya saat cetak di layar ini. Diverifikasi lewat gaya terhitung (sidebar `hidden`, margin konten `0px`) dan PNG `L05-cetak` |
| Skrip uji | `page.unroute` diberi fungsi pencocok baru, bukan referensi yang sama, sehingga rute pemutus tidak pernah dilepas. Diperbaiki dengan konstanta `matchIs`/`matchIsExport`. Perbaikan ini mengubah skrip, bukan source |
