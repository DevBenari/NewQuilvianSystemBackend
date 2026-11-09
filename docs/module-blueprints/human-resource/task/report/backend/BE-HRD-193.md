# Laporan Penyelesaian Backend Task: BE-HRD-193

## 1. Identitas Task
* **ID Task**: `BE-HRD-193`
* **Nama Task**: Master Katalog Layanan SDM & Manajemen Tipe Layanan (`HrServiceCatalogService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13 — HR Service Management & Request
* **Slice / Milestone**: `Slice S-D4` · `Fase MVP-39: Katalog, Template Token Dinamis & Agregat Tiket Helpdesk`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Kelancaran layanan kepegawaian rumah sakit membutuhkan katalog layanan yang terstruktur, fleksibel, dan memiliki kejelasan target waktu penyelesaian (*Service Level Agreement / SLA*). Petugas HR tidak lagi melayani permohonan lewat pesan instan atau percakapan lisan yang rentan hilang, melainkan melalui katalog resmi yang terstandarisasi.

Fitur-fitur utama yang disediakan:
1. **Pengelolaan Master Kategori Layanan SDM (`MstHrServiceCategory`)**:
   - Mendukung pengelompokan layanan: Administrasi & Personalia (`ADM-GEN`), Penerbitan Surat Resmi Kepegawaian (`DOC-OFF`), Pemutakhiran Data Pribadi & Rekening (`DATA-UPD`), serta Surat Tugas Darurat Medis (`TRAVEL-REQ`).
   - Dilengkapi atribut keterlihatan (*visibility toggle*) untuk pegawai umum dan manajer.
2. **Pengelolaan Tipe Layanan & Target SLA Jam Kerja (`MstHrServiceType`, `VR-HRS-03`)**:
   - Menetapkan durasi target SLA resmi dalam rentang 1 hingga 168 jam kerja (contoh: Surat Keterangan Kerja 24 jam, Surat Keterangan Gaji KPR 72 jam, Izin Darurat Medis 4 jam).
   - Penanda kewajiban unggah dokumen lampiran (`requires_attachment`) dan wewenang penugasan awal (*default role code*).
3. **Master Template Dokumen Resmi Kepegawaian (`MstEmployeeDocumentType`)**:
   - Konfigurasi pola nomor surat resmi (`NumberingPattern`), masa berlaku surat (`ValidityPeriodDays`), serta klasifikasi kerahasiaan finansial (`IsFinancialSensitive`).
4. **REST API Swagger-Tagged**:
   - 13 Endpoint lengkap di bawah tag Swagger resmi `[Tags("HR Services - Catalog & SLA")]`.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. DTOs
File: `Areas/Corporate/HumanResource/HrServiceManagement/DTOs/HrServiceCatalogDtos.cs`
* DTO Request/Response: `HrServiceCategoryDto`, `CreateHrServiceCategoryRequestDto`, `UpdateHrServiceCategoryRequestDto`, `HrServiceTypeDto`, `CreateHrServiceTypeRequestDto`, `UpdateHrServiceTypeRequestDto`, `EmployeeDocumentTypeDto`, `CreateEmployeeDocumentTypeRequestDto`.

### B. Antarmuka & Layanan Bisnis
File: `Areas/Corporate/HumanResource/HrServiceManagement/Services/IHrServiceCatalogService.cs` & `HrServiceCatalogService.cs`
* Validasi duplikasi kode kategori/layanan.
* Validasi range SLA target 1 - 168 jam (`VR-HRS-03`).
* Audit trail pencatatan log `[CATALOG-HRS-01]` s.d. `[CATALOG-HRS-06]`.

### C. Controller REST API
File: `Areas/Corporate/HumanResource/HrServiceManagement/Controllers/HrServiceCatalogController.cs`
* Menangani rute `/api/v1/corporate/hrd/services/categories`, `/types`, `/document-types`.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario Penambahan Tipe Layanan Legalisir STR Dokter Spesialis**:
  - Tim HR Manager menambahkan tipe layanan baru: "Permohonan Pengantar Perpanjangan STR Dokter Spesialis".
  - Parameter input: `Category = "DOC-OFF"`, `DefaultSlaHours = 48`, `RequiresAttachment = true`, `RequiredAttachmentDescription = "Wajib melampirkan fotokopi STR lama dan bukti kecukupan SKP IDI"`.
  - Sistem memvalidasi input, mendaftarkan tipe layanan baru, dan dokter spesialis di seluruh rumah sakit langsung dapat memilih layanan ini di aplikasi ESS masing-masing.

---

## 5. Spesifikasi Endpoint REST API
Grup Swagger Tag: `[Tags("HR Services - Catalog & SLA")]`

| Method | Path | Deskripsi | Otorisasi |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/services/categories` | Daftar kategori layanan aktif | All Staff / HR |
| `GET` | `/api/v1/corporate/hrd/services/categories/{id}` | Detail kategori layanan | All Staff / HR |
| `POST` | `/api/v1/corporate/hrd/services/categories` | Tambah kategori layanan baru | HR Admin / Manager |
| `PUT` | `/api/v1/corporate/hrd/services/categories/{id}` | Ubah kategori layanan | HR Admin / Manager |
| `DELETE` | `/api/v1/corporate/hrd/services/categories/{id}` | Hapus kategori layanan (soft-delete) | HR Manager |
| `GET` | `/api/v1/corporate/hrd/services/types` | Daftar tipe layanan dengan filter kategori | All Staff / HR |
| `GET` | `/api/v1/corporate/hrd/services/types/{id}` | Detail tipe layanan | All Staff / HR |
| `POST` | `/api/v1/corporate/hrd/services/types` | Tambah tipe layanan baru | HR Admin / Manager |
| `PUT` | `/api/v1/corporate/hrd/services/types/{id}` | Ubah tipe layanan | HR Admin / Manager |
| `DELETE` | `/api/v1/corporate/hrd/services/types/{id}` | Hapus tipe layanan | HR Manager |
| `GET` | `/api/v1/corporate/hrd/services/document-types` | Daftar template dokumen naskah dinas | All Staff / HR |
| `GET` | `/api/v1/corporate/hrd/services/document-types/{id}` | Detail template dokumen naskah dinas | All Staff / HR |
| `POST` | `/api/v1/corporate/hrd/services/document-types` | Tambah template dokumen resmi baru | HR Admin / Manager |

---

## 6. Verifikasi & Integritas Bukti
* **Audit Trail**: Setiap perubahan katalog tercatat dalam logger.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-HRS-01` dan `VR-HRS-03`.