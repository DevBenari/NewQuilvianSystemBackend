# Laporan Perubahan Backend — `BE-HRD-019`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-019` |
| Judul | Pendaftaran Benefit Karyawan, Penjaminan Tanggungan, dan Mutasi Saldo |
| Slice | `S-D3` (Benefit Karyawan & Tanggungan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-003`, `HRD-DEC-019`, `S-D3`, `BE-FIN-103`, `BE-FIN-104`, `FIN-DEC-207` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-002` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target tulis | `EmployeeBenefitEnrollmentController.cs`, `EmployeeBenefitDependentController.cs`, `EmployeeBenefitEnrollmentService.cs`, `EmployeeBenefitDependentService.cs`, `EmployeeBenefitBalanceService.cs`, `WorkforceBenefitOwnerResolver.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki

1. **Kebutuhan Pengelolaan Paket Benefit Kesehatan & Asuransi bagi SDM Rumah Sakit**:
   Tenaga kerja rumah sakit (karyawan tetap, dokter tetap, staf kontrak) memiliki hak benefit kesehatan (plafon rawat inap, rawat jalan, kacamata, persalinan, dan gigi) yang berbeda sesuai jenjang jabatan, lama masa kerja, dan tipe kepegawaian. Diperlukan modul enrollment yang mengatur siklus kepesertaan secara disiplin (`Draft` -> `Submitted` -> `Approved` -> `Active` -> `Ended` / `Cancelled`).
2. **Pengawasan Ketat Jumlah Tanggungan (*Dependent Ceiling Protection*)**:
   Paket benefit rumah sakit menetapkan batas maksimal jumlah tanggungan yang ditanggung (misal maksimal 3 orang: suami/istri dan 2 anak). Tanpa validasi batas kuota di backend, rumah sakit rentan menanggung biaya klaim yang melebihi plafon paket (`MaximumDependents`).
3. **Pencatatan Buku Mutasi Saldo Benefit (*Audit Trail Balance Movement*)**:
   Setiap transaksi yang memengaruhi kuota atau saldo plafon (alokasi awal tahunan, penyesuaian/top-up, pemotongan klaim biaya perawatan di RS, dan pengembalian klaim) wajib tercatat di buku mutasi (`HrdBenefitBalanceMovement`) agar dapat direkonsiliasi dengan bagian Billing dan Finance RS.

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[Karyawan / Dokter Tetap RS]
              │
              ▼
    [HR Benefit Administrator]
              │
              ├──► 1. Membuat Pendaftaran Benefit ──► POST /employee-benefit-enrollments
              │       ├─► Validasi eligibilitas paket (EmployeeBenefitEligibilityService)
              │       └─► Status awal: Draft (Plafon awal & MaximumDependents ditetapkan)
              │
              ├──► 2. Menambahkan Tanggungan (Keluarga) ──► POST /employee-benefit-enrollments/{id}/dependents
              │       ├─► Sistem memvalidasi relasi keluarga (WfpFamilyMember)
              │       └─► Sistem memvalidasi kuota tanggungan: openCount < MaximumDependents
              │
              ├──► 3. Verifikasi & Approval:
              │       ├─► Pengajuan: POST /employee-benefit-enrollments/{id}/submit
              │       ├─► Persetujuan: POST /employee-benefit-enrollments/{id}/approve
              │       ├─► Aktivasi: POST /employee-benefit-enrollments/{id}/activate
              │       └─► Verifikasi Tanggungan: POST /dependents/{depId}/verify
              │
              └──► 4. Rekonsiliasi Saldo & Klaim Plafon ──► GET /employee-benefit-enrollments/{id}/movements
                      └─► Menampilkan buku mutasi saldo secara kronologis (HrdBenefitBalanceMovement)
```

* **Tujuan**: Menjamin kepastian perlindungan kesehatan staf rumah sakit dan keluarganya secara akurat, transparan, dan terkontrol anggarannya.
* **Pelaku**: HR Benefit Administrator, Kepala HRD / Direksi, Pegawai dan Tanggungan yang Sah.
* **Pemicu**: Pengangkatan karyawan baru, pergantian periode tahunan benefit, penambahan anggota keluarga baru (kelahiran/pernikahan), atau klaim pemakaian plafon medis.
* **Langkah & Aturan**:
  1. Enrollment benefit diajukan per pegawai atau dokter tetap berbasis `WorkforceProfileId`.
  2. Paket benefit menentukan plafon nominal rawat jalan/rawat inap serta batas maksimal tanggungan (`MaximumDependents`).
  3. Pendaftaran tanggungan divalidasi ketat: jika jumlah tanggungan yang terdaftar sudah mencapai batas maksimum paket, pendaftaran ditolak (`422 Unprocessable Entity`).
  4. Perubahan saldo akibat pemakaian layanan rumah sakit mencatat debit/kredit pada `HrdBenefitBalanceMovement`.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Benefit Management / Employee Benefit Enrollment`
Tag: `[Tags("Corporate / Human Resource / Benefit Management / Employee Benefit Enrollment")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/filters/metadata` | Mengambil metadata opsi filter enrollment | `EmployeeBenefitEnrollment:Read` | - | `ApiResponse<EmployeeBenefitEnrollmentFilterMetadataResponse>` |
| `GET` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/summary` | Mengambil ringkasan statistik enrollment | `EmployeeBenefitEnrollment:Read` | - | `ApiResponse<EmployeeBenefitEnrollmentSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments` | Mengambil daftar enrollment benefit berhalaman | `EmployeeBenefitEnrollment:Read` | `EmployeeBenefitEnrollmentPagedQuery` | `ApiResponse<PagedResult<EmployeeBenefitEnrollmentListResponse>>` |
| `GET` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}` | Mengambil detail lengkap enrollment benefit | `EmployeeBenefitEnrollment:Read` | `id` (Guid) | `ApiResponse<EmployeeBenefitEnrollmentResponse>` |
| `GET` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/by-workforce-profile/{workforceProfileId}` | Mengambil ringkasan benefit SDM berdasarkan WorkforceProfileId | `EmployeeBenefitEnrollment:Read` | `workforceProfileId` (Guid) | `ApiResponse<EmployeeBenefitSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/status-history` | Mengambil riwayat alur status enrollment | `EmployeeBenefitEnrollment:Read` | `id` (Guid) | `ApiResponse<List<EnrollmentStatusHistoryResponse>>` |
| `GET` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/movements` | Mengambil buku mutasi pergerakan saldo plafon | `EmployeeBenefitEnrollment:Read` | `id`, `BenefitMovementPagedQuery` | `ApiResponse<PagedResult<BenefitMovementResponse>>` |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments` | Mendaftarkan enrollment benefit baru | `EmployeeBenefitEnrollment:Create` | `CreateEmployeeBenefitEnrollmentRequest` | `ApiResponse<EmployeeBenefitEnrollmentResponse>` (201 Created) |
| `PUT` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}` | Memperbarui data pendaftaran benefit | `EmployeeBenefitEnrollment:Update` | `UpdateEmployeeBenefitEnrollmentRequest` | `ApiResponse<EmployeeBenefitEnrollmentResponse>` |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/submit` | Mengajukan enrollment untuk persetujuan atasan | `EmployeeBenefitEnrollment:Submit` | `EnrollmentTransitionRequest` | `ApiResponse<EmployeeBenefitEnrollmentResponse>` |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/approve` | Menyetujui pengajuan enrollment benefit | `EmployeeBenefitEnrollment:Approve` | `EnrollmentTransitionRequest` | `ApiResponse<EmployeeBenefitEnrollmentResponse>` |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/activate` | Mengaktifkan status kepesertaan benefit | `EmployeeBenefitEnrollment:Activate` | `EnrollmentTransitionRequest` | `ApiResponse<EmployeeBenefitEnrollmentResponse>` |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/cancel` | Membatalkan enrollment benefit | `EmployeeBenefitEnrollment:Cancel` | `EnrollmentCancelRequest` | `ApiResponse<EmployeeBenefitEnrollmentResponse>` |

### Grup: `Corporate / Human Resource / Benefit Management / Employee Benefit Dependent`
Tag: `[Tags("Corporate / Human Resource / Benefit Management / Employee Benefit Dependent")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{enrollmentId}/dependents` | Mengambil daftar tanggungan dalam satu enrollment | `EmployeeBenefitDependent:Read` | `enrollmentId` (Guid) | `ApiResponse<List<EmployeeBenefitDependentResponse>>` |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{enrollmentId}/dependents` | Mendaftarkan tanggungan baru (Validasi kuota & batas max) | `EmployeeBenefitDependent:Create` | `CreateEmployeeBenefitDependentRequest` | `ApiResponse<EmployeeBenefitDependentResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{enrollmentId}/dependents/{id}/verify` | Memverifikasi berkas dan keabsahan tanggungan | `EmployeeBenefitDependent:Verify` | `VerifyEmployeeBenefitDependentRequest` | `ApiResponse<EmployeeBenefitDependentResponse>` |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{enrollmentId}/dependents/{id}/reject` | Menolak pendaftaran tanggungan | `EmployeeBenefitDependent:Reject` | `RejectEmployeeBenefitDependentRequest` | `ApiResponse<EmployeeBenefitDependentResponse>` |
| `POST` | `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{enrollmentId}/dependents/{id}/end` | Mengakhiri penjaminan tanggungan | `EmployeeBenefitDependent:End` | `EndEmployeeBenefitDependentRequest` | `ApiResponse<EmployeeBenefitDependentResponse>` |

---

## 4. Bukti implementasi kode

1. **Controllers Benefit & Tanggungan**:
   - Berkas: [EmployeeBenefitEnrollmentController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/BenefitManagement/Controllers/EmployeeBenefitEnrollmentController.cs)
   - Berkas: [EmployeeBenefitDependentController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/BenefitManagement/Controllers/EmployeeBenefitDependentController.cs)

2. **Domain Services**:
   - Berkas: [EmployeeBenefitEnrollmentService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitEnrollmentService.cs)
   - Berkas: [EmployeeBenefitDependentService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitDependentService.cs)
   - Berkas: [EmployeeBenefitBalanceService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitBalanceService.cs)
   - Berkas: [WorkforceBenefitOwnerResolver.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/BenefitManagement/Services/WorkforceBenefitOwnerResolver.cs)

3. **Registrasi Service**:
   - Terdaftar pada [Program.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Program.cs#L984-L988).

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: `EmployeeBenefitEnrollmentController` dan `EmployeeBenefitDependentController` mengelola eligibilitas dan plafon -> **LULUS**.
* **Acceptance Criteria 2**: Setiap mutasi saldo benefit (klaim/top-up tahunan) tercatat pada `HrdBenefitBalanceMovement` -> **LULUS**.
* **Acceptance Criteria 3**: Validasi mencegah pendaftaran tanggungan melebihi batas ketentuan paket benefit (`MaximumDependents`) -> **LULUS**.
