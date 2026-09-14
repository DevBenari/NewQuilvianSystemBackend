# Laporan Perubahan Backend — `BE-BKC-042`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-042` |
| **Judul** | Master rute reimbursement perusahaan penjamin |
| **Slice / Milestone** | `MVP-16` (Fondasi skema penjamin perusahaan dan penanggung per baris tagihan) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1711 |
| **Trace** | `FR-BKC-083`; `MPY-DEC-008`, `MPY-DES-014`; `CAP-35` (Ready to reuse) |
| **Contract Version** | Grup `Administrator / Master Data / Company Guarantor Reimbursement Route`, 9 endpoint (`BIL-API-1.0`); `BIL-VAL-087`–`091` |
| **Dependency** | `BE-BKC-041` (Tabel fondasi skema `MstCompanyGuarantorReimbursementRoute` telah dibuat dan dimigrasi ke database) |
| **Klasifikasi** | `MEDIUM` (1 Controller 9 endpoint, 1 Domain Service, 1 set DTO lengkap dengan validasi bisnis BIL-VAL-087–091, integrasi hak akses terstandarisasi, registrasi DI pada `Program.cs`) |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | 🟡 `Sebagian` (Implementasi DTO, Service domain, Controller 9 endpoint, dan registrasi DI selesai 100%; kompilasi build terminal dan pengujian unit test dilakukan secara manual oleh pengguna sesuai instruksi eksplisit) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `Administrator` |
| **Module / Submodule** | `Administrator / MasterData` |
| **Owner / Prefix Registry** | Prefix `Mst` (`Administrator / MasterData`, Category: `BUSINESS DOMAIN / MASTER / REFERENCE`, Status: `ACTIVE`) |
| **Keberlakuan** | `NEW CODE` — CRUD master data rute reimbursement perusahaan penjamin (`CompanyGuarantorReimbursementRouteController`, `CompanyGuarantorReimbursementRouteService`, `CompanyGuarantorReimbursementRouteDtos`) |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-MOD-003`, `QBE-NAM-001`, `QBE-NAM-004`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-OPT-001` |
| **Pengecualian / Temuan** | Sesuai instruksi eksplisit pengguna (*"Tanpa build, test, dan migration secara automatis. Biarkan saya lakukan manual"*), proses kompilasi build terminal (`dotnet build`) dan eksekusi unit test (`dotnet test`) ditiadakan dari eksekusi otomatis agen untuk dijalankan manual oleh pengguna. |

---

## 1. Masalah yang Diselesaikan

Dalam operasional penagihan rumah sakit kepada perusahaan penjamin (*Company Guarantor*):
1. **Ketidakpastian Mekanisme Penggantian Biaya (`CAP-35`, `MPY-DEC-008`):** Setiap perusahaan yang bekerja sama dengan rumah sakit memiliki skema penggantian biaya yang berbeda. Sebagian perusahaan menanggung dan membayar tagihan secara mandiri langsung ke rekening rumah sakit (*SELF*), sedangkan sebagian lainnya menggunakan pihak ketiga perusahaan asuransi atau Third Party Administrator (TPA) mitra sebagai penjamin teknis (*INSURANCE_PROVIDER*).
2. **Ketiadaan Konfigurasi Rute Bawaan & Prioritas:** Rumah sakit memerlukan konfigurasi rute bawaan (*default route*) per perusahaan agar saat pendaftaran atau kasir memproses tagihan, sistem secara otomatis mengetahui apakah invoice ditagihkan langsung ke perusahaan penjamin atau diproses melalui asuransi mitra.
3. **Kepatuhan Aturan Validasi Bisnis (`BIL-VAL-087`–`091`):**
   - Rute mandiri (*SELF*) tidak boleh dibebani rujukan asuransi mitra yang membingungkan (`BIL-VAL-087`).
   - Rute melalui asuransi mitra (*INSURANCE_PROVIDER*) wajib menyertakan asuransi mitra yang valid dan aktif (`BIL-VAL-088`, `BIL-VAL-089`).
   - Tepat satu rute bawaan aktif per perusahaan penjamin (`BIL-VAL-090`).
   - Tanggal berakhirnya masa berlaku tidak boleh mendahului tanggal mulai (`BIL-VAL-091`).

Task `BE-BKC-042` menyediakan antarmuka API master data lengkap (9 endpoint) beserta layanan domain untuk mengelola rute reimbursement perusahaan penjamin secara terstruktur, aman, dan dapat diaudit.

---

## 2. Alur Proses Bisnis & Spesifikasi Endpoint

### 2.1 Alur Kerja Master Rute Reimbursement

```text
Admin Master Data
  │
  ├─► GET /filters/metadata (Membaca opsi filter, sort, dan field metadata form)
  │
  ├─► POST / (Mendaftarkan rute baru)
  │     ├── Validasi RouteType: SELF atau INSURANCE_PROVIDER
  │     ├── Validasi BIL-VAL-087: Jika SELF, InsuranceProviderId harus kosong
  │     ├── Validasi BIL-VAL-088: Jika INSURANCE_PROVIDER, InsuranceProviderId wajib diisi
  │     ├── Validasi BIL-VAL-089: Asuransi mitra harus terdaftar dan berstatus aktif
  │     ├── Validasi BIL-VAL-090: Cek apakah perusahaan sudah punya rute bawaan (default) aktif
  │     └── Validasi BIL-VAL-091: EffectiveEndDate >= EffectiveStartDate
  │
  ├─► GET / (Menampilkan daftar rute dengan paging, pencarian, dan penyaringan)
  │
  ├─► GET /{id} (Mengambil rincian lengkap rute reimbursement)
  │
  ├─► PUT /{id} (Memperbarui data rute dengan penegakan validasi BIL-VAL-087 s.d. 091)
  │
  ├─► PATCH /{id}/status (Mengaktifkan / menonaktifkan rute, menjaga integritas rute bawaan tunggal)
  │
  └─► DELETE /{id} (Menandai rute terhapus / soft-delete dengan pencatatan jejak audit)
```

### 2.2 Spesifikasi 9 Endpoint API (Bergaya Swagger)

Grup Tag: `[Tags("Administrator / Master Data / Company Guarantor Reimbursement Route")]`  
Base URL: `api/v1/administrator/master-data/company-guarantor-reimbursement-routes`  
Kontrak: `BIL-API-1.0`  
Resource Hak Akses: `CompanyGuarantorReimbursementRoute`

| Method | Path | Deskripsi | Hak Akses | Request Body / Query | Status Code Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/filters/metadata`<br>`/filter-metadata` | Mengambil konfigurasi metadata filter, pengurutan, pagination, dan field form create/update | `CompanyGuarantorReimbursementRoute : Read` | — | `200 OK`<br>`ApiResponse<CompanyGuarantorReimbursementRouteFilterMetadataResponse>` |
| `GET` | `/summary` | Ringkasan jumlah rute (Total, Aktif, Nonaktif, Self, Asuransi Mitra, Bawaan) | `CompanyGuarantorReimbursementRoute : Read` | — | `200 OK`<br>`ApiResponse<CompanyGuarantorReimbursementRouteSummaryResponse>` |
| `GET` | `/` | Daftar rute reimbursement dengan penyaringan penjamin, mitra, tipe rute, status, tanggal, dan pagination | `CompanyGuarantorReimbursementRoute : Read` | Query:<br>`companyGuarantorId`, `insuranceProviderId`, `routeType`, `isDefault`, `isActive`, `search`, `startDate`, `endDate`, `customPeriod`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `200 OK`<br>`ApiResponse<PagedResult<CompanyGuarantorReimbursementRouteResponse>>` |
| `GET` | `/options` | Pilihan ringkas rute reimbursement aktif untuk komponen dropdown / select | `CompanyGuarantorReimbursementRoute : Read` | Query:<br>`companyGuarantorId`, `routeType`, `search`, `onlyActive`, `pageNumber`, `pageSize` | `200 OK`<br>`ApiResponse<List<CompanyGuarantorReimbursementRouteOptionResponse>>` |
| `GET` | `/{id:guid}` | Rincian lengkap satu rute reimbursement beserta jejak audit pembuat dan pengubah | `CompanyGuarantorReimbursementRoute : Read` | Route: `id` (Guid) | `200 OK`: `ApiResponse<CompanyGuarantorReimbursementRouteDetailResponse>`<br>`404 Not Found`: Data tidak ditemukan |
| `POST` | `/` | Menambah rute reimbursement baru untuk perusahaan penjamin | `CompanyGuarantorReimbursementRoute : Create` | Body:<br>`CreateCompanyGuarantorReimbursementRouteRequest` | `200 OK`: `ApiResponse<CompanyGuarantorReimbursementRouteResponse>`<br>`400 Bad Request`: Validasi gagal (`BIL-VAL-087`, `088`, `091`)<br>`404 Not Found`: Penjamin/asuransi tidak ada<br>`422 Unprocessable`: `BIL-VAL-089`, `090` |
| `PUT` | `/{id:guid}` | Memperbarui konfigurasi rute reimbursement | `CompanyGuarantorReimbursementRoute : Update` | Route: `id` (Guid)<br>Body:<br>`UpdateCompanyGuarantorReimbursementRouteRequest` | `200 OK`: `ApiResponse<CompanyGuarantorReimbursementRouteResponse>`<br>`400 Bad Request`: Validasi gagal (`BIL-VAL-087`, `088`, `091`)<br>`404 Not Found`: Data tidak ada<br>`422 Unprocessable`: `BIL-VAL-089`, `090` |
| `PATCH` | `/{id:guid}/status` | Mengubah status aktif / nonaktif rute reimbursement | `CompanyGuarantorReimbursementRoute : Update` | Route: `id` (Guid)<br>Body:<br>`UpdateCompanyGuarantorReimbursementRouteStatusRequest` | `200 OK`: `ApiResponse<CompanyGuarantorReimbursementRouteResponse>`<br>`404 Not Found`: Data tidak ada<br>`422 Unprocessable`: `BIL-VAL-090` saat aktivasi |
| `DELETE` | `/{id:guid}` | Menghapus rute reimbursement secara soft-delete | `CompanyGuarantorReimbursementRoute : Delete` | Route: `id` (Guid) | `200 OK`: `ApiResponse<bool>`<br>`404 Not Found`: Data tidak ada |

### 2.3 Matriks Penegakan Validasi Bisnis

| Aturan | Kondisi Pelanggaran | Kode | Pesan Kesalahan |
| :--- | :--- | :---: | :--- |
| `BIL-VAL-087` | `RouteType = "SELF"` tetapi `InsuranceProviderId` diisi | `400` | *"Perusahaan yang menanggung sendiri tidak memerlukan asuransi mitra."* |
| `BIL-VAL-088` | `RouteType = "INSURANCE_PROVIDER"` tetapi `InsuranceProviderId` kosong/null | `400` | *"Pilih perusahaan asuransi mitra untuk rute ini."* |
| `BIL-VAL-089` | `RouteType = "INSURANCE_PROVIDER"` dan asuransi mitra berstatus nonaktif | `422` | *"Perusahaan asuransi yang dipilih sudah tidak aktif."* |
| `BIL-VAL-090` | Rute ditandai `IsDefault = true` dan `IsActive = true`, padahal sudah ada rute bawaan aktif lain pada perusahaan yang sama | `422` | *"Perusahaan ini sudah memiliki rute bawaan. Nonaktifkan yang lama lebih dulu."* |
| `BIL-VAL-091` | `EffectiveEndDate < EffectiveStartDate` | `400` | *"Tanggal akhir masa berlaku tidak boleh mendahului tanggal mulai."* |

---

## 3. Detail Perubahan Berkas

| Berkas | Status | Penjelasan |
| :--- | :---: | :--- |
| `Areas/Administrator/MasterData/DTOs/CompanyGuarantorReimbursementRouteDtos.cs` | `Baru` | Mendefinisikan DTO lengkap: FilterMetadata, DefaultFilter, CustomPeriod, SortOption, QueryParameter, FormField, Summary, Response, DetailResponse, OptionResponse, CreateRequest, UpdateRequest, UpdateStatusRequest, serta `CompanyGuarantorReimbursementRouteValidationException`. |
| `Areas/Administrator/MasterData/Services/CompanyGuarantorReimbursementRouteService.cs` | `Baru` | Implementasi `ICompanyGuarantorReimbursementRouteService` yang mengorkestrasi logika bisnis, query database EF Core, penegakan aturan validasi `BIL-VAL-087`–`091`, soft delete, jejak audit, dan pencatatan logger terstruktur. |
| `Areas/Administrator/MasterData/Controllers/CompanyGuarantorReimbursementRouteController.cs` | `Baru` | Controller ASP.NET Core mengekspos 9 endpoint master data, atribut otorisasi `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, Swagger tag, serta pemetaan penanganan status code HTTP. |
| `Program.cs` | `Diubah` | Menambahkan namespace `QuilvianSystemBackend.Areas.Administrator.MasterData.Services` dan mendaftarkan `ICompanyGuarantorReimbursementRouteService` serta `CompanyGuarantorReimbursementRouteService` ke dalam kontainer dependency injection (AddScoped). |

---

## 4. Acceptance Criteria & Verifikasi

| Kriteria Penerimaan | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| **AC-01:** Tersedia 9 endpoint master data baseline sesuai kontrak `BIL-API-1.0` | Terpenuhi | Sembilan endpoint terdefinisi di `CompanyGuarantorReimbursementRouteController.cs`: `GET /filters/metadata` (dan alias `/filter-metadata`), `GET /summary`, `GET /`, `GET /options`, `GET /{id}`, `POST /`, `PUT /{id}`, `PATCH /{id}/status`, `DELETE /{id}` |
| **AC-02:** Penegakan pasangan jenis rute dan asuransi mitra (`BIL-VAL-087`, `BIL-VAL-088`, `BIL-VAL-089`) | Terpenuhi | Diperiksa pada `CreateRouteAsync` dan `UpdateRouteAsync`: menolak SELF ber-mitra (`400`), menolak INSURANCE_PROVIDER tanpa mitra (`400`), menolak asuransi mitra nonaktif (`422`) dengan pesan baku kontrak |
| **AC-03:** Penegakan maksimal satu rute bawaan aktif per perusahaan (`BIL-VAL-090`, `BIL-AT-099`) | Terpenuhi | Diperiksa pada `CreateRouteAsync`, `UpdateRouteAsync`, dan `UpdateRouteStatusAsync`: jika `IsDefault && IsActive`, mengecek apakah sudah ada rute bawaan aktif lain untuk `CompanyGuarantorId` yang sama |
| **AC-04:** Penegakan masa berlaku (`BIL-VAL-091`) | Terpenuhi | Memastikan `EffectiveEndDate >= EffectiveStartDate` bila keduanya diisi |
| **AC-05:** Pendaftaran resource hak akses otomatis melalui atribut | Terpenuhi | `[AccessController(ControllerName = "CompanyGuarantorReimbursementRoute")]` dipasangkan dengan `[AccessPermission("CompanyGuarantorReimbursementRoute", ...)]` dan `[AccessAction]` dengan tipe akses Read, Create, Update, Delete yang valid |
| **AC-06:** Kompilasi build & uji | Menunggu Verifikasi Manual Pengguna | Sesuai instruksi pengguna, `dotnet build` dan `dotnet test` tidak dijalankan secara otomatis oleh agen dan diserahkan ke pengguna |

---

## 5. Status & Tindak Lanjut

1. **Status Task:** 🟡 **Sebagian.** Seluruh source code DTO, Domain Service, Controller 9 endpoint, dan registrasi DI pada `Program.cs` telah terimplementasi 100% sesuai spesifikasi kontrak API dan matriks validasi. Sesuai permintaan eksplisit, proses kompilasi build dan pengujian diserahkan kepada pengguna untuk dilakukan secara manual.
2. **Langkah Berikutnya bagi Pengguna:**
   - Jalankan kompilasi: `dotnet build`
   - Uji verifikasi endpoint master data rute reimbursement dan aturan `BIL-VAL-087`–`091` (melalui Swagger atau HTTP client)
3. **Task Lanjutan di Roadmap:**
   - `BE-BKC-043`: Master aturan tanggungan perusahaan penjamin (`CompanyGuarantorCoverageRuleController`, 9 endpoint CRUD dengan derivasi server-side urun biaya)
   - `BE-BKC-044`: Mesin kalkulasi tanggungan perusahaan penjamin (`CompanyGuarantorCoverageService`)
