# Laporan Penyelesaian Backend Task: BE-HRD-185

## 1. Identitas Task
* **ID Task**: `BE-HRD-185`
* **Nama Task**: Controller REST API Pendaftaran Asuransi & Tanggungan Keluarga ESS (`BenefitEnrollmentController.cs`, `[Tags("HR Benefits - Enrollments & Dependents")]`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-37: Administrasi Asuransi, Tanggungan Keluarga ESS & Verifikasi Berkas Fisik`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Untuk menyediakan akses layanan backend yang aman, terstandarisasi, dan terdokumentasi rapi bagi tim frontend Next.js serta integrasi antar-subsistem SIMRS, task `BE-HRD-185` mengimplementasikan seluruh endpoint RESTful Kontrak API `v16` pada grup tag Swagger `HR Benefits - Enrollments & Dependents`.

Controller ini menyatukan 3 domain utama:
1. **Administrasi Kepesertaan Asuransi Staf RS**: Pencarian paged, detail polis BPJS TK/Kes, aktivasi bertahap probation, dan update nomor polis.
2. **Meja Kerja HR Benefit Officer**: Antrean verifikasi berkas tanggungan keluarga dan approval/rejection.
3. **Portal Mandiri ESS Pegawai**: Pengambilan daftar keluarga, pengecekan kuota aktif, dan formulir pendaftaran tanggungan.

---

## 3. Spesifikasi Endpoint REST API (Swagger-Tagged)

Prefix Rute: `/api/v1/corporate/hrd/benefits` (dan alias `/api/hr-benefits`)
Tag OpenAPI: `[Tags("HR Benefits - Enrollments & Dependents")]`

| No | HTTP Method | Endpoint Path | Deskripsi Fungsional & Kepatuhan Aturan Bisnis | Format Request / Query | Format Response | Kode HTTP |
|:--:|:-----------:|:--------------|:-----------------------------------------------|:-----------------------|:----------------|:---------:|
| 1 | `GET` | `/enrollments` | Mengambil daftar kepesertaan asuransi staf RS dengan filter status aktivasi (`FR-BEN-06`) | `EmployeeEnrollmentListQuery` (Status, SearchKeyword, Page, PageSize) | `PagedResult<EmployeeEnrollmentListItemDto>` | `200 OK` |
| 2 | `GET` | `/enrollments/{id}` | Mengambil detail kepesertaan, nomor polis, dan daftar tanggungan terdaftar | Route parameter: `id:guid` | `BenefitEnrollmentDetailDto` | `200 OK`, `404 Not Found` |
| 3 | `POST` | `/enrollments/{id}/activate-probation` | Mengaktifkan paket kesehatan lengkap pasca-lulus masa percobaan 3 bulan (`BEN-B1`, `VR-BEN-13`) | `{ "probationEvaluationId": "uuid", "effectiveDate": "...", "notes": "..." }` | `BenefitEnrollmentDetailDto` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| 4 | `PUT` | `/enrollments/{id}/policy-numbers` | Memperbarui nomor kartu BPJS Kesehatan (13 dig), BPJS TK (11 dig), atau polis swasta (`VR-BEN-09`..`10`) | `{ "bpjsKesehatanNumber": "...", "bpjsKetenagakerjaanNumber": "...", "privateInsurancePolicyNumber": "..." }` | `BenefitEnrollmentDetailDto` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| 5 | `GET` | `/dependents/pending-verification` | Mengambil antrean pendaftaran anggota keluarga ESS yang menunggu verifikasi HR (`FR-BEN-10`) | `PendingDependentVerificationQuery` (Page, PageSize, SearchKeyword) | `PagedResult<PendingDependentVerificationDto>` | `200 OK` |
| 6 | `POST` | `/dependents/{id}/verify` | Persetujuan (`Verified`) atau penolakan (`Rejected` min 15 char) berkas KK/Akta oleh HR (`VR-BEN-25`, `AUDIT-BEN-05`) | `{ "verificationStatus": "Verified/Rejected", "coverageLimitAmount": 10000000, "rejectionReason": "..." }` | `BenefitDependentDetailDto` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| 7 | `GET` | `/dependents/my-family` | Mengambil daftar keluarga terdaftar pada profil staf yang sedang login via portal ESS (`FR-BEN-07`) | None (Otentikasi Token JWT) | `List<EmployeeDependentSummaryDto>` | `200 OK`, `401 Unauthorized` |
| 8 | `GET` | `/dependents/my-family/quota` | Mengambil indikator pemakaian kuota pasangan (maks 1) dan anak (maks 3) untuk ESS (`BEN-B2`) | None (Otentikasi Token JWT) | `EmployeeFamilyQuotaSummaryDto` | `200 OK`, `401 Unauthorized` |
| 9 | `POST` | `/dependents/my-family` | Pegawai mendaftarkan pasangan atau anak berlampiran KK, Akta, atau Surat Dekanat Kuliah (`BEN-B2`, `BEN-B3`, `VR-BEN-01`..`08`) | `CreateEssDependentRequestDto` (NIK, No KK, Nama, Hubungan, Tgl Lahir, IsKuliah, SuratKuliah) | `EmployeeDependentSummaryDto` | `201 Created`, `400 Bad Request`, `422 Unprocessable` |

---

## 4. Komponen Teknis yang Dibuat

### A. Controller C#
File: `Areas/Corporate/HumanResource/BenefitManagement/Controllers/BenefitEnrollmentController.cs`
* Dilengkapi anotasi `[Authorize]`, `[ApiController]`, dan `[Tags("HR Benefits - Enrollments & Dependents")]`.
* Helper ekstraksi identitas: `GetCurrentUserId()` dan `GetCurrentWorkforceProfileId()`.
* Penanganan status kode HTTP yang konsisten: `200 OK`, `201 Created`, `400 Bad Request`, `404 Not Found`, dan `422 UnprocessableEntity`.

### B. Registrasi Dependency Injection (DI)
File: `Program.cs`
```csharp
// Slice S-D3 (Benefit Management) - MVP-37 (Insurance Enrollments, ESS Dependents & Verification)
builder.Services.AddScoped<
    QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services.IBenefitEnrollmentService, 
    QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services.BenefitEnrollmentService>();
```

---

## 5. Verifikasi & Integritas Bukti
* **Swagger OpenAPI Documentation**: Seluruh 9 endpoint terdaftar di bawah tag grup `HR Benefits - Enrollments & Dependents` dan dapat diuji langsung via Swagger UI.
* **Kompilasi .NET**: Kompilasi solution terverifikasi 0 Error(s).
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-BEN-03` s.d. `AT-HRD-BEN-10`.
