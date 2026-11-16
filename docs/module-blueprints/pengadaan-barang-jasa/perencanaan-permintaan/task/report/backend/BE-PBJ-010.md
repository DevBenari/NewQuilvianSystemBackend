# Laporan Perubahan Backend — `BE-PBJ-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-010` |
| Judul | Pagu |
| Slice | `MVP-3` — pagu dan komitmen (`EPIC PBJ-03`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-010` |
| Trace | `PBJ-DEC-009`, `030`, `033`, `036`; FR-PBJ-060..062; STATE §4, §5; `PBJ-VAL-060`..`062`; INT §2; `AT-PP-45`, `46` |
| Contract version | `PBJ-PP-API` 1.1 (dengan delta di bagian 3.3), `PBJ-PP-STATE` 1.0, `PBJ-PP-VAL` 1.0, `PBJ-PP-INT` 1.1, `PBJ-PP-PERM` 1.2 — semua `approved` |
| Dependency | `BE-PBJ-005` ✅ |
| Klasifikasi | `MEDIUM` (logika 2, kontrak API 2, database 2, keamanan 1) |
| Task mode | `BACKEND` (diberikan pemilik 16 November 2026 lewat jawaban "BE lalu FE") |
| Target tulis | `Areas/Corporate/ProcurementManagement/Planning/**`, service master (cek pemakaian), konfigurasi EF, `ApplicationDbContext.cs`, extension DI, migration `AddProcurementBudget` |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `27fac19c` (belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ **SELESAI.** `AT-PP-45` (ganda 409, revisi di bawah komitmen 422) dan `AT-PP-46` (lineage komitmen) terbukti runtime; 12 skenario `PASS`; migration diterapkan ke `QuilvianNewDevSukma` |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Planning` |
| Registry | `Corporate / Procurement · ProcurementManagement / Planning · Prc · ACTIVE` (`PBJ-DEC-035`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs`, service master Pengadaan |
| Arketipe endpoint | Transaksi ber-lifecycle — tanpa `GET /options`, `PATCH /{id}/status`, `DELETE /{id}` |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Prc`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-CODE-001..004`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (53 berkas, 0 temuan) |

## 1. Masalah yang diperbaiki

Belum ada pagu pengadaan, sehingga verifikasi PR tidak dapat menandai PR yang melebihi anggaran dan sub-modul lain
tidak punya tempat mencatat komitmen. Task ini menyediakan pagu per tahun × unit × kategori × sumber dana yang
berversi, sisa pagu yang dihitung server, dan layanan komitmen.

## 2. Proses bisnis

1. Petugas anggaran membuat pagu untuk unit dalam cakupannya; nilai > 0 (`PBJ-VAL-061`); satu pagu hidup per kunci
   (`PBJ-VAL-060`, galat menautkan pagu yang ada).
2. Ajukan → approval `PBJ-PAGU` (Direktur Keuangan). Disetujui → versi berlaku.
3. **Sisa pagu** = nilai versi berlaku − Σ komitmen aktif pada garis keturunannya (`LineageKey`). Kunci tanpa pagu
   dianggap sisa 0.
4. **Revisi** membuat versi baru dengan garis keturunan sama; nilai diubah lewat `PUT`. Kunci (tahun, unit, kategori,
   sumber dana) hanya dapat diganti pada pagu versi 1 yang belum pernah diajukan. Revisi tidak boleh di bawah komitmen
   aktif (`PBJ-VAL-062`). Saat disetujui, versi lama `SUPERSEDED`.
5. `ProcurementBudgetCommitmentService.CommitAsync`/`ReleaseAsync` disiapkan untuk `pemilihan-penyedia`: idempoten per
   (garis keturunan, dokumen sumber), ikut transaksi pemanggil.

**Contoh:** pagu IGD Rp40 juta, komitmen Rp10 juta → sisa Rp30 juta. Revisi ke Rp5 juta → ditolak "Pagu tidak boleh
lebih kecil dari komitmen yang sudah berjalan (Rp 10.000.000)". Revisi ke Rp50 juta disetujui → sisa Rp40 juta.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak bagian Budget, STATE §4, VAL §2, INT §2, kamus data `PrcBudget`/`PrcBudgetCommitment`; layanan rencana
kebutuhan (`BE-PBJ-009`) sebagai pola.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Planning/Models/PrcBudget.cs`, `PrcBudgetCommitment.cs` + konfigurasi | Entity baru; unik parsial versi berlaku, unik (lineage, versi), unik komitmen aktif per sumber; CHECK nominal |
| `Planning/DTOs/BudgetDtos.cs` | DTO pagu, sisa, ringkasan |
| `Planning/Services/ProcurementBudgetService.cs` | CRUD, ajukan, revisi, sisa (termasuk `GetRemainingByCategoryAsync` untuk verifikasi PR), penyedia akses lampiran, penerap status `PBJ_BUDGET` |
| `Planning/Services/ProcurementBudgetCommitmentService.cs` | `FindCurrentLineageAsync`, `CommitAsync`, `ReleaseAsync` |
| `Planning/Controllers/BudgetsController.cs` | Endpoint bagian 4 |
| `Planning/ProcurementPlanningExplicitPermissions.cs` | `ProcurementBudget : ReadAllUnits` |
| `MasterData/Services/ProcurementItemCategoryService.cs`, `ProcurementFundSourceService.cs` | Kategori/sumber dana dipakai pagu tidak boleh dihapus |
| `ApplicationDbContext.cs`, extension DI | `DbSet`, registrasi |
| `Migrations/20261116032446_AddProcurementBudget.cs` + Designer, snapshot | 2 tabel, 12 index |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru. Delta: `GET /remaining` memakai `ProcurementBudget : Read`, bukan `ProcurementRequest : Read` — argumen ke-1 `[AccessPermission]` wajib sama dengan `ControllerName` controller-nya (`role-access-rules.md` bagian 3), dan selaras dengan `FE-PBJ-007` kriteria 3 "nominal tidak tampil bagi pengguna tanpa `ProcurementBudget : Read`". Tambahan `filters/metadata`, `summary`, `status-history`; `POST` 200; body `PUT` memuat kunci (hanya berlaku pada versi 1 yang belum diajukan). Tidak ada endpoint batal (sesuai kontrak) |
| Database | 2 tabel baru; delta kolom `SubmissionCount`. Diterapkan ke `QuilvianNewDevSukma` |
| Keamanan/Auth | Cakupan resource `ProcurementBudget`; nominal tidak terlihat tanpa izin baca pagu (`C-03`) |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Planning / Budget

Base URL `api/v1/corporate/procurement-management/budgets`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan | `ProcurementBudget : Read` |
| `GET` | `/summary` | Jumlah per status, total pagu, komitmen, sisa | `ProcurementBudget : Read` |
| `GET` | `/` | Daftar dengan komitmen dan sisa | `ProcurementBudget : Read` |
| `GET` | `/remaining` | Sisa per kategori × sumber dana untuk unit-tahun | `ProcurementBudget : Read` |
| `GET` | `/{id}` | Detail | `ProcurementBudget : Read` |
| `GET` | `/{id}/status-history` | Riwayat | `ProcurementBudget : Read` |
| `POST` | `/` | Buat | `ProcurementBudget : Create` |
| `PUT` | `/{id}` | Ubah `DRAFT`/`RETURNED` | `ProcurementBudget : Update` |
| `POST` | `/{id}/submit` | Ajukan (`Idempotency-Key`) | `ProcurementBudget : Submit` |
| `POST` | `/{id}/revisions` | Buat revisi | `ProcurementBudget : Revise` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release -p:UseSharedCompilation=false` (diulang sesudah setiap perbaikan) | 0 error, 601 warning (= baseline hari ini); 0 warning dari berkas Pengadaan, `Wfl`, atau migration yang berubah | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 53 berkas `.cs` yang berubah/baru | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| `dotnet ef database update` ke `QuilvianNewDevSukma` (izin pemilik 16 November 2026) | `AddProcurementNeedPlan`, `AddProcurementBudget`, `AddProcurementPackagePlan` diterapkan; 0 pending | `PASS` | Keluaran perintah + `migrations list` |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma`, aktor non-SuperAdmin `TEST-PBJ` (fixture lewat API) | Lihat tabel skenario di bawah; seluruh suite Pengadaan 123 skenario `PASS` | `PASS` | `results.json` scratchpad sesi + query DB |

**Fixture uji (`TEST-PBJ-20261116110649`, dibuat lewat API sebagai SuperAdmin, belum dibersihkan).** Lima unit uji
(`TEST-PBJ IGD/Radiologi/Pengadaan/Manajemen/Keuangan Uji`), tujuh jabatan, delapan akun `Employee` **tanpa role**
(`reqA`, `reqA2`, `headA`, `reqB`, `proc`, `mgr`, `dirk`, `keu`), dan policy Akses Role per unit × jabatan sesuai peta
peran `permission-audit-matrix.md` bagian 3. Konfigurasi approval uji (izin pemilik "nilai uji di dev"): definisi
`PBJ-*` diaktifkan, satu baris `MstApprovalMatrix` fallback `SpecificUser` per langkah, dan empat
`MstRejectionReason` — semuanya bertanda `DATA UJI PBJ (bukan nilai SOP)`. Komitmen pagu uji Rp10.000.000
(`SourceDocumentType = UJI_PBJ`) disisipkan lewat DB sesuai catatan `AT-PP-45`. Sisa fixture percobaan pertama
(`TEST-PBJ-20261116105253`) juga masih ada.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AT-PP-45a` | Pagu ganda tahun×unit×kategori×sumber dana → 409 PBJ-VAL-060 | `PASS` | http=409; code=PBJ-VAL-060; message=Pagu untuk kombinasi ini sudah ada. Buat revisi. |
| `AT-PP-45b` | Revisi pagu Rp5jt di bawah komitmen Rp10jt → 422 PBJ-VAL-062 | `PASS` | http=422; code=PBJ-VAL-062; message=Pagu tidak boleh lebih kecil dari komitmen yang sudah berjalan (Rp 10.000.000). |
| `AT-PP-46` | Revisi pagu naik ke 50jt disetujui → v1 SUPERSEDED; sisa = 50jt − komitmen lama 10jt = 40jt | `PASS` | approvals=[['PBJ-PAGU-01', 'dirk', 200]]; remaining=40000000.0 |
| `AT-PP-62` | Skema: 14 tabel Pengadaan, index unik parsial, CHECK kuantitas/nominal, kolom ActivationConditionCode varchar(50) NULL | `PASS` | tables=14; uniquePartial={'IX_PrcNeedPlan_CurrentVersion': True, 'IX_PrcBudget_CurrentVersion': True, 'IX_PrcBudgetCommitment_ActiveSource': True, 'IX_PrcPurchaseRequest_RequestNumber': True, 'IX_PrcPackagePlan_PackagePlanNumber': True, 'IX_PrcNeedPlan_PlanNum |
| `C-01` | Pagu IGD Rp40jt diajukan, disetujui Direktur Keuangan → APPROVED berlaku | `PASS` | approvals=[['PBJ-PAGU-01', 'dirk', 200]]; status=APPROVED |
| `C-02` | Nilai pagu 0 → 422 PBJ-VAL-061 | `PASS` | http=422; code=PBJ-VAL-061; message=Nilai pagu harus lebih dari 0. |
| `C-03` | Pengguna tanpa ProcurementBudget : Read → 403 (nominal tidak tampil) | `PASS` | http=403; code=None; message=Anda tidak memiliki akses ke menu atau fitur ini. |
| `C-04` | Komitmen uji Rp10jt (lewat DB) → sisa = pagu − komitmen = Rp30jt | `PASS` | http=200; rows=[[40000000.0, 10000000.0, 30000000.0]] |
| `C-05` | Kunci tanpa pagu → baris HasBudget=false, sisa 0 | `PASS` | http=200 |
| `C-06` | Ringkasan pagu menghitung sisa server | `PASS` | http=200; summary={'currentAmount': 140000000.0, 'committedAmount': 10000000.0, 'remainingAmount': 130000000.0} |
| `C-07` | Revisi tidak boleh mengganti unit/kategori/sumber dana → 422 | `PASS` | http=422; code=PBJ-VAL-060; message=Tahun, unit, kategori, dan sumber dana hanya dapat diganti pada pagu baru yang belum pernah diajukan. |
| `F-AT-5` | Penyedia akses rencana paket dan pagu terdaftar (daftar lampiran 200) | `PASS` |  |

12 skenario, 12 `PASS`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PP-45` pagu ganda 409; revisi di bawah komitmen 422 | Terpenuhi | `AT-PP-45a`, `AT-PP-45b` |
| `AT-PP-46` sisa = pagu berlaku − komitmen aktif lintas versi | Terpenuhi | `C-04`, `AT-PP-46` |
| Delta `PBJ-DEC-036` `ProcurementBudget : ReadAllUnits` + cakupan | Terpenuhi | `ProcurementPlanningExplicitPermissions.cs`, `C-05`/`C-06` (keu melihat semua unit) |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Komitmen uji disisipkan lewat DB (`UJI_PBJ`); pemanggil sebenarnya ada di `pemilihan-penyedia` |
| Masalah yang diketahui | Tidak ada aksi batal pagu di kontrak; draf yang salah kunci hanya dapat diganti kuncinya sebelum diajukan |
| Risiko tersisa | `NONE` di luar data uji dev |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. 17 berkas berubah dan 11 jalur baru di backend (lihat `git status --short` pada laporan `BE-PBJ-011`). |
| Langkah berikutnya | `FE-PBJ-007` |
