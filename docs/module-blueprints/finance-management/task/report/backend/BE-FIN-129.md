# Laporan Perubahan Backend — `BE-FIN-129`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-129` |
| Judul | Petugas HR menekan Ajukan Benefit dan enrollment langsung berstatus Diajukan dalam satu operasi |
| Slice | `S1` Ajukan Benefit atomik (`MVP-17`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-23" |
| Trace | `FR-FIN-261`; `FIN-DEC-243`; `FIN-DES-109`; `FIN-API-1.12` V.1 (`POST /submit`); `FIN-STATE-1.11` V.1; `FIN-VAL-290` |
| Contract version | `FIN-API-1.12` — approved 3 November 2026 |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 7 (repo 0, berkas diperiksa 1, berkas diubah 0, logika 1, kontrak 2, database 1, keamanan 1, workflow 1) |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. Frontend tidak disentuh |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `ba312826` |
| Tanggal | 3 November 2026 |
| Status | 🟡 **Source selesai, kompilasi belum dibuktikan.** `dotnet build` **NOT RUN** atas instruksi pemilik ("tanpa build dotnet otomatis") |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module | `Corporate` / `HumanResource` / `BenefitManagement`. Modul ini **tidak terdaftar** pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; entity yang dipakai adalah legacy `TrxEmployeeBenefitEnrollment` |
| Keberlakuan | `TOUCHED LEGACY` pada `EmployeeBenefitEnrollmentService` dan `EmployeeBenefitEnrollmentController`. Tidak ada entity, model, atau configuration baru, sehingga `QBE-MOD-002`, `QBE-MOD-003`, dan `QBE-NAM-002` tidak berlaku |
| QBE yang berlaku | `QBE-SVC-001` (controller memanggil service, tidak ke `DbContext`), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-CODE-002` (controller tidak mengalokasikan nomor), `QBE-DTO-001` |
| Branch | `Yasmina`, sesuai penetapan roadmap |
| Working tree sebelum task | Memuat pekerjaan lain yang **tidak disentuh**: `FinanceReceivablePayrollSyncService.cs`, `PayrollManagement/*`, `Program.cs` |

## 1. Masalah yang diperbaiki

Sebelum task ini, tombol pendaftaran benefit hanya dapat membuat enrollment berstatus **Draft**. Agar masuk antrean HRD, petugas harus
membukanya lagi dan menekan Ajukan. Bila Frontend memanggil Create lalu Submit sebagai dua permintaan, kegagalan permintaan kedua
meninggalkan Draft yang tidak disengaja. Permintaan pemilik (`FIN-DEC-243`): pembuatan dan pengajuan harus **satu operasi atomik di Backend**.

## 2. Proses bisnis

1. Petugas HR memilih penerima (karyawan atau dokter tetap), program, tanggal pendaftaran dan mulai berlaku, lalu menekan **Ajukan Benefit**.
2. Backend memvalidasi periode, penerima dari HR, dan program benefit (aturan yang sama dengan pembuatan Draft).
3. Backend mengalokasikan nomor enrollment, membuka transaksi Serializable, dan mengunci kombinasi penerima + program.
4. Bila sudah ada enrollment terbuka untuk penerima dan program yang sama, Backend menolak dengan 409.
5. Bila lolos, enrollment disimpan **langsung berstatus Diajukan**, dengan `SubmittedAt` dan `SubmittedByUserId` terisi, dalam transaksi yang sama.
6. Audit `Enrollment.SubmittedNew` dicatat dengan aktor, lalu detail enrollment dikembalikan dengan kode 201.

Contoh: Staf HR Rina mengajukan benefit "Medical Staf" untuk Rendy Saputra mulai 1 November 2026.
- Berhasil → enrollment `BEN000123` berstatus **Submitted**, pengaju Rina, menunggu HRD.
- Rina menekan tombol dua kali cepat → permintaan pertama membuat `BEN000123`; yang kedua ditolak 409 "Pegawai ini sudah punya enrollment yang masih berjalan pada plan yang sama." dan tidak ada baris kedua.
- Program benefit kosong → 422 "Plan benefit tidak ditemukan atau tidak aktif." dan **tidak ada baris tersimpan**, jadi tidak ada Draft sisa.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/backend/` (`TASK_RULES`, `TASK_CLASSIFICATION`, `BACKEND_ENGINEERING_CONTRACT`, `transaction-endpoint-standard`, `role-access-rules`, `REPORT_TEMPLATE`);
`EmployeeBenefitEnrollmentService.cs`; `EmployeeBenefitEnrollmentController.cs`; `EmployeeBenefitEnrollmentDtos.cs`;
`TrxEmployeeBenefitEnrollmentConfiguration.cs` (constraint dan index); `ApplicationDbContext.cs` (`SaveChanges` tidak menimpa `CreateDateTime`);
`Seeders/AccessMenuSeeder.cs` dan `PermissionRegistryDescriptor.cs` (pendaftaran izin); kontrak `FIN-API-1.12`, `FIN-STATE-1.11`, `FIN-VAL-1.14`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitEnrollmentService.cs` | `CreateAsync` kini membungkus inti baru `CreateCoreAsync(…, submitImmediately, …)`; method publik baru `CreateAndSubmitAsync` memanggil inti yang sama dengan `submitImmediately: true`. Status awal `Submitted`, `SubmittedAt` dan `SubmittedByUserId` diisi dari satu cap waktu `now` yang juga dipakai `CreateDateTime`. Audit `Enrollment.SubmittedNew` vs `Enrollment.Created`. `GetStatusHistoryAsync` tidak menampilkan baris Draft bila `SubmittedAt == CreateDateTime` (enrollment tidak pernah menjadi Draft) |
| `Areas/Corporate/HumanResource/BenefitManagement/Controllers/EmployeeBenefitEnrollmentController.cs` | Action baru `SubmitNew` (`POST submit`) memanggil `CreateAndSubmitAsync` |

Perilaku `CreateAsync` untuk Draft **tidak berubah**: status `Draft`, `SubmittedAt` kosong, audit `Enrollment.Created`. `Program.cs` tidak disentuh karena service sudah terdaftar.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `POST …/employee-benefit-enrollments/submit` (aditif). `POST /`, `PUT /{id}`, `POST /{id}/submit`, `POST /{id}/cancel` tidak berubah. Riwayat status enrollment yang langsung diajukan tidak lagi memuat baris Draft (perubahan tampilan) |
| Database | `NOT APPLICABLE`. Tanpa kolom, tabel, atau migration baru. Kolom `SubmittedAt` dan `SubmittedByUserId` sudah ada. Index unik enrollment terbuka `(WorkforceProfileId, BenefitPlanId)` sudah mencakup status `Submitted`, sehingga duplikat juga dijaga database |
| Keamanan/Auth | Izin `EmployeeBenefitEnrollment : Create` (`FIN-DES-109`). `[AccessAction("Create", …)]` dan `[AccessPermission("EmployeeBenefitEnrollment", "Create")]` cocok dengan nama `ControllerName` dan dengan `Create` yang sudah ada, sehingga tidak ada baris izin baru yang perlu dicentang admin. Aktor diambil dari token (`CurrentUserId()`), tidak dari body |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Benefit Management / Employee Benefit Enrollment

Base URL: `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/submit` | **Ajukan Benefit**: membuat enrollment langsung berstatus Diajukan | `EmployeeBenefitEnrollment : Create` |

Request: `CreateEmployeeBenefitEnrollmentRequest` (sama dengan `POST /`): `workforceProfileId` atau `employeeId`, `benefitPlanId`, `enrollmentDate`, `effectiveStartDate`, `effectiveEndDate?`, `notes?`.
Response: `ApiResponse<EmployeeBenefitEnrollmentResponse>`.

| Kode | Arti bagi pengguna |
| --- | --- |
| `201` | Pengajuan tersimpan dan langsung berstatus Diajukan |
| `400` | Isian tidak lolos pengikatan model |
| `403` | Tidak punya hak akses tindakan ini |
| `409` | Sudah ada enrollment terbuka pada penerima dan program yang sama |
| `422` | Penerima, program, atau tanggal tidak sah |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: tanpa build dotnet otomatis |
| QBE preflight / conformance | Sesuai (lihat bagian Preflight) | `PASS` | Telaah terhadap `BACKEND_ENGINEERING_CONTRACT.md` |
| Review diff dan scope | Dua berkas source berubah; tidak ada perubahan di luar scope | `PASS` | `git diff` atas dua berkas |
| Verifikasi kontrak API | Rute, izin, request, response, dan kode status sama dengan `FIN-API-1.12` V.1 | `PASS` | Telaah `api-contract.md` bagian V.1 vs controller |
| Verifikasi proses bisnis, jalur berhasil | Status awal `Submitted`, `SubmittedAt`/`SubmittedByUserId` terisi, audit mencatat aktor | `PASS` (penelusuran kode) | `CreateCoreAsync` |
| Verifikasi proses bisnis, validasi gagal | `ValidatePeriod`, resolver penerima, dan pencarian plan melempar pengecualian sebelum transaksi dibuka; tidak ada baris | `PASS` (penelusuran kode) | Urutan di `CreateCoreAsync` |
| Verifikasi proses bisnis, klik ganda | Kunci advisory per penerima+program lalu `hasOpen` → 409; index unik terfilter menjadi pagar kedua | `PASS` (penelusuran kode) | `AcquireLockAsync`, konfigurasi index |
| Penelusuran kompilasi | Ekspresi `cond ? now : null` dan `cond ? actorUserId : null` ke `DateTime?`/`Guid?` memakai target-typed conditional (C# 9+; proyek `net9.0`) | `NOT RUN` | Belum dibuktikan kompiler |
| Verifikasi runtime | Tidak ada server atau database yang dijalankan | `NOT RUN` | — |
| Automated test | `AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | — | — |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Respons 201 dengan `enrollmentStatus = Submitted` | Terpenuhi menurut source; **belum terbukti runtime** | `StatusCode(201, …)`; status awal `Submitted` |
| (2) Isian tidak sah → 422, nol baris | Terpenuhi menurut source; belum terbukti runtime | Validasi dilakukan sebelum `Add` dan transaksi |
| (3) Klik ganda → satu baris, kedua 409 | Terpenuhi menurut source; belum terbukti runtime | Kunci advisory, `hasOpen`, index unik |
| (4) `POST /`, `PUT`, `POST /{id}/submit` lama tidak berubah | Terpenuhi | `CreateAsync` mendelegasikan ke inti yang sama dengan `submitImmediately: false`; `SubmitAsync` tidak disentuh |
| DoD: build lulus | **Belum terpenuhi** | `dotnet build` NOT RUN atas instruksi pemilik |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ini tidak boleh ditandai ✅ sebelum `dotnet build` dijalankan atas instruksi pemilik dan lulus |
| Masalah yang diketahui | Validasi `[Required]` pada `Guid`/`DateTime` non-nullable di DTO tidak menolak nilai default pada tingkat pengikatan model; perilaku sama dengan `POST /` dan tidak diubah. Pemetaan kegagalan serialisasi Postgres (`40001`) saat commit oleh middleware belum diperiksa; sama dengan jalur Create |
| Risiko tersisa | Kompilasi belum terbukti. Pengurutan riwayat status bergantung pada kesamaan `SubmittedAt`/`CreateDateTime` untuk mengenali pengajuan langsung; enrollment Draft lama tidak terpengaruh karena waktunya selalu berbeda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | (1) Pemilik menjalankan `dotnet build` untuk mengangkat task ke ✅; (2) `FE-FIN-056` kini dapat memakai endpoint; (3) `BE-FIN-130` berikutnya |

`git status --short` di akhir pekerjaan (perubahan task ini ditandai **[task]**, sisanya milik pekerjaan lain dan tidak disentuh):

```text
 M Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivablePayrollSyncService.cs
 M Areas/Corporate/HumanResource/BenefitManagement/Controllers/EmployeeBenefitEnrollmentController.cs   [task]
 M Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitEnrollmentService.cs         [task]
 D Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunInstallmentDeductionController.cs
 M Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollInstallmentDeductionDtos.cs
 M Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollInstallmentDeductionService.cs
 M Program.cs
?? Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunsController.cs
?? Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollRunDtos.cs
?? Areas/Corporate/HumanResource/PayrollManagement/Models/PayrollRunStatuses.cs
?? Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollRunService.cs
 M docs/module-blueprints/finance-management/** (blueprint Revisi 22, roadmap REV-23, laporan ini)
```
