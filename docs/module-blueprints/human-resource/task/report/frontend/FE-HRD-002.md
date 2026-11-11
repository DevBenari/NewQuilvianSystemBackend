# Laporan Perubahan Frontend — `FE-HRD-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-002` |
| Judul | Penyediaan rute dan layar administrasi lintas-pegawai untuk Workforce Core |
| Slice | `S-A1` (Workforce Core Cross-Employee Administration) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-001`, `HRD-DEC-002`, `HRD-DEC-012`, `HRD-DEC-019`, `HRD-DEC-033`, `ISS-HRD-001-01`, `ISS-HRD-001-T2`, `ISS-HRD-001-T3`, `FIX-HRD-001-01`, `FIX-HRD-001-02`, `ISS-HRD-001-T1`, `FIX-HRD-001-03` |
| Contract version | Workforce Core Management API v1 (`WorkforceCoreCrossEmployeeController.cs`, `EmployeeProfileChangeController.cs`) |
| Dependency | `BE-HRD-003` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/`, `src/components/view/hr/workforce-core/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 (Diperbarui: 2 November 2026 via `FIX-HRD-001-01`, `FIX-HRD-001-02`, & `FIX-HRD-001-03`) |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Petugas administrasi kepegawaian rumah sakit membutuhkan kemampuan untuk melihat rekapitulasi data seluruh staf dalam satu layar terpadu, tidak hanya perorangan saat membuka profil satu per satu.

Task ini menyediakan 6 menu dan halaman administrasi lintas-pegawai di area `/hr/workforce-core/`:
1. **Perubahan Profil Pegawai**: Rekapitulasi permohonan perubahan data identitas atau kontak pegawai.
2. **Penempatan Organisasi**: Rekapitulasi penempatan unit, instalasi, dan divisi staf di rumah sakit.
3. **Penugasan Jabatan**: Rekapitulasi formasi jabatan struktural dan fungsional.
4. **Penetapan Manajer / Atasan**: Matriks atasan langsung pegawai untuk jalur persetujuan (*approval chain*).
5. **Riwayat Kepegawaian**: Rekam jejak perjalanan karier dan status kerja pegawai.
6. **Penetapan Gaji**: Rekapitulasi penetapan struktur gaji dan golongan remunerasi seluruh tenaga kerja rumah sakit (nominal gaji pokok dilindungi dan disembunyikan sesuai `HRD-DEC-033`).

Setiap layar dilengkapi dengan kotak pencarian dan penyaringan data (*DataFilter*), tabel data responsif (*DataTable*), status badge (*StatusBadge*), dan paginasi (*Pagination*).

---

## 1. Wewenang UI yang Dipakai

Menggunakan pola layout dashboard operasional Quilvian standar di bawah rute Next.js App Router: `src/app/hr/workforce-core/*`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman & breadcrumb | `Hero` | `REUSE` | Standar header Quilvian |
| Filter & pencarian | `DataFilter` | `REUSE` | Input pencarian kata kunci terintegrasi |
| Tabel penyajian data | `DataTable` | `REUSE` | Menampilkan kolom sesuai DTO backend |
| Penanda status data | `StatusBadge` | `REUSE` | Menampilkan badge status Aktif / Terverifikasi / Pending |
| Paginasi halaman | `Pagination` | `REUSE` | Navigasi halaman data |
| Peringatan & pesan kosong | `InformationAlert` | `REUSE` | Alert informasi dan penanganan error |

---

## 3. Berkas yang Berubah

### Komponen dan Layanan (Diperbarui per 2 November 2026)
* `src/components/view/hr/workforce-core/workforce-core-cross-employee-list-view.jsx`: Penanganan error diperbaiki via FIX-HRD-001-03 (InformationAlert message={error}, terjemahan Network Error, dan state tabel gagal menampilkan 'Data gagal dimuat.' dengan tombol 'Coba lagi').
* `src/lib/services/hr/workforce-core-cross-employee-service.js`: Diperbaiki alamat endpoint-nya dari `/v1/hr/...` menjadi `/v1/corporate/human-resource/workforce-core` dan `/v1/corporate/human-resource/employee-profile-changes` sesuai `WorkforceCoreCrossEmployeeController.cs:13` dan `EmployeeProfileChangeController.cs:18`.
* `src/components/view/hr/workforce-core/employee-profile-changes-view.jsx`: Kolom diselaraskan dengan `EmployeeProfileChangeListResponse` (`requestNumber`, `requestCategory`, `submittedAt`, `requestStatus`, `requestReasonText`, `appliedAt`).
* `src/components/view/hr/workforce-core/organization-assignments-view.jsx`: Kolom diselaraskan dengan `WfpOrganizationAssignmentResponse` (`departmentName`, `positionName`, `assignmentType`, `isPrimary`, `effectiveStartDate`, `isActive`). Kolom yang hanya ID backend dibersihkan.
* `src/components/view/hr/workforce-core/position-assignments-view.jsx`: Kolom diselaraskan dengan `WfpPositionAssignmentResponse` (`positionName`, `departmentName`, `assignmentType`, `isPrimary`, `isActing`, `effectiveStartDate`, `isActive`).
* `src/components/view/hr/workforce-core/manager-assignments-view.jsx`: Kolom diselaraskan dengan `WfpManagerAssignmentResponse` (`managerDisplayName`, `managerProfileCode`, `managerType`, `isPrimaryManager`, `canApproveRequests`, `effectiveStartDate`, `isActive`).
* `src/components/view/hr/workforce-core/employment-histories-view.jsx`: Kolom disahkan oleh diselaraskan dengan `approvedByUserName`.
* `src/components/view/hr/workforce-core/salary-assignments-view.jsx`: Kolom diselaraskan dengan `WfpSalaryAssignmentCrossEmployeeResponse`. Kolom "Gaji Pokok" dihapus sesuai regulasi kerahasiaan `HRD-DEC-033`.
* `src/components/view/hr/workforce-core/workforce-core-cross-employee-list-view.jsx`: Pagination ganda dihapus via FIX-HRD-001-04, digantikan integrasi internal DataTable (`PaginationComponent={RegionPagination}`, `sortLatestFirst={false}`). Dukungan kartu ringkasan SummaryGrid dan filter Pola A ditambahkan via FIX-HRD-001-11.
* `src/components/view/hr/workforce-core/employee-profile-changes-view.jsx`: Kartu ringkasan SummaryGrid (Total, Sedang Diverifikasi, Perlu Revisi, Disetujui, Diterapkan) dan saringan Status & Kategori Pola A diimplementasikan via FIX-HRD-001-11.
* `tests/unit/workforce-core-cross-employee.test.mjs`: Test unit otomatis untuk memverifikasi alamat endpoint, kolom DTO, alert error, pagination tunggal, dan ringkasan/saringan Pola A.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada seluruh berkas view dan service |
| Komunikasi API Backend | `PASS` | Seluruh endpoint terhubung ke controller `BE-HRD-003` (`WorkforceCoreCrossEmployeeController` dan `EmployeeProfileChangeController`) di backend |
| Unit Test Otomatis | `PASS` | `node --test tests/unit/workforce-core-cross-employee.test.mjs` — 14 tests pass |
| Penanganan State | `PASS` | Mendukung state memuat (*loading*), data kosong (*empty*), dan kegagalan jaringan (*error*) |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia 6 rute dan layar administrasi kepegawaian lintas-pegawai | Terpenuhi | 6 route di bawah `/hr/workforce-core/` |
| 2. Panggilan API terhubung ke endpoint `BE-HRD-003` | Terpenuhi | `WorkforceCoreCrossEmployeeService` memanggil `/v1/corporate/human-resource/...` |
| 3. Seluruh elemen UI menggunakan base component baku Quilvian | Terpenuhi | Menggunakan Hero, SummaryGrid, DataFilter, FilterSelect, DataTable, StatusBadge, RegionPagination |
| 4. Memiliki penanganan loading, error, dan pagination tunggal yang benar | Terpenuhi | Implementasi lengkap di `workforce-core-cross-employee-list-view.jsx` |
| 5. Sesuai kontrak DTO backend dan mematuhi batas privasi gaji | Terpenuhi | Kolom disesuaikan dengan DTO; kolom 'Gaji Pokok' ditiadakan (`HRD-DEC-033`) |

---

## 6. Catatan Penutup

Task `FE-HRD-002` telah diperbaiki secara tuntas melalui implementasi `FIX-HRD-001-01`, `FIX-HRD-001-02`, `FIX-HRD-001-03`, `FIX-HRD-001-04`, dan `FIX-HRD-001-11` (merujuk ke [PLAN-REPAIR-HRD-001](../../docs/plan-repair/plan-repair-001-layar-hr-kosong-salah-alamat-api.md)).
