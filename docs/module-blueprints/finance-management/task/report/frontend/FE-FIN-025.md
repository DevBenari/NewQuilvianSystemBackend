# Laporan Perubahan Frontend — `FE-FIN-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-025` |
| Judul | Petugas dapat menelusuri mengapa sisa piutang atau utang berubah, baris per baris — Dua Permukaan Riwayat Mutasi Subledger |
| Slice | `REV-14A` — `EPIC FIN-20` (Buku Mutasi dan Tanggal WIB) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` Bagian REV-14A |
| Trace | `FR-FIN-140`; `FIN-DEC-123`; `03-frontend-architecture.md` Bagian 19.2 nomor 3-4 |
| Contract version | `FIN-API-1.5` Bagian F.5; `FIN-PERM-1.7` Bagian G.3 — seluruhnya `approved` 2 Oktober 2026 |
| Wewenang UI | Frontend Owner — **sebagian tertahan** `FIN-OQ-079` untuk penempatan menu sidebar. Tampilan tabel, format nominal bertanda, teks keadaan kosong cutover, proteksi kerahasiaan `Notes`, dan tautan berkas bukti **mengikat**. Bentuk tabel, lebar kolom, dan tata letak adalah `DEV_DISCRETION` |
| Dependency | `BE-FIN-063` ✅ (Endpoint baca mutasi subledger berpaging dan berfilter, `dotnet build` PASS) |
| Klasifikasi | `MEDIUM` — frontend target; 2 view baru + 2 page rute + 2 hook + 2 constants/utils + 2 slice (1 baru, 1 diubah); 2 endpoint mutasi utama + 2 endpoint induk; verifikasi linter dan build Next.js sukses |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk verifikasi DTO/kontrak serta pembaruan laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev` — rute `src/app/finance/receivable/[slug]/movements/page.jsx`, `src/app/finance/payable/[slug]/movements/page.jsx`, komponen view `receivable-movements-view.jsx`, `supplier-payable-movements-view.jsx`, hook `use-receivable-movements.jsx`, `use-supplier-payable-movements.jsx`, Redux slice, konstanta, utilitas, unit test, serta laporan ini |
| Model | Gemini 3.8 Flash (High) |
| Commit frontend | Branch `yasmina` |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ **Selesai 2 Oktober 2026.** Seluruh acceptance criteria terpenuhi, `npm run lint:errors` **PASS** (0 errors), unit test **PASS** (6/6 pass), `npm run build` **PASS** (exit code 0, TurboPack standalone, rute `/finance/receivable/[slug]/movements` dan `/finance/payable/[slug]/movements` terkompilasi sukses). |

---

## 1. Masalah yang Diperbaiki

Sebelum diberlakukannya buku mutasi subledger, modul Keuangan (Finance) hanya mencatat dan menyajikan saldo agregat pada master piutang (`TotalReceivable` dan `RemainingBalance`) serta master utang supplier (`TotalAmount` dan `RemainingAmount`). Ketika terjadi perubahan saldo (seperti tagihan baru masuk, alokasi pembayaran kasir rawat inap/jalan, pelunasan sebagian faktur obat farmasi, diskon pelunasan cepat/termin, retur barang medis rusak, pembatalan penerimaan kas, atau koreksi audit), staf keuangan dan auditor rumah sakit menghadapi kendala serius:

1. **Ketiadaan Jejak Audit Kronologis per Baris:** Petugas tidak dapat membuktikan transaksi mana yang menambah atau mengurangi sisa piutang/utang pada tanggal dan jam tertentu.
2. **Kekeliruan Makna Tanda Nominal:** Tanpa visualisasi bertanda (`+` dan `-`) yang jelas, staf sering salah menafsirkan apakah suatu angka berarti penambahan utang/piutang baru atau pembayaran yang melunasi sisa.
3. **Ambigu Keadaan Kosong (Empty State):** Sebelum tanggal cutover diberlakukan di rumah sakit, riwayat mutasi memang belum terbentuk. Menampilkan teks generik `"tidak ada data"` akan membingungkan pengguna yang mengira sistem sedang rusak atau data hilang. Sesuai kontrak `FR-FIN-140` dan `FIN-DEC-123`, keadaan kosong wajib menyatakan secara lugas: `"buku mutasi baru berjalan sejak tanggal cutover"`.
4. **Perlindungan Kerahasiaan Pihak Ketiga (Data Privacy):** Kolom catatan (`Notes`) pada mutasi subledger berpotensi mengandung rincian internal atau data pihak ketiga yang sensitif. Sesuai arsitektur keamanan `FIN-PERM-1.7` Bagian G.3 dan `03-frontend-architecture.md` Bagian 19.6, kolom `Notes` **dilarang keras** ditampilkan pada ringkasan tabel publik yang dapat dilihat luas.
5. **Keterbatasan Penempatan Menu (`FIN-OQ-079`):** Penempatan butir menu permanen pada navigasi sidebar induk masih ditahan oleh pertanyaan terbuka `FIN-OQ-079`. Petugas membutuhkan sarana yang sah, intuitif, dan terdokumentasi untuk mencapai kedua layar mutasi tanpa melanggar tata kelola menu.

Task **`FE-FIN-025`** menyelesaikan seluruh kendala di atas dengan menghadirkan dua permukaan layar mutasi subledger berpaging dan tersaring penuh, lengkap dengan penanganan nominal bertanda, perlindungan data, verifikasi cutover, dan tautan navigasi kontekstual.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Pelaku:** Staf Piutang (AR Staff), Staf Utang Supplier (AP Staff), Supervisor Penagihan Asuransi/BPJS, Kasir Induk, Manajer Keuangan, dan Auditor Internal Rumah Sakit.

**Pemicu:** Rekonsiliasi saldo piutang penjamin/pasien, konfirmasi mutasi tagihan dengan supplier farmasi/alkes, pemeriksaan perbedaan sisa saldo buku besar, atau audit kepatuhan keuangan berkala.

### Skenario Konkret Rumah Sakit

#### Skenario A: Rekonsiliasi Piutang Klaim BPJS Kesehatan / Asuransi Swasta
1. Pasien rawat inap selesai dirawat dengan total klaim Rp 150.000.000. Sistem mencatat mutasi jenis **Pencatatan Piutang Baru (`NEW_RECEIVABLE`)** sebesar `+Rp 150.000.000`. Saldo awal `Rp 0`, saldo akhir `Rp 150.000.000`.
2. BPJS mengirimkan pembayaran termin pertama sebesar Rp 100.000.000. Sistem mencatat mutasi jenis **Penerimaan Pembayaran (`PAYMENT_RECEIPT`)** sebesar `-Rp 100.000.000`. Saldo sebelum `Rp 150.000.000`, saldo sesudah `Rp 50.000.000`.
3. Terdapat sengketa biaya administrasi sebesar Rp 2.000.000 yang disetujui untuk dihapusbukukan. Sistem mencatat mutasi jenis **Penghapusan Piutang (`WRITE_OFF`)** sebesar `-Rp 2.000.000`. Saldo sesudah menjadi `Rp 48.000.000`.
4. Petugas AR membuka layar riwayat mutasi piutang untuk melihat rincian 3 baris mutasi tersebut secara runut dari tanggal terlama hingga terbaru (atau sebaliknya).
5. Pada baris mutasi pembayaran, petugas dapat mengklik tombol **"Unduh Bukti"** untuk mengunduh berkas PDF bukti transfer bank yang dilampirkan.

#### Skenario B: Rekonsiliasi Utang Faktur Supplier Farmasi (PT Kimia Farma)
1. Bagian Pengadaan Farmasi menerima pasokan obat kemoterapi senilai Rp 80.000.000 dengan tempo pembayaran 30 hari. Sistem mencatat mutasi jenis **Faktur Masuk (`INVOICE_ENTRY`)** sebesar `+Rp 80.000.000`.
2. Rumah sakit melakukan pembayaran melalui bilyet giro sebesar Rp 50.000.000. Sistem mencatat mutasi jenis **Pembayaran Utang (`PAYMENT_DISBURSEMENT`)** sebesar `-Rp 50.000.000`. Sisa utang berkurang menjadi `Rp 30.000.000`.
3. Ditemukan beberapa botol infus yang rusak dan diretur ke supplier senilai Rp 5.000.000. Sistem mencatat mutasi jenis **Retur Pembelian (`PURCHASE_RETURN`)** sebesar `-Rp 5.000.000`. Sisa utang menjadi `Rp 25.000.000`.
4. Petugas AP membuka `/finance/payable/[id]/movements` untuk memastikan kartu utang supplier cocok dengan surat penagihan bulanan dari PT Kimia Farma.

### Penjelasan Matematika Tanda Nominal
Sesuai check constraint basis data backend (`BalanceAfter = BalanceBefore + Amount`):
- **Nilai Positif (`+`):** Ditampilkan dengan warna peringatan tegas (orange/kuning emas), menandakan transaksi yang **menaikkan sisa tagihan/utang**.
- **Nilai Negatif (`-`):** Ditampilkan dengan warna hijau sukses, menandakan transaksi yang **menurunkan sisa tagihan/utang** (pelunasan, alokasi kasir, diskon, retur, atau penghapusbukuan).
- **Nilai Nol (`Rp 0`):** Ditampilkan netral tanpa tanda.

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

### 3.1 Hierarki Wewenang UI

| Aspek UI | Wewenang | Rujukan / Bukti | Status Kepatuhan |
|---|---|---|---|
| Nominal bertanda terbaca jelas menaikkan/menurunkan sisa | **Mengikat** | `02-frontend-roadmap.md` baris 551; `FR-FIN-140` | **Dipatuhi** — format `formatSignedMoney` menampilkan tanda `+` dan `-` secara eksplisit beserta badge warna pembeda efek (`INCREASE` vs `DECREASE`). |
| Teks keadaan kosong buku mutasi cutover | **Mengikat** | `02-frontend-roadmap.md` baris 551; `FIN-DEC-123` | **Dipatuhi** — jika mutasi kosong, `DataTable` menampilkan judul: `"buku mutasi baru berjalan sejak tanggal cutover"`, bukan `"tidak ada data"`. |
| Proteksi kerahasiaan kolom Catatan (`Notes`) | **Mengikat** | `02-frontend-roadmap.md` baris 551; `FIN-PERM-1.7` Bagian G.3; `03-frontend-architecture.md` §19.6 | **Dipatuhi** — kolom `Notes` **tidak** dimasukkan ke dalam daftar kolom ringkasan tabel publik. |
| Penempatan menu tertahan `FIN-OQ-079` | **Mengikat** | `02-frontend-roadmap.md` baris 551; `FIN-OQ-079` | **Dipatuhi** — menu sidebar belum didaftarkan permanen; akses dibuka lewat URL langsung serta tautan navigasi kontekstual dari halaman detail dan tabel daftar terkait. |
| Tombol/tautan unduh berkas bukti transaksi | **Mengikat** | `02-frontend-roadmap.md` baris 551; `FIN-API-1.5` F.5 | **Dipatuhi** — tombol unduh bukti aktif jika `ProofId` tersedia; mengunduh berkas dengan nama aman melalui blob stream terautentikasi. |
| Tata letak, lebar kolom, dan tipografi | `DEV_DISCRETION` | Design token Quilvian & CSS Modules | **Dipatuhi** — tata letak responsif menggunakan token CSS Quilvian standar. |

### 3.2 Tabel Keputusan Komponen Dasar (UI Gate Decision Table)

| No | Elemen Layar | Status Keputusan | Komponen Sumber | Rekomendasi & Justifikasi |
|:---:|---|:---:|---|---|
| 1 | Header Halaman & Breadcrumb | `REUSE` | `@/components/features/base-features/hero` | Menggunakan komponen standar `Hero` dengan judul, deskripsi, breadcrumb hierarkis, dan tombol kembali ke halaman induk. |
| 2 | Kartu Ringkasan Induk (Summary Card) | `REUSE` | Bootstrap standard card & grid layout | Menampilkan nomor transaksi induk, pihak ketiga (pasien/penjamin/supplier), total nilai, dan sisa outstanding terkini. |
| 3 | Filter Tanggal & Jenis Mutasi | `REUSE` | `@/components/features/base-features/data-filter` | Menggunakan `DataFilter` standar dengan `FilterSelect` untuk jenis mutasi, pengurutan waktu, dan rentang tanggal `dateFrom` - `dateTo`. |
| 4 | Tabel Riwayat Mutasi Berpaging | `REUSE` | `@/components/features/base-features/data-table` | Menggunakan `DataTable` terstandarisasi dengan perataan numerik, badge visual, dan slot kustomisasi `emptyTitle`. |
| 5 | Paginasi Halaman | `REUSE` | `@/components/features/base-features/pagination` | Menggunakan `Pagination` terstandarisasi dengan pilihan ukuran halaman (10, 25, 50, 100) dan navigasi aman. |
| 6 | Badge Status & Badge Jenis Mutasi | `REUSE` | `@/components/features/base-features/status-badge` | Menampilkan badge status transaksi dan kategori mutasi dengan tone warna konsisten. |
| 7 | Alert Informasi Cutover | `REUSE` | `@/components/features/base-features/information-alert` | Menampilkan banner edukatif penjelasan saldo sebelum/sesudah dan tanggal cutover subledger. |
| 8 | Pembatas Hak Akses | `REUSE` | `@/components/features/base-features/access-denied-gate` | Menutup akses layar secara aman dengan kode 403 Forbidden bila pengguna tidak memiliki izin baca. |
| 9 | Notifikasi Feedback Sistem | `REUSE` | `@/components/features/base-features/toast-stack` | Menampilkan pesan keberhasilan unduh bukti atau peringatan galat jaringan API. |
| 10 | Tombol Aksi & Unduh Bukti | `REUSE` | `@/components/features/base-features/base-button` | Menggunakan `BaseButton` standar Quilvian dengan varian `primary`, `outline`, dan `secondary`. |

**Hasil UI Gate:** 10 REUSE, 0 NEW. Tidak ada pembuatan komponen visual dasar baru yang menyimpang dari pustaka token sistem.

---

## 4. Cara Mencapai Layar Selama Menu Tertahan (`FIN-OQ-079`)

Sesuai ketentuan `02-frontend-roadmap.md` baris 551, penempatan menu sidebar masih tertahan oleh pertanyaan arsitektur `FIN-OQ-079`. Agar staf keuangan tetap dapat bekerja tanpa kendala, akses ke kedua layar disediakan melalui **tiga jalur resmi**:

### 4.1 Jalur URL Langsung (Direct Deep-Link)
Petugas atau pengembang dapat langsung membuka URL rute di peramban:
- **Riwayat Mutasi Piutang:** `/finance/receivable/{id}/movements`
- **Riwayat Mutasi Utang Supplier:** `/finance/payable/{id}/movements`

### 4.2 Jalur Navigasi dari Layar Rincian Induk (Detail View)
Pada layar rincian piutang (`/finance/receivable/[slug]`):
- Pada bilah header **Hero Actions**, telah ditambahkan tombol aksi utama: **"Riwayat Mutasi"** (ikon `RiHistoryLine`).
- Mengklik tombol ini akan membuka layar riwayat mutasi piutang yang bersangkutan secara instan.

### 4.3 Jalur Navigasi Cepat dari Tabel Daftar Transaksi (List View)
1. **Daftar Piutang (`/finance/receivable`):**
   - Pada kolom aksi di setiap baris tabel, tersedia tautan cepat **"Mutasi →"** berwarna biru kontras.
   - Petugas dapat langsung melihat riwayat mutasi tanpa harus masuk ke layar detail terlebih dahulu.
2. **Daftar Utang Supplier (`/finance/payable`):**
   - Pada kolom aksi di setiap baris tabel, tersedia tautan cepat **"Mutasi →"**.
   - Selain itu, pada modal rincian utang supplier, tombol footer **"Lihat Riwayat Mutasi"** juga tersedia untuk navigasi langsung.

---

## 5. Dokumentasi Endpoint yang Dikonsumsi

`[Tags("Corporate / Finance Management / Subledger Movement")]`

Base URL: `/api/v1/corporate/finance-management`

| Method | Path | Deskripsi & Kegunaan | Hak Akses | Status Konsumsi |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/receivables/{id}/movements` | Membaca daftar mutasi subledger piutang secara berpaging (`pageNumber`, `pageSize`), tersaring jenis mutasi (`movementType`), rentang tanggal (`dateFrom`, `dateTo`), dan urutan (`sortDirection`). | `FinanceReceivable : Read` | Terkonsumsi aktif via Redux Thunk `fetchReceivableMovements` |
| `GET` | `/supplier-payables/{id}/movements` | Membaca daftar mutasi subledger utang supplier secara berpaging, tersaring jenis mutasi, rentang tanggal, dan arah pengurutan waktu. | `FinanceSupplierPayable : Read` | Terkonsumsi aktif via Redux Thunk `fetchSupplierPayableMovements` |
| `GET` | `/receivables/{id}` | Mengambil rincian ringkasan piutang induk (Nomor Faktur/Klaim, Nama Debitur/Penjamin, Saldo Awal, Sisa Saldo). | `FinanceReceivable : Read` | Terkonsumsi aktif pada hook `useReceivableMovements` |
| `GET` | `/supplier-payables/{id}` | Mengambil rincian ringkasan utang supplier induk (Nomor Tagihan/PO, Nama Rekanan/Supplier, Total Tagihan, Sisa Utang). | `FinanceSupplierPayable : Read` | Terkonsumsi aktif pada hook `useSupplierPayableMovements` |
| `GET` | `/proofs/{proofId}/download` | Mengunduh berkas fisik bukti transaksi (transfer/kuitansi/giro) dalam format biner aman (PDF/gambar). | Hak akses transaksi terkait | Terkonsumsi aktif via `handleDownloadProof` |

---

## 6. Bukti Verifikasi & Pengujian

### 6.1 Hasil Unit Test Otomatis
Unit test khusus utilitas mutasi subledger dibangun pada berkas `tests/unit/finance-movements-utils.test.mjs` dan dieksekusi menggunakan test runner resmi Node.js:

```text
TAP version 13
# Subtest: FE-FIN-025: formatSignedMoney returns positive amount with '+' sign
ok 1 - FE-FIN-025: formatSignedMoney returns positive amount with '+' sign
# Subtest: FE-FIN-025: formatSignedMoney returns negative amount with '-' sign
ok 2 - FE-FIN-025: formatSignedMoney returns negative amount with '-' sign
# Subtest: FE-FIN-025: formatSignedMoney returns Rp 0 without sign when zero
ok 3 - FE-FIN-025: formatSignedMoney returns Rp 0 without sign when zero
# Subtest: FE-FIN-025: getMovementEffect classifies positive and negative amounts correctly
ok 4 - FE-FIN-025: getMovementEffect classifies positive and negative amounts correctly
# Subtest: FE-FIN-025: payable utilities format signed money and effects identically
ok 5 - FE-FIN-025: payable utilities format signed money and effects identically
# Subtest: FE-FIN-025: movement types are mapped to appropriate Indonesian labels
ok 6 - FE-FIN-025: movement types are mapped to appropriate Indonesian labels
1..6
# tests 6
# suites 0
# pass 6
# fail 0
# cancelled 0
# skipped 0
# todo 0
```
**Hasil:** 6 dari 6 pengujian lolos (100% PASS).

### 6.2 Hasil Pemeriksaan Linter (`npm run lint:errors`)
Pemeriksaan kode statis ESLint dijalankan untuk memastikan tidak ada pelanggaran aturan sintaks atau impor:
```bash
npm run lint:errors
```
**Hasil:** Exit code 0 (0 errors).

### 6.3 Hasil Kompilasi & Build Standalone (`npm run build`)
Kompilasi Next.js dijalankan untuk memverifikasi validitas rute App Router, Suspense boundaries, dan dependensi Redux store:
```text
✓ Compiled successfully
✓ Generating static pages
✓ Collecting build traces
✓ Finalizing page optimization

Rute terverifikasi:
├ ƒ /finance/payable/[slug]/movements
├ ƒ /finance/receivable/[slug]/movements

> quilvian-app-system@0.1.0 postbuild
> node scripts/prepare-standalone.mjs

[prepare-standalone] Berhasil menyalin static assets.
[prepare-standalone] Berhasil menyalin public assets.
[prepare-standalone] Standalone runtime siap dijalankan.
```
**Hasil:** Exit code 0 (Build Standalone Sukses).

---

## 7. Analisis Risiko & Mitigasi

| Risiko | Tingkat | Dampak | Mitigasi yang Diterapkan |
| :--- | :---: | :--- | :--- |
| Kebocoran informasi sensitif pihak ketiga dari catatan internal | Tinggi | Pelanggaran kerahasiaan data asuransi/supplier | Sesuai `FIN-PERM-1.7` G.3, kolom `Notes` **ditiadakan** dari daftar kolom tabel publik. |
| Salah tafsir kenaikan vs penurunan saldo akibat tanda nominal | Sedang | Salah estimasi arus kas rumah sakit | Format `+` dan `-` ditegaskan dengan pewarnaan badge kontras (`INCREASE` = penambahan sisa, `DECREASE` = pengurangan sisa). |
| Pengguna panik mengira data hilang pada periode awal | Sedang | Komplain operasional tidak perlu | Teks keadaan kosong berbunyi eksplisit: `"buku mutasi baru berjalan sejak tanggal cutover"`. |
| Pengguna tidak dapat menemukan layar karena menu belum ada | Sedang | Layar tidak termanfaatkan | Disediakan tombol navigasi kontekstual pada Hero detail piutang, tabel daftar piutang, dan tabel daftar utang supplier. |
| Kegagalan unduh berkas bukti transaksi rusak/hilang | Rendah | Kendala lampiran audit | Tombol unduh diproteksi dengan penanganan try-catch dan notifikasi toast kegagalan yang ramah pengguna. |

---

## 8. Checklist Acceptance Criteria

- [x] **2 Permukaan Riwayat Mutasi:** Layar riwayat mutasi piutang (`/finance/receivable/[slug]/movements`) dan utang supplier (`/finance/payable/[slug]/movements`) dibangun lengkap dengan hook dan Redux slice.
- [x] **Nominal Bertanda Terbaca Jelas:** Format `+` dan `-` membedakan secara tegas transaksi yang menaikkan sisa tagihan/utang dan yang menurunkan sisa tagihan/utang.
- [x] **Keadaan Kosong Cutover:** Keadaan kosong berbunyi: `"buku mutasi baru berjalan sejak tanggal cutover"`, **bukan** `"tidak ada data"`.
- [x] **Kerahasiaan Catatan:** Kolom `Notes` **tidak** ditampilkan pada ringkasan tabel publik (`FIN-PERM-1.7` G.3).
- [x] **Informasi Rujukan & Bukti:** Menampilkan kolom metode pembayaran, rujukan transaksi, serta tombol unduh berkas bukti transaksi (`ProofId`).
- [x] **Dokumentasi Cara Mencapai Layar:** Laporan merinci cara mencapai layar via URL langsung dan tautan navigasi kontekstual selama menu tertahan `FIN-OQ-079`.
- [x] **Verifikasi Kualitas:** `npm run lint:errors` PASS (0 errors), unit test PASS (6/6 pass), `npm run build` PASS (exit code 0).
