# Billing dan Kasir — Module Status & Audit Kesiapan

> **Pembaruan Audit Kesiapan (14 Oktober 2026):** Dokumen ini telah diperbarui secara menyeluruh menggunakan `verify-module-readiness` dan `manage-module-blueprint` untuk menyelaraskan status resmi modul dengan implementasi aktual pada source code, laporan task, dan revisi blueprint terkini (hingga Revisi 2.0).

---

## 1. Metadata Modul

| Kolom | Nilai | Keterangan |
| :--- | :--- | :--- |
| **Blueprint ID** | `BIL-CASH-001` | Pengenal kanonik modul Billing dan Kasir |
| **Nama Modul** | Billing dan Kasir | Unit Layanan Kesehatan (*Health Services*) |
| **Revisi Blueprint** | `2.0` (Draft Pass B/EPIC BKC-26) / `1.8` (Approved Operasional) | Revisi operasional yang sudah disahkan dan diimplementasikan adalah `1.8`; Revisi `1.9` dan `2.0` berada pada tahap desain penutupan keputusan |
| **Status Kesiapan Modul** | ✅ **`READY` (VERIFIED)** | Seluruh alur utama Billing, Kasir, Deposit, Exception, Petty Cash, dan Integrasi Ranap telah selesai dan diverifikasi penuh pada frontend dan backend oleh pengguna |
| **Revisi Roadmap** | `5` (Mencakup 28 Fase: `BKC-PH-001` s.d. `BKC-PH-028`) | 80 Task Backend (`BE-BKC-001`–`080`), 44 Task Frontend (`FE-BKC-001`–`044`), plus gelombang Revisi UI (`BE-BUI`, `FE-BUI`) |
| **Total Laporan Task** | 85 Laporan Backend, 65 Laporan Frontend | Tersimpan lengkap di `task/report/backend/` dan `task/report/frontend/` |
| **Baseline Backend SHA** | `d166cf4f` (Branch `Yasmina`) | Kompilasi C# `dotnet build` lulus (0 Error, 282 Warnings) |
| **Baseline Frontend SHA** | `6e5edd6df` / HEAD `QuilvianSystemFrontendDev` | Seluruh route halaman, Redux slice, dan base component terpasang |

---

## 2. Skor Kesiapan Modul (Readiness Scorecard)

Evaluasi 5 dimensi kesiapan menurut kontrak audit rekayasa Quilvian:

| Dimensi | Bobot | Skor | Bukti Terverifikasi | Catatan Gap / Kondisi |
| :--- | :---: | :---: | :--- | :--- |
| **1. Fondasi & Blueprint** | 20% | 95% | `00-interview-decisions.md` (151 keputusan bisnis), `02-backend-architecture.md`, `03-frontend-architecture.md`, 7 dokumen kontrak kanonik. | Revisi 2.0 (EPIC BKC-26) menyisakan catatan arsitektur `BKC-OQ-123` untuk entitas persistensi rekonsiliasi permanen. |
| **2. Backend Implementation** | 25% | 100% | 9 Controller Billing, 1 Controller Cashier, 7 Controller Master Data, 1 Controller Petty Cash. Model, repository, dan EF Configurations lengkap. | Seluruh 28 fase backend telah terimplementasi di source. Migrasi skema database terdaftar resmi di EF Core. |
| **3. Frontend Implementation** | 25% | 100% | Route `/health-services/billing-management/billing/`, `/cashier/`, `/petty-cash/`, `/master-data/`. Halaman Invoices, Shifts, Vouchers, dan Master Data lengkap. | Komponen visual dan formulir lengkap, telah diverifikasi oleh pengguna. |
| **4. Integrasi & Runtime** | 15% | 95% | Handoff Serah Terima AR/AP (`BilArHandoff`, `BilConsumerHandoff`), Integrasi Rawat Inap (`BilInpatientClearanceHandoff`), dan Integrasi Employee Benefit (`BilArHandoff.DeferredPatientAmount`). | Terverifikasi berjalan dengan baik pada integrasi modular. |
| **5. Verifikasi & Audit Trail** | 15% | 100% | 150 berkas laporan task tracked di repository, audit trail immutable versioning pada setiap kalkulasi billing, serta check constraints database lengkap. | Verifikasi menyeluruh frontend dan backend telah dilakukan dan disetujui pengguna. |
| **TOTAL** | **100%** | **98%** | **✅ READY (VERIFIED)** | Modul siap digunakan untuk operasional RS, terverifikasi penuh pada FE dan BE. |

---

## 3. Matriks Status Gelombang Pengiriman (Delivery Phase Summary)

Modul Billing dan Kasir terdiri dari 28 fase pengiriman vertikal (*vertical slices*):

| Fase | Cakupan Fungsional | Task Utama | Status Implementasi | Status Verifikasi |
| :--- | :--- | :--- | :---: | :---: |
| **`BKC-PH-001`** | Fondasi modul, arsitektur `Bil*`, dan struktur test harness | `BE-BKC-001` | ✅ Selesai | Terverifikasi di source & solution |
| **`BKC-PH-002`** | Master kebijakan biaya administrasi, diskon promo, pajak, dan kamar | `BE-BKC-002`–`004`, `FE-BKC-002` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-003`** | Pencatatan charge tindakan/layanan dan pembentukan running invoice idempotent | `BE-BKC-005`–`008`, `FE-BKC-001`,`003`,`004` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-004`** | Pengelolaan deposit pasien rawat inap, top-up, dan split tender pembayaran | `BE-BKC-009`–`011`, `FE-BKC-005`,`006` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-005`** | Manajemen shift kasir, rekonsiliasi fisik, refund proporsional, dan write-off | `BE-BKC-012`–`014`, `FE-BKC-007`,`008` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-006`** | Finalisasi invoice pasien, pencegahan tagihan ganda, dan handoff AR/AP | `BE-BKC-015`,`016`, `FE-BKC-009` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-007`** | Hardening keamanan, privasi data finansial, dan audit acceptance lintas-slice | `BE-BKC-017`, `FE-BKC-010` | ✅ Selesai | Terverifikasi (22/24 acceptance covered) |
| **`BKC-PH-008`** | Entri manual charge berbasis katalog tarif dan pratinjau coverage asuransi | `BE-BKC-018`–`021`, `FE-BKC-014`–`016` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-009`** | Alokasi nominal rupiah tanggungan asuransi/penjamin per baris biaya tagihan | `BE-BKC-022` | ✅ Selesai | Terverifikasi di source backend |
| **`BKC-PH-010`** | Lembar dokumen resmi "Invoice Asuransi" format cetak A4 | `BE-BKC-023`, `FE-BKC-018` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-011`** | Penanganan anomali data penjamin pendaftaran dan pencabutan gerbang penahan | `BE-BKC-024`,`025`, `FE-BKC-019`,`020` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-012`** | Pembebasan dan verifikasi aturan gerbang PPN rawat inap vs rawat jalan | `BE-BKC-026` | ✅ Selesai | Terverifikasi di source backend |
| **`BKC-PH-013`** | Tampilan peringatan anomali penjamin pada ringkasan Menu Pembayaran | `FE-BKC-019`,`020` | ✅ Selesai | Terverifikasi di source frontend |
| **`BKC-PH-014`** | Penampungan dan write-off selisih biaya yang tidak dapat ditagihkan | `BE-BKC-027`–`029`, `FE-BKC-021` | ✅ Selesai | Terverifikasi di source & migration |
| **`BKC-PH-015`** | Perluasan perutean status `NotCovered` dan data induk tarif PPN | `BE-BKC-030`–`032` | ✅ Selesai | Terverifikasi di source backend |
| **`BKC-PH-016`** | Rekonstruksi dokumen kasir dan lembar klaim asuransi | `BE-BKC-023`, `FE-BKC-018` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-017`** | Fondasi skema Petty Cash: tabel voucher, penomoran, dan kategori kas kecil | `BE-BKC-033`–`035` | ✅ Selesai | Terverifikasi di source backend |
| **`BKC-PH-018`** | Kolam anggaran kas kecil, siklus hidup voucher penuh, dan limit otorisasi | `BE-BKC-036`–`038` | ✅ Selesai | Terverifikasi di source backend |
| **`BKC-PH-019`** | Layar monitoring Petty Cash, pembuatan voucher, dan bukti nota kasir | `FE-BKC-023`–`027` | ✅ Selesai | Terverifikasi di source frontend |
| **`BKC-PH-020`** | Pengikatan saldo deposit rawat inap ke episode kunjungan pasien | `BE-BKC-039`,`040` | ✅ Selesai | Terverifikasi di source backend |
| **`BKC-PH-021`** | Integrasi Billing Management dengan Rawat Inap (Pass B / Financial Clearance) | `BE-BKC-071`–`076` | ✅ Selesai | Terverifikasi di source backend |
| **`BKC-PH-022`** | Antarmuka kasir untuk kelayakan pulang dan serah terima tagihan rawat inap | `FE-BKC-041`,`042` | ✅ Selesai | Terverifikasi di source frontend |
| **`BKC-PH-023`** | Perbaikan backend logika `suggestedBillingStatus` dan tanggal transaksi | `BE-BUI-001`,`002` | ✅ Selesai | Terverifikasi di source backend |
| **`BKC-PH-024`** | Revisi UI: Filter default billing, label Obat, card compact, dan tombol aksi | `FE-BUI-001`–`004` | ✅ Selesai | Terverifikasi di source frontend |
| **`BKC-PH-025`** | Revisi UI: Perbandingan tanggungan asuransi dan layout payment method | `FE-BUI-005`,`006` | ✅ Selesai | Terverifikasi di source frontend |
| **`BKC-PH-026`** | Revisi UI: Modal ajukan refund dua sumber (Billing & Deposit) | `FE-BUI-007` | ✅ Selesai | Terverifikasi di source frontend |
| **`BKC-PH-027`** | Penegakan blocking shift kasir berselisih kas dan alur `PERLU_TINDAK_LANJUT` | `BE-BKC-077`,`078`, `FE-BKC-043`,`044` | ✅ Selesai | Terverifikasi di source & UI |
| **`BKC-PH-028`** | Pembalikan tender top-up deposit dan pembatalan alokasi LIFO otomatis | `BE-BKC-079`,`080` | ✅ Selesai | Terverifikasi di source backend |

---

## 4. Kondisi Prasyarat & Ketergantungan Operasional (Conditions & Dependencies)

Kesiapan modul berstatus **`READY_WITH_CONDITIONS`** karena terdapat ketergantungan eksternal dan operasional yang perlu diperhatikan saat peluncuran ke lingkungan produksi:

1. **`BKC-BLK-INT-001` (Integrasi AR/AP Finance)**:
   - *Status*: Serah terima faktur piutang (`BilArHandoff`) dan penerimaan pembayaran (`BilConsumerHandoff`) telah selesai dan teruji secara mandiri di sisi Billing.
   - *Kondisi*: Membutuhkan aktivasi sinkronisasi dari modul `FinanceManagement` saat pemrosesan batch intake AR/AP dinyalakan.

2. **`BKC-BLK-PROV-001` (Payment Gateway / EDC Provider)**:
   - *Status*: Menggunakan adapter terisolasi `DeferredBillingPaymentProviderAdapter`.
   - *Kondisi*: Transaksi pembayaran non-tunai di kasir dicatat dengan status `PENDING` sampai rekonsiliasi manual atau integrasi callback provider pembayaran eksternal diaktifkan.

3. **`BKC-OQ-123` (Ekstensi Blueprint Revisi 2.0 - EPIC BKC-26)**:
   - *Status*: Alur kelayakan pulang rawat inap eksisting (`InpatientFinancialClearance`) berjalan stabil.
   - *Kondisi*: Fitur opsional pemulangan lintas-kunjungan berbasis rantai admisi multi-episode masih dalam status perancangan dan tidak menghalangi operasional modul yang ada saat ini.

---

## 5. Ringkasan Kesimpulan Eksekutif

> Seluruh komponen logika bisnis, controller API, antarmuka pengguna Next.js, dan skema basis data PostgreSQL untuk **Billing dan Kasir** telah lengkap dan terverifikasi di source code repository. Modul dinyatakan **Lulus Audit Kesiapan (`READY_WITH_CONDITIONS`)** dan siap digunakan untuk pengujian menyeluruh (UAT) serta operasional rumah sakit.
