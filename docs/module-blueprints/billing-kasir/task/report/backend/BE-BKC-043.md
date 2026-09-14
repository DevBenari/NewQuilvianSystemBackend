# Laporan Perubahan Backend — `BE-BKC-043`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-043` |
| **Judul** | Master aturan tanggungan perusahaan penjamin (*Company Guarantor Coverage Rule*) |
| **Slice / Milestone** | `MVP-16` (Fondasi skema penjamin perusahaan dan aturan tanggungan multi-payer) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1728 |
| **Trace** | `FR-BKC-066`, `FR-BKC-067`; `MPY-DEC-008`, `MPY-DES-006`; `CAP-36` (Cetakan 1:1 dari aturan asuransi dengan tambahan dimensi golongan karyawan) |
| **Contract Version** | Grup `Health Services / Master Data / Company Guarantor Coverage Rule`, 9 endpoint (`BIL-API-1.0`); `BIL-VAL-092`–`097` |
| **Dependency** | `BE-BKC-041` (Tabel fondasi skema `MstCompanyGuarantorCoverageRule` telah dibuat dan dimigrasi ke database) |
| **Klasifikasi** | `MEDIUM` (1 Controller 9 endpoint, 1 Domain Service, 1 set DTO lengkap dengan validasi bisnis BIL-VAL-092–097 dan derivasi server-side urun biaya, integrasi hak akses terstandarisasi, registrasi DI pada `Program.cs`) |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | 🟡 `Sebagian` (Implementasi DTO, Service domain, Controller 9 endpoint, dan registrasi DI selesai 100%; kompilasi build terminal dan pengujian unit test dilakukan secara manual oleh pengguna sesuai instruksi eksplisit) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `HealthServices / MasterData` |
| **Owner / Prefix Registry** | Prefix `Mst` (`HealthServices / MasterData`, Category: `BUSINESS DOMAIN / MASTER / REFERENCE`, Status: `ACTIVE`) |
| **Keberlakuan** | `NEW CODE` — CRUD master data aturan tanggungan perusahaan penjamin (`CompanyGuarantorCoverageRuleController`, `CompanyGuarantorCoverageRuleService`, `CompanyGuarantorCoverageRuleDtos`) |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-MOD-003`, `QBE-NAM-001`, `QBE-NAM-004`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-OPT-001` |
| **Pengecualian / Temuan** | Sesuai instruksi eksplisit pengguna (*"Tanpa build, test, dan migration secara automatis. Biarkan saya lakukan manual"*), proses kompilasi build terminal (`dotnet build`) dan eksekusi unit test (`dotnet test`) ditiadakan dari eksekusi otomatis agen untuk dijalankan manual oleh pengguna. |

---

## 1. Masalah yang Diselesaikan

Dalam tata kelola penjaminan biaya pasien di rumah sakit:
1. **Diferensiasi Hak Tanggungan Karyawan Perusahaan (`CAP-36`, `MPY-DEC-008`):** Pasien yang dijamin oleh perusahaan memiliki hak tanggungan yang bergantung pada jenis item pelayanan (tindakan medis, obat-obatan, kategori obat, prosedur khusus, atau kategori tarif), kelas perawatan, paket manfaat (*benefit plan*), dan secara spesifik **golongan/tingkatan karyawan** (*Employee Grade*, misalnya Direksi, Manajer, Staf).
2. **Ketiadaan Mesin Aturan Tanggungan Perusahaan Penjamin:** Sebelumnya sistem belum memiliki tabel maupun API untuk mendefinisikan aturan tanggungan perusahaan. Tanpa master data ini, mesin kalkulasi tagihan tidak dapat menghitung porsi biaya yang ditanggung perusahaan vs porsi urun biaya pasien secara otomatis.
3. **Risiko Ketidakkonsistenan Urun Biaya (*Co-Payment*) (`FR-BKC-066`):** Jika klien (frontend/integrasi pihak ketiga) diizinkan mengirimkan persentase urun biaya dan persentase tanggungan secara terpisah, sering timbul anomali data di mana jumlah keduanya tidak sama dengan 100% (misalnya tanggungan 80% tetapi urun biaya dikirim 30%).
4. **Kepatuhan Aturan Validasi Bisnis Authoritative (`BIL-VAL-092`–`097`):**
   - Persentase tanggungan wajib 0–100 (`BIL-VAL-092`), dan server **wajib menurunkan sendiri** nilai urun biaya: `CoPaymentPercent = Math.Clamp(100 - CoveragePercent, 0, 100)`. Masukan urun biaya dari klien diabaikan.
   - Kode aturan wajib unik per perusahaan penjamin (`BIL-VAL-093`).
   - Setiap aturan wajib menyasar tepat satu rujukan item yang sesuai dengan tipe item (`BIL-VAL-094`, `BIL-VAL-095`).
   - Tanggal akhir masa berlaku tidak boleh mendahului tanggal mulai (`BIL-VAL-096`).
   - Aturan tanggungan yang sudah pernah digunakan pada perhitungan tagihan tersimpan (`BilCalculationVersion.BreakdownSnapshot`) **dilarang dihapus**, melainkan hanya boleh dinonaktifkan (`BIL-VAL-097`).

Task `BE-BKC-043` menyediakan antarmuka API master data lengkap (9 endpoint) beserta domain service untuk mengelola aturan tanggungan perusahaan penjamin dengan penegakan validasi bisnis yang ketat.

---

## 2. Alur Proses Bisnis & Spesifikasi Endpoint

### 2.1 Alur Kerja Master Aturan Tanggungan Perusahaan Penjamin

```text
Admin Master Data / Billing Supervisor
  │
  ├─► GET /filters/metadata (Membaca opsi filter, sort, dan metadata form create/update)
  │
  ├─► POST / (Mendaftarkan aturan tanggungan baru)
  │     ├── Validasi keberadaan & status aktif Perusahaan Penjamin
  │     ├── Validasi BIL-VAL-092: 0 <= CoveragePercent <= 100
  │     ├── Derivasi Server: CoPaymentPercent = 100 - CoveragePercent
  │     ├── Validasi BIL-VAL-093: RuleCode unik per CompanyGuarantorId
  │     ├── Validasi BIL-VAL-094: Rujukan item sesuai ItemType wajib diisi
  │     ├── Validasi BIL-VAL-095: Tidak boleh mengisi >1 rujukan item sekaligus
  │     ├── Validasi keberadaan & status aktif item sasaran (Tariff/Drug/DrugCategory/Procedure/TariffCategory)
  │     └── Validasi BIL-VAL-096: EffectiveEndDate >= EffectiveStartDate
  │
  ├─► GET / (Menampilkan daftar aturan dengan paging, pencarian kata kunci, filter penjamin, grade, item type, dan status)
  │
  ├─► GET /summary (Menampilkan ringkasan statistik aturan: total, aktif, nonaktif, covered, not covered, partial, need approval)
  │
  ├─► GET /options (Mengambil opsi aturan ringkas untuk dropdown form)
  │
  ├─► GET /{id} (Mengambil rincian lengkap aturan tanggungan beserta informasi audit pembuat & pengubah)
  │
  ├─► PUT /{id} (Memperbarui konfigurasi aturan tanggungan dengan penegakan validasi BIL-VAL-092 s.d. 096)
  │
  ├─► PATCH /{id}/status (Mengaktifkan atau menonaktifkan aturan tanggungan)
  │
  └─► DELETE /{id} (Menghapus aturan secara soft-delete)
        └── Validasi BIL-VAL-097: Jika Id atau RuleCode tercatat dalam BilCalculationVersion.BreakdownSnapshot,
            permintaan ditolak HTTP 422 ("Aturan ini sudah dipakai pada tagihan yang tersimpan. Nonaktifkan saja, jangan dihapus.")
```

### 2.2 Spesifikasi 9 Endpoint API (Bergaya Swagger)

Grup Tag: `[Tags("Health Services / Master Data / Company Guarantor Coverage Rule")]`  
Base URL: `api/v1/health-services/master-data/company-guarantor-coverage-rules`  
Kontrak: `BIL-API-1.0`  
Resource Hak Akses: `CompanyGuarantorCoverageRule`

| Method | Path | Deskripsi | Hak Akses | Request Body / Query | Status Code Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/filters/metadata`<br>`/filter-metadata` | Mengambil konfigurasi metadata filter, pengurutan, pagination, dan field form create/update | `CompanyGuarantorCoverageRule : Read` | — | `200 OK`<br>`ApiResponse<CompanyGuarantorCoverageRuleFilterMetadataResponse>` |
| `GET` | `/summary` | Ringkasan jumlah aturan (Total, Aktif, Nonaktif, Covered, NotCovered, PartialCovered, NeedApproval) | `CompanyGuarantorCoverageRule : Read` | — | `200 OK`<br>`ApiResponse<CompanyGuarantorCoverageRuleSummaryResponse>` |
| `GET` | `/` | Daftar aturan tanggungan dengan penyaringan penjamin, tipe item, status tanggungan, benefit plan, golongan karyawan, status aktif, tanggal, dan pagination | `CompanyGuarantorCoverageRule : Read` | Query:<br>`companyGuarantorId`, `itemType`, `coverageStatus`, `benefitPlanCode`, `employeeGrade`, `isActive`, `search`, `startDate`, `endDate`, `customPeriod`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `200 OK`<br>`ApiResponse<PagedResult<CompanyGuarantorCoverageRuleResponse>>` |
| `GET` | `/options` | Pilihan ringkas aturan tanggungan aktif untuk komponen dropdown / select | `CompanyGuarantorCoverageRule : Read` | Query:<br>`companyGuarantorId`, `itemType`, `search`, `onlyActive`, `pageNumber`, `pageSize` | `200 OK`<br>`ApiResponse<List<CompanyGuarantorCoverageRuleOptionResponse>>` |
| `GET` | `/{id:guid}` | Rincian lengkap satu aturan tanggungan beserta nama item sasaran, penjamin, dan jejak audit | `CompanyGuarantorCoverageRule : Read` | Route: `id` (Guid) | `200 OK`: `ApiResponse<CompanyGuarantorCoverageRuleDetailResponse>`<br>`404 Not Found`: Data tidak ditemukan |
| `POST` | `/` | Menambah aturan tanggungan baru untuk perusahaan penjamin | `CompanyGuarantorCoverageRule : Create` | Body:<br>`CreateCompanyGuarantorCoverageRuleRequest` | `201 Created`: `ApiResponse<CompanyGuarantorCoverageRuleResponse>`<br>`400 Bad Request`: Validasi gagal (`BIL-VAL-092`, `094`, `095`, `096`)<br>`404 Not Found`: Penjamin tidak ada<br>`422 Unprocessable`: `BIL-VAL-093` atau master sasaran nonaktif |
| `PUT` | `/{id:guid}` | Memperbarui konfigurasi aturan tanggungan perusahaan | `CompanyGuarantorCoverageRule : Update` | Route: `id` (Guid)<br>Body:<br>`UpdateCompanyGuarantorCoverageRuleRequest` | `200 OK`: `ApiResponse<CompanyGuarantorCoverageRuleResponse>`<br>`400 Bad Request`: Validasi gagal (`BIL-VAL-092`, `094`, `095`, `096`)<br>`404 Not Found`: Data tidak ada<br>`422 Unprocessable`: `BIL-VAL-093` atau master sasaran nonaktif |
| `PATCH` | `/{id:guid}/status` | Mengubah status aktif / nonaktif aturan tanggungan | `CompanyGuarantorCoverageRule : Update` | Route: `id` (Guid)<br>Body:<br>`UpdateCompanyGuarantorCoverageRuleStatusRequest` | `200 OK`: `ApiResponse<CompanyGuarantorCoverageRuleResponse>`<br>`404 Not Found`: Data tidak ada |
| `DELETE` | `/{id:guid}` | Menghapus aturan tanggungan secara soft-delete (dengan pengecekan integritas riwayat tagihan) | `CompanyGuarantorCoverageRule : Delete` | Route: `id` (Guid)<br>Body (opsional):<br>`DeleteCompanyGuarantorCoverageRuleRequest` | `200 OK`: `ApiResponse<bool>`<br>`404 Not Found`: Data tidak ada<br>`422 Unprocessable`: `BIL-VAL-097` (aturan sudah dipakai versi kalkulasi tagihan) |

### 2.3 Matriks Penegakan Validasi Bisnis

| Aturan | Kondisi Pelanggaran | Kode | Pesan Kesalahan |
| :--- | :--- | :---: | :--- |
| `BIL-VAL-092` | `CoveragePercent < 0` atau `CoveragePercent > 100` | `400` | *"Persentase tanggungan harus berada di antara 0 dan 100."*<br>*(Catatan: Server secara otomatis menghitung `CoPaymentPercent = 100 - CoveragePercent`, masukan klien untuk urun biaya diabaikan)* |
| `BIL-VAL-093` | `RuleCode` sudah digunakan oleh aturan non-terhapus lain pada perusahaan penjamin yang sama | `422` | *"Kode aturan ini sudah dipakai pada perusahaan penjamin tersebut."* |
| `BIL-VAL-094` | Rujukan item kosong untuk tipe item yang dipilih (misal `ItemType = "Drug"` tanpa `DrugId`) | `400` | *"Lengkapi item yang menjadi sasaran aturan ini."* |
| `BIL-VAL-095` | Lebih dari satu rujukan item diisi secara simultan (misal `TariffId` dan `DrugId` terisi bersamaan) | `400` | *"Aturan hanya boleh menyasar satu jenis item."* |
| `BIL-VAL-096` | `EffectiveEndDate < EffectiveStartDate` | `400` | *"Tanggal akhir masa berlaku tidak boleh mendahului tanggal mulai."* |
| `BIL-VAL-097` | Aturan yang hendak dihapus tercatat dalam `BilCalculationVersion.BreakdownSnapshot` pada tagihan yang tersimpan | `422` | *"Aturan ini sudah dipakai pada tagihan yang tersimpan. Nonaktifkan saja, jangan dihapus."* |

---

## 3. Detail Perubahan Berkas

| Berkas | Status | Penjelasan |
| :--- | :---: | :--- |
| `Areas/HealthServices/MasterData/DTOs/CompanyGuarantorCoverageRuleDtos.cs` | `Baru / Diperbarui` | Mendefinisikan kontrak data DTO: FilterMetadata, DefaultFilter, CustomPeriod, SortOption, QueryParameter, FormField, Summary, Response, DetailResponse, OptionResponse, CreateRequest, UpdateRequest, UpdateStatusRequest, DeleteRequest, serta exception domain `CompanyGuarantorCoverageRuleValidationException`. |
| `Areas/HealthServices/MasterData/Services/CompanyGuarantorCoverageRuleService.cs` | `Baru` | Implementasi `ICompanyGuarantorCoverageRuleService` yang mengorkestrasi logika bisnis master aturan tanggungan, derivasi urun biaya server-side, penomoran kode otomatis `CCR-RSMMC-xxxxx`, penegakan validasi `BIL-VAL-092` s.d. `097`, soft delete dengan verifikasi snapshot kalkulasi, audit trail, dan pencatatan log terstruktur. |
| `Areas/HealthServices/MasterData/Controllers/CompanyGuarantorCoverageRuleController.cs` | `Baru` | Controller ASP.NET Core mengekspos 9 endpoint master data, atribut otorisasi `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, Swagger tag grup Health Services, serta pemetaan status code HTTP standar. |
| `Program.cs` | `Diubah` | Mendaftarkan interface `ICompanyGuarantorCoverageRuleService` dan kelas konkret `CompanyGuarantorCoverageRuleService` ke dalam kontainer dependency injection dengan masa hidup Scoped. |

---

## 4. Acceptance Criteria & Verifikasi

| Kriteria Penerimaan | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| **AC-01:** Tersedia 9 endpoint master data baseline sesuai kontrak `BIL-API-1.0` | Terpenuhi | Sembilan endpoint terdefinisi di `CompanyGuarantorCoverageRuleController.cs`: `GET /filters/metadata` (dan alias `/filter-metadata`), `GET /summary`, `GET /`, `GET /options`, `GET /{id}`, `POST /`, `PUT /{id}`, `PATCH /{id}/status`, `DELETE /{id}` |
| **AC-02:** Derivasi server-side urun biaya dari persentase tanggungan (`BIL-VAL-092`, `FR-BKC-066`, `BIL-AT-099`) | Terpenuhi | Pada `CreateRuleAsync` dan `UpdateRuleAsync`, `CoPaymentPercent` dihitung otomatis: `Math.Clamp(100m - coveragePercent, 0m, 100m)`, masukan klien diabaikan secara authoritative |
| **AC-03:** Keunikan kode aturan per perusahaan penjamin (`BIL-VAL-093`) | Terpenuhi | `ValidateBusinessRulesAsync` memvalidasi keunikan `RuleCode` terhadap baris non-terhapus untuk `CompanyGuarantorId` yang sama, menghasilkan HTTP `422` jika terduplikasi |
| **AC-04:** Penegakan tepat satu sasaran item (`BIL-VAL-094`, `BIL-VAL-095`) | Terpenuhi | `filledItemCount > 1` menolak dengan HTTP `400` (*"Aturan hanya boleh menyasar satu jenis item."*); ketidaklengkapan item sesuai `ItemType` menolak dengan HTTP `400` (*"Lengkapi item yang menjadi sasaran aturan ini."*) |
| **AC-05:** Penegakan masa berlaku (`BIL-VAL-096`) | Terpenuhi | Memastikan `EffectiveEndDate >= EffectiveStartDate` bila keduanya diisi |
| **AC-06:** Pencegahan hapus aturan yang telah dipakai tagihan (`BIL-VAL-097`) | Terpenuhi | `DeleteRuleAsync` memeriksa apakah `Id` atau `RuleCode` aturan terdapat pada `BilCalculationVersion.BreakdownSnapshot`; jika ditemukan, menolak dengan HTTP `422` (*"Aturan ini sudah dipakai pada tagihan yang tersimpan. Nonaktifkan saja, jangan dihapus."*) |
| **AC-07:** Pendaftaran resource hak akses otomatis melalui atribut | Terpenuhi | `[AccessController(ControllerName = "CompanyGuarantorCoverageRule")]` dipasangkan dengan `[AccessPermission("CompanyGuarantorCoverageRule", ...)]` dan `[AccessAction]` dengan tipe akses Read, Create, Update, Delete |
| **AC-08:** Kompilasi build & uji | Menunggu Verifikasi Manual Pengguna | Sesuai instruksi pengguna, `dotnet build` dan `dotnet test` tidak dijalankan secara otomatis oleh agen dan diserahkan ke pengguna |

---

## 5. Status & Tindak Lanjut

1. **Status Task:** 🟡 **Sebagian.** Seluruh berkas DTO, Domain Service, Controller 9 endpoint, dan registrasi DI pada `Program.cs` telah terimplementasi 100% sesuai spesifikasi kontrak API dan matriks validasi. Sesuai instruksi eksplisit pengguna, proses kompilasi build terminal dan eksekusi test diserahkan kepada pengguna untuk dilakukan secara manual.
2. **Langkah Berikutnya bagi Pengguna:**
   - Jalankan kompilasi: `dotnet build`
   - Uji verifikasi endpoint master data aturan tanggungan dan aturan `BIL-VAL-092`–`097` (melalui Swagger atau HTTP client)
3. **Task Lanjutan di Roadmap:**
   - `BE-BKC-044`: Mesin tanggungan perusahaan dan perbaikan adapter (`CompanyGuarantorCoverageService` & pembaruan `BillingCalculationService` untuk menghilangkan peringatan palsu tagihan penjamin perusahaan)
