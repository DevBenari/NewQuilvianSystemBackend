# Laporan Perubahan Backend — `BE-HRD-003`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-003` |
| Judul | Penyediaan Endpoint Rekapitulasi Lintas-Pegawai Administrasi Kepegawaian (Workforce Core) |
| Slice | `S-A1` (Administrasi Kepegawaian Lintas-Pegawai) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-012`, `HRD-DEC-033`, `IN-02` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-002` (Selesai ✅) |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `WorkforceCoreCrossEmployeeController.cs`, `WorkforceCoreCrossEmployeeQueryService.cs`, `WorkforceCoreCrossEmployeeDtos.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `16f6b2ad28e0e9cbd374354abe240ff3b163ace0` |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki

Sebelumnya, seluruh endpoint manajemen data anak kepegawaian (penempatan organisasi, penempatan jabatan, penugasan atasan/manajer, riwayat kepegawaian, dan penetapan gaji) dirancang secara kaku hanya dalam lingkup per profil tunggal (`/workforce-profiles/{workforceProfileId}/...`). Kondisi ini membuat enam menu administratif pada antarmuka frontend (Administrasi Kepegawaian) tidak memiliki endpoint backend yang dapat menyajikan daftar rekapitulasi data lintas seluruh pegawai rumah sakit dalam satu periode kerja.

Selain itu, penetapan gaji (`salary-assignments`) merupakan data yang sangat rahasia. Sesuai keputusan arsitektur `HRD-DEC-012` dan audit keamanan `HRD-DEC-033`, staf HR membutuhkan halaman pemantauan status penetapan gaji (siapa yang sudah ditetapkan gajinya, kelas gaji, tanggal efektif) tanpa boleh melihat nominal rupiah gaji orang lain secara borongan. Pengiriman nominal ke peramban untuk kemudian disembunyikan di layer frontend adalah celah keamanan fatal (*security by obscurity*).

Oleh karena itu, task `BE-HRD-003` menyediakan endpoint rekapitulasi lintas pegawai resmi di bawah base URL `api/v1/corporate/human-resource/workforce-core` dengan penegakan pemisahan hak akses:
1. Rekapitulasi penetapan gaji lintas pegawai **wajib meniadakan nominal `BaseSalary`** pada response DTO-nya.
2. Pembacaan nominal gaji hanya dilayani melalui endpoint detail satu per satu yang dilindungi oleh hak akses sensitif tersendiri (`WfpSalaryAssignment:ViewAmount`).

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[HR Admin / Kepala Bagian Kepegawaian]
         │
         ├──► 1. Mengakses menu rekapitulasi (Organisasi / Jabatan / Atasan / Riwayat / Gaji)
         │       └─► Backend menyaring data berdasarkan Unit Kerja, Jabatan, Status Aktif, & Rentang Tanggal
         │
         ├──► 2. Melihat ringkasan status kepegawaian seluruh staf RS pada satu tabel terpadu
         │       └─► Pada tabel Gaji: Nominal TIDAK dikirim (HRD-DEC-033)
         │
         └──► 3. Membuka detail nominal satu pegawai (Hanya jika memiliki wewenang ViewAmount)
                 └─► Endpoint /salary-assignments/{id}/amount memvalidasi permission & mengembalikan nilai
```

* **Tujuan**: Memungkinkan Bagian SDM / HRD memantau penempatan, pergerakan, dan kepatuhan penetapan remunerasi ratusan staf medis maupun non-medis secara terpusat tanpa membuka nominal gaji secara massal.
* **Pelaku**: Tim Administrasi SDM Rumah Sakit, Auditor Internal, Kepala Unit Kerja.
* **Pemicu**: Monitoring rotasi staf, audit kepegawaian berkala, atau penutupan periode payroll.
* **Langkah & Aturan**:
  1. Pengguna memanggil endpoint rekapitulasi penempatan atau riwayat dengan filter unit kerja, jabatan, dan tanggal efektif.
  2. Sistem memproyeksikan data profil pegawai (`DisplayName`, `ProfileCode`) beserta unit penempatannya dalam format paging terstandar `ApiResponse<PagedResult<T>>`.
  3. Pada query penetapan gaji lintas pegawai, backend mengecualikan kolom `BaseSalary` dari DTO response.
  4. Pengguna dengan hak akses khusus `WfpSalaryAssignment:ViewAmount` dapat memanggil endpoint spesifik per penetapan untuk melihat nominal gaji aktual pegawai tersebut.
* **Hasil Akhir**: Daftar rekapitulasi lintas pegawai tersaji cepat, akurat, dan patuh terhadap standar kerahasiaan data rumah sakit.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang dibuat & dimodifikasi

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `Areas/Corporate/HumanResource/WorkforceCore/DTOs/WorkforceCoreCrossEmployeeDtos.cs` | Baru | Definisi `WfpSalaryAssignmentCrossEmployeeResponse` (tanpa `BaseSalary`) dan `WfpSalaryAmountResponse` (untuk `ViewAmount`). |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/WorkforceCoreCrossEmployeeQueryService.cs` | Baru | Service query lintas pegawai dengan optimasi `AsNoTracking()`, filter kriteria, sorting dinamis, resolver tanggal, dan lookup nama aktor. |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforceCoreCrossEmployeeController.cs` | Baru | Controller administrasi lintas-pegawai dengan rute `api/v1/corporate/human-resource/workforce-core` dan 6 endpoint terproteksi. |
| `Program.cs` | Diperbarui | Registrasi `WorkforceCoreCrossEmployeeQueryService` ke DI container ASP.NET Core (`AddScoped`). |
| `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` | Diperbarui | Penandaan task `BE-HRD-003` menjadi status selesai (`✅`). |
| `docs/module-blueprints/human-resource/roadmap/requirement-traceability.md` | Diperbarui | Penandaan trace requirement `IN-02` menjadi status terbukti penuh (`✅`). |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| :--- | :--- |
| **Kontrak API** | Penambahan 6 endpoint baru di bawah `api/v1/corporate/human-resource/workforce-core` sesuai spesifikasi `contracts/api-contract.md` Bagian Daftar lintas-pegawai (`S-A1`). |
| **Database** | `NOT APPLICABLE` — Tidak ada migrasi tabel baru atau perubahan skema database; menggunakan model data `Wfp*` yang sudah ada di database. |
| **Keamanan & Otorisasi** | Mengimplementasikan hak akses terpisah: `ReadAll` untuk daftar umum dan `ViewAmount` untuk pembacaan nominal gaji sesuai ketetapan `contracts/permission-audit-matrix.md` dan `HRD-DEC-033`. |

---

## 4. Dokumentasi endpoint

#### `[Tags("Corporate / Human Resource / Workforce Core / Cross-Employee Administration")]`
Base URL: `api/v1/corporate/human-resource/workforce-core`

| Method | Path | Deskripsi | Hak Akses | Request / Filter Query | Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/organization-assignments` | Rekapitulasi penempatan organisasi seluruh pegawai | `[AccessPermission("WfpOrganizationAssignment", "ReadAll")]` | `startDate`, `endDate`, `customPeriod`, `departmentId`, `positionId`, `organizationUnitId`, `workforceProfileId`, `assignmentType`, `isPrimary`, `isManagerialAssignment`, `isActive`, `search`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<WfpOrganizationAssignmentResponse>>` |
| `GET` | `/position-assignments` | Rekapitulasi penempatan jabatan seluruh pegawai | `[AccessPermission("WfpPositionAssignment", "ReadAll")]` | `startDate`, `endDate`, `customPeriod`, `positionId`, `departmentId`, `jobLevelId`, `workforceProfileId`, `assignmentType`, `isPrimary`, `isActing`, `isActive`, `search`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<WfpPositionAssignmentResponse>>` |
| `GET` | `/manager-assignments` | Rekapitulasi relasi atasan seluruh pegawai | `[AccessPermission("WfpManagerAssignment", "ReadAll")]` | `startDate`, `endDate`, `customPeriod`, `managerProfileId`, `workforceProfileId`, `departmentId`, `managerType`, `isPrimaryManager`, `canApproveRequests`, `isActive`, `search`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<WfpManagerAssignmentResponse>>` |
| `GET` | `/employment-histories` | Rekapitulasi riwayat mutasi/perubahan status kepegawaian | `[AccessPermission("WfpEmploymentHistory", "ReadAll")]` | `startDate`, `endDate`, `customPeriod`, `workforceProfileId`, `historyType`, `departmentId`, `positionId`, `search`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<WfpEmploymentHistoryResponse>>` |
| `GET` | `/salary-assignments` | Rekapitulasi penetapan gaji seluruh pegawai (**tanpa nominal gaji**) | `[AccessPermission("WfpSalaryAssignment", "ReadAll")]` | `startDate`, `endDate`, `customPeriod`, `workforceProfileId`, `salaryStructureId`, `salaryGradeId`, `paymentFrequency`, `currencyCode`, `isPrimary`, `isApproved`, `isActive`, `search`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<WfpSalaryAssignmentCrossEmployeeResponse>>` |
| `GET` | `/salary-assignments/{id:guid}/amount` | Pembacaan nominal gaji spesifik satu penetapan | `[AccessPermission("WfpSalaryAssignment", "ViewAmount")]` | Route parameter `id` (GUID penetapan gaji) | `ApiResponse<WfpSalaryAmountResponse>` |

---

## 5. Bukti verifikasi & jaminan kualitas

1. **Integritas Aturan Keamanan `HRD-DEC-033`**: DTO `WfpSalaryAssignmentCrossEmployeeResponse` tidak memiliki properti `BaseSalary` sehingga tidak ada kemungkinan nominal gaji bocor lewat inspeksi jaringan browser.
2. **Kesesuaian Hak Akses**: Setiap endpoint memiliki dekorator `[AccessAction]` dan `[AccessPermission]` yang sesuai dengan kamus izin Quilvian.
3. **Pemisahan Tanggung Jawab**: Query lintas pegawai diisolasi pada `WorkforceCoreCrossEmployeeQueryService` tanpa mengubah atau merusak controller eksisting per-pegawai (`WfpSalaryAssignmentController`, dll.).
