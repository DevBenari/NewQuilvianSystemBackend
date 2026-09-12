# FE-BKC-031 — Lembar tagihan penjamin perusahaan pada Dokumen Kasir

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-031` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `1.1`) |
| Task type | Frontend, satu tab baru pada halaman Dokumen Kasir yang sudah ada + satu komponen lembar cetak baru |
| Task mode | `FRONTEND` (backend read-only — endpoint `BE-BKC-050` dibaca langsung dari source, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-050` ✅ (`GET /{id}/company-guarantor-invoice-document`, dikonfirmasi ada persis sesuai kontrak, termasuk gerbang `isPrintable`/`warnings` dibaca langsung dari `BillingCompanyGuarantorInvoiceDocumentService.GetDocumentAsync`) |
| Status task | **Source selesai.** Tab "Tagihan Penjamin Perusahaan" terpasang pada halaman Dokumen Kasir yang sudah ada. **Sesuai instruksi baku pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — hanya `node --check` pada berkas logika non-JSX (lihat § DoD). Belum di-commit |

## Ringkasan untuk pembaca umum

Halaman "Dokumen Kasir" (tempat kasir mencetak Kwitansi, Struk Pasien, dan Invoice Asuransi) kini
punya satu tab baru: **Tagihan Penjamin Perusahaan**. Finance dapat membuka tab ini untuk mencetak
lembar tagihan resmi yang ditujukan ke perusahaan tempat pasien bekerja — berisi identitas
perusahaan, identitas karyawan (nomor karyawan, paket manfaat, golongan), rincian biaya yang
ditanggung, dan keterangan bagaimana perusahaan itu mendapat penggantian biaya (menanggung sendiri
atau lewat asuransi mitra).

Pada kunjungan yang penjaminnya BUKAN perusahaan (tunai atau asuransi pribadi), tab ini tetap ada
tapi menampilkan keterangan kenapa lembar tidak dapat diterbitkan — bukan menyembunyikan tab atau
menampilkan galat, persis seperti tab Invoice Asuransi memperlakukan kunjungan yang bukan
berasuransi.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-082`–`084`; `MPY-DEC-006`, `MPY-DES-013`. `frontend-roadmap.md` § `FE-BKC-031`.
`03-frontend-architecture.md` § "Skema fitur — `FE-MPY-05`, `FE-MPY-06`, `FE-MPY-07`". `CAP-38`.

**Temuan yang wajib dilaporkan:**

1. **Tab SELALU ditampilkan, tidak disembunyikan untuk kunjungan non-perusahaan** — memilih opsi
   pertama dari dua opsi acceptance yang sah ("tab tidak tersedia **atau** menampilkan
   keterangan"). Alasan: `GET /{id}/company-guarantor-invoice-document` (dibaca langsung dari
   `BillingInvoicesController.cs`) **selalu** mengembalikan `200` dengan `isPrintable=false` dan
   `warnings` untuk kunjungan tunai/asuransi pribadi/tanpa data penjamin — bukan `404`/`422`.
   Tab Invoice Asuransi yang sudah ada (sibling langsung task ini) memakai pola yang SAMA PERSIS
   (selalu tampil, kontennya yang berubah) — mengikuti pola sibling yang sudah terbukti jalan,
   bukan menciptakan mekanisme "sembunyikan tab" baru yang tidak ada presedennya di halaman ini.
2. **Tiga perbedaan isi dari `InvoiceAsuransiDocument` diterapkan persis sesuai wireframe**: (a)
   "Informasi Perusahaan Penjamin" (nama, grup, no. kontrak, alamat) menggantikan "Informasi
   Penjamin" asuransi; (b) blok baru "Identitas Karyawan" (no. karyawan, nama, paket, golongan,
   kode paket) ditambahkan — tidak ada padanannya di dokumen asuransi; (c) satu baris "Rute
   penggantian biaya" ditambahkan (Menanggung sendiri / Melalui asuransi mitra, MPY-DES-014) —
   murni keterangan, tidak memindahkan piutang rumah sakit ke pihak lain.
3. **Persentase "% Coverage" pada tabel item memakai rumus turunan yang SAMA PERSIS dengan
   `InvoiceAsuransiDocument.getCoveragePercentText`** (disalin, bukan ditulis ulang secara
   independen) — `CoveredNetAmount / (NetAmount - TaxAmount)`. Ini murni derivasi tampilan dari
   angka yang SUDAH dihitung server (`CompanyGuarantorInvoiceItemResponse.CoveredNetAmount`),
   bukan kalkulasi bisnis baru di peramban.
4. **Nama berkas PDF memakai nomor invoice secara otomatis, tanpa kode baru** — `buildPdf`
   (`use-dokumen-kasir.js`, milik task lama) SUDAH membangun nama berkas dari
   `invoice.invoiceNumber` untuk ketiga dokumen lain, dan TIDAK PERNAH memakai nama pasien
   (komentar sumber: alasan privasi). Memanggilnya dengan prefiks baru
   (`"Invoice-Penjamin-Perusahaan"`) otomatis mewarisi jaminan itu — tidak ada logika penamaan
   baru yang perlu ditulis atau diverifikasi ulang.
5. **Ukuran kertas PDF A4** (bukan A5 seperti Kwitansi/Struk Pasien) — disamakan dengan Invoice
   Asuransi karena tabel item punya delapan kolom yang sama lebarnya (Item, Qty, Harga, PPN,
   Total, Ditanggung Penjamin, % Coverage, Porsi Pasien); A5 akan memotong kolom seperti yang
   sudah dicatat pada keputusan desain Invoice Asuransi.
6. **`"INVOICE_PENJAMIN"` ditambahkan ke `DOKUMEN_KASIR_REAL_TABS`** (satu sumber kebenaran tab
   nyata, dipakai baik label Nav maupun pengenalan `?tab=...`) — tanpa ini, tautan
   `?tab=INVOICE_PENJAMIN` akan jatuh ke tab default (bug yang sama seperti yang pernah dicatat
   `FE-BKC-018` sebelum sumber kebenaran ini dibuat).

## Base Component Decision Gate

`UI GATE: 0 elemen NEW pada base component, seluruh elemen REUSE — komponen lembar baru adalah pola dokumen presentasional yang sudah mapan (InvoiceAsuransiDocument), bukan komponen dasar baru`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Struktur lembar cetak (header, dua kolom identitas, tabel, tanda tangan) | `REUSE (pola)` | Disalin dari `invoice-asuransi-document.jsx` dengan tiga perbedaan isi terdokumentasi (§ Keputusan butir 2) |
| Tab navigasi | `REUSE` | `Nav`/`Nav.Item`/`Nav.Link` (react-bootstrap), pola identik tab Invoice Asuransi |
| Alert kosong/galat/peringatan | `REUSE` | `InformationAlert`, pola identik |
| Tombol cetak | `REUSE` | `BaseButton`, pola identik tombol Cetak Invoice Asuransi |
| Mekanisme PDF (html2pdf.js) | `REUSE` | `buildPdf` yang sudah ada di `use-dokumen-kasir.js`, dipanggil dengan prefiks dan paper size baru saja |

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}/company-guarantor-invoice-document` | Menyusun lembar tagihan penjamin perusahaan (identitas perusahaan, karyawan, rute penggantian biaya, rincian item, total) | `BillingInvoice : Read` (dipakai ulang, tidak ada hak akses baru) | `-` | `CompanyGuarantorInvoiceDocumentResponse` |

Tidak ada endpoint baru yang ditambahkan — `BE-BKC-050` sudah menyediakan endpoint ini secara
lengkap.

Kode status: `404` (invoice tidak ditemukan), `422` (kegagalan kalkulasi — bukan kasus "kunjungan
bukan perusahaan", yang itu tetap `200` dengan `isPrintable=false`).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `.../menu-pembayaran/company-guarantor-invoice-document.jsx` | **Baru.** Komponen lembar cetak presentasional, pola `InvoiceAsuransiDocument` + tiga perbedaan isi |
| `src/lib/state/slice/.../billing-invoice-slice.jsx` | **Diubah.** Tambah thunk `getCompanyGuarantorInvoiceDocument` + state/reducer/selector pasangannya (pola identik `getInsuranceInvoiceDocument`), reducer `clearCompanyGuarantorInvoiceDocument`. Export lama tidak diubah |
| `src/lib/hooks/.../billing-invoices/billing-invoice-constants.js` | **Diubah.** `"INVOICE_PENJAMIN"` ditambahkan ke `DOKUMEN_KASIR_REAL_TABS`. Export lama tidak diubah |
| `.../billing-invoices/use-dokumen-kasir.js` | **Diubah.** Tambah `companyGuarantorInvoicePrintRef`, `downloadCompanyGuarantorInvoice` (memanggil `buildPdf` yang sudah ada, prefiks `"Invoice-Penjamin-Perusahaan"`, kertas `"a4"`). Fungsi lama tidak diubah |
| `.../billing-invoices/use-dokumen-kasir-page.js` | **Diubah.** Tambah efek fetch (hanya jalan saat tab aktif, pola identik Invoice Asuransi), `companyGuarantorInvoiceDocumentProps`, diteruskan ke return hook. Logika lama tidak diubah |
| `.../menu-pembayaran/dokumen-kasir-view.jsx` | **Diubah.** Tambah `Nav.Link` tab baru, tombol cetak di `heroActions` (gated `isPrintable`, pola identik), blok konten (warnings/locked-snapshot/lembar-atau-ringkasan, pola identik blok Invoice Asuransi) |

Total: **1 berkas baru, 4 berkas diubah.**

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` / `test:unit` / `build` | **SKIPPED** — instruksi baku pengguna | Tidak dijalankan sesi ini |
| Sintaks berkas logika non-JSX valid | **LULUS** | `node --check` pada `use-dokumen-kasir.js`, `use-dokumen-kasir-page.js`, dan `billing-invoice-slice.jsx` (via salinan `.js` sementara) — ketiganya `exit 0` |
| Sintaks berkas JSX baru/diubah | **BELUM DIVERIFIKASI ALAT** | Diverifikasi manual lewat pembacaan ulang menyeluruh (`dokumen-kasir-view.jsx` dibaca penuh pasca-edit untuk memastikan tag seimbang dan tidak ada referensi yatim) |
| Lembar terbit hanya untuk kunjungan berpenjamin perusahaan | **LULUS (tinjauan kode)** | `IsPrintable` dihitung backend (`PayerKind == CompanyGuarantor && Payer != null && Items.Count > 0`) — frontend murni membaca nilai ini, tidak mengevaluasi ulang |
| Kunjungan tunai/asuransi pribadi menampilkan keterangan, bukan error | **LULUS (tinjauan kode)** | Backend mengembalikan `200` + `Warnings` untuk kasus ini (dikonfirmasi dari source service); frontend merender tiap `warnings[]` sebagai `InformationAlert` |
| Nama berkas memakai nomor tagihan | **LULUS (tinjauan kode)** | `downloadCompanyGuarantorInvoice` memanggil `buildPdf` yang sudah ada — fungsi itu membangun nama berkas dari `invoice.invoiceNumber`, tidak pernah nama pasien, tidak ada perubahan pada `buildPdf` itu sendiri |
| Verifikasi manual (klik-coba: buka tab pada kunjungan tunai/asuransi/penjamin perusahaan, cetak PDF, cek isi identitas karyawan dan rute penggantian biaya) | **NOT FEASIBLE (sesi ini)** | Tidak ada environment ter-autentikasi pada sesi ini |

**Task ini belum bisa ditandai selesai.** Source selesai dan ditinjau kode menyeluruh terhadap
kontrak backend nyata, tetapi lint/test/build serta klik-coba ter-autentikasi belum dijalankan.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — perlu environment ter-autentikasi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna)

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat `npm run lint`/`test:unit`/`build` maupun browser** pada
   sesi ini.
2. **PDF belum pernah benar-benar dirender lewat html2pdf.js** — `buildPdf` dipanggil dengan
   argumen baru (prefiks, `"a4"`) yang sudah didukung fungsi itu untuk Invoice Asuransi, tetapi
   kombinasi tepat untuk dokumen ini belum diuji nyata (perlu klik-coba).
3. **`companyGuarantorInvoiceDocumentProps.reimbursementRoute` bisa `null`** pada snapshot lama
   (invoice yang sudah difinalkan sebelum `MstCompanyGuarantorReimbursementRoute` diaktifkan,
   lihat `BE-BKC-052` — masih berisi data placeholder per laporan sebelumnya) — komponen sudah
   menjaga ini (`routeLabel` bernilai `null` bila `routeType` kosong, baris keterangan tidak
   dirender), tetapi belum diuji terhadap data placeholder `BE-BKC-052` yang sesungguhnya.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint:errors`/`test:unit`/`build`, lalu klik-coba manual: buka tab
   pada kunjungan tunai (harapkan keterangan "dibayar mandiri"), asuransi pribadi (harapkan
   keterangan "gunakan Invoice Asuransi"), dan penjamin perusahaan (harapkan lembar lengkap +
   tombol cetak), termasuk mengunduh PDF dan memeriksa nama berkasnya.
2. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-031`) dan
   `requirement-traceability.md`, tandai `✅`, tautkan laporan ini.
3. Lanjutkan `FE-BKC-032`/`033` (Master Data Rute Reimbursement dan Aturan Tanggungan Penjamin
   Perusahaan) — keduanya jalur tercepat menuju data penjamin yang benar-benar valid, menggantikan
   placeholder `BE-BKC-052` yang saat ini masih dipakai lembar ini untuk kelima perusahaan aktif.
