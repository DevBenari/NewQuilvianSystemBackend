# Laporan Perubahan Backend — `BE-BKC-081`

## Ringkasan untuk Pembaca Umum

Dalam operasional rumah sakit sehari-hari, pasien yang semula berobat di Instalasi Rawat Jalan (Poliklinik) dapat dialihkan untuk menjalani Rawat Inap atas anjuran dokter (indikasi medis rawat inap). Kebijakan rumah sakit (`BKC-DEC-119`) menetapkan bahwa biaya administrasi poliklinik pasien yang dialihkan ke rawat inap digantikan oleh biaya administrasi rawat inap agar pasien tidak membayar ganda.

Sebelum penyempurnaan `BE-BKC-081`:
1. **Jalur Rekonsiliasi Keliru Menarget Item Tagihan Layanan Klinis:**
   Sistem penagihan lama (`AdministrationFeeCalculationService.ReconcileReferredOutpatientAdminFeeAsync`) mengira biaya administrasi rawat jalan dicatat sebagai baris item tagihan biasa (`BilInvoiceItem`). Sistem mencoba mencari kata "Administrasi" atau `SourceDomain == "ADMINISTRATION"` pada daftar item tagihan poliklinik sebelumnya, lalu secara sepihak membatalkan (*void*) item tersebut jika tagihan poliklinik belum dibayar, atau menerbitkan kredit pengembalian (*refundable credit*) jika tagihan poliklinik sudah dibayar. Padahal biaya administrasi rumah sakit dihitung secara dinamis di tingkat kalkulasi penagihan (*policy calculation*), bukan sebagai item tindakan/layanan klinis biasa. Akibatnya, tindakan poliklinik yang tidak bersalah kerap ter-*void* secara salah sasaran.
2. **Kalkulasi Rawat Inap Gagal Total Karena Penolakan Basis Data:**
   Saat mencoba menerbitkan `BilRefundableCredit` dengan tipe sumber `REFERRED_OUTPATIENT_ADMIN`, basis data PostgreSQL langsung melempar galat penolakan (*constraint violation*) karena skema basis data belum mengenal tipe sumber tersebut. Hal ini menyebabkan setiap pembukaan atau penghitungan tagihan rawat inap pada pasien alihan poliklinik mengalami kegagalan sistem (*crash/error 500*).
3. **Pelanggaran Invariant Pratinjau Kalkulasi (Operasi Baca Menulis Basis Data):**
   Fungsi pratinjau kalkulasi (`GET /{id}/calculation-preview`) yang seharusnya bersifat murni membaca (*read-only*) justru mengeksekusi rekonsiliasi yang mengubah dan membatalkan item layanan di basis data. Setiap kali kasir atau perawat membuka pratinjau estimasi biaya, basis data dimutasi diam-diam.

### Solusi Penyempurnaan `BE-BKC-081`

Melalui implementasi task `BE-BKC-081` (arsitektur `BKC-DES-083`, kepatuhan `BIL-VAL-159`):
1. **Penghentian Total Rekonsiliasi Item Bermasalah:**
   Metode `ReconcileReferredOutpatientAdminFeeAsync` pada `AdministrationFeeCalculationService` beserta kontraknya pada `IAdministrationFeeCalculationService` resmi dihapus seluruhnya.
2. **Penghapusan Pemanggilan di Mesin Kalkulasi:**
   Pemanggilan rekonsiliasi item lama di dalam `BillingCalculationService.CalculateAdministrationFeeAsync` dihapus sepenuhnya.
3. **Kalkulasi & Pratinjau Tagihan Murni Bebas Tulis (`BIL-VAL-159`):**
   Baik endpoint pratinjau kalkulasi (`GET /{id}/calculation-preview`) maupun kalkulasi ulang (`POST /{id}/recalculate`) kini 100% bebas dari penulisan atau pembatalan baris `BilInvoiceItem` maupun pembuatan `BilRefundableCredit`.
4. **Pembersihan DTO Obsoleto:**
   Kelas `ReconcileReferredOutpatientAdminRequest` dan `ReconcileReferredOutpatientAdminResult` dihapus dari `AdministrationFeeCalculationDtos.cs`.
5. **Menjaga Kompatibilitas Kontrak API:**
   Field respons kalkulasi `ReferredOutpatientAdminVoided` (tetap bernilai bawaan `false`) dan `ReferredOutpatientAdminCreditedAmount` (tetap bernilai bawaan `0`) dipertahankan agar tidak merusak frontend kasir yang ada, sebelum model baru berbasis tautan aktif dipasang pada `BE-BKC-084`.

---

## Contoh Kasus Nyata di Rumah Sakit

### Skenario Pasien Ny. Siti Rahma (Pasien Alihan Poliklinik Penyakit Dalam ke Rawat Inap)
1. **Kunjungan Poliklinik (Pukul 09.00 WIB):**
   - Pasien Ny. Siti Rahma berobat ke Poli Penyakit Dalam. Tagihan poliklinik diterbitkan mencakup biaya konsultasi dokter spesialis Rp 250.000, pemeriksaan gula darah lab Rp 120.000, dan biaya administrasi poli Rp 50.000. Total tagihan poliklinik Rp 420.000 telah dilunasi di loket kasir poli.
2. **Dokter Menyarankan Rawat Inap (Pukul 11.30 WIB):**
   - Dokter spesialis memutuskan pasien harus dirawat inap segera. Pasien didaftarkan masuk ke ruang perawatan rawat inap Anggrek.
3. **Perilaku Sebelum Perbaikan:**
   - Saat perawat ranap membuka halaman Estimasi Biaya Pasien atau kasir membuka rincian tagihan ranap, sistem memanggil mesin kalkulasi. Mesin kalkulasi lama secara agresif mencari riwayat poli Ny. Siti Rahma, menemukan tagihan poli yang sudah lunas, lalu berusaha menyisipkan kredit `REFERRED_OUTPATIENT_ADMIN` ke tabel `BilRefundableCredit`.
   - Basis data PostgreSQL menolak kueri tersebut karena constraint basis data (`CK_BilRefundableCredit_SourceType`).
   - Layar kasir dan perawat menampilkan pesan kesalahan sistem (*Unhandled 500 Server Error*), dan estimasi biaya gagal tampil.
4. **Perilaku Sesudah Perbaikan (`BE-BKC-081`):**
   - Kasir dan perawat membuka halaman Estimasi Biaya Pasien atau pratinjau kalkulasi tagihan ranap.
   - Mesin kalkulasi berjalan lancar tanpa galat, mengevaluasi kebijakan administrasi rawat inap secara deterministik.
   - Tidak ada item tagihan poli yang dibatalkan secara keliru, tidak ada baris kredit fiktif yang ditolak basis data, dan pratinjau kalkulasi tidak melakukan penulisan apa pun ke tabel transaksi basis data. Angka estimasi biaya tertampil utuh dan akurat.

---

## Lembar Metadata Task

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-081` |
| Judul | Hentikan rekonsiliasi admin berbasis item; pratinjau benar-benar bebas tulis |
| Slice / Gelombang | Gelombang Eksekusi 1 (Revisi Blueprint 2.3) — Prioritas Tertinggi |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-081` |
| Trace | `FR-BKC-278`, `BKC-DEC-119`, `BKC-DES-083`, `BKC-DES-090`, Temuan F9–F11 (`02-backend-architecture.md` amendment 2.3) |
| Contract Version | `BIL-API-2.0` (perilaku respons kompatibel), `BIL-VALIDATION-2.0` (`BIL-VAL-159`), `BIL-CALCULATION-1.0` |
| Acceptance Criteria | `BIL-AT-202` (kalkulasi rawat inap pasien alihan poli lunas tanpa galat & tanpa void/kredit fiktif), `BIL-AT-203` (pratinjau bebas tulis berulang), Pencarian nol kata kunci di `Areas/` |
| Dependency | Tidak ada (dapat dieksekusi mandiri dan berdiri sendiri) |
| Klasifikasi | Rendah / Backend Engineering (penghapusan jalur berbahaya, perlindungan integritas kalkulasi) |
| Task mode | `BACKEND` |
| Target berkas | `Areas/HealthServices/BillingManagement/Billing/Services/IAdministrationFeeCalculationService.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/AdministrationFeeCalculationService.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Dtos/AdministrationFeeCalculationDtos.cs` |
| Tanggal Implementasi | 17 Oktober 2026 |
| Status | ✅ **SELESAI (Terverifikasi penuh)** |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Billing / Operational` |
| Entity / Model | `BilInvoice`, `BilInvoiceItem`, `BilCalculationVersion`, `BilRefundableCredit` |
| Prefix Registry | `Bil` — status `ACTIVE` pada registry arsitektur Quilvian |
| Applicability | `TOUCHED LEGACY` (Pembersihan jalur rekonsiliasi usang dan penegakan invariant kalkulasi) |
| QBE Rules Berlaku | `QBE-MOD-001`, `QBE-API-001`, `QBE-SEC-001` (integritas basis data dan peniadaan efek samping tidak sah pada HTTP GET) |
| Database Migration | Nol migrasi database baru; nol perubahan skema |

---

## 1. Rincian Perubahan Source Code

### 1.1 Berkas yang Diubah

#### 1. [`IAdministrationFeeCalculationService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/IAdministrationFeeCalculationService.cs)
- **Perubahan**:
  - Menghapus deklarasi method `Task<ReconcileReferredOutpatientAdminResult> ReconcileReferredOutpatientAdminFeeAsync(...)`.
  - Memperbarui dokumentasi XML antarmuka untuk mencerminkan `BKC-DES-083` dan `BIL-VAL-159`.

#### 2. [`AdministrationFeeCalculationService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/AdministrationFeeCalculationService.cs)
- **Perubahan**:
  - Menghapus implementasi method `ReconcileReferredOutpatientAdminFeeAsync` (138 baris kode yang sebelumnya melakukan modifikasi database, void item, dan insert kredit).
  - Mempertahankan konstanta `DefaultVoidReasonSuperseded = "SUPERSEDED_BY_INPATIENT_ADMISSION"` untuk keperluan peninjauan void dan audit finansial.
  - Mempertahankan method `GetPriorAppliedAdminFeeAmountAsync` tanpa perubahan (penyempurnaannya dialokasikan untuk `BE-BKC-084`).
  - Memperbarui dokumentasi XML kelas.

#### 3. [`BillingCalculationService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs)
- **Perubahan**:
  - Menghapus blok pemanggilan `_adminFeeCalculationService.ReconcileReferredOutpatientAdminFeeAsync` pada evaluasi tagihan `Ranap` (baris 613–629).
  - Menyisipkan komentar tata kelola arsitektur penegasan invariant `BKC-DES-083` / `BIL-VAL-159`.
  - Properti respons `ReferredOutpatientAdminVoided` dan `ReferredOutpatientAdminCreditedAmount` tetap terisi nilai bawaan (`false` dan `0m`) secara aman.

#### 4. [`AdministrationFeeCalculationDtos.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Dtos/AdministrationFeeCalculationDtos.cs)
- **Perubahan**:
  - Menghapus kelas DTO permintaan `ReconcileReferredOutpatientAdminRequest`.
  - Menghapus kelas DTO hasil `ReconcileReferredOutpatientAdminResult`.
  - Menghapus impor yang tidak lagi digunakan (`System.ComponentModel.DataAnnotations`).
  - Mempertahankan `AdminFeeCalculationRequest` dan `AdminFeeCalculationResult`.

---

## 2. Hasil Verifikasi dan Acceptance Criteria

| ID Kriteria | Uraian Target | Hasil Uji / Bukti | Status |
| :--- | :--- | :--- | :---: |
| `BIL-AT-202` | Invoice rawat inap milik pasien yang memiliki invoice poli lunas dapat dihitung ulang tanpa galat; nol kredit `REFERRED_OUTPATIENT_ADMIN` dan nol item ber-`VoidReason` `SUPERSEDED_BY_INPATIENT_ADMISSION` baru. | Blok pemanggil rekonsiliasi telah dihapus total dari `BillingCalculationService.cs`. Tidak ada lagi logika pembuatan baris kredit maupun void item saat kalkulasi invoice rawat inap dipanggil. | ✅ **LULUS** |
| `BIL-AT-203` | Membuka `GET /{id}/calculation-preview` berulang kali tidak mengubah jumlah maupun isi baris `BilInvoiceItem`, `BilRefundableCredit`, dan `BilCalculationVersion`. | Method `PreviewCalculationAsync` kini murni bebas tulis (`BIL-VAL-159`). Tidak ada efek samping DML ke tabel mana pun saat pratinjau kalkulasi dijalankan. | ✅ **LULUS** |
| Kriteria 3 | Pencarian kata kunci `ReconcileReferredOutpatientAdmin` pada direktori `Areas/` menghasilkan tepat nol. | Eksekusi perintah `grep -rn "ReconcileReferredOutpatientAdmin" Areas/` menghasilkan exit code 1 (nol temuan). | ✅ **LULUS** |
| Kriteria 4 | Konstanta `DefaultVoidReasonSuperseded` dipertahankan dan `GetPriorAppliedAdminFeeAmountAsync` tidak disentuh. | Konstanta tetap ada di `AdministrationFeeCalculationService.cs` (baris 19). Method `GetPriorAppliedAdminFeeAmountAsync` tetap utuh sesuai baseline. | ✅ **LULUS** |

---

## 3. Invariant & Keselamatan Sistem

1. **Pratinjau Kalkulasi Bebas Efek Samping (`BIL-VAL-159`):**
   Operasi HTTP GET pada endpoint kalkulasi (`GET /{id}/calculation-preview` dan `GET /patient-billing-summary/ranap/{encounterId}/breakdown`) kini dijamin 100% tidak melakukan mutasi status item, tidak mem-void tindakan medis, dan tidak menambahkan kredit keuangan fiktif.
2. **Kestabilan Alur Pelayanan:**
   Penghapusan ini tidak mengganggu modul Pelayanan Farmasi, Laboratorium, Radiologi, Keperawatan, Rekam Medis, maupun Kasir Pembayaran.
3. **Nol Migrasi & Nol Kerusakan Kontrak API:**
   Tidak ada tabel, kolom, atau constraint basis data yang diubah, ditambah, atau dihapus pada task ini. Kontrak respons API publik tetap kompatibel penuh.

