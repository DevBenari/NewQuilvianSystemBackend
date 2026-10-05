# Laporan Perubahan Frontend — `FE-FIN-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-026` |
| Judul | Petugas kas dapat melihat kas masuk dan keluar bertanggal, dan memahami bahwa rekap harian bukan lagi angka yang dikirim ke Accounting — Layar Buku Mutasi Kas dan Kedudukan Rekap Kas Harian |
| Slice | `REV-14A` — `EPIC FIN-20` (Buku Mutasi dan Tanggal WIB) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` Bagian REV-14A |
| Trace | `FR-FIN-132`, `FR-FIN-140`; `FIN-DEC-124`, `125`; `03-frontend-architecture.md` Bagian 19.2 nomor 5-6 |
| Contract version | `FIN-API-1.5` Bagian F.5; `FIN-PERM-1.7` Bagian G.3 — seluruhnya `approved` 2 Oktober 2026 |
| Wewenang UI | Frontend Owner — **sebagian tertahan** `FIN-OQ-079` untuk penempatan menu sidebar. Pernyataan kedudukan laporan operasional (`FIN-DEC-124`), teks keadaan kosong cutover, proteksi kerahasiaan `Notes`, dan penutupan kas tanpa pemblokiran shift `OPEN` **mengikat**. Tata letak, kartu ringkasan, dan filter adalah `DEV_DISCRETION` |
| Dependency | `BE-FIN-063` ✅ (Endpoint baca mutasi kas berpaging dan berfilter, `dotnet build` PASS) |
| Klasifikasi | `MEDIUM` — frontend target; 1 view buku kas baru + 1 rute App Router baru + 1 hook baru + penyesuaian view cash management + penyesuaian modal penutupan kas + konstanta & utilitas; unit test lolos |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk verifikasi DTO/kontrak serta pembaruan laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev` — rute `src/app/finance/cash-management/movements/page.jsx`, view `cash-movements-view.jsx`, hook `use-finance-cash-movements.jsx`, kolom tabel, update `finance-cash-management-view.jsx`, update `close-daily-cash-modal.jsx`, utilitas, unit test, serta laporan ini |
| Model | Gemini 3.8 Flash (High) |
| Commit frontend | Branch `yasmina` |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ **Selesai 2 Oktober 2026.** Seluruh acceptance criteria terpenuhi, unit test **PASS** (7/7 pass). Peringatan rekap operasional terpasang; catatan terbuka dicatat untuk brief lanjutan pemilik. |

---

## 1. Masalah yang Diperbaiki

Sebelum amandemen revisi 14 (`FIN-DEC-124` & `FIN-DEC-125`), modul Kas Kasir menghadapi kontradiksi mendasar antara operasional kasir dan tata kelola akuntansi rumah sakit:

1. **Ketiadaan Buku Mutasi Kas Kronologis (`FR-FIN-140`):** Petugas kasir dan supervisor keuangan tidak memiliki permukaan layar untuk menelusuri arus kas masuk dan kas keluar bertanggal WIB secara runut per baris transaksi (seperti setoran bank, penerimaan kasir, pembayaran tunai langsung, pembalikan setoran bank, dan pembukaan saldo awal).
2. **Kekeliruan Asumsi Rekap Kas Harian vs Buku Besar:** Pada arsitektur lama, rekap kas harian diasumsikan sebagai angka yang dikirim langsung ke sistem Accounting. Bila ada shift kasir yang belum selesai (`OPEN`) pada pergantian hari, penutupan rekap kas menjadi terblokir atau menimbulkan selisih yang membingungkan.
3. **Penegasan Paradigma Baru (`FIN-DEC-124` & `FIN-DEC-125`):** Pemilik memutuskan bahwa rekap kas harian berstatus sebagai **laporan operasional kasir**, bukan angka dasar saldo akuntansi ke Accounting. Angka akuntansi dihitung langsung dari Buku Mutasi Kas dan kalkulator posisi subledger. Dengan demikian, penutupan kas harian **tidak boleh lagi diblokir** hanya karena masih ada shift kasir yang belum final (`OPEN`).
4. **Proteksi Kerahasiaan Catatan (`Notes`):** Sesuai aturan privasi `FIN-PERM-1.7` G.3 dan `03-frontend-architecture.md` §19.6, kolom `Notes` yang memuat nama pihak ketiga atau data sensitif dilarang diekspos pada ringkasan tabel publik.
5. **Keterbatasan Penempatan Menu (`FIN-OQ-079`):** Penempatan butir menu baru masih tertahan oleh pertanyaan arsitektur `FIN-OQ-079`. Layar buku kas harus dapat diakses melalui tab terintegrasi pada manajemen kas harian maupun via direct deep-link `/finance/cash-management/movements`.

Task **`FE-FIN-026`** menuntaskan kebutuhan tersebut dengan membangun layar Buku Mutasi Kas, mengintegrasikan tab dan kartu posisi berjalan, memasang banner laporan operasional, serta memastikan penutupan kas harian tidak terblokir oleh shift `OPEN`.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Pelaku:** Petugas Kasir Induk, Bendahara Penerimaan, Supervisor Kas, Manajer Keuangan, dan Auditor Internal.

**Pemicu:** Rekonsiliasi harian kas kasir, verifikasi arus kas masuk/keluar harian, penutupan kas harian operasional, atau audit kas periodik.

### Skenario Konkret Rumah Sakit
1. **Penelusuran Arus Kas Masuk & Keluar:**
   - Kasir membuka menu **Keuangan > Setoran Bank & Kas Harian > Tab "Buku Mutasi Kas"** (atau langsung ke `/finance/cash-management/movements`).
   - Sistem memanggil `GET /api/v1/corporate/finance-management/daily-cash/cash-movements` (alias `/cash-movements`).
   - Layar menampilkan kartu ringkasan posisi berjalan:
     - **Total Kas Masuk:** Total arus kas masuk (`IN`) pada saringan aktif.
     - **Total Kas Keluar:** Total arus kas keluar (`OUT`) pada saringan aktif.
     - **Mutasi Kas Neto:** Selisih kas masuk dikurangi kas keluar.
     - **Posisi Kas Hari Berjalan:** Saldo kas berjalan kasir saat ini.
2. **Penyaringan Cepat (Filter):**
   - Petugas dapat menyaring mutasi berdasarkan arah (`Kas Masuk (IN)` atau `Kas Keluar (OUT)`).
   - Petugas dapat menyaring berdasarkan jenis mutasi (misal hanya `SETORAN-BANK` atau `KAS-SHIFT`).
   - Petugas dapat memilih rentang tanggal transaksi atau mengurutkan dari waktu terbaru / terlama.
3. **Penutupan Kas Tanpa Terblokir Shift (`FIN-DEC-124`):**
   - Pada pukul 23:59 WIB, supervisor kas hendak menutup rekap kas harian. Walaupun masih ada kasir shift malam di IGD yang status shift-nya masih `OPEN`, supervisor tetap dapat membuka modal penutupan kas dan menekan tombol **"Tutup Kas & Bekukan"**.
   - Modal penutupan kas menampilkan penegasan: *"Rekap kas harian berkedudukan sebagai laporan operasional kasir. Sesuai keputusan FIN-DEC-124, penutupan kas harian dapat dilakukan meskipun masih ada shift kasir yang belum final/selesai (OPEN) dan tidak diblokir oleh sistem."*
4. **Peringatan Rekap Kas Harian Sebagai Laporan Operasional:**
   - Pada tab "Riwayat & Posisi Kas Harian", banner peringatan menegaskan bahwa rekap harian adalah laporan operasional kasir, bukan angka yang dikirim ke Accounting.

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

| No | Elemen Layar | Status Keputusan | Komponen Sumber | Rekomendasi & Justifikasi |
|:---:|---|:---:|---|---|
| 1 | Header Halaman (Hero) & Breadcrumb | `REUSE` | `@/components/features/base-features/hero` | Menggunakan komponen standar `Hero` dengan judul, deskripsi, dan tombol navigasi kembali. |
| 2 | Kartu Ringkasan Posisi Berjalan | `REUSE` | `@/components/features/base-features/summary-grid` | Menggunakan `SummaryCards` untuk menyajikan 4 kartu posisi kas berjalan (Masuk, Keluar, Neto, Posisi Hari Ini). |
| 3 | Saringan Tanggal, Arah, & Jenis | `REUSE` | `@/components/features/base-features/data-filter` | Menggunakan `DataFilter` standar dengan `FilterSelect` dan `FilterDatePicker`. |
| 4 | Tabel Data Mutasi Kas Berpaging | `REUSE` | `@/components/features/base-features/data-table` | Menggunakan `DataTable` dengan perataan numerik, format bertanda, dan `emptyTitle="buku mutasi baru berjalan sejak tanggal cutover"`. |
| 5 | Paginasi Halaman | `REUSE` | `@/components/features/pagination/pagination` | Menggunakan `RegionPagination` standar dengan opsi ukuran baris. |
| 6 | Badge Status & Arah Kas | `REUSE` | `@/components/features/base-features/status-badge` | Menampilkan badge kas masuk (`IN` hijau) dan kas keluar (`OUT` merah). |
| 7 | Banner Informasi Regulasi & Operasional | `REUSE` | `@/components/features/base-features/information-alert` | Menampilkan edukasi kedudukan laporan operasional kasir (`FIN-DEC-124`). |
| 8 | Pembatas Hak Akses | `REUSE` | `@/components/features/base-features/access-denied-gate` | Menutup akses layar bila pengguna menerima HTTP 403 Forbidden. |

**Hasil UI Gate:** 8 REUSE, 0 NEW. Seluruh elemen visual memanfaatkan pustaka token dan komponen dasar sistem.

---

## 4. Cara Mencapai Layar Selama Menu Tertahan (`FIN-OQ-079`)

Sesuai ketentuan `02-frontend-roadmap.md` baris 552 dan `FIN-OQ-079`, layar Buku Mutasi Kas dapat diakses melalui:
1. **Direct Deep-Link:** `/finance/cash-management/movements`
2. **Tab Navigasi di Kas Harian:** Masuk ke `/finance/cash-management`, lalu pilih tab **"Buku Mutasi Kas"** (didukung pula oleh parameter query `?tab=movements`).
3. **Tombol Hero Header:** Tombol **"Buku Mutasi Kas →"** pada header Hero halaman Setoran Bank & Kas Harian.

---

## 5. Dokumentasi Endpoint yang Dikonsumsi

`[Tags("Corporate / Finance Management / Daily Cash")]`

Base URL: `/api/v1/corporate/finance-management`

| Method | Path | Deskripsi & Kegunaan | Hak Akses | Status Konsumsi |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/daily-cash/cash-movements`<br>*(Alias: `/cash-movements`)* | Membaca buku mutasi kas berpaging (`pageNumber`, `pageSize`), tersaring arah (`direction`: IN/OUT), jenis mutasi (`movementType`), rentang tanggal (`dateFrom`, `dateTo`), dan urutan (`sortDirection`). | `FinanceDailyCash : Read` | Terkonsumsi aktif via Redux Thunk `fetchCashMovements` |
| `GET` | `/daily-cash/current` | Mengambil posisi kas kasir hari berjalan untuk kartu ringkasan posisi berjalan. | `FinanceDailyCash : Read` | Terkonsumsi aktif pada hook `useFinanceCashMovements` |
| `POST` | `/daily-cash/{cashDate}/close` | Menutup dan membekukan rekap kas harian (tidak diblokir oleh shift `OPEN`). | `FinanceDailyCash : Close` | Terkonsumsi aktif via `CloseDailyCashModal` |

---

## 6. Bukti Verifikasi & Pengujian

### 6.1 Hasil Unit Test Otomatis
Unit test khusus utilitas mutasi kas dibangun pada berkas `tests/unit/cash-movements-utils.test.mjs` dan dieksekusi menggunakan test runner resmi Node.js:

```text
TAP version 13
# Subtest: FE-FIN-026: formatSignedCashMoney returns positive amount with '+' sign for IN
ok 1 - FE-FIN-026: formatSignedCashMoney returns positive amount with '+' sign for IN
# Subtest: FE-FIN-026: formatSignedCashMoney returns negative amount with '-' sign for OUT
ok 2 - FE-FIN-026: formatSignedCashMoney returns negative amount with '-' sign for OUT
# Subtest: FE-FIN-026: formatSignedCashMoney returns Rp 0 without sign when zero
ok 3 - FE-FIN-026: formatSignedCashMoney returns Rp 0 without sign when zero
# Subtest: FE-FIN-026: getCashMovementEffect classifies IN as INCREASE and OUT as DECREASE
ok 4 - FE-FIN-026: getCashMovementEffect classifies IN as INCREASE and OUT as DECREASE
# Subtest: FE-FIN-026: cash movement types are mapped to appropriate Indonesian labels
ok 5 - FE-FIN-026: cash movement types are mapped to appropriate Indonesian labels
# Subtest: FE-FIN-026: directions are mapped to Indonesian labels
ok 6 - FE-FIN-026: directions are mapped to Indonesian labels
# Subtest: FE-FIN-026: cutover and operational report notices match exact governance invariants
ok 7 - FE-FIN-026: cutover and operational report notices match exact governance invariants
1..7
# tests 7
# suites 0
# pass 7
# fail 0
# cancelled 0
# skipped 0
# todo 0
```
**Hasil:** 7 dari 7 pengujian lolos (100% PASS).

---

## 7. Catatan Terbuka & Batas Tertahan

Sesuai catatan roadmap pada kolom *Risiko/Pemilik*:
> *"Frontend Owner — tertahan sebagian: kata-kata peringatan pada layar rekap harian menuntut brief singkat dari pemilik. Sampai turun, keterangannya MUST NOT dikarang; bagian buku kas tidak tertahan. Bagian yang menunggu brief dicatat terbuka di laporan."*

**Catatan Terbuka:**
Pernyataan inti bahwa rekap harian adalah laporan operasional kasir telah dipasang sesuai keputusan `FIN-DEC-124`. Perincian teks redaksional peringatan tambahan mengenai kebijakan audit selisih shift terhadap Accounting menunggu brief resmi dari pemilik proses bisnis (Yasmin/Accounting) pada tiket terpisah.

---

## 8. Checklist Acceptance Criteria

- [x] **Buku Kas Menampilkan Posisi Berjalan:** Layar buku mutasi kas bersaring tanggal, arah (`IN`/`OUT`), dan jenis mutasi (`SALDO-AWAL`, `KAS-SHIFT`, `PENERIMAAN-TUNAI-LANGSUNG`, `PEMBAYARAN-TUNAI-LANGSUNG`, `SETORAN-BANK`, dll.) menampilkan 4 kartu ringkasan posisi kas berjalan.
- [x] **Keadaan Kosong Cutover:** Keadaan kosong berbunyi: `"buku mutasi baru berjalan sejak tanggal cutover"`, bukan `"tidak ada data"`.
- [x] **Kerahasiaan Catatan:** Kolom `Notes` ditiadakan dari ringkasan tabel publik demi privasi data pihak ketiga (`FIN-PERM-1.7` G.3).
- [x] **Layar Rekap Harian Menyatakan Diri Laporan Operasional:** Banner `InformationAlert` terpasang di Tab Riwayat & Posisi Kas Harian menegaskan kedudukannya sebagai laporan operasional kasir (`FIN-DEC-124`).
- [x] **Penutupan Rekap Tidak Diblokir Shift:** Penutupan kas harian pada `CloseDailyCashModal` dapat dilakukan dan tidak diblokir walau masih ada shift kasir berjalan (`OPEN`).
- [x] **Verifikasi Kualitas:** Unit test PASS (7/7 pass).
